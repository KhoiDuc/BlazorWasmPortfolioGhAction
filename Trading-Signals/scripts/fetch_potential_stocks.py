import asyncio
import time
import asyncpg
import os
import json
import sys
from dotenv import load_dotenv
from curl_cffi.requests import AsyncSession, RequestsError
from price_alert_utils import check_multiple_alerts

# Set standard output to UTF-8 to prevent encoding errors on Windows
sys.stdout.reconfigure(encoding='utf-8')

# Load variables from the .env file
ENV_FILE = os.path.join(os.path.dirname(__file__), '.env')
load_dotenv(ENV_FILE)

# Configuration constants
MIN_TRADE_VOLUME = 500000  # Minimum trade volume threshold for stock filtering 
INVALID_SYMBOLS_FILE = os.path.join(os.path.dirname(__file__), 'invalid_stock_symbols.json')
SIGNAL_NEAR_52W_ATH = 'near_52w_ath'
SIGNAL_EMA9_ABOVE_EMA21 = 'ema9_above_ema21'
SIGNAL_TOP_GROWTH_20D = 'top_growth_20d'


def load_invalid_symbols():
    """Load symbols that should be skipped in future scans."""
    if not os.path.exists(INVALID_SYMBOLS_FILE):
        return set()

    try:
        with open(INVALID_SYMBOLS_FILE, 'r', encoding='utf-8') as f:
            symbols = json.load(f)
        if isinstance(symbols, list):
            return set(symbols)
    except Exception as e:
        print(f"⚠️ Could not read invalid symbols cache: {e}")

    return set()


def save_invalid_symbols(symbols):
    """Persist invalid symbols to disk for future runs."""
    try:
        with open(INVALID_SYMBOLS_FILE, 'w', encoding='utf-8') as f:
            json.dump(sorted(symbols), f, ensure_ascii=True, indent=2)
    except Exception as e:
        print(f"⚠️ Could not save invalid symbols cache: {e}")


def get_signal_label(signal_type):
    if signal_type == SIGNAL_NEAR_52W_ATH:
        return 'Highest 52W'
    if signal_type == SIGNAL_EMA9_ABOVE_EMA21:
        return 'Uptrend'
    if signal_type == SIGNAL_TOP_GROWTH_20D:
        return 'Top Growth 20D'
    return signal_type


def _calc_sma(closes, period):
    """Simple Moving Average of the last `period` values."""
    if len(closes) < period:
        return None
    return sum(closes[-period:]) / period


def _calc_ema(closes, period):
    """Exponential Moving Average seeded with the first SMA."""
    if len(closes) < period:
        return None
    k = 2.0 / (period + 1)
    ema = sum(closes[:period]) / period
    for price in closes[period:]:
        ema = price * k + ema * (1 - k)
    return ema


def check_ema9_above_ema21(indicator_list):
    """Return True when the latest EMA9 >= EMA21 of the close series."""
    closes = [
        item['closePrice']
        for item in indicator_list
        if item.get('closePrice') is not None
    ]
    if not closes:
        return False
    ema9 = _calc_ema(closes, 9)
    ema21 = _calc_ema(closes, 21)
    if ema9 is None or ema21 is None:
        return False
    return ema9 >= ema21


def check_above_ma20(indicator_list):
    """Return True when the latest closePrice is >= MA20 (SMA 20) of the close series."""
    closes = [
        item['closePrice']
        for item in indicator_list
        if item.get('closePrice') is not None
    ]
    if not closes:
        return False
    ma20 = _calc_sma(closes, 20)
    if ma20 is None:
        return False
    return closes[-1] >= ma20


def calc_growth_percent(indicator_list, period=20):
    """Return growth percentage over `period` sessions using closePrice."""
    closes = [
        item['closePrice']
        for item in indicator_list
        if item.get('closePrice') is not None
    ]
    if len(closes) < period + 1:
        return None

    start_price = closes[-(period + 1)]
    end_price = closes[-1]

    if start_price in (None, 0):
        return None

    return ((end_price - start_price) / start_price) * 100.0


async def get_stock_indicators(client, stock_code, token):
    """Fetch technical indicator history from TCBS indicator API."""
    try:
        response = await client.get(
            f"https://apiextaws.tcbs.com.vn/tcanalysis/v1/data-charts/indicator?ticker={stock_code}",
            headers={
                "Accept": "application/json",
                "Authorization": f"Bearer {token}",
                "Referer": "https://tcinvest.tcbs.com.vn/"
            },
            timeout=10.0
        )
        response.raise_for_status()
        data = response.json()
        return data.get('listTechnicalIndicator', [])
    except Exception as e:
        print(f"⚠️ Could not fetch indicators for {stock_code}: {e}")
        return []


async def send_slack_error(error_message):
    """Send potential stock symbols to Slack"""
    slack_enabled = os.environ.get('SLACK_NOTIFICATIONS_ENABLED', 'false').lower() == 'true'
    if not slack_enabled:
        print("Slack notifications disabled, skipping")
        return
    
    slack_webhook_url = os.environ.get('SLACK_WEBHOOK_URL')
    if not slack_webhook_url:
        return
    
    message = {
        "text": f"🚨 *Error in fetch_potential_stocks.py*\n\n{error_message}"
    }
    
    try:
        async with AsyncSession() as client:
            await client.post(
                slack_webhook_url,
                json=message,
                timeout=10.0
            )
    except Exception:
        pass  # Silently fail if Slack notification fails


async def send_slack_message(symbols_list):
    """Send potential stock symbols to Slack"""
    slack_webhook_url = os.environ.get('SLACK_WEBHOOK_URL')
    if not slack_webhook_url:
        print("SLACK_WEBHOOK_URL not set, skipping Slack notification")
        return
    
    # Filter only near_52w_ath items
    filtered_list = [s for s in symbols_list if s[3] == SIGNAL_NEAR_52W_ATH]
    
    if not filtered_list:
        print("No potential near_52w_ath stocks to report")
        return
    
    # Format message
    symbols_text = "\n".join([
        f"• *{s[0]}* [{get_signal_label(s[3])}] - Volume: {s[4]:,}, Highest: {s[1]:,}, Lowest: {s[2]:,}"
        for s in filtered_list
    ])
    message = {
        "text": f"*Potential Stocks Detected ({len(filtered_list)} symbols)*\n\n{symbols_text}"
    }
    
    try:
        async with AsyncSession() as client:
            response = await client.post(
                slack_webhook_url,
                json=message,
                timeout=10.0
            )
            if response.status_code == 200:
                print(f"✅ Slack notification sent successfully ({len(filtered_list)} stocks)")
            else:
                print(f"⚠️ Failed to send Slack notification: {response.status_code}")
    except Exception as e:
        print(f"❌ Error sending Slack message: {e}")


async def get_stock_volume(client, stock_code, token):
    """Get trade volume for a stock from stockratio API"""
    try:
        volume_response = await client.get(
            f"https://apiextaws.tcbs.com.vn/tcanalysis/v1/ticker/{stock_code}/stockratio",
            headers={
                "Accept": "application/json",
                "Authorization": f"Bearer {token}",
                "Referer": "https://tcinvest.tcbs.com.vn/"
            },
            timeout=10.0
        )
        volume_response.raise_for_status()
        volume_data = volume_response.json()
        return volume_data.get('tradeVolume'), False
    except RequestsError as e:
        status_code = e.response.status_code if getattr(e, 'response', None) is not None else None
        if status_code == 404:
            print(f"⚠️ {stock_code} returned 404 in stockratio API, will cache for skip")
            return None, True
        print(f"⚠️ Error fetching volume for {stock_code}: {e}")
        return None, False
    except Exception as e:
        print(f"⚠️ Error fetching volume for {stock_code}: {e}")
        return None, False


async def get_tcbs_token():
    """Login to TCBS and get authentication token"""
    login_url = "https://apipub.tcbs.com.vn/authen/v1/login"
    
    # Check if credentials are set
    username = os.environ.get('TCBS_USR')
    password = os.environ.get('TCBS_PWD')
    
    if not username or not password:
        error_msg = "❌ TCBS_USR or TCBS_PWD environment variables are not set"
        print(error_msg)
        await send_slack_error(error_msg)
        return None
    
    headers = {
        "Accept": "application/json",
        "Content-Type": "application/json",
        "Referer": "https://tcinvest.tcbs.com.vn/"
    }
    login_data = {
        "username": username,
        "password": password,
        "device_info": '{"os.name":"macOS","os.version":"10.15","browser.name":"Chrome","browser.version":"120","device.platform":"web","device.name":"Chrome Mac","device.physicalID":"tcbs-bot-001","navigator.userAgent":"Mozilla/5.0","webVersion":"stable"}'
    }
    
    try:
        async with AsyncSession() as client:
            print(f"🔐 Attempting login with username: {username[:3]}***")
            response = await client.post(login_url, json=login_data, headers=headers, timeout=10.0)
            
            # Check for 400 error and show response body
            if response.status_code == 400:
                try:
                    error_detail = response.json()
                    error_msg = f"❌ TCBS login failed (400 Bad Request): {error_detail}"
                except:
                    error_msg = f"❌ TCBS login failed (400 Bad Request): {response.text}"
                print(error_msg)
                await send_slack_error(error_msg)
                return None
            
            response.raise_for_status()
            result = response.json()
            token = result.get('token')
            if token:
                print("✅ TCBS login successful, token obtained")
                return token
            else:
                error_msg = f"⚠️ No token in response. Response: {result}"
                print(error_msg)
                await send_slack_error(error_msg)
                return None
    except RequestsError as e:
        error_msg = f"❌ Error getting TCBS token: {e}"
        print(error_msg)
        await send_slack_error(error_msg)
        return None
    except Exception as e:
        error_msg = f"❌ Unexpected error getting TCBS token: {e}"
        print(error_msg)
        await send_slack_error(error_msg)
        return None


async def fetch_potential_stocks(stocks, conn):
    invalid_symbols = load_invalid_symbols()
    newly_invalid_symbols = set()

    if invalid_symbols:
        print(f"⏭️ Loaded {len(invalid_symbols)} cached invalid symbols (404) to skip")
    stocks = [stock for stock in stocks if stock['code'] not in invalid_symbols]

    # Get authentication token first
    token = await get_tcbs_token()
    if not token:
        print("❌ Failed to get authentication token, aborting...")
        return
    
    print(f"token: {token}")
    
    # controller is not directly translatable; httpx handles timeouts
    async with AsyncSession(impersonate="chrome") as client:
        data_to_insert = []  # List to accumulate data for bulk insertion

        vnindex_indicators = await get_stock_indicators(client, 'VNINDEX', token)
        vnindex_growth_20d = calc_growth_percent(vnindex_indicators, 20)
        vnindex_growth_30d = calc_growth_percent(vnindex_indicators, 30) or 0.0
        if vnindex_growth_20d is None:
            print("⚠️ Could not calculate VNINDEX 20-day growth, top_growth_20d signal will be skipped")
        else:
            print(f"📊 VNINDEX 20-day growth: {vnindex_growth_20d:.2f}%")
        print(f"📊 VNINDEX 30-day growth (benchmark): {vnindex_growth_30d:.2f}%")
        
        # Set up headers with Bearer token matching curl format
        headers = {
            "Authorization": f"Bearer {token}",
            "Accept": "application/json",
            "Referer": "https://tcinvest.tcbs.com.vn/"
        }
        
        for stock in stocks:
            stock_code = stock['code']
            try:
                # Use timeout to handle potential network issues
                response = await client.get(
                    f"https://apiextaws.tcbs.com.vn/tcanalysis/v1/ticker/{stock_code}/price-volatility",
                    headers=headers,
                    timeout=10.0
                )
                response.raise_for_status()
                await asyncio.sleep(1)
                data = response.json()

                highest_price_percent = data.get('highestPricePercent')

                # Check trade volume from stockratio API
                trade_volume, not_found = await get_stock_volume(client, stock_code, token)

                if not_found:
                    invalid_symbols.add(stock_code)
                    newly_invalid_symbols.add(stock_code)
                    continue

                # Cache symbols with missing/low volume to skip in future runs.
                if trade_volume is None or trade_volume < MIN_TRADE_VOLUME:
                    invalid_symbols.add(stock_code)
                    newly_invalid_symbols.add(stock_code)
                    print(
                        f"⚠️ {stock_code} cached for skip - Volume: {trade_volume} "
                        f"(skip threshold: {MIN_TRADE_VOLUME:,})"
                    )
                    continue

                indicators = await get_stock_indicators(client, stock_code, token)
                await asyncio.sleep(0.5)

                if not check_above_ma20(indicators):
                    print(f"⏭️ {stock_code} skipped - Price below MA20")
                    continue

                growth_20d = calc_growth_percent(indicators, 20)

                matched_signals = []
                if highest_price_percent is not None and highest_price_percent >= -0.05:
                    matched_signals.append(SIGNAL_NEAR_52W_ATH)

                if check_ema9_above_ema21(indicators):
                    matched_signals.append(SIGNAL_EMA9_ABOVE_EMA21)

                if (
                    vnindex_growth_20d is not None
                    and growth_20d is not None
                    and growth_20d >= vnindex_growth_20d
                ):
                    matched_signals.append(SIGNAL_TOP_GROWTH_20D)

                if not matched_signals:
                    print(f"⏭️ {stock_code} skipped - No matching stock signal")
                    continue

                growth_30d = calc_growth_percent(indicators, 30) or 0.0
                score_diff = growth_30d - vnindex_growth_30d

                for signal_type in matched_signals:
                    print(
                        f"🔹 Potential stock found: {stock_code} "
                        f"(Volume: {trade_volume:,}, Signal: {get_signal_label(signal_type)}, Score Diff: {score_diff:+.2f}%)"
                    )
                    data_to_insert.append((
                        data['ticker'],
                        data['highestPrice'],
                        data['lowestPrice'],
                        signal_type,
                        trade_volume,
                        score_diff,
                    ))

            except RequestsError as e:
                status_code = e.response.status_code if getattr(e, 'response', None) is not None else None
                if status_code == 404:
                    invalid_symbols.add(stock_code)
                    newly_invalid_symbols.add(stock_code)
                    print(f"⚠️ {stock_code} returned 404 in price-volatility API, cached for skip")
                elif status_code is not None:
                    print(f"HTTP error for {stock_code}: {e}")
                else:
                    print(f"Network error for {stock_code}: {e}")
            except asyncio.TimeoutError:
                print(f"Timeout for {stock_code}")
            except Exception as e:
                print(f"Error for {stock_code}: {e}")

            await asyncio.sleep(1)

        if newly_invalid_symbols:
            save_invalid_symbols(invalid_symbols)
            print(
                f"💾 Saved {len(newly_invalid_symbols)} new invalid symbols. "
                f"Total cached invalid symbols: {len(invalid_symbols)}"
            )
            
        # Delete all items before adding
        transaction = conn.transaction()
        await transaction.start()
        try:
            await conn.execute('''
                DELETE FROM symbols_watchlist
            ''')
        except asyncpg.PostgresError as e:
            print(f"Error deleting existing data: {e}")
            await transaction.rollback()
            return  # Exit if deletion fails
        else:
            await transaction.commit()
            
        # adding new items
        transaction = conn.transaction()
        await transaction.start()
        try:
            if data_to_insert:
                await conn.executemany('''
                    INSERT INTO symbols_watchlist (symbol, highest_price, lowest_price, signal_type, volume, score_diff)
                    VALUES ($1, $2, $3, $4, $5, $6)
                    ON CONFLICT (symbol, signal_type) DO UPDATE
                    SET highest_price = EXCLUDED.highest_price,
                        lowest_price = EXCLUDED.lowest_price,
                        volume = EXCLUDED.volume,
                        score_diff = EXCLUDED.score_diff
                ''', data_to_insert)
        except asyncpg.PostgresError as e:
            print(f"Database error during bulk insert: {e}")
            await transaction.rollback()
        else:
            await transaction.commit()
            # Send Slack notification with the inserted data
            await send_slack_message(data_to_insert)

async def update_current_prices_portfolio(conn):
    try:
        # Get all data from the user_trading_symbols table
        symbols = await conn.fetch('SELECT symbol FROM user_trading_symbols')

        data_to_update = []
        async with AsyncSession(impersonate="chrome") as client:
            for record in symbols:
                symbol = record['symbol']
                url = f"https://services.entrade.com.vn/dnse-financial-product/securities/{symbol}"
                try:
                    response = await client.get(url, timeout=10.0)
                    response.raise_for_status()
                    data = response.json()
                    basic_price = data.get('basicPrice')  # Use .get() for safety
                    if basic_price is not None:
                        data_to_update.append((basic_price, symbol))  #price, symbol
                    else:
                        data_to_update.append((0, symbol)) # Default to 0 if basic_price is None
                except RequestsError as e:
                    print(f"Error for {symbol}: {e}")
                except asyncio.TimeoutError:
                    print(f"Timeout for {symbol}")
                except Exception as e:
                    print(f"Error for {symbol}: {e}")

                await asyncio.sleep(1) # Comply with rate limit

        # Update the current_price field in the database using a single SQL statement
        if data_to_update:
            transaction = conn.transaction()
            await transaction.start()
            try:
                await conn.executemany('''
                    UPDATE user_trading_symbols
                    SET current_price = $1
                    WHERE symbol = $2
                ''', data_to_update)
            except asyncpg.PostgresError as e:
                print(f"Database error during bulk update: {e}")
                await transaction.rollback()
            else:
                await transaction.commit()


    except Exception as e:
        print(f"An error occurred: {e}")

async def main():
    # Fetch stock data
    try:
        async with AsyncSession(impersonate="chrome") as client:
            headers = {
                "User-Agent": "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36"
            }
            response = await client.get("https://api-finfo.vndirect.com.vn/v4/stocks?q=type:STOCK~status:LISTED&fields=code&size=3000", headers=headers)
            response.raise_for_status()
            stocks_data = response.json().get('data', [])
    except RequestsError as e:
        print(f"Error fetching stock data: {e}")
        return

    # Database connection
    conn = None
    try:
        db_port_str = os.environ.get('DB_PORT')
        if db_port_str is None:
            raise ValueError("Environment variable DB_PORT is not set.")
        conn = await asyncpg.connect(
            user=os.environ.get('DB_USER'),
            password=os.environ.get('DB_PASSWORD'),
            database=os.environ.get('DB_NAME'),
            host=os.environ.get('DB_HOST'),
            port=int(db_port_str)
        )

        stocks_data = [item for item in stocks_data if len(item['code']) == 3]

        await fetch_potential_stocks(stocks_data, conn)

        await update_current_prices_portfolio(conn)

        # Check price alerts for stocks
        print("🔹 Checking price alerts for stocks...")
        stocks_in_watchlist = await conn.fetch('''
            SELECT symbol, highest_price
            FROM symbols_watchlist
            WHERE signal_type = $1
        ''', SIGNAL_NEAR_52W_ATH)
        price_data = {}
        for record in stocks_in_watchlist:
            symbol = record['symbol']
            highest_price = record['highest_price']
            if highest_price and highest_price > 0:
                price_data[symbol] = float(highest_price) / 1000.0  # Convert to thousands
        
        if price_data:
            triggered = check_multiple_alerts("stock", price_data)
            print(f"🔔 Triggered {triggered} stock price alerts")

    except Exception as e:
        print(f"An error occurred: {e}")

    finally:
        if conn:
            await conn.close()


if __name__ == "__main__":
    asyncio.run(main())