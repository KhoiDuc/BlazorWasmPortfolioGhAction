#!/usr/bin/env python3
import time
import os
import sys
import requests
from datetime import datetime, timezone, timedelta
from zoneinfo import ZoneInfo
from dotenv import load_dotenv
import psycopg2
from vnstock import Quote

try:
    import yfinance as yf
except ImportError:
    yf = None


# Load environment variables
env_file_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), '.env')
if os.path.exists(env_file_path):
    load_dotenv(env_file_path)
else:
    load_dotenv()

# Setup cross-platform Beep alert sound
try:
    import winsound
    def play_alert(symbol, asset_type):
        winsound.Beep(800, 250)  # Frequency 800Hz, duration 250ms
        print(f">>> CẢNH BÁO: PHÁT HIỆN LỆNH LỚN CHO {symbol} ({asset_type.upper()})! <<<")
except ImportError:
    # Fallback for macOS/Linux using terminal bell
    def play_alert(symbol, asset_type):
        print("\a", end="", flush=True)  # Terminal bell
        print(f">>> CẢNH BÁO: PHÁT HIỆN LỆNH LỚN CHO {symbol} ({asset_type.upper()})! <<<")

def get_db_connection():
    """Create database connection using environment variables"""
    return psycopg2.connect(
        host=os.getenv('DB_HOST', 'localhost'),
        port=int(os.getenv('DB_PORT', 5432)),
        database=os.getenv('DB_NAME', 'trading'),
        user=os.getenv('DB_USER', 'postgres'),
        password=os.getenv('DB_PASSWORD', '')
    )

def get_scan_toggles():
    """Fetch scan toggles from public.system_settings, defaulting to True if not set"""
    toggles = {
        'scan_stock_vn': True,
        'scan_stock_us': True,
        'scan_crypto': True,
        'scan_futures': True,
        'scan_commodities': True,
        'scan_forex': True,
        'scan_yields': True
    }
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        cur.execute("SELECT key, value FROM public.system_settings;")
        rows = cur.fetchall()
        for row in rows:
            key, val = row[0], row[1]
            if key in toggles:
                toggles[key] = (val.lower() == 'true')
        cur.close()
    except Exception as e:
        # Default to True on failure
        pass
    finally:
        if conn:
            conn.close()
    return toggles

def update_heartbeat():
    """Update heartbeat timestamp in database to signal the script is alive"""
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        cur.execute("""
            INSERT INTO public.system_settings (key, value)
            VALUES ('alert_script_last_heartbeat', 'true')
            ON CONFLICT (key) DO UPDATE SET value = 'true', updated_at = CURRENT_TIMESTAMP;
        """)
        conn.commit()
        cur.close()
    except Exception as e:
        pass
    finally:
        if conn:
            conn.close()

def get_vn_time():
    """Get current time in Vietnam timezone (Asia/Ho_Chi_Minh) with UTC+7 fallback"""
    try:
        return datetime.now(ZoneInfo("Asia/Ho_Chi_Minh"))
    except Exception:
        return datetime.now(timezone(timedelta(hours=7)))

def get_us_time():
    """Get current time in US Eastern timezone (America/New_York) with fallback"""
    try:
        return datetime.now(ZoneInfo("America/New_York"))
    except Exception:
        now_utc = datetime.now(timezone.utc)
        month = now_utc.month
        offset_hours = -4 if 3 <= month <= 11 else -5
        return now_utc.astimezone(timezone(timedelta(hours=offset_hours)))

def get_us_watchlist_symbols():
    """Get all US stock symbols currently in the world_symbols_watchlist"""
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        query = """
        SELECT symbol
        FROM public.world_symbols_watchlist
        WHERE country = 'Mỹ';
        """
        cur.execute(query)
        rows = cur.fetchall()
        # Map each US symbol to a dummy 0.0 highest_price (no breakout comparison is in schema)
        symbols = {row[0]: 0.0 for row in rows}
        cur.close()
        return symbols
    except Exception as e:
        print(f"Lỗi query world_symbols_watchlist: {e}")
        return {}
    finally:
        if conn:
            conn.close()

def monitor_us_stocks_step(us_symbols, last_alerted_prices):
    """Performs one scan cycle on the list of US stock symbols using Yahoo Finance"""
    if not us_symbols:
        return

    print(f"🔍 [US STOCK] Đang quét {list(us_symbols.keys())}...")
    headers = {"User-Agent": "Mozilla/5.0"}
    for symbol in us_symbols:
        try:
            ticker = symbol.split(':')[-1]
            url = f"https://query1.finance.yahoo.com/v8/finance/chart/{ticker}"
            res = requests.get(url, headers=headers, timeout=5)
            if res.status_code != 200:
                continue

            data = res.json()
            results = data.get("chart", {}).get("result", [])
            if not results:
                continue

            meta = results[0].get("meta", {})
            current_price = meta.get("regularMarketPrice")
            fifty_two_high = meta.get("fiftyTwoWeekHigh")

            if current_price is None or fifty_two_high is None or fifty_two_high <= 0:
                continue

            # --- Check custom price alerts FIRST ---
            check_custom_stock_alerts(symbol, current_price)

            # Check if US stock price is breaking/approaching the 52-week high (within 1%)
            if current_price >= fifty_two_high * 0.99:
                last_price = last_alerted_prices.get(symbol, 0.0)
                # Alert again only if price moved >= 0.5%
                if abs(current_price - last_price) / current_price >= 0.005:
                    if ":" in symbol:
                        symbol = symbol.split(":")[-1]
                    message = f"Cảnh báo Stock US: Cổ phiếu {symbol} đã tiệm cận hoặc vượt đỉnh 52 tuần."
                    print(f"🚨 [US Stock Breakout] {symbol} tại giá {current_price} >= 99% Đỉnh 52 tuần {fifty_two_high}")
                    play_alert(symbol, "stock")
                    insert_triggered_alert("stock", symbol, current_price, message)
                    last_alerted_prices[symbol] = current_price

            time.sleep(0.5)  # Avoid rate limiting
        except Exception as e:
            print(f"⚠️ Lỗi quét US stock {symbol}: {e}")


def get_watchlist_symbols():
    """Get stock symbols meeting all 3 signals (EMA9, 52W High, Top Growth)"""
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        query = """
        SELECT symbol, MAX(highest_price)
        FROM public.symbols_watchlist
        WHERE signal_type IN ('ema9_above_ema21', 'near_52w_ath', 'top_growth_20d')
        GROUP BY symbol
        HAVING COUNT(DISTINCT signal_type) = 3;
        """
        cur.execute(query)
        rows = cur.fetchall()
        symbols = {row[0]: float(row[1]) if row[1] is not None else 0.0 for row in rows}
        cur.close()
        return symbols
    except Exception as e:
        print(f"Lỗi query symbols_watchlist: {e}")
        return {}
    finally:
        if conn:
            conn.close()

def get_watchlist_cryptos():
    """Get all cryptos currently in the cryptos_watchlist"""
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        query = """
        SELECT crypto, MAX(highest_price)
        FROM public.cryptos_watchlist
        GROUP BY crypto;
        """
        cur.execute(query)
        rows = cur.fetchall()
        cryptos = {row[0]: float(row[1]) if row[1] is not None else 0.0 for row in rows}
        cur.close()
        return cryptos
    except Exception as e:
        print(f"Lỗi query cryptos_watchlist: {e}")
        return {}
    finally:
        if conn:
            conn.close()

def get_watchlist_futures():
    """Get all futures contracts currently in the futures_watchlist"""
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        query = """
        SELECT symbol, MAX(highest_price)
        FROM public.futures_watchlist
        GROUP BY symbol;
        """
        cur.execute(query)
        rows = cur.fetchall()
        futures = {row[0]: float(row[1]) if row[1] is not None else 0.0 for row in rows}
        cur.close()
        return futures
    except Exception as e:
        print(f"Lỗi query futures_watchlist: {e}")
        return {}
    finally:
        if conn:
            conn.close()

def get_watchlist_forex():
    """Get all forex pairs currently in the forex_watchlist"""
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        query = """
        SELECT pair
        FROM public.forex_watchlist;
        """
        cur.execute(query)
        rows = cur.fetchall()
        # Returns a dict of {pair: 0.0} similar to other watchlist queries
        forex = {row[0]: 0.0 for row in rows}
        cur.close()
        return forex
    except Exception as e:
        print(f"Lỗi query forex_watchlist: {e}")
        return {}
    finally:
        if conn:
            conn.close()

def send_slack_message(text):
    """Send alert message to Slack if enabled"""
    slack_enabled = os.getenv('SLACK_NOTIFICATIONS_ENABLED', 'false').lower() == 'true'
    slack_webhook_url = os.getenv('SLACK_WEBHOOK_URL')
    if not slack_enabled or not slack_webhook_url:
        return
    
    try:
        res = requests.post(slack_webhook_url, json={"text": text}, timeout=5)
        if res.status_code == 200:
            print("🔔 Đã gửi cảnh báo thành công qua Slack!")
        else:
            print(f"⚠️ Lỗi gửi Slack: status={res.status_code}")
    except Exception as e:
        print(f"⚠️ Lỗi kết nối gửi Slack: {e}")

def insert_triggered_alert(asset_type, symbol, price, message):
    """Log the alert to public.triggered_alerts so the web UI reads it in real-time"""
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        
        # Delete any previous alerts for the same symbol to keep only the single latest message
        cur.execute("DELETE FROM public.triggered_alerts WHERE symbol = %s;", (symbol,))
        
        query = """
        INSERT INTO public.triggered_alerts (asset_type, symbol, price, message, is_read)
        VALUES (%s, %s, %s, %s, false);
        """
        cur.execute(query, (asset_type, symbol, price, message))
        conn.commit()
        cur.close()
        print(f"💾 Đã cập nhật báo động mới nhất cho {symbol} ({asset_type}) vào website database!")
        
        # Send to Slack if enabled
        send_slack_message(message)
    except Exception as e:
        print(f"❌ Lỗi ghi triggered_alert vào DB: {e}")
    finally:
        if conn:
            conn.close()

def cleanup_triggered_alerts():
    """Dọn dẹp bảng triggered_alerts:
    - Xóa các bản ghi quá 5 ngày
    - Giữ tối đa 200 bản ghi mới nhất
    """
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()

        # Xóa bản ghi quá 5 ngày
        cur.execute("DELETE FROM public.triggered_alerts WHERE created_at < NOW() - INTERVAL '5 days';")
        deleted_old = cur.rowcount

        # Nếu còn hơn 200 bản ghi, xóa các bản ghi cũ nhất, chỉ giữ 200 bản ghi mới nhất
        cur.execute("""
            DELETE FROM public.triggered_alerts
            WHERE id IN (
                SELECT id FROM public.triggered_alerts
                ORDER BY created_at DESC
                OFFSET 200
            );
        """)
        deleted_excess = cur.rowcount

        total_deleted = deleted_old + deleted_excess
        if total_deleted > 0:
            print(f"🧹 Đã dọn dẹp {total_deleted} bản ghi cũ từ triggered_alerts ({deleted_old} quá hạn, {deleted_excess} vượt giới hạn).")

        conn.commit()
        cur.close()
    except Exception as e:
        print(f"⚠️ Lỗi dọn dẹp triggered_alerts: {e}")
    finally:
        if conn:
            conn.close()

def monitor_stocks_step(symbols, last_processed_time, last_alerted_breakout_prices, threshold=5000):
    """Performs one scan cycle on the list of stock symbols"""
    if not symbols:
        return
    
    print(f"🔍 [STOCK] Đang quét {list(symbols.keys())} | Ngưỡng lệnh: >={threshold} CP...")
    for symbol in symbols:
        try:
            highest_price = symbols[symbol]
            q = Quote(symbol=symbol, source='kbs')
            df = q.intraday(page_size=30, show_log=False)
            if df is None or df.empty:
                continue

            recent_trades = df.tail(10)  # Check last 10 ticks
            if recent_trades.empty:
                continue

            # Check if stock price is breaking/above highest price (KBS price is in thousands, check within 1%)
            current_price_vnd = float(recent_trades.iloc[-1]['price']) * 1000.0
            
            # --- Check custom price alerts FIRST ---
            check_custom_stock_alerts(symbol, current_price_vnd)
            
            # --- Check for price breakout above highest_price ---
            if highest_price > 0 and current_price_vnd >= highest_price * 0.99:
                last_price = last_alerted_breakout_prices.get(symbol, 0.0)
                if abs(current_price_vnd - last_price) / current_price_vnd >= 0.005:
                    clean_sym = symbol.split(':')[-1] if ':' in symbol else symbol
                    message = f"Cảnh báo Cổ phiếu VN: Cổ phiếu {clean_sym} đã tiệm cận hoặc vượt đỉnh gần nhất ở mức {current_price_vnd:,.0f}đ (Đỉnh cũ: {highest_price:,.0f}đ)."
                    print(f"🚨 [VN Stock Breakout] {clean_sym} tại {current_price_vnd} >= 99% Đỉnh cũ {highest_price}")
                    play_alert(clean_sym, "stock")
                    insert_triggered_alert("stock", clean_sym, current_price_vnd, message)
                    last_alerted_breakout_prices[symbol] = current_price_vnd

            if highest_price > 0 and current_price_vnd < highest_price * 0.99:
                continue

            for _, trade in recent_trades.iterrows():
                current_time = trade['time']
                volume = int(trade['volume'])
                price = float(trade['price'])
                side = trade.get('side', 'N/A')

                # Initialize tracking set for new symbols
                if symbol not in last_processed_time:
                    last_processed_time[symbol] = set()

                if current_time not in last_processed_time[symbol] and volume >= threshold:
                    # Stock prices in KBS API are typically in thousands (e.g. 52.5 means 52,500 VND)
                    price_vnd = price * 1000.0
                    if ':' in symbol:
                        symbol = symbol.split(':')[-1]  # Remove any prefix like 'HOSE:'
                    message = f"Cảnh báo Stock: Tín hiệu lớn cho cổ phiếu {symbol}."
                    
                    print(f"🚨 [{current_time}] Cổ phiếu {symbol}: {side} {volume:,} cp tại giá {price}")
                    play_alert(symbol, "stock")
                    insert_triggered_alert("stock", symbol, price_vnd, message)
                    
                    last_processed_time[symbol].add(current_time)
                    if len(last_processed_time[symbol]) > 100:
                        # Pop oldest elements to prevent memory grow
                        last_processed_time[symbol] = set(list(last_processed_time[symbol])[-100:])
            
            # Avoid API rate-limiting
            time.sleep(0.5)
        except Exception as e:
            print(f"⚠️ Lỗi quét stock {symbol}: {e}")

def monitor_cryptos_step(cryptos, last_processed_trade_ids, last_alerted_breakout_prices, threshold_usd=10000.0):
    """Performs one scan cycle on the list of Binance cryptos"""
    if not cryptos:
        return

    print(f"🔍 [CRYPTO] Đang quét {list(cryptos.keys())} | Ngưỡng lệnh: >=${threshold_usd:,.0f} USDT...")
    for crypto in cryptos:
        try:
            url = f"https://api.binance.com/api/v3/trades?symbol={crypto}&limit=30"
            res = requests.get(url, timeout=5)
            if res.status_code != 200:
                continue

            trades = res.json()
            if not trades:
                continue

            # Get current price to compute dynamic coin volume threshold
            current_price = float(trades[-1]["price"])
            if current_price <= 0:
                continue
            
            # --- Check custom price alerts FIRST ---
            check_custom_crypto_alerts(crypto, current_price)
            
            # --- Check for price breakout above highest_price ---
            highest_price = cryptos[crypto]
            if highest_price > 0 and current_price >= highest_price * 0.99:
                last_price = last_alerted_breakout_prices.get(crypto, 0.0)
                if abs(current_price - last_price) / current_price >= 0.005:
                    message = f"Cảnh báo Crypto: Coin {crypto} đã tiệm cận hoặc vượt đỉnh gần nhất ở mức {current_price} (Đỉnh cũ: {highest_price})."
                    print(f"🚨 [Crypto Breakout] {crypto} tại {current_price} >= 99% Đỉnh cũ {highest_price}")
                    play_alert(crypto, "crypto")
                    insert_triggered_alert("crypto", crypto, current_price, message)
                    last_alerted_breakout_prices[crypto] = current_price

            if highest_price > 0 and current_price < highest_price * 0.99:
                continue

            coin_threshold = threshold_usd / current_price

            for trade in trades:
                trade_id = trade["id"]
                qty = float(trade["qty"])
                price = float(trade["price"])
                trade_time_ms = trade["time"]
                trade_time = datetime.fromtimestamp(trade_time_ms / 1000.0).strftime('%H:%M:%S')
                
                is_buyer_maker = trade["isBuyerMaker"]
                side = "SELL" if is_buyer_maker else "BUY"

                # Initialize tracking set for new cryptos
                if crypto not in last_processed_trade_ids:
                    last_processed_trade_ids[crypto] = set()

                if trade_id not in last_processed_trade_ids[crypto] and qty >= coin_threshold:
                    val_usd = qty * price
                    # Dynamic Voice message for TTS
                    message = f"Cảnh báo Crypto: Phát hiện lệnh lớn cho {crypto}."
                    
                    print(f"🚨 [{trade_time}] Crypto {crypto}: {side} {qty:,.4f} coins (${val_usd:,.2f}) at price {price}")
                    play_alert(crypto, "crypto")
                    insert_triggered_alert("crypto", crypto, price, message)

                    last_processed_trade_ids[crypto].add(trade_id)
                    if len(last_processed_trade_ids[crypto]) > 100:
                        # Pop oldest elements to prevent memory grow
                        last_processed_trade_ids[crypto] = set(list(last_processed_trade_ids[crypto])[-100:])
            
            # Avoid API rate-limiting
            time.sleep(0.5)
        except Exception as e:
            print(f"⚠️ Lỗi quét crypto {crypto}: {e}")

def monitor_futures_step(futures, last_processed_trade_ids, last_alerted_breakout_prices, threshold_usd=10000.0):
    """Performs one scan cycle on the list of Binance Futures perpetual contracts"""
    if not futures:
        return

    print(f"🔍 [FUTURES] Đang quét {list(futures.keys())} | Ngưỡng lệnh: >=${threshold_usd:,.0f} USDT...")
    for symbol in futures:
        try:
            url = f"https://fapi.binance.com/fapi/v1/trades?symbol={symbol}&limit=30"
            res = requests.get(url, timeout=5)
            if res.status_code != 200:
                continue

            trades = res.json()
            if not trades:
                continue

            # Get current price to compute dynamic coin volume threshold
            current_price = float(trades[-1]["price"])
            if current_price <= 0:
                continue
            
            # --- Check custom price alerts FIRST ---
            check_custom_futures_alerts(symbol, current_price)
            
            # --- Check for price breakout above highest_price ---
            highest_price = futures[symbol]
            if highest_price > 0 and current_price >= highest_price * 0.99:
                last_price = last_alerted_breakout_prices.get(symbol, 0.0)
                if abs(current_price - last_price) / current_price >= 0.005:
                    message = f"Cảnh báo Futures: Hợp đồng {symbol} đã tiệm cận hoặc vượt đỉnh gần nhất ở mức {current_price} (Đỉnh cũ: {highest_price})."
                    print(f"🚨 [Futures Breakout] {symbol} tại {current_price} >= 99% Đỉnh cũ {highest_price}")
                    play_alert(symbol, "futures")
                    insert_triggered_alert("futures", symbol, current_price, message)
                    last_alerted_breakout_prices[symbol] = current_price

            if highest_price > 0 and current_price < highest_price * 0.99:
                continue

            coin_threshold = threshold_usd / current_price

            for trade in trades:
                trade_id = trade["id"]
                qty = float(trade["qty"])
                price = float(trade["price"])
                trade_time_ms = trade["time"]
                trade_time = datetime.fromtimestamp(trade_time_ms / 1000.0).strftime('%H:%M:%S')
                
                is_buyer_maker = trade["isBuyerMaker"]
                side = "SELL" if is_buyer_maker else "BUY"

                # Initialize tracking set for new futures
                if symbol not in last_processed_trade_ids:
                    last_processed_trade_ids[symbol] = set()

                if trade_id not in last_processed_trade_ids[symbol] and qty >= coin_threshold:
                    val_usd = qty * price
                    # Dynamic Voice message for TTS
                    message = f"Cảnh báo Futures: Phát hiện lệnh lớn cho hợp đồng phái sinh {symbol}."
                    
                    print(f"🚨 [{trade_time}] Futures {symbol}: {side} {qty:,.4f} contracts (${val_usd:,.2f}) at price {price}")
                    play_alert(symbol, "futures")
                    insert_triggered_alert("futures", symbol, price, message)

                    last_processed_trade_ids[symbol].add(trade_id)
                    if len(last_processed_trade_ids[symbol]) > 100:
                        # Pop oldest elements to prevent memory grow
                        last_processed_trade_ids[symbol] = set(list(last_processed_trade_ids[symbol])[-100:])
            
            # Avoid API rate-limiting
            time.sleep(0.5)
        except Exception as e:
            print(f"⚠️ Lỗi quét futures {symbol}: {e}")

def fetch_tradingview_yields(tickers):
    """Fetches yields from TradingView scanner API"""
    url = "https://scanner.tradingview.com/global/scan"
    payload = {
        "symbols": {
            "tickers": tickers
        },
        "columns": ["close", "price_52_week_high"]
    }
    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        "Content-Type": "application/json"
    }
    results = {}
    try:
        res = requests.post(url, headers=headers, json=payload, timeout=8)
        if res.status_code == 200:
            data = res.json()
            for item in data.get("data", []):
                s = item.get("s")
                d = item.get("d", [])
                if len(d) >= 2:
                    close = d[0]
                    high_52w = d[1]
                    if close is not None and high_52w is not None:
                        results[s] = (float(close), float(high_52w))
    except Exception as e:
        print(f"⚠️ Error fetching from TradingView scanner: {e}")
    return results

YIELD_SYMBOLS = {
    # US
    '^IRX': 'US02Y',
    '^FVX': 'US05Y',
    '^TNX': 'US10Y',
    '^TYX': 'US30Y',
    # Japan
    'TVC:JP02Y': 'JP02Y',
    'TVC:JP10Y': 'JP10Y',
    'TVC:JP30Y': 'JP30Y',
    # UK
    'TVC:GB02Y': 'GB02Y',
    'TVC:GB10Y': 'GB10Y',
    'TVC:GB30Y': 'GB30Y',
    # Germany (Europe)
    'TVC:DE02Y': 'DE02Y',
    'TVC:DE10Y': 'DE10Y',
    'TVC:DE30Y': 'DE30Y'
}

def check_custom_yield_alerts(symbol, current_price):
    """Check if any user price alerts in the database are triggered for this yield"""
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        
        # Check alerts with asset_type = 'yield'
        cur.execute("""
            SELECT alert_price, operator, last_notified_at
            FROM public.price_alerts
            WHERE asset_type = 'yield' 
            AND symbol = %s 
            AND is_active = true;
        """, (symbol,))
        
        alerts = cur.fetchall()
        for alert_price, operator, last_notified_at in alerts:
            alert_price = float(alert_price)
            alert_triggered = False
            
            if operator == '<=':
                alert_triggered = current_price <= (alert_price * 1.01)
                condition = "giảm xuống dưới hoặc bằng"
                emoji = "🔻"
            elif operator == '>=':
                alert_triggered = current_price >= (alert_price * 0.99)
                condition = "tăng lên trên hoặc bằng"
                emoji = "🚀"

            if alert_triggered:
                should_notify = True
                if last_notified_at:
                    now = datetime.now(timezone.utc)
                    last_notified = last_notified_at.astimezone(timezone.utc) if last_notified_at.tzinfo else last_notified_at.replace(tzinfo=timezone.utc)
                    if now - last_notified < timedelta(hours=1):
                        should_notify = False

                if should_notify:
                    message = f"Cảnh báo Lợi suất: {emoji} Lợi suất trái phiếu {symbol} đã {condition} mức {current_price:.3f}%."
                    print(f"🚨 [Yield Price Alert Triggered] {symbol} tại {current_price:.3f}% kích hoạt {operator} {alert_price:.3f}%")
                    
                    play_alert(symbol, "yield")
                    insert_triggered_alert("yield", symbol, current_price, message)
                    
                    cur.execute("""
                        UPDATE public.price_alerts
                        SET last_notified_at = CURRENT_TIMESTAMP
                        WHERE symbol = %s AND asset_type = 'yield';
                    """, (symbol,))
                    conn.commit()
                    
        cur.close()
    except Exception as e:
        print(f"⚠️ Lỗi check custom yield alerts cho {symbol}: {e}")
    finally:
        if conn:
            conn.close()

def check_custom_crypto_alerts(symbol, current_price):
    """Check if any user price alerts in the database are triggered for this crypto"""
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        
        # Check alerts with asset_type = 'crypto'
        clean_symbol = symbol.split(':')[-1] if ':' in symbol else symbol
        cur.execute("""
            SELECT symbol, alert_price, operator, last_notified_at
            FROM public.price_alerts
            WHERE asset_type = 'crypto' 
            AND (symbol = %s OR symbol = %s) 
            AND is_active = true;
        """, (symbol, clean_symbol))
        
        alerts = cur.fetchall()
        for alert_symbol, alert_price, operator, last_notified_at in alerts:
            alert_price = float(alert_price)
            alert_triggered = False
            
            if operator == '<=':
                alert_triggered = current_price <= (alert_price * 1.01)
                condition = "giảm xuống dưới hoặc bằng"
                emoji = "🔻"
            elif operator == '>=':
                alert_triggered = current_price >= (alert_price * 0.99)
                condition = "tăng lên trên hoặc bằng"
                emoji = "🚀"

            if alert_triggered:
                should_notify = True
                if last_notified_at:
                    now = datetime.now(timezone.utc)
                    last_notified = last_notified_at.astimezone(timezone.utc) if last_notified_at.tzinfo else last_notified_at.replace(tzinfo=timezone.utc)
                    if now - last_notified < timedelta(hours=1):
                        should_notify = False

                if should_notify:
                    message = f"Cảnh báo Crypto: {emoji} Giá {alert_symbol} đã {condition} mức {alert_price} (Giá hiện tại: {current_price})."
                    print(f"🚨 [Crypto Price Alert Triggered] {alert_symbol} tại {current_price} kích hoạt {operator} {alert_price}")
                    
                    play_alert(alert_symbol, "crypto")
                    insert_triggered_alert("crypto", alert_symbol, current_price, message)
                    
                    cur.execute("""
                        UPDATE public.price_alerts
                        SET last_notified_at = CURRENT_TIMESTAMP
                        WHERE symbol = %s AND asset_type = 'crypto';
                    """, (alert_symbol,))
                    conn.commit()
                    
        cur.close()
    except Exception as e:
        print(f"⚠️ Lỗi check custom crypto alerts cho {symbol}: {e}")
    finally:
        if conn:
            conn.close()

def check_custom_futures_alerts(symbol, current_price):
    """Check if any user price alerts in the database are triggered for this futures contract"""
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        
        # Check alerts with asset_type = 'futures'
        clean_symbol = symbol.split(':')[-1] if ':' in symbol else symbol
        cur.execute("""
            SELECT symbol, alert_price, operator, last_notified_at
            FROM public.price_alerts
            WHERE asset_type = 'futures' 
            AND (symbol = %s OR symbol = %s) 
            AND is_active = true;
        """, (symbol, clean_symbol))
        
        alerts = cur.fetchall()
        for alert_symbol, alert_price, operator, last_notified_at in alerts:
            alert_price = float(alert_price)
            alert_triggered = False
            
            if operator == '<=':
                alert_triggered = current_price <= (alert_price * 1.01)
                condition = "giảm xuống dưới hoặc bằng"
                emoji = "🔻"
            elif operator == '>=':
                alert_triggered = current_price >= (alert_price * 0.99)
                condition = "tăng lên trên hoặc bằng"
                emoji = "🚀"

            if alert_triggered:
                should_notify = True
                if last_notified_at:
                    now = datetime.now(timezone.utc)
                    last_notified = last_notified_at.astimezone(timezone.utc) if last_notified_at.tzinfo else last_notified_at.replace(tzinfo=timezone.utc)
                    if now - last_notified < timedelta(hours=1):
                        should_notify = False

                if should_notify:
                    message = f"Cảnh báo Futures: {emoji} Giá hợp đồng {alert_symbol} đã {condition} mức {alert_price} (Giá hiện tại: {current_price})."
                    print(f"🚨 [Futures Price Alert Triggered] {alert_symbol} tại {current_price} kích hoạt {operator} {alert_price}")
                    
                    play_alert(alert_symbol, "futures")
                    insert_triggered_alert("futures", alert_symbol, current_price, message)
                    
                    cur.execute("""
                        UPDATE public.price_alerts
                        SET last_notified_at = CURRENT_TIMESTAMP
                        WHERE symbol = %s AND asset_type = 'futures';
                    """, (alert_symbol,))
                    conn.commit()
                    
        cur.close()
    except Exception as e:
        print(f"⚠️ Lỗi check custom futures alerts cho {symbol}: {e}")
    finally:
        if conn:
            conn.close()

def check_custom_stock_alerts(symbol, current_price):
    """Check if any user price alerts in the database are triggered for this stock"""
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        
        # Check alerts with asset_type = 'stock'
        clean_symbol = symbol.split(':')[-1] if ':' in symbol else symbol
        cur.execute("""
            SELECT symbol, alert_price, operator, last_notified_at
            FROM public.price_alerts
            WHERE asset_type = 'stock' 
            AND (symbol = %s OR symbol = %s) 
            AND is_active = true;
        """, (symbol, clean_symbol))
        
        alerts = cur.fetchall()
        for alert_symbol, alert_price, operator, last_notified_at in alerts:
            alert_price = float(alert_price)
            alert_triggered = False
            
            if operator == '<=':
                alert_triggered = current_price <= (alert_price * 1.01)
                condition = "giảm xuống dưới hoặc bằng"
                emoji = "🔻"
            elif operator == '>=':
                alert_triggered = current_price >= (alert_price * 0.99)
                condition = "tăng lên trên hoặc bằng"
                emoji = "🚀"

            if alert_triggered:
                should_notify = True
                if last_notified_at:
                    now = datetime.now(timezone.utc)
                    last_notified = last_notified_at.astimezone(timezone.utc) if last_notified_at.tzinfo else last_notified_at.replace(tzinfo=timezone.utc)
                    if now - last_notified < timedelta(hours=1):
                        should_notify = False

                if should_notify:
                    message = f"Cảnh báo Cổ phiếu: {emoji} Giá {alert_symbol} đã {condition} mức {alert_price} (Giá hiện tại: {current_price})."
                    print(f"🚨 [Stock Price Alert Triggered] {alert_symbol} tại {current_price} kích hoạt {operator} {alert_price}")
                    
                    play_alert(alert_symbol, "stock")
                    insert_triggered_alert("stock", alert_symbol, current_price, message)
                    
                    cur.execute("""
                        UPDATE public.price_alerts
                        SET last_notified_at = CURRENT_TIMESTAMP
                        WHERE symbol = %s AND asset_type = 'stock';
                    """, (alert_symbol,))
                    conn.commit()
                    
        cur.close()
    except Exception as e:
        print(f"⚠️ Lỗi check custom stock alerts cho {symbol}: {e}")
    finally:
        if conn:
            conn.close()

def monitor_yields_step(yield_symbols, last_alerted_yields):
    """Performs one scan cycle on Treasury Yields using yfinance (with HTTP fallback) or TradingView scanner"""
    if not yield_symbols:
        return

    print(f"🔍 [YIELDS] Đang quét {list(yield_symbols.values())}...")
    
    # Batch fetch TradingView tickers
    tv_tickers = [t for t in yield_symbols.keys() if t.startswith("TVC:")]
    tv_data = {}
    if tv_tickers:
        tv_data = fetch_tradingview_yields(tv_tickers)
        
    for ticker, symbol in yield_symbols.items():
        try:
            current_price = None
            fifty_two_high = None
            
            if ticker.startswith("TVC:"):
                # Use TradingView data
                if ticker in tv_data:
                    current_price, fifty_two_high = tv_data[ticker]
            else:
                # 1. Try using yfinance library if available
                if yf is not None:
                    try:
                        t = yf.Ticker(ticker)
                        hist_1d = t.history(period="1d")
                        if not hist_1d.empty:
                            current_price = float(hist_1d["Close"].iloc[-1])
                            
                        hist_1y = t.history(period="1y")
                        if not hist_1y.empty:
                            prices_1y = hist_1y["Close"].dropna()
                            if not prices_1y.empty:
                                fifty_two_high = float(prices_1y.max())
                    except Exception as yfe:
                        print(f"⚠️ yfinance library error for {symbol}: {yfe}. Trying HTTP fallback...")
                
                # 2. HTTP Fallback
                if current_price is None or fifty_two_high is None:
                    headers = {"User-Agent": "Mozilla/5.0"}
                    url = f"https://query1.finance.yahoo.com/v8/finance/chart/{ticker}"
                    res = requests.get(url, headers=headers, timeout=5)
                    if res.status_code == 200:
                        data = res.json()
                        results = data.get("chart", {}).get("result", [])
                        if results:
                            meta = results[0].get("meta", {})
                            current_price = meta.get("regularMarketPrice")
                            fifty_two_high = meta.get("fiftyTwoWeekHigh")

            if current_price is None or fifty_two_high is None or fifty_two_high <= 0:
                continue

            # CBOE yields are 10x the actual yield rate (e.g. 38.8 means 3.88%)
            if not ticker.startswith("TVC:") and (current_price > 10.0 or fifty_two_high > 10.0):
                current_price = current_price / 10.0
                fifty_two_high = fifty_two_high / 10.0

            # Check 52-Week High Breakout (check within 1%)
            if current_price >= fifty_two_high * 0.99:
                last_price = last_alerted_yields.get(symbol, 0.0)
                # Alert again only if yield moved significantly (e.g., >= 0.005%)
                if abs(current_price - last_price) >= 0.005:
                    country = "Mỹ"
                    if symbol.startswith("JP"):
                        country = "Nhật Bản"
                    elif symbol.startswith("GB"):
                        country = "Anh"
                    elif symbol.startswith("DE"):
                        country = "Đức (Châu Âu)"
                        
                    message = f"Cảnh báo Lợi suất: Lợi suất trái phiếu Chính phủ {country} {symbol} đã tiệm cận hoặc vượt đỉnh 52 tuần tại mức {current_price:.3f}%."
                    print(f"🚨 [Yield Breakout] {symbol} ({country}) tại lợi suất {current_price:.3f}% >= 99% Đỉnh 52 tuần {fifty_two_high:.3f}%")
                    play_alert(symbol, "yield")
                    insert_triggered_alert("yield", symbol, current_price, message)
                    last_alerted_yields[symbol] = current_price

            # Check custom user-configured alerts
            check_custom_yield_alerts(symbol, current_price)

            time.sleep(0.5)
        except Exception as e:
            print(f"⚠️ Lỗi quét yield {symbol}: {e}")

COMMODITIES_SYMBOLS = {
    'GC=F': 'Vàng (XAUUSD)',
    'SI=F': 'Bạc (XAGUSD)',
    'BZ=F': 'Dầu Brent (UKOIL)',
    'CL=F': 'Dầu WTI (USOIL)'
}

COMMODITY_ALIASES = {
    'GC=F': ['GC=F', 'XAUUSD', 'GOLD', 'VÀNG', 'VANG'],
    'SI=F': ['SI=F', 'XAGUSD', 'SILVER', 'BẠC', 'BAC'],
    'CL=F': ['CL=F', 'USOIL', 'WTI', 'OIL', 'DẦU WTI'],
    'BZ=F': ['BZ=F', 'UKOIL', 'BRENT', 'DẦU BRENT']
}

def check_custom_commodity_alerts(symbol, name, current_price):
    """Check if any user price alerts in the database are triggered for this commodity"""
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        
        aliases = list(set(COMMODITY_ALIASES.get(symbol, [symbol]) + [symbol]))
        asset_types = ['commodities', 'commodity', 'gold', 'silver', 'oil', 'forex']
        
        cur.execute("""
            SELECT symbol, asset_type, alert_price, operator, last_notified_at
            FROM public.price_alerts
            WHERE asset_type = ANY(%s) 
            AND symbol = ANY(%s) 
            AND is_active = true;
        """, (asset_types, aliases))
        
        alerts = cur.fetchall()
        for alert_symbol, alert_asset_type, alert_price, operator, last_notified_at in alerts:
            alert_price = float(alert_price)
            alert_triggered = False
            
            if operator == '<=':
                alert_triggered = current_price <= (alert_price * 1.01)
                condition = "giảm xuống dưới hoặc bằng"
                emoji = "🔻"
            elif operator == '>=':
                alert_triggered = current_price >= (alert_price * 0.99)
                condition = "tăng lên trên hoặc bằng"
                emoji = "🚀"

            if alert_triggered:
                should_notify = True
                if last_notified_at:
                    now = datetime.now(timezone.utc)
                    last_notified = last_notified_at.astimezone(timezone.utc) if last_notified_at.tzinfo else last_notified_at.replace(tzinfo=timezone.utc)
                    if now - last_notified < timedelta(hours=1):
                        should_notify = False

                if should_notify:
                    display_sym = 'XAUUSD' if symbol == 'GC=F' else ('XAGUSD' if symbol == 'SI=F' else alert_symbol)
                    message = f"Cảnh báo Hàng hóa: {emoji} {name} ({display_sym}) đã {condition} mức giá ${current_price:,.2f}."
                    print(f"🚨 [Commodity Price Alert Triggered] {name} at {current_price} triggers {operator} {alert_price}")
                    
                    play_alert(display_sym, "commodities")
                    insert_triggered_alert("commodities", display_sym, current_price, message)
                    
                    cur.execute("""
                        UPDATE public.price_alerts
                        SET last_notified_at = CURRENT_TIMESTAMP
                        WHERE symbol = %s AND asset_type = %s;
                    """, (alert_symbol, alert_asset_type))
                    conn.commit()
                    
        cur.close()
    except Exception as e:
        print(f"⚠️ Lỗi check custom commodity alerts cho {symbol}: {e}")
    finally:
        if conn:
            conn.close()

def monitor_commodities_step(commodities_symbols, last_alerted_prices):
    """Performs one scan cycle on global commodities using Yahoo Finance"""
    if not commodities_symbols:
        return

    print(f"🔍 [COMMODITIES] Đang quét {list(commodities_symbols.keys())}...")
    headers = {"User-Agent": "Mozilla/5.0"}
    for symbol, name in commodities_symbols.items():
        try:
            url = f"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?range=1mo&interval=1d"
            res = requests.get(url, headers=headers, timeout=5)
            if res.status_code != 200:
                continue

            data = res.json()
            results = data.get("chart", {}).get("result", [])
            if not results:
                continue

            meta = results[0].get("meta", {})
            current_price = meta.get("regularMarketPrice")
            fifty_two_high = meta.get("fiftyTwoWeekHigh")

            if current_price is None:
                continue

            # Calculate 30-day recent high from daily candles (excluding current in-progress candle)
            quotes = results[0].get("indicators", {}).get("quote", [{}])[0]
            highs = [h for h in quotes.get("high", []) if h is not None]
            recent_high = max(highs[:-1]) if len(highs) > 1 else (max(highs) if highs else fifty_two_high)

            display_sym = 'XAUUSD' if symbol == 'GC=F' else ('XAGUSD' if symbol == 'SI=F' else symbol)

            # 1. Check for 30-day Recent High Breakout (check within 1%)
            if recent_high and recent_high > 0 and current_price >= recent_high * 0.99:
                last_price = last_alerted_prices.get(symbol, 0.0)
                if abs(current_price - last_price) / current_price >= 0.002:
                    message = f"Cảnh báo Hàng hóa: {name} đã tiệm cận hoặc vượt đỉnh gần nhất ở mức ${current_price:,.2f} (Đỉnh cũ: ${recent_high:,.2f})."
                    print(f"🚨 [Commodity Breakout] {name} tại giá {current_price} >= 99% Đỉnh gần nhất {recent_high}")
                    play_alert(display_sym, "commodities")
                    insert_triggered_alert("commodities", display_sym, current_price, message)
                    last_alerted_prices[symbol] = current_price

            # 2. Check for 52-Week High Breakout (check within 1%)
            elif fifty_two_high and fifty_two_high > 0 and current_price >= fifty_two_high * 0.99:
                last_price = last_alerted_prices.get(symbol, 0.0)
                if abs(current_price - last_price) / current_price >= 0.002:
                    message = f"Cảnh báo Hàng hóa: {name} ({display_sym}) đã tiệm cận hoặc vượt đỉnh 52 tuần tại ${current_price:,.2f}."
                    print(f"🚨 [Commodity 52W Breakout] {name} ({display_sym}) tại giá {current_price} >= 99% Đỉnh 52 tuần {fifty_two_high}")
                    play_alert(display_sym, "commodities")
                    insert_triggered_alert("commodities", display_sym, current_price, message)
                    last_alerted_prices[symbol] = current_price

            # 3. Check for custom user-configured alerts
            check_custom_commodity_alerts(symbol, name, current_price)

            time.sleep(0.5)
        except Exception as e:
            print(f"⚠️ Lỗi quét commodity {symbol}: {e}")

def map_forex_symbol_to_yahoo(symbol):
    """Map standard forex pair name to Yahoo Finance symbol"""
    mapping = {
        'EURUSD': 'EURUSD=X',
        'USDJPY': 'USDJPY=X',
        'GBPUSD': 'GBPUSD=X',
        'USDCHF': 'USDCHF=X',
        'AUDUSD': 'AUDUSD=X',
        'USDCAD': 'USDCAD=X',
        'XAUUSD': 'GC=F',
        'XAGUSD': 'SI=F',
        'WTI': 'CL=F',
        'DXY': 'DX-Y.NYB'
    }
    if symbol in mapping:
        return mapping[symbol]
    if len(symbol) == 6:
        return f"{symbol}=X"
    return symbol

def check_custom_forex_alerts(symbol, pair_name, current_price):
    """Check if any user price alerts in the database are triggered for this forex pair"""
    conn = None
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        
        # Aliases for forex pairs (e.g. XAUUSD <-> GC=F, XAGUSD <-> SI=F)
        aliases = [symbol, pair_name]
        if pair_name == 'XAUUSD' or symbol == 'GC=F':
            aliases.extend(['XAUUSD', 'GC=F', 'GOLD', 'VÀNG'])
        elif pair_name == 'XAGUSD' or symbol == 'SI=F':
            aliases.extend(['XAGUSD', 'SI=F', 'SILVER', 'BẠC'])
        elif pair_name == 'WTI' or symbol == 'CL=F':
            aliases.extend(['WTI', 'CL=F', 'USOIL'])
        elif pair_name == 'DXY' or symbol == 'DX-Y.NYB':
            aliases.extend(['DXY', 'DX-Y.NYB'])
        
        asset_types = ['forex', 'commodities', 'commodity', 'gold', 'silver']
        
        cur.execute("""
            SELECT symbol, asset_type, alert_price, operator, last_notified_at
            FROM public.price_alerts
            WHERE asset_type = ANY(%s) 
            AND symbol = ANY(%s) 
            AND is_active = true;
        """, (asset_types, list(set(aliases))))
        
        alerts = cur.fetchall()
        for alert_symbol, alert_asset_type, alert_price, operator, last_notified_at in alerts:
            alert_price = float(alert_price)
            alert_triggered = False
            
            if operator == '<=':
                alert_triggered = current_price <= (alert_price * 1.01)
                condition = "giảm xuống dưới hoặc bằng"
                emoji = "🔻"
            elif operator == '>=':
                alert_triggered = current_price >= (alert_price * 0.99)
                condition = "tăng lên trên hoặc bằng"
                emoji = "🚀"

            if alert_triggered:
                should_notify = True
                if last_notified_at:
                    now = datetime.now(timezone.utc)
                    last_notified = last_notified_at.astimezone(timezone.utc) if last_notified_at.tzinfo else last_notified_at.replace(tzinfo=timezone.utc)
                    if now - last_notified < timedelta(hours=1):
                        should_notify = False

                if should_notify:
                    price_diff = ((current_price - alert_price) / alert_price) * 100
                    message = f"Cảnh báo Forex: {emoji} Cặp tiền {pair_name} ({symbol}) đã {condition} mức giá {current_price:,.4f}."
                    print(f"🚨 [Forex Price Alert Triggered] {pair_name} tại {current_price} kích hoạt {operator} {alert_price}")
                    
                    play_alert(pair_name, "forex")
                    insert_triggered_alert("forex", pair_name, current_price, message)
                    
                    cur.execute("""
                        UPDATE public.price_alerts
                        SET last_notified_at = CURRENT_TIMESTAMP
                        WHERE symbol = %s AND asset_type = %s;
                    """, (alert_symbol, alert_asset_type))
                    conn.commit()
                    
        cur.close()
    except Exception as e:
        print(f"⚠️ Lỗi check custom forex alerts cho {symbol}: {e}")
    finally:
        if conn:
            conn.close()

def monitor_forex_step(forex_pairs, last_alerted_prices):
    """Performs one scan cycle on forex pairs using Yahoo Finance"""
    if not forex_pairs:
        return

    # Skip commodity pairs already handled in monitor_commodities_step to avoid duplicate alerts
    commodity_pairs = {'XAUUSD', 'XAGUSD', 'WTI', 'USOIL', 'UKOIL', 'BRENT'}
    filtered_pairs = {k: v for k, v in forex_pairs.items() if k.upper() not in commodity_pairs}
    if not filtered_pairs:
        return

    print(f"🔍 [FOREX] Đang quét {list(filtered_pairs.keys())}...")
    headers = {"User-Agent": "Mozilla/5.0"}
    for pair in filtered_pairs:
        try:
            # Map standard pair to Yahoo Finance symbol
            symbol = map_forex_symbol_to_yahoo(pair)
            
            url = f"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?range=1mo&interval=1d"
            res = requests.get(url, headers=headers, timeout=5)
            if res.status_code != 200:
                continue

            data = res.json()
            results = data.get("chart", {}).get("result", [])
            if not results:
                continue

            meta = results[0].get("meta", {})
            current_price = meta.get("regularMarketPrice")
            fifty_two_high = meta.get("fiftyTwoWeekHigh")

            if current_price is None:
                continue

            # Calculate 30-day recent high from daily candles (excluding current in-progress candle)
            quotes = results[0].get("indicators", {}).get("quote", [{}])[0]
            highs = [h for h in quotes.get("high", []) if h is not None]
            recent_high = max(highs[:-1]) if len(highs) > 1 else (max(highs) if highs else fifty_two_high)

            # 1. Check for 30-day Recent High Breakout (check within 1%)
            if recent_high and recent_high > 0 and current_price >= recent_high * 0.99:
                last_price = last_alerted_prices.get(pair, 0.0)
                if abs(current_price - last_price) / current_price >= 0.002:
                    message = f"Cảnh báo Forex: Cặp tiền {pair} ({symbol}) đã tiệm cận hoặc vượt đỉnh gần nhất ở mức {current_price:,.4f} (Đỉnh cũ: {recent_high:,.4f})."
                    print(f"🚨 [Forex Breakout] {pair} tại giá {current_price} >= 99% Đỉnh gần nhất {recent_high}")
                    play_alert(pair, "forex")
                    insert_triggered_alert("forex", pair, current_price, message)
                    last_alerted_prices[pair] = current_price

            # 2. Check for 52-Week High Breakout (check within 1%)
            elif fifty_two_high and fifty_two_high > 0 and current_price >= fifty_two_high * 0.99:
                last_price = last_alerted_prices.get(pair, 0.0)
                if abs(current_price - last_price) / current_price >= 0.002:
                    message = f"Cảnh báo Forex: Cặp tiền {pair} ({symbol}) đã tiệm cận hoặc vượt đỉnh 52 tuần."
                    print(f"🚨 [Forex 52W Breakout] {pair} tại giá {current_price} >= 99% Đỉnh 52 tuần {fifty_two_high}")
                    play_alert(pair, "forex")
                    insert_triggered_alert("forex", pair, current_price, message)
                    last_alerted_prices[pair] = current_price

            # 3. Check for custom user-configured alerts
            check_custom_forex_alerts(symbol, pair, current_price)

            time.sleep(0.5)
        except Exception as e:
            print(f"⚠️ Lỗi quét forex {pair}: {e}")

def main():
    print("🤖 Bắt đầu khởi tạo dịch vụ Báo Động Lệnh Lớn & Vượt Đỉnh...")
    print("Mô hình hoạt động:")
    print("  • Stocks VN: Thứ 2 đến Thứ 6 (09:00 - 14:45 UTC+7). Dựa trên symbols_watchlist.")
    print("  • Stocks US: Thứ 2 đến Thứ 6 (09:30 - 16:00 ET). Dựa trên world_symbols_watchlist (Mỹ).")
    print("  • Cryptos Spot: Quét 24/7 hàng ngày. Dựa trên cryptos_watchlist.")
    print("  • Cryptos Futures: Quét 24/7 hàng ngày. Dựa trên futures_watchlist.")
    print("  • Commodities: Thứ 2 đến Thứ 6 (Quét 24/5 trong tuần). Hỗ trợ Vàng, Bạc, UKOIL, USOIL.")
    
    # State caches in memory to prevent duplicate alarms
    last_processed_time_stocks = {}
    last_processed_trade_ids_cryptos = {}
    last_processed_trade_ids_futures = {}
    last_alerted_prices_us = {}
    last_alerted_prices_commodities = {}
    last_alerted_prices_forex = {}
    last_alerted_yields = {}
    last_alerted_breakout_prices = {}
    
    # Read USD threshold for crypto and share count threshold for stock
    crypto_threshold_usd = float(os.getenv('CRYPTO_ALERT_THRESHOLD_USD', 10000.0))
    stock_threshold_shares = int(os.getenv('STOCK_ALERT_THRESHOLD_SHARES', 5000))

    while True:
        try:
            # Ping database to let the UI know script is alive
            update_heartbeat()

            # Dọn dẹp bảng triggered_alerts cũ
            cleanup_triggered_alerts()

            # Query real-time system scan toggles from the database
            toggles = get_scan_toggles()

            # 1. Stocks Watchlist check (Mon to Fri, 09:00 - 14:45 UTC+7)
            if toggles['scan_stock_vn']:
                vn_now = get_vn_time()
                vn_weekday = vn_now.weekday()
                vn_hour = vn_now.hour
                vn_minute = vn_now.minute

                is_vn_market_open = (
                    vn_weekday < 5 and
                    ((vn_hour == 9 and vn_minute >= 0) or (10 <= vn_hour < 14) or (vn_hour == 14 and vn_minute <= 45))
                )

                if is_vn_market_open:
                    stock_watchlist = get_watchlist_symbols()
                    if stock_watchlist:
                        monitor_stocks_step(stock_watchlist, last_processed_time_stocks, last_alerted_breakout_prices, threshold=stock_threshold_shares)
                    else:
                        print("💤 Không có cổ phiếu VN nào đạt đủ 3 tín hiệu trong symbols_watchlist.")
                else:
                    print(f"💤 Ngoài giờ giao dịch Stock VN (T2-T6, 09:00 - 14:45). Hiện tại: {vn_now.strftime('%d/%m %H:%M:%S')} UTC+7. Tạm ngưng quét VN.")
            else:
                print("💤 Tắt quét Stock VN (theo cấu hình hệ thống).")

            # 2. US Stocks Watchlist check (Mon to Fri, 09:30 - 16:00 US/Eastern)
            if toggles['scan_stock_us']:
                us_now = get_us_time()
                us_weekday = us_now.weekday()
                us_hour = us_now.hour
                us_minute = us_now.minute

                is_us_market_open = (
                    us_weekday < 5 and
                    ((us_hour == 9 and us_minute >= 30) or (10 <= us_hour < 16))
                )

                if is_us_market_open:
                    us_watchlist = get_us_watchlist_symbols()
                    if us_watchlist:
                        monitor_us_stocks_step(us_watchlist, last_alerted_prices_us)
                    else:
                        print("💤 Không có cổ phiếu Mỹ nào trong world_symbols_watchlist.")
                else:
                    print(f"💤 Ngoài giờ giao dịch Stock US (T2-T6, 09:30 - 16:00 ET). Hiện tại: {us_now.strftime('%d/%m %H:%M:%S')} ET. Tạm ngưng quét US.")
            else:
                print("💤 Tắt quét Stock US (theo cấu hình hệ thống).")

            # 3. Cryptos Watchlist check (Every day, 24/7)
            if toggles['scan_crypto']:
                crypto_watchlist = get_watchlist_cryptos()
                if crypto_watchlist:
                    monitor_cryptos_step(crypto_watchlist, last_processed_trade_ids_cryptos, last_alerted_breakout_prices, threshold_usd=crypto_threshold_usd)
                else:
                    print("💤 Không có crypto nào trong cryptos_watchlist.")
            else:
                print("💤 Tắt quét Crypto Spot (theo cấu hình hệ thống).")

            # 4. Cryptos Futures Watchlist check (Every day, 24/7)
            if toggles['scan_futures']:
                futures_watchlist = get_watchlist_futures()
                if futures_watchlist:
                    monitor_futures_step(futures_watchlist, last_processed_trade_ids_futures, last_alerted_breakout_prices, threshold_usd=crypto_threshold_usd)
                else:
                    print("💤 Không có futures nào trong futures_watchlist.")
            else:
                print("💤 Tắt quét Crypto Futures (theo cấu hình hệ thống).")

            # 5. Commodities Watchlist check (Mon to Fri, CME/ICE open hours)
            if toggles.get('scan_commodities', True):
                us_now = get_us_time()
                us_weekday = us_now.weekday()
                
                # Commodities trade Monday to Friday (weekday < 5)
                is_commodities_market_open = (us_weekday < 5)

                if is_commodities_market_open:
                    monitor_commodities_step(COMMODITIES_SYMBOLS, last_alerted_prices_commodities)
                else:
                    print(f"💤 Ngoài giờ giao dịch Commodities (T2-T6). Hiện tại: {us_now.strftime('%d/%m %H:%M:%S')} ET. Tạm ngưng quét Commodities.")
            else:
                print("💤 Tắt quét Commodities (theo cấu hình hệ thống).")

            # 6. Forex Watchlist check (Mon to Fri, 24/5)
            if toggles.get('scan_forex', True):
                us_now = get_us_time()
                us_weekday = us_now.weekday()
                is_forex_market_open = (us_weekday < 5)

                if is_forex_market_open:
                    forex_watchlist = get_watchlist_forex()
                    if forex_watchlist:
                        monitor_forex_step(forex_watchlist, last_alerted_prices_forex)
                    else:
                        print("💤 Không có forex nào trong forex_watchlist.")
                else:
                    print(f"💤 Ngoài giờ giao dịch Forex (T2-T6). Hiện tại: {us_now.strftime('%d/%m %H:%M:%S')} ET. Tạm ngưng quét Forex.")
            else:
                print("💤 Tắt quét Forex (theo cấu hình hệ thống).")

            # 7. US Treasury Yields check (Mon to Fri, 24/5)
            if toggles.get('scan_yields', True):
                us_now = get_us_time()
                us_weekday = us_now.weekday()
                is_yields_market_open = (us_weekday < 5)

                if is_yields_market_open:
                    monitor_yields_step(YIELD_SYMBOLS, last_alerted_yields)
                else:
                    print(f"💤 Ngoài giờ giao dịch US Treasury Yields (T2-T6). Hiện tại: {us_now.strftime('%d/%m %H:%M:%S')} ET. Tạm ngưng quét Yields.")
            else:
                print("💤 Tắt quét US Treasury Yields (theo cấu hình hệ thống).")

            # 8. Print separators and sleep for 15 seconds
            print(f"🕒 Lượt quét hoàn thành lúc {datetime.now().strftime('%H:%M:%S')}. Nghỉ 15 giây...\n")
            time.sleep(15)

        except KeyboardInterrupt:
            print("\n👋 Dừng dịch vụ Báo Động. Hẹn gặp lại!")
            sys.exit(0)
        except Exception as e:
            print(f"❌ Lỗi hệ thống: {e}")
            time.sleep(10)

if __name__ == "__main__":
    main()
