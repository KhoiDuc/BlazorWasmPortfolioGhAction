import asyncio
import asyncpg
import time
import httpx
import os
import ccxt.async_support as ccxt
from datetime import datetime, timedelta
from dotenv import load_dotenv

# Load environment variables from the .env file in the same directory as this script
script_dir = os.path.dirname(os.path.abspath(__file__))
dotenv_path = os.path.join(script_dir, '.env')
load_dotenv(dotenv_path=dotenv_path, override=True)

# Signal Constants
SIGNAL_NEAR_52W_ATH = 'near_52w_ath'
SIGNAL_NEAR_ATH = 'near_ath'

# Scan Intervals (Seconds)
NORMAL_INTERVAL = 300  # 5 minutes for standard monitoring
FAST_INTERVAL = 60     # 1 minute for continuous high-frequency breakout monitoring

class CoinState:
    def __init__(self, symbol, breakout_price):
        self.symbol = symbol
        self.breakout_price = breakout_price
        self.timeframe = '1D'               # Timeframe stages: '1D' -> '4H' -> '1H' -> '5m'
        self.previous_price = None          # Last interval price in current timeframe stage
        self.highest_price = 0.0            # Peak observed price since tracking started/reset
        self.highest_price_above_breakout = 0.0 # Peak price observed above breakout level
        self.consecutive_increases = 0      # Count of consecutive price increases in current stage
        self.is_close = False               # True if price is within 1.5% of breakout
        self.last_queried = 0.0             # Timestamp of the last price query for this coin
        self.has_broken_out = False         # Tracks if a breakout above ATH/52W has been initiated
        self.alerted_near_breakout = False  # Tracks if Slack warning for 4% gap has been sent
        self.is_new_high = False            # Tracks if the coin just broke its highest price above breakout

# Global State caches
breakout_prices = {}     # Caches breakout level for each symbol: { 'BTCUSDT': price }
coin_states = {}         # Maps symbol to its CoinState object: { 'BTCUSDT': CoinState }
triggered_symbols = set() # Set of symbols already traded to avoid double entry

# Global HTTP client to reuse connections and prevent IP blocks
http_client = httpx.AsyncClient(
    timeout=10.0,
    limits=httpx.Limits(max_connections=50, max_keepalive_connections=20),
    headers={"User-Agent": "Binance-Uptrend-Bot/1.0"}
)

async def send_custom_slack_message(text):
    """Print message to console and send to Slack if enabled."""
    print(f"[{datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] {text}")
    slack_enabled = os.environ.get('SLACK_NOTIFICATIONS_ENABLED', 'false').lower() == 'true'
    if not slack_enabled:
        return
    slack_webhook_url = os.environ.get('SLACK_WEBHOOK_URL')
    if not slack_webhook_url:
        return
    try:
        await http_client.post(slack_webhook_url, json={"text": text})
    except Exception as e:
        print(f"⚠️ Error sending Slack message: {e}")


async def fetch_coingecko_ath(base_symbol):
    """Fetch All-Time High price from CoinGecko for a given base symbol."""
    try:
        url = "https://api.coingecko.com/api/v3/coins/list"
        res = await http_client.get(url)
        if res.status_code == 200:
            coins = res.json()
            coin_id = None
            for coin in coins:
                if coin['symbol'].upper() == base_symbol.upper():
                    coin_id = coin['id']
                    break
            
            if coin_id:
                url = f"https://api.coingecko.com/api/v3/coins/{coin_id}"
                params = {
                    "localization": False, 
                    "tickers": False, 
                    "community_data": False, 
                    "developer_data": False
                }
                res = await http_client.get(url, params=params)
                if res.status_code == 200:
                    data = res.json()
                    ath = data.get('market_data', {}).get('ath', {}).get('usd')
                    if ath:
                        return float(ath)
    except Exception as e:
        print(f"⚠️ Error fetching CoinGecko ATH for {base_symbol}: {e}")
    return None


async def fetch_52w_high(symbol):
    """Fetch 52-week high from Binance klines or fallback to MEXC daily OHLCV."""
    try:
        url = "https://api.binance.com/api/v3/klines"
        params = {
            "symbol": symbol,
            "interval": "1d",
            "limit": 365
        }
        res = await http_client.get(url, params=params)
        if res.status_code == 200:
            klines = res.json()
            if klines:
                highs = [float(k[2]) for k in klines]
                return max(highs)
        
        # Fallback to MEXC using ccxt
        print(f"⚠️ Binance klines failed for {symbol}, trying MEXC via CCXT...")
        mexc = ccxt.mexc()
        symbol_formatted = symbol.replace('USDT', '/USDT')
        since = int((datetime.now() - timedelta(days=365)).timestamp() * 1000)
        
        try:
            ohlcv = await mexc.fetch_ohlcv(symbol_formatted, '1d', since=since, limit=365)
            if ohlcv:
                highs = [candle[2] for candle in ohlcv]
                return max(highs)
        finally:
            await mexc.close()
    except Exception as e:
        print(f"⚠️ Error fetching 52-week high for {symbol}: {e}")
    return None


async def execute_binance_trade(symbol, current_price, breakout_price):
    """Enter a spot position on Binance using CCXT and set a -7% Stop Loss order, checking for existing positions first."""
    binance_api_key = os.environ.get('BINANCE_API_KEY')
    binance_secret_key = os.environ.get('BINANCE_SECRET_KEY')
    
    if not binance_api_key or not binance_secret_key:
        msg = (
            f"⚠️ Binance API keys are not set. Skipped live trading for {symbol}.\n"
            f"Breakout/Pullback confirmed: Current price {current_price} > Breakout price {breakout_price}"
        )
        await send_custom_slack_message(msg)
        return
 
    # Add symbol to triggered set to avoid duplicate entries during this run
    triggered_symbols.add(symbol)
    
    # Initialize CCXT Binance async client
    exchange = ccxt.binance({
        'apiKey': binance_api_key,
        'secret': binance_secret_key,
        'enableRateLimit': True,
        'options': {
            'defaultType': 'spot',
        }
    })
    
    try:
        symbol_ccxt = symbol.replace('USDT', '/USDT')
        base_asset = symbol.replace('USDT', '')
        
        await exchange.load_markets()
        
        if symbol_ccxt not in exchange.markets:
            raise ValueError(f"Symbol {symbol_ccxt} not found on Binance spot markets")

        # 1. Fetch current spot balance of the base currency to prevent overexposure
        print(f"🔍 [Binance Bot] Checking existing balance for {base_asset} before placing trade...")
        balance = await exchange.fetch_balance()
        base_balance = float(balance['total'].get(base_asset, 0.0))
        position_value = base_balance * current_price
        
        # Position is defined as having a value of >= 100 USD/USDT
        if position_value >= 100.0:
            msg_skip = (
                f"⚠️ [SKIP TRADE ENTRY] Already holding an active position in {symbol}!\n"
                f"• Current Balance: {base_balance:.6f} {base_asset}\n"
                f"• Position Value: {position_value:.2f} USDT (>= 100 USDT limit)\n"
                f"• Skipped entering trade to prevent overexposure."
            )
            await send_custom_slack_message(msg_skip)
            return

        print(f"✓ No active position found (Value: {position_value:.2f} USDT < 100 USDT limit). Proceeding with order.")

        # 2. Calculate trade amount
        trade_amount_usdt = float(os.environ.get('BINANCE_TRADE_AMOUNT_USDT', '20.0'))
        amount = trade_amount_usdt / current_price
        
        # Format amount based on spot precision
        amount_precision = float(exchange.amount_to_precision(symbol_ccxt, amount))
        
        msg_buy = f"🛒 [Binance Bot] Placing Market Buy for {symbol_ccxt} of cost ~{trade_amount_usdt} USDT..."
        await send_custom_slack_message(msg_buy)
        
        # Place Market Buy
        order = await exchange.create_market_buy_order(symbol_ccxt, amount_precision)
        
        entry_price = float(order.get('average') or order.get('price') or current_price)
        filled_amount = float(order.get('filled') or amount_precision)
        
        msg_success = f"🎉 [Binance Bot] Entry Filled: Bought {filled_amount} {symbol_ccxt} at avg price {entry_price:.6f} USDT"
        await send_custom_slack_message(msg_success)
        
        # Calculate stop-loss (7% lower than entry price)
        stop_loss_price = entry_price * 0.93
        # Limit price for stop loss limit order (set slightly lower to guarantee execution)
        limit_price = stop_loss_price * 0.99
        
        # Format prices to match exchange requirements
        stop_price_precision = float(exchange.price_to_precision(symbol_ccxt, stop_loss_price))
        limit_price_precision = float(exchange.price_to_precision(symbol_ccxt, limit_price))
        filled_amount_precision = float(exchange.amount_to_precision(symbol_ccxt, filled_amount))
        
        msg_sl = f"🛡️ [Binance Bot] Setting Stop-Loss Limit at stop: {stop_price_precision:.6f}, limit: {limit_price_precision:.6f}..."
        await send_custom_slack_message(msg_sl)
        
        # Set STOP_LOSS_LIMIT sell order
        sl_order = await exchange.create_order(
            symbol=symbol_ccxt,
            type='STOP_LOSS_LIMIT',
            side='sell',
            amount=filled_amount_precision,
            price=limit_price_precision,
            params={
                'stopPrice': stop_price_precision,
            }
        )
        
        msg_sl_success = f"🔒 [Binance Bot] Stop-Loss active! Order ID: {sl_order.get('id')}"
        await send_custom_slack_message(msg_sl_success)
        
    except Exception as e:
        err_msg = f"❌ [Binance Bot] Error executing trade for {symbol}: {e}"
        await send_custom_slack_message(err_msg)
    finally:
        await exchange.close()


async def query_individual_price(symbol):
    """Fetch price for a single symbol from Binance to save weight and prevent rate limit blocks."""
    try:
        url = f"https://api.binance.com/api/v3/ticker/price?symbol={symbol}"
        res = await http_client.get(url)
        if res.status_code == 200:
            data = res.json()
            return float(data['price'])
    except Exception as e:
        print(f"⚠️ Error querying individual price for {symbol}: {e}")
    return None


async def check_uptrend_signals(conn):
    """Fetch watchlist coins, update state machines, evaluate consecutive timeframe changes above breakout level, check drawdowns and breakouts."""
    try:
        # 1. Fetch watchlist items for uptrend (near_52w_ath or near_ath)
        records = await conn.fetch(
            """
            SELECT DISTINCT crypto, signal_type, is_ath 
            FROM cryptos_watchlist 
            WHERE signal_type IN ($1, $2)
            """,
            SIGNAL_NEAR_52W_ATH,
            SIGNAL_NEAR_ATH
        )
        
        if not records:
            print("💤 No uptrend coins in watchlist at the moment.")
            return False

        active_symbols = []
        for record in records:
            symbol = record['crypto']
            signal_type = record['signal_type']
            base_symbol = symbol.replace('USDT', '')
            active_symbols.append(symbol)
            
            # Fetch breakout level if not cached
            if symbol not in breakout_prices:
                print(f"🔍 Fetching breakout level for {symbol} ({signal_type})...")
                if signal_type == SIGNAL_NEAR_ATH:
                    ath = await fetch_coingecko_ath(base_symbol)
                    if ath:
                        breakout_prices[symbol] = ath
                        print(f"   🏆 ATH for {symbol}: {ath}")
                    else:
                        # Fallback to 52W high
                        high_52w = await fetch_52w_high(symbol)
                        if high_52w:
                            breakout_prices[symbol] = high_52w
                            print(f"   🏆 (CG Fallback) 52W High for {symbol}: {high_52w}")
                else:
                    high_52w = await fetch_52w_high(symbol)
                    if high_52w:
                        breakout_prices[symbol] = high_52w
                        print(f"   ✅ 52W High for {symbol}: {high_52w}")

        if not active_symbols:
            return False

        # Synchronize states with database watchlist
        for symbol in list(coin_states.keys()):
            if symbol not in active_symbols:
                print(f"🗑️ Removing state tracking for {symbol} (no longer in DB watchlist)")
                del coin_states[symbol]

        for symbol in active_symbols:
            if symbol not in coin_states:
                breakout_price = breakout_prices.get(symbol)
                if breakout_price:
                    coin_states[symbol] = CoinState(symbol, breakout_price)
                    print(f"🚀 Initialized 1D tracking state for {symbol} | Breakout level: {breakout_price:.6f}")

        # 2. Optimize Querying: Fetch prices selectively to prevent API blocks
        current_time = time.time()
        symbols_to_query = []
        
        # Check if any coin is currently close and needs high-frequency scanning
        any_close = any(st.is_close for st in coin_states.values())
        
        for symbol in active_symbols:
            state = coin_states[symbol]
            # Always query if in close scan, or if 5 minutes have elapsed since last check
            if state.is_close or (current_time - state.last_queried >= NORMAL_INTERVAL) or not state.previous_price:
                symbols_to_query.append(symbol)

        current_prices = {}
        if symbols_to_query:
            if len(symbols_to_query) == len(active_symbols) and not any_close:
                # Standard bulk fetch (uses only 1 API weight)
                try:
                    url = "https://api.binance.com/api/v3/ticker/price"
                    res = await http_client.get(url)
                    if res.status_code == 200:
                        tickers = res.json()
                        ticker_map = {t['symbol']: float(t['price']) for t in tickers}
                        for sym in symbols_to_query:
                            if sym in ticker_map:
                                current_prices[sym] = ticker_map[sym]
                                coin_states[sym].last_queried = current_time
                except Exception as e:
                    print(f"⚠️ Error fetching bulk prices: {e}")
            else:
                # Fetch individually for only high-frequency active targets to avoid spamming the endpoint
                for sym in symbols_to_query:
                    price = await query_individual_price(sym)
                    if price:
                        current_prices[sym] = price
                        coin_states[sym].last_queried = current_time
                    await asyncio.sleep(0.1) # Safe spacing to prevent burst rate limit blocks

        # 3. Process states & evaluate multi-timeframe checks
        for record in records:
            symbol = record['crypto']
            current_price = current_prices.get(symbol)
            state = coin_states.get(symbol)
            
            if not current_price or not state:
                continue

            # Reset is_new_high flag for this run
            state.is_new_high = False

            # If the trade has already been entered, reset and bypass high-frequency scanning
            if symbol in triggered_symbols:
                state.is_close = False
                state.timeframe = '1D'
                state.consecutive_increases = 0
                state.previous_price = current_price
                continue

            # Update highest observed peak price (overall since tracking/reset)
            if current_price > state.highest_price:
                state.highest_price = current_price

            # Check drawdown condition: price drop of >= 7% from peak
            drawdown = 0.0
            if state.highest_price > 0:
                drawdown = (state.highest_price - current_price) / state.highest_price

            if drawdown >= 0.07:
                # Trigger reset to a larger timeframe stage
                old_tf = state.timeframe
                if state.timeframe == '5m':
                    state.timeframe = '1H'
                elif state.timeframe == '1H':
                    state.timeframe = '4H'
                elif state.timeframe == '4H':
                    state.timeframe = '1D'
                
                # Cache peak price before resetting
                peak_price = state.highest_price
                
                # Reset tracking history for this coin state
                state.consecutive_increases = 0
                state.is_close = False
                state.highest_price = current_price # Reset the peak to current price
                state.highest_price_above_breakout = 0.0 # Reset peak above breakout
                state.previous_price = None         # Reset interval history
                state.has_broken_out = False
                
                msg_drawdown = (
                    f"\a\a⚠️ [DRAWDOWN ALERT] {symbol} dropped {drawdown:.2%} from its last updated peak of {peak_price:.6f} to {current_price:.6f}!\n"
                    f"• Transitioning back from {old_tf} to {state.timeframe}."
                )
                await send_custom_slack_message(msg_drawdown)
                continue

            # Evaluate high-frequency promotion: price is close to breakout level
            gap_pct = (state.breakout_price - current_price) / state.breakout_price
            is_close_below = (0 <= gap_pct <= 0.015)
            is_above_breakout = current_price > state.breakout_price

            # Send Slack alert when within 4% gap from breakout level (once per approach)
            if 0 <= gap_pct <= 0.04:
                if not state.alerted_near_breakout:
                    state.alerted_near_breakout = True
                    msg_near = (
                        f"🔔 [NEAR BREAKOUT ZONE] {symbol} is within 4% of its breakout level ({state.breakout_price:.6f})!\n"
                        f"• Current Price: {current_price:.6f}\n"
                        f"• Gap: {gap_pct:.2%}"
                    )
                    await send_custom_slack_message(msg_near)
            else:
                if gap_pct > 0.04:
                    state.alerted_near_breakout = False

            # Update highest price above breakout and check if it's a new high
            if is_above_breakout:
                if current_price > state.highest_price_above_breakout:
                    state.highest_price_above_breakout = current_price
                    state.is_new_high = True
                    
                    # Activate Fast Scan since we broke to a new high!
                    if not state.is_close:
                        state.is_close = True
                        msg_close = (
                            f"⚡ [Fast Scan Activated] {symbol} broke to a new high above breakout level ({state.breakout_price:.6f})!\n"
                            f"• New High: {current_price:.6f}\n"
                            f"• Initiating continuous high-frequency monitoring."
                        )
                        await send_custom_slack_message(msg_close)
                else:
                    # Above breakout but NOT a new high
                    # We only keep Fast scan active if it's within the early breakout/pullback re-test zone (<= 1% above breakout)
                    if current_price <= state.breakout_price * 1.01:
                        if not state.is_close:
                            state.is_close = True
                            print(f"⚡ [Fast Scan Activated] {symbol} is in early breakout/pullback zone ({current_price:.6f} <= {state.breakout_price * 1.01:.6f})")
                    else:
                        # Otherwise, it's consolidating high above breakout. Switch to Normal scan!
                        if state.is_close:
                            state.is_close = False
                            print(f"➖ [Consolidation] {symbol} price ({current_price:.6f}) is below previous peak ({state.highest_price_above_breakout:.6f}). Switching to Normal scan.")
            else:
                # Below breakout:
                # Activate Fast Scan if within 1.5% below breakout
                if is_close_below:
                    if not state.is_close:
                        state.is_close = True
                        old_tf = state.timeframe
                        state.timeframe = '5m'
                        state.consecutive_increases = 0
                        state.previous_price = current_price
                        msg_close = (
                            f"⚡ [Continuous Scan Activated] {symbol} is within {gap_pct:.2%} (<= 1.5%) of breakout level ({state.breakout_price:.6f})!\n"
                            f"• Promoting directly from {old_tf} -> 5m and initiating continuous high-frequency monitoring."
                        )
                        await send_custom_slack_message(msg_close)
                else:
                    # Out of close zone below breakout
                    if state.is_close:
                        state.is_close = False
                        print(f"➖ {symbol} price drifted away from breakout ({gap_pct:.2%} > 1.5%). Exiting continuous scan.")

            # Check consecutive price increase ONLY when price is above the breakout price (ATH/52W)
            if is_above_breakout:
                state.has_broken_out = True  # Record that price has broken out
                
                if state.previous_price is not None:
                    if current_price > state.previous_price:
                        state.consecutive_increases += 1
                        print(f"📈 [Increase & Above Breakout] {symbol} price rose: {state.previous_price:.6f} -> {current_price:.6f} ({state.consecutive_increases}/2 consecutive in {state.timeframe})")
                    else:
                        state.consecutive_increases = 0
                        print(f"➖ [No Increase] {symbol} price did not rise: {state.previous_price:.6f} -> {current_price:.6f} (Resetting consecutive counter in {state.timeframe})")
                else:
                    print(f"📊 [First Price Record] {symbol} starts at {current_price:.6f} (above breakout) in timeframe {state.timeframe}")
                
                # Update previous interval price
                state.previous_price = current_price
            else:
                state.consecutive_increases = 0
                state.previous_price = current_price # still update previous price
                print(f"🛑 [Below Breakout] {symbol} current price {current_price:.6f} is below breakout level {state.breakout_price:.6f} (Resetting consecutive counter in {state.timeframe})")

            # Evaluate timeframe transition if price has increased consecutively 2 times while above breakout
            if state.consecutive_increases >= 2:
                old_tf = state.timeframe
                if state.timeframe == '1D':
                    state.timeframe = '4H'
                    state.consecutive_increases = 0
                    state.previous_price = None  # require new history for next stage
                    msg_tf = f"⚡ [Timeframe Promotion] {symbol} rose above breakout 2 times consecutively! Moving from 1D -> 4H timeframe."
                    await send_custom_slack_message(msg_tf)
                elif state.timeframe == '4H':
                    state.timeframe = '1H'
                    state.consecutive_increases = 0
                    state.previous_price = None
                    msg_tf = f"⚡ [Timeframe Promotion] {symbol} rose above breakout 2 times consecutively! Moving from 4H -> 1H timeframe."
                    await send_custom_slack_message(msg_tf)
                elif state.timeframe == '1H':
                    state.timeframe = '5m'
                    state.consecutive_increases = 0
                    state.previous_price = None
                    msg_tf = f"⚡ [Timeframe Promotion] {symbol} rose above breakout 2 times consecutively! Moving from 1H -> 5m timeframe."
                    await send_custom_slack_message(msg_tf)

            # Check for live breakout trigger only if the state is in the '5m' stage
            if state.timeframe == '5m':
                if symbol not in triggered_symbols:
                    if is_above_breakout:
                        # Direct breakout entry (if within 3% limit)
                        if current_price >= state.breakout_price * 1.03:
                            print(f"🛑 [Chasing Avoided] {symbol} price {current_price:.6f} is >= 3% above breakout level {state.breakout_price:.6f}. Waiting for pullback to breakout level.")
                        else:
                            msg_breakout = (
                                f"🔥 [BREAKOUT CONFIRMED ACROSS ALL TIMEFRAMES] {symbol} is above its breakout level of {state.breakout_price:.6f} in the 5m timeframe!\n"
                                f"• Current Price: {current_price:.6f}\n"
                                f"• Initiating live trade and setting stop loss..."
                            )
                            await send_custom_slack_message(msg_breakout)
                            await execute_binance_trade(symbol, current_price, state.breakout_price)
                    
                    elif state.has_broken_out:
                        # Pullback entry: if it has broken out previously, and has now returned to the breakout level (ATH/52W)
                        # Price range: within -0.5% to +1% of breakout price
                        pullback_min = state.breakout_price * 0.995
                        pullback_max = state.breakout_price * 1.01
                        
                        if pullback_min <= current_price <= pullback_max:
                            msg_pullback = (
                                f"🔄 [PULLBACK SUPPORT RE-TEST] {symbol} has successfully pulled back to its breakout level of {state.breakout_price:.6f}!\n"
                                f"• Current Price: {current_price:.6f} (within re-test range of {pullback_min:.6f} - {pullback_max:.6f})\n"
                                f"• Initiating live pullback trade on support re-test..."
                            )
                            await send_custom_slack_message(msg_pullback)
                            await execute_binance_trade(symbol, current_price, state.breakout_price)

        # 4. Print beautiful status log in console for easy monitoring
        print("\n================= TRACKING STATUS =================")
        for s, st in coin_states.items():
            curr = current_prices.get(s, 0.0)
            
            # Skip showing above-breakout coins if they are consolidating (not a new high)
            if curr > st.breakout_price and not st.is_new_high:
                continue

            if curr <= 0:
                status_text = "below (Waiting for price...)"
                beep_char = ""
            elif curr > st.breakout_price:
                status_text = f"ABOVE Breakout (New Peak: {st.highest_price_above_breakout:.6f})"
                beep_char = "\a\a"  # Terminal sound warning for ABOVE Breakout!
            else:
                status_text = f"below ({((st.breakout_price - curr)/st.breakout_price)*100:.2f}% gap)"
                beep_char = ""
            
            scan_mode = "FAST-1m" if st.is_close else "NORMAL-300s"
            print(f"{beep_char}• {s:<10} | Stage: {st.timeframe:<3} | Consec: {st.consecutive_increases}/2 | Peak: {st.highest_price:<10.6f} | Curr: {curr:<10.6f} | Breakout: {st.breakout_price:<10.6f} | Mode: {scan_mode:<11} | Status: {status_text}")
        print("===================================================\n")

        # Determine the next loop sleep duration
        return any(st.is_close for st in coin_states.values())

    except Exception as e:
        print(f"⚠️ Error inside check_uptrend_signals: {e}")
        return False


async def main():
    await send_custom_slack_message("🤖 Multi-Timeframe Confirmation & High-Frequency Breakout Bot initialized.")
    
    try:
        while True:
            conn = None
            is_fast_mode = False
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

                is_fast_mode = await check_uptrend_signals(conn)

            except Exception as e:
                print(f"⚠️ Connection error or main loop exception: {e}")
            finally:
                if conn:
                    await conn.close()
            
            # Sleep based on active monitoring mode
            sleep_duration = FAST_INTERVAL if is_fast_mode else NORMAL_INTERVAL
            if is_fast_mode:
                print(f"⏳ Fast scan active (price close to breakout). Waiting {FAST_INTERVAL} seconds...")
            else:
                print(f"⏳ Normal scan active. Waiting {NORMAL_INTERVAL // 60} minutes...")
            
            await asyncio.sleep(sleep_duration)
            
    finally:
        # Gracefully shut down HTTP connection pool on termination
        await http_client.aclose()


if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\n👋 Bot stopped by user.")
