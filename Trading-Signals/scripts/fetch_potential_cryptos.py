import asyncio
import asyncpg
import time
import httpx
import os
import ccxt
import re
from datetime import datetime, timedelta
from dotenv import load_dotenv
from price_alert_utils import check_multiple_alerts

# Signal type constants
SIGNAL_NEAR_52W_ATH = 'near_52w_ath'
SIGNAL_NEAR_ATH = 'near_ath'
SIGNAL_EMA9_ABOVE_EMA21 = 'ema9_above_ema21'

EXCLUDE_KEYWORDS = [
    "USDC", "USDE", "FDUSD", "USD1", "TUSD", "USDD", "USDP", "DAI", "BUSD", "GUSD", "USTC", "BFUSD", "XUSD", "EUR",
    "PYUSD", "SUSD", "SUSDE", "USDT0", "RLUSD", "SUSDS", "USDF", "USYC", "USDG", "USDS", "UUSDT", "USDS",  # Stablecoins & derivatives
    "WETH", "WBTC", "STETH", "WSTETH", "RETH", "RSETH", "WEETH", "FBTC", "CBBTC", "JITOSOL", "JLP",  # Wrapped tokens
    "WBNB", "LEO", "GT", "USDT", "CRO", "CC", "BGB", "OKB", "HTX", "KCS", "XAUT",  # Exchange tokens & Commodity
    "BSC-USD", "FIGR_HELOC", "SYRUPUSDC", "NIGHT", "HASH", "HYPE", "KAS",  # Special/problematic symbols
    "M", "MNT", "RAIN", "BUIDL", "PI"  # Other problematic symbols
]

# Number of top coins to scan
TOP_COINS_LIMIT = 25

# Coins with unreliable Binance data - force check via CoinGecko
FORCE_COINGECKO_CHECK = ["XMR", "ZEC"]  # Delisted or frozen price data

# Hardcoded CoinGecko IDs for important coins to avoid rate limits
COINGECKO_IDS = {
    "XMR": "monero",
    "ZEC": "zcash",
}

# Cache for CoinGecko coin list to avoid rate limits
_COINGECKO_COIN_LIST_CACHE = None

# Load environment variables from .env file
load_dotenv()


# ================================
#  MA9 / EMA21 INDICATOR FUNCTIONS
# ================================
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


def check_ema9_above_ema21(closes):
    """Return True when the latest EMA9 >= EMA21 of the close series."""
    if not closes or len(closes) < 21:
        return False
    ema9 = _calc_ema(closes, 9)
    ema21 = _calc_ema(closes, 21)
    if ema9 is None or ema21 is None:
        return False
    return ema9 >= ema21


async def fetch_daily_closes(symbol, days=30):
    """Fetch daily close prices from Binance for MA/EMA calculation."""
    try:
        url = "https://api.binance.com/api/v3/klines"
        params = {
            "symbol": symbol,
            "interval": "1d",
            "limit": days
        }
        async with httpx.AsyncClient(timeout=10) as client:
            res = await client.get(url, params=params)
            if res.status_code != 200:
                return None
            klines = res.json()
            if not klines:
                return None
            # kline format: [timestamp, open, high, low, close, volume, ...]
            return [float(k[4]) for k in klines]
    except Exception as e:
        print(f"   ⚠️  Error fetching daily closes for {symbol}: {e}")
        return None


async def fetch_30d_perf(symbol):
    closes = await fetch_daily_closes(symbol, days=30)
    if closes and len(closes) >= 2:
        return ((closes[-1] - closes[0]) / closes[0]) * 100
    return 0.0


def get_signal_label(signal_type):
    """Return human-readable label for a signal type."""
    if signal_type == SIGNAL_NEAR_52W_ATH:
        return 'Near 52W High'
    if signal_type == SIGNAL_NEAR_ATH:
        return 'Near ATH'
    if signal_type == SIGNAL_EMA9_ABOVE_EMA21:
        return 'Uptrend'
    return signal_type

async def send_slack_message(cryptos_list):
    """Send potential crypto symbols to Slack"""
    slack_enabled = os.environ.get('SLACK_NOTIFICATIONS_ENABLED', 'false').lower() == 'true'
    if not slack_enabled:
        print("Slack notifications disabled, skipping")
        return
    
    slack_webhook_url = os.environ.get('SLACK_WEBHOOK_URL')
    if not slack_webhook_url:
        print("SLACK_WEBHOOK_URL not set, skipping Slack notification")
        return
    
    if not cryptos_list:
        print("No potential cryptos to report")
        return
    
    # Separate by signal type
    ath_coins = [c["symbol"] for c in cryptos_list if c.get("signal_type") == SIGNAL_NEAR_ATH]
    week_52_coins = [c["symbol"] for c in cryptos_list if c.get("signal_type") == SIGNAL_NEAR_52W_ATH]
    ema9_coins = [c["symbol"] for c in cryptos_list if c.get("signal_type") == SIGNAL_EMA9_ABOVE_EMA21]
    
    message_parts = [f"🚀 *Potential Cryptos Detected ({len(cryptos_list)} signals)*\n"]
    
    if ath_coins:
        ath_text = "\n".join([f"• {s}" for s in ath_coins])
        message_parts.append(f"\n*Near ATH ({len(ath_coins)}):*\n{ath_text}")
    
    if week_52_coins:
        week_text = "\n".join([f"• {s}" for s in week_52_coins])
        message_parts.append(f"\n*Near 52-Week High ({len(week_52_coins)}):*\n{week_text}")
    
    if ema9_coins:
        ema9_text = "\n".join([f"• {s}" for s in ema9_coins])
        message_parts.append(f"\n*EMA9 >= EMA21 ({len(ema9_coins)}):*\n{ema9_text}")
    
    message = {"text": "".join(message_parts)}
    
    try:
        async with httpx.AsyncClient() as client:
            response = await client.post(
                slack_webhook_url,
                json=message,
                timeout=10.0
            )
            if response.status_code == 200:
                print(f"Slack notification sent successfully ({len(cryptos_list)} cryptos)")
            else:
                print(f"Failed to send Slack notification: {response.status_code}")
    except Exception as e:
        print(f"Error sending Slack message: {e}")

# ================================
#  COINGECKO + MULTI-EXCHANGE FUNCTIONS
# ================================

async def get_coingecko_market_caps():
    """Lấy danh sách vốn hoá từ CoinGecko để làm map tra cứu"""
    try:
        url = "https://api.coingecko.com/api/v3/coins/markets"
        params = {
            "vs_currency": "usd",
            "order": "market_cap_desc",
            "per_page": 250,
            "page": 1,
            "sparkline": False
        }
        async with httpx.AsyncClient(timeout=15) as client:
            res = await client.get(url, params=params)
            if res.status_code == 200:
                coins = res.json()
                return {c.get('symbol', '').upper(): float(c.get('market_cap') or 0) for c in coins}
    except Exception as e:
        print(f"⚠️ Lỗi fetch CoinGecko market caps: {e}")
    return {}

def get_base_symbol(symbol):
    """Trích xuất symbol cơ bản"""
    base = symbol[:-4] if symbol.endswith('USDT') else symbol
    return re.sub(r'^\d+', '', base).upper()
async def get_top_coins_from_coingecko(limit=TOP_COINS_LIMIT):
    """Get top coins by market cap from CoinGecko (includes XMR, ZEC, etc.)"""
    try:
        url = "https://api.coingecko.com/api/v3/coins/markets"
        params = {
            "vs_currency": "usd",
            "order": "market_cap_desc",
            "per_page": limit,
            "page": 1,
            "sparkline": False
        }
        
        async with httpx.AsyncClient(timeout=15) as client:
            res = await client.get(url, params=params)
            if res.status_code != 200:
                print(f"   ⚠️  CoinGecko API error: {res.status_code}")
                return []
            
            coins = res.json()
            symbols = []
            for coin in coins:
                symbol = coin.get('symbol', '').upper()
                if symbol and symbol not in EXCLUDE_KEYWORDS:
                    symbols.append(symbol + 'USDT')
            
            print(f"   Got {len(symbols)} coins from CoinGecko")
            return symbols
    except Exception as e:
        print(f"   ⚠️  Error fetching from CoinGecko: {e}")
        return []


async def get_top_usdt_pairs_by_volume(limit=TOP_COINS_LIMIT):
    """Get top coins from CoinGecko, fallback to Binance if needed"""
    # Try CoinGecko first
    symbols = await get_top_coins_from_coingecko(limit=limit)
    
    # If CoinGecko fails, fallback to Binance
    if not symbols:
        print("   Falling back to Binance...")
        loop = asyncio.get_event_loop()
        exchange = ccxt.binance()
        
        try:
            tickers = await loop.run_in_executor(None, exchange.fetch_tickers)
            usdt_pairs = []
            for symbol, ticker in tickers.items():
                base_currency = symbol.split('/')[0]
                if (symbol.endswith('/USDT') and 
                    ticker.get('quoteVolume') and 
                    base_currency not in EXCLUDE_KEYWORDS):
                    usdt_pairs.append({
                        'symbol': symbol.replace('/', ''),
                        'volume': ticker['quoteVolume']
                    })
            
            usdt_pairs.sort(key=lambda x: x['volume'], reverse=True)
            symbols = [pair['symbol'] for pair in usdt_pairs[:limit]]
        except Exception as e:
            print(f"   ⚠️  Binance fallback also failed: {e}")
            return []
    
    return symbols


async def check_ath_from_coingecko(base_symbol):
    """Check if a coin is at/near ATH using CoinGecko"""
    try:
        # Get coin ID from symbol
        url = f"https://api.coingecko.com/api/v3/coins/list"
        async with httpx.AsyncClient(timeout=10) as client:
            res = await client.get(url)
            if res.status_code != 200:
                return None
            
            coins = res.json()
            coin_id = None
            for coin in coins:
                if coin['symbol'].upper() == base_symbol.upper():
                    coin_id = coin['id']
                    break
            
            if not coin_id:
                print(f"   ⚠️  Could not find CoinGecko ID for {base_symbol}")
                return None
            
            # Get market data including ATH
            url = f"https://api.coingecko.com/api/v3/coins/{coin_id}"
            params = {"localization": False, "tickers": False, "community_data": False, "developer_data": False}
            res = await client.get(url, params=params)
            
            if res.status_code != 200:
                return None
            
            data = res.json()
            current_price = data.get('market_data', {}).get('current_price', {}).get('usd')
            ath = data.get('market_data', {}).get('ath', {}).get('usd')
            
            if not current_price or not ath:
                return None
            
            # Check if within 10% of ATH
            diff_from_ath = (ath - current_price) / ath
            if diff_from_ath <= 0.10:
                print(f"   🏆 {base_symbol} near ATH: Price ${current_price:.2f} | ATH: ${ath:.2f} | Gap: -{diff_from_ath:.2%}")
                return {"is_ath": True, "price": current_price, "ath": ath}
            
            return None
    except Exception as e:
        print(f"   ⚠️  Error checking ATH for {base_symbol}: {e}")
        return None


async def check_52week_high_from_coingecko(base_symbol):
    """Check if a coin is near its 52-week high using CoinGecko"""
    global _COINGECKO_COIN_LIST_CACHE
    
    try:
        # Use hardcoded ID if available
        coin_id = COINGECKO_IDS.get(base_symbol)
        
        # Otherwise, fetch from API
        if not coin_id:
            if _COINGECKO_COIN_LIST_CACHE is None:
                url = f"https://api.coingecko.com/api/v3/coins/list"
                async with httpx.AsyncClient(timeout=15) as client:
                    res = await client.get(url)
                    if res.status_code == 429:
                        print(f"   ⚠️  CoinGecko rate limit, waiting 5s...")
                        await asyncio.sleep(5)
                        res = await client.get(url)
                    
                    if res.status_code != 200:
                        return None
                    
                    _COINGECKO_COIN_LIST_CACHE = res.json()
            
            coins = _COINGECKO_COIN_LIST_CACHE
            for coin in coins:
                if coin['symbol'].upper() == base_symbol.upper():
                    coin_id = coin['id']
                    break
        
        if not coin_id:
            return None
        
        # Get 365 days of price data
        url = f"https://api.coingecko.com/api/v3/coins/{coin_id}/market_chart"
        params = {"vs_currency": "usd", "days": "365"}
        
        async with httpx.AsyncClient(timeout=15) as client:
            res = await client.get(url, params=params)
            
            if res.status_code == 429:
                print(f"   ⚠️  CoinGecko rate limit for {base_symbol}, waiting 5s...")
                await asyncio.sleep(5)
                res = await client.get(url, params=params)
            
            if res.status_code != 200:
                return None
            
            data = res.json()
            prices = [p[1] for p in data.get('prices', [])]
            
            if not prices or len(prices) < 30:
                return None
            
            max_52w = max(prices)
            current_price = prices[-1]
            
            # Check if within 10% of 52-week high
            diff = (max_52w - current_price) / max_52w
            if diff <= 0.10:
                print(f"   ✅ {base_symbol} (CoinGecko): Price ${current_price:.2f} | 52W High: ${max_52w:.2f} | Gap: -{diff:.2%}")
                return {"price": current_price, "high_52w": max_52w, "diff": diff}
            
            return None
    except Exception as e:
        print(f"   ⚠️  Error checking CoinGecko for {base_symbol}: {e}")
        return None


async def check_52week_high_async(symbol):
    """Check if a symbol is near its 52-week high, try multiple sources"""
    try:
        symbol_formatted = symbol.replace('USDT', '/USDT')
        base_symbol = symbol.replace('USDT', '')
        
        # Force CoinGecko check for coins with unreliable Binance data
        if base_symbol in FORCE_COINGECKO_CHECK:
            print(f"   🔍 {symbol} - Forcing CoinGecko check (unreliable Binance data)...")
            coingecko_result = await check_52week_high_from_coingecko(base_symbol)
            if coingecko_result:
                return {
                    "symbol": symbol, 
                    "is_ath": False, 
                    "signal_type": SIGNAL_NEAR_52W_ATH,
                    "highest_price": coingecko_result["high_52w"]
                }
            else:
                print(f"   ⚠️  {symbol} not near 52w high on CoinGecko")
                return None
        
        # Try Binance first (fastest)
        url = "https://api.binance.com/api/v3/klines"
        params = {
            "symbol": symbol,
            "interval": "1d",
            "limit": 365
        }
        
        async with httpx.AsyncClient(timeout=10) as client:
            res = await client.get(url, params=params)
            
            # If Binance fails, try MEXC using ccxt
            if res.status_code != 200:
                print(f"   🔍 {symbol} not on Binance, trying MEXC...")
                
                # Try to get 52-week data from MEXC first
                try:
                    loop = asyncio.get_event_loop()
                    exchange = ccxt.mexc()
                    
                    # Fetch 52 weeks of daily data
                    since = int((datetime.now() - timedelta(days=365)).timestamp() * 1000)
                    ohlcv = await loop.run_in_executor(
                        None, 
                        lambda: exchange.fetch_ohlcv(symbol_formatted, '1d', since=since, limit=365)
                    )
                    
                    if not ohlcv or len(ohlcv) == 0:
                        print(f"   ⚠️  No OHLCV data from MEXC for {symbol}")
                        return None
                    
                    if len(ohlcv) < 30:  # Not enough data
                        print(f"   ⚠️  Insufficient data from MEXC for {symbol}: only {len(ohlcv)} days")
                        return None
                    
                    # Extract highs and current price from OHLCV
                    highs = [candle[2] for candle in ohlcv]
                    current_price = ohlcv[-1][4]  # Last close price
                    max_52w = max(highs)
                    
                    print(f"   ✓ Got MEXC data for {symbol}: {len(ohlcv)} candles, price: {current_price}")
                    
                    # Check if within 10% of 52W high
                    diff = (max_52w - current_price) / max_52w
                    if diff <= 0.10:
                        ath_result = await check_ath_from_coingecko(base_symbol)
                        is_ath = ath_result is not None
                        highest_price = ath_result["ath"] if is_ath else max_52w
                        return {
                            "symbol": symbol, 
                            "is_ath": is_ath, 
                            "signal_type": SIGNAL_NEAR_ATH if is_ath else SIGNAL_NEAR_52W_ATH,
                            "highest_price": highest_price
                        }
                    
                except Exception as e:
                    # If MEXC also fails, try CoinGecko as last resort
                    print(f"   ⚠️  MEXC failed for {symbol}, trying CoinGecko as fallback...")
                    ath_result = await check_ath_from_coingecko(base_symbol)
                    if ath_result:
                        print(f"   🏆 {symbol} is near ATH (CoinGecko)")
                        return {
                            "symbol": symbol, 
                            "is_ath": True, 
                            "signal_type": SIGNAL_NEAR_ATH,
                            "highest_price": ath_result["ath"]
                        }
                    return None
            else:
                # Parse Binance response
                klines = res.json()
                if not klines:
                    return None
                
                highs = [float(k[2]) for k in klines]
                current_price = float(klines[-1][4])
            
            max_52w = max(highs)
            
            # Check if within 10% of 52-week high
            diff = (max_52w - current_price) / max_52w
            if diff <= 0.10:
                # Only if near 52-week high, then check if it's also ATH
                ath_result = await check_ath_from_coingecko(base_symbol)
                is_ath = ath_result is not None
                highest_price = ath_result["ath"] if is_ath else max_52w
                
                if is_ath:
                    print(f"   🏆 {symbol_formatted}: Price {current_price:.2f} | Near ATH!")
                else:
                    print(f"   ✅ {symbol_formatted}: Price {current_price:.2f} | 52W High: {max_52w:.2f} | Gap: -{diff:.2%}")
                
                return {
                    "symbol": symbol, 
                    "is_ath": is_ath, 
                    "signal_type": SIGNAL_NEAR_ATH if is_ath else SIGNAL_NEAR_52W_ATH,
                    "highest_price": highest_price
                }
            
            return None
    except Exception as e:
        print(f"   ⚠️  Error checking {symbol}: {e}")
        return None

# ================================
#  DATABASE UPDATE LOGIC
# ================================
async def check_ema9_ema21_batch(symbols):
    """Check EMA9 >= EMA21 for a list of symbols, return matching ones."""
    ema9_results = []
    batch_size = 10
    total_batches = (len(symbols) + batch_size - 1) // batch_size
    
    for i in range(0, len(symbols), batch_size):
        batch = symbols[i:i + batch_size]
        batch_num = i // batch_size + 1
        print(f"   EMA9/EMA21 batch {batch_num}/{total_batches} ({len(batch)} symbols)...")
        
        tasks = [fetch_daily_closes(symbol, days=30) for symbol in batch]
        results = await asyncio.gather(*tasks, return_exceptions=True)
        
        for symbol, closes in zip(batch, results):
            if closes and not isinstance(closes, Exception):
                if check_ema9_above_ema21(closes):
                    base = symbol.replace('USDT', '')
                    print(f"   📈 {base}: EMA9 >= EMA21 ✓")
                    ema9_results.append({
                        "symbol": symbol,
                        "is_ath": False,
                        "signal_type": SIGNAL_EMA9_ABOVE_EMA21,
                        "highest_price": max(closes) if closes else None
                    })
        
        if i + batch_size < len(symbols):
            await asyncio.sleep(0.2)
    
    return ema9_results


async def update_cryptos_watchlist(conn):
    print("🔹 Fetching top coins from CoinGecko...")
    top_symbols = await get_top_usdt_pairs_by_volume(limit=TOP_COINS_LIMIT)
    
    print(f"🔹 Found {len(top_symbols)} top trading pairs")
    print(f"   Symbols: {', '.join([s.replace('USDT', '') for s in top_symbols[:10]])}...")

    # --- Phase 1: Check 52-week highs (async batch processing) ---
    print("🔹 Checking 52-week highs...")
    near_52w = []
    
    # Process in concurrent batches
    batch_size = 10
    total_batches = (len(top_symbols) + batch_size - 1) // batch_size
    
    for i in range(0, len(top_symbols), batch_size):
        batch = top_symbols[i:i + batch_size]
        batch_num = i // batch_size + 1
        print(f"   Processing batch {batch_num}/{total_batches} ({len(batch)} symbols)...")
        
        # Execute batch concurrently
        tasks = [check_52week_high_async(symbol) for symbol in batch]
        results = await asyncio.gather(*tasks, return_exceptions=True)
        
        # Filter successful results
        for res in results:
            if res and not isinstance(res, Exception):
                near_52w.append(res)
        
        # Small delay between batches to respect rate limits
        if i + batch_size < len(top_symbols):
            await asyncio.sleep(0.2)
    
    print(f"   Found {len(near_52w)} coins near 52-week high")

    # --- Phase 2: Check EMA9 >= EMA21 ---
    print("🔹 Checking EMA9 >= EMA21 (1d timeframe)...")
    ema9_results = await check_ema9_ema21_batch(top_symbols)
    print(f"   Found {len(ema9_results)} coins with EMA9 >= EMA21")

    # --- Combine all signals ---
    data_to_insert = near_52w + ema9_results
    
    print("🔹 Đang tải dữ liệu vốn hoá từ CoinGecko...")
    market_caps = await get_coingecko_market_caps()
    for item in data_to_insert:
        base_symbol = get_base_symbol(item["symbol"])
        item["market_cap"] = market_caps.get(base_symbol, 0.0)

    # Calculate score_diff against BTCUSDT
    print("🔹 Calculating relative strength against BTCUSDT...")
    btc_perf = await fetch_30d_perf("BTCUSDT")
    print(f"   BTCUSDT 30d performance: {btc_perf:+.2f}%")
    for item in data_to_insert:
        symbol = item["symbol"]
        coin_perf = await fetch_30d_perf(symbol)
        item["score_diff"] = coin_perf - btc_perf
        print(f"   {symbol}: perf={coin_perf:+.2f}%, score_diff={item['score_diff']:+.2f}%")

    if data_to_insert:
        print(f"🔹 Updating {len(data_to_insert)} records into DB...")
        transaction = conn.transaction()
        await transaction.start()
        try:
            await conn.execute("DELETE FROM cryptos_watchlist")
            await conn.executemany(
                """
                INSERT INTO cryptos_watchlist (crypto, is_ath, signal_type, highest_price, market_cap, score_diff)
                VALUES ($1, $2, $3, $4, $5, $6)
                ON CONFLICT (crypto, signal_type)
                DO UPDATE SET is_ath = EXCLUDED.is_ath, highest_price = EXCLUDED.highest_price, market_cap = EXCLUDED.market_cap, score_diff = EXCLUDED.score_diff, updated_at = CURRENT_TIMESTAMP
                """,
                [(d["symbol"], d["is_ath"], d["signal_type"], float(d["highest_price"]) if d.get("highest_price") is not None else None, d["market_cap"], d["score_diff"]) for d in data_to_insert],
            )
        except asyncpg.PostgresError as e:
            print("Database error:", e)
            await transaction.rollback()
        else:
            await transaction.commit()
            print("Database updated successfully.")
            # Send Slack notification with the inserted data
            await send_slack_message(data_to_insert)
            
            # Check price alerts
            print("🔹 Checking price alerts...")
            price_data = {}
            for item in data_to_insert:
                symbol = item["symbol"]
                if symbol in price_data:
                    continue  # Avoid duplicate price fetches
                try:
                    # Get current price
                    url = f"https://api.binance.com/api/v3/ticker/price"
                    params = {"symbol": symbol}
                    async with httpx.AsyncClient(timeout=5) as client:
                        res = await client.get(url, params=params)
                        if res.status_code == 200:
                            data = res.json()
                            price_data[symbol] = float(data["price"])
                except Exception as e:
                    print(f"   Error getting price for {symbol}: {e}")
            
            if price_data:
                triggered = check_multiple_alerts("crypto", price_data)
                print(f"🔔 Triggered {triggered} price alerts")
    else:
        print("No data to insert.")


# ================================
#  MAIN ENTRY
# ================================
async def main():
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

        await update_cryptos_watchlist(conn)

    except Exception as e:
        print("Error:", e)
    finally:
        if conn:
            await conn.close()


if __name__ == "__main__":
    asyncio.run(main())
