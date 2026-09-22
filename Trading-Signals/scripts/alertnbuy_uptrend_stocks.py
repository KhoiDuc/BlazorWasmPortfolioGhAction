import asyncio
import asyncpg
import time
import httpx
import os
from datetime import datetime
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Scan Intervals (Seconds)
NORMAL_INTERVAL = 300  # 5 minutes for standard monitoring
FAST_INTERVAL = 60     # 1 minute for continuous high-frequency breakout monitoring

class StockState:
    def __init__(self, symbol, breakout_price):
        self.symbol = symbol
        self.breakout_price = breakout_price
        self.timeframe = '1D'               # Timeframe stages: '1D' -> '4H' -> '1H' -> '5m'
        self.previous_price = None          # Last interval price in current timeframe stage
        self.highest_price = 0.0            # Peak observed price since tracking started/reset
        self.consecutive_increases = 0      # Count of consecutive price increases in current stage
        self.has_broken_out = False         # Tracks if a breakout above 52W High has occurred
        self.is_close = False               # True if price is within 1.5% of breakout
        self.last_queried = 0.0             # Timestamp of the last price query for this stock
        self.alerted_near_breakout = False  # Tracks if Slack warning for 4% gap has been sent

# Global State caches
breakout_prices = {}     # Caches breakout level for each symbol: { 'SSI': price }
stock_states = {}         # Maps symbol to its StockState object: { 'SSI': StockState }
triggered_symbols = set() # Set of symbols already alerted to avoid double entry

# Global HTTP client to reuse connections and prevent IP blocks for DNSE Entrade API
http_client = httpx.AsyncClient(
    timeout=10.0,
    limits=httpx.Limits(max_connections=50, max_keepalive_connections=20),
    headers={"User-Agent": "VN-Stock-Uptrend-Bot/1.0"}
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


async def fetch_vn_stock_price(client, symbol):
    """Fetch the current price of a VN stock from DNSE Entrade API."""
    try:
        url = f"https://services.entrade.com.vn/dnse-financial-product/securities/{symbol}"
        res = await client.get(url, timeout=10.0)
        if res.status_code == 200:
            data = res.json()
            basic_price = data.get('basicPrice')
            if basic_price is not None:
                return float(basic_price)
    except Exception as e:
        print(f"⚠️ Error fetching price for VN stock {symbol}: {e}")
    return None


async def check_uptrend_signals(conn):
    """Fetch watchlist stocks, update state machines, evaluate consecutive timeframe changes above breakout level, check drawdowns and breakouts."""
    try:
        # 1. Fetch potential stocks from symbols_watchlist (near_52w_ath)
        records = await conn.fetch(
            """
            SELECT DISTINCT symbol, highest_price 
            FROM symbols_watchlist 
            WHERE signal_type = $1 AND highest_price IS NOT NULL
            """,
            'near_52w_ath'
        )
        
        if not records:
            print("💤 No VN stocks with near_52w_ath signal in watchlist.")
            return False

        active_symbols = []
        for record in records:
            symbol = record['symbol']
            breakout_price = float(record['highest_price'])
            active_symbols.append(symbol)
            breakout_prices[symbol] = breakout_price

        # Synchronize states with database watchlist
        for symbol in list(stock_states.keys()):
            if symbol not in active_symbols:
                print(f"🗑️ Removing state tracking for VN stock {symbol} (no longer in DB watchlist)")
                del stock_states[symbol]

        # Initialize tracking states for newly added stocks
        for symbol in active_symbols:
            if symbol not in stock_states:
                breakout_price = breakout_prices.get(symbol)
                if breakout_price:
                    stock_states[symbol] = StockState(symbol, breakout_price)
                    print(f"🚀 Initialized 1D tracking state for VN stock {symbol} | Breakout level: {breakout_price:.2f} VND")

        # 2. Optimize Querying: Fetch prices selectively to prevent DNSE API blocking
        current_time = time.time()
        symbols_to_query = []
        
        # Check if any VN stock is currently close and needs high-frequency scanning
        any_close = any(st.is_close for st in stock_states.values())
        
        for symbol in active_symbols:
            state = stock_states[symbol]
            # Always query if in close scan, or if 5 minutes have elapsed since last check
            if state.is_close or (current_time - state.last_queried >= NORMAL_INTERVAL) or not state.previous_price:
                symbols_to_query.append(symbol)

        current_prices = {}
        if symbols_to_query:
            # Query sequentially with connection reuse and tiny spacing delay
            for sym in symbols_to_query:
                price = await fetch_vn_stock_price(http_client, sym)
                if price:
                    current_prices[sym] = price
                    stock_states[sym].last_queried = current_time
                await asyncio.sleep(0.1) # Safe rate limit pacing to prevent blocking

        # 3. Process states & evaluate multi-timeframe checks
        for record in records:
            symbol = record['symbol']
            current_price = current_prices.get(symbol)
            state = stock_states.get(symbol)
            
            if not current_price or not state:
                continue

            # If the alert has already been sent, reset and bypass high-frequency scanning
            if symbol in triggered_symbols:
                state.is_close = False
                state.timeframe = '1D'
                state.consecutive_increases = 0
                state.previous_price = current_price
                continue

            # Update highest observed peak price
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
                
                # Reset tracking history for this stock state
                state.consecutive_increases = 0
                state.is_close = False
                state.highest_price = current_price # Reset the peak to current price
                state.previous_price = None         # Reset interval history
                state.has_broken_out = False
                
                msg_drawdown = (
                    f"📉 [VN Stock Drawdown] {symbol} dropped {drawdown:.2%} from its peak of {state.highest_price:.2f} VND to {current_price:.2f} VND!\n"
                    f"• Transitioning back from {old_tf} to {state.timeframe}."
                )
                await send_custom_slack_message(msg_drawdown)
                continue

            # Evaluate high-frequency promotion: price is within 1.5% of breakout level (52W High)
            gap_pct = (state.breakout_price - current_price) / state.breakout_price
            is_close_now = (0 <= gap_pct <= 0.015)
            is_above_breakout = current_price > state.breakout_price

            # Send Slack alert when within 4% gap from 52W High breakout level (once per approach)
            if 0 <= gap_pct <= 0.04:
                if not state.alerted_near_breakout:
                    state.alerted_near_breakout = True
                    msg_near = (
                        f"🔔 [VN STOCK NEAR BREAKOUT] {symbol} is within 4% of its 52W High ({state.breakout_price:.2f} VND)!\n"
                        f"• Current Price: {current_price:.2f} VND\n"
                        f"• Gap: {gap_pct:.2%}"
                    )
                    await send_custom_slack_message(msg_near)
            else:
                if gap_pct > 0.04:
                    state.alerted_near_breakout = False

            # If within 1.5% of breakout, promote directly to the continuous 5m scan stage!
            if is_close_now and not state.is_close:
                state.is_close = True
                old_tf = state.timeframe
                state.timeframe = '5m'
                state.consecutive_increases = 0
                state.previous_price = current_price
                msg_close = (
                    f"⚡ [VN Stock Continuous Scan] {symbol} is within {gap_pct:.2%} (<= 1.5%) of 52W High ({state.breakout_price:.2f})!\n"
                    f"• Promoting directly from {old_tf} -> 5m and initiating continuous high-frequency monitoring."
                )
                await send_custom_slack_message(msg_close)

            # Check consecutive price increase ONLY when price is above the breakout price (52W High)
            if is_above_breakout:
                state.has_broken_out = True  # Record that price has broken out
                
                if state.previous_price is not None:
                    if current_price > state.previous_price:
                        state.consecutive_increases += 1
                        print(f"📈 [VN Stock Increase] {symbol} price rose: {state.previous_price:.2f} -> {current_price:.2f} ({state.consecutive_increases}/2 consecutive in {state.timeframe})")
                    else:
                        state.consecutive_increases = 0
                        print(f"➖ [VN Stock No Increase] {symbol} price did not rise: {state.previous_price:.2f} -> {current_price:.2f} (Resetting consecutive counter in {state.timeframe})")
                else:
                    print(f"📊 [VN Stock First Price] {symbol} starts at {current_price:.2f} (above breakout) in timeframe {state.timeframe}")
                
                # Update previous interval price
                state.previous_price = current_price
            else:
                # If it fell below 1.5% gap again, exit continuous scan mode
                if not is_close_now and state.is_close:
                    state.is_close = False
                    print(f"➖ {symbol} price drifted away from breakout ({gap_pct:.2%} > 1.5%). Exiting continuous scan.")
                
                state.consecutive_increases = 0
                state.previous_price = current_price # still update previous price
                print(f"🛑 [VN Stock Below Breakout] {symbol} current price {current_price:.2f} is below 52W high {state.breakout_price:.2f} (Resetting consecutive counter in {state.timeframe})")

            # Evaluate timeframe transition if price has increased consecutively 2 times while above breakout
            if state.consecutive_increases >= 2:
                old_tf = state.timeframe
                if state.timeframe == '1D':
                    state.timeframe = '4H'
                    state.consecutive_increases = 0
                    state.previous_price = None  # require new history for next stage
                    msg_tf = f"⚡ [VN Stock Timeframe Shift] {symbol} rose above breakout 2 times consecutively! Moving from 1D -> 4H timeframe."
                    await send_custom_slack_message(msg_tf)
                elif state.timeframe == '4H':
                    state.timeframe = '1H'
                    state.consecutive_increases = 0
                    state.previous_price = None
                    msg_tf = f"⚡ [VN Stock Timeframe Shift] {symbol} rose above breakout 2 times consecutively! Moving from 4H -> 1H timeframe."
                    await send_custom_slack_message(msg_tf)
                elif state.timeframe == '1H':
                    state.timeframe = '5m'
                    state.consecutive_increases = 0
                    state.previous_price = None
                    msg_tf = f"⚡ [VN Stock Timeframe Shift] {symbol} rose above breakout 2 times consecutively! Moving from 1H -> 5m timeframe."
                    await send_custom_slack_message(msg_tf)

            # Check for breakout/pullback triggers only if the state is in the '5m' stage
            if state.timeframe == '5m':
                if symbol not in triggered_symbols:
                    if is_above_breakout:
                        # Direct breakout entry (if within 3% limit)
                        if current_price >= state.breakout_price * 1.03:
                            print(f"🛑 [VN Stock Chasing Avoided] {symbol} price {current_price:.2f} is >= 3% above 52W High {state.breakout_price:.2f}. Waiting for pullback to breakout level.")
                        else:
                            triggered_symbols.add(symbol)
                            msg_breakout = (
                                f"🚨 [VN STOCK BREAKOUT CONFIRMED] {symbol} has successfully broken above its 52W High of {state.breakout_price:.2f} VND across all timeframes!\n"
                                f"• Current Price: {current_price:.2f} VND\n"
                                f"• Alert: No trading is performed for VN stocks, Slack notification only."
                            )
                            await send_custom_slack_message(msg_breakout)
                    
                    elif state.has_broken_out:
                        # Pullback entry: if it has broken out previously, and has now returned to the 52W high
                        # Price range: within -0.5% to +1% of breakout price
                        pullback_min = state.breakout_price * 0.995
                        pullback_max = state.breakout_price * 1.01
                        
                        if pullback_min <= current_price <= pullback_max:
                            triggered_symbols.add(symbol)
                            msg_pullback = (
                                f"🔄 [VN STOCK PULLBACK SUPPORT RE-TEST] {symbol} has successfully pulled back to its 52W High of {state.breakout_price:.2f} VND!\n"
                                f"• Current Price: {current_price:.2f} VND (within re-test range of {pullback_min:.2f} - {pullback_max:.2f})\n"
                                f"• Alert: No trading is performed for VN stocks, Slack notification only."
                            )
                            await send_custom_slack_message(msg_pullback)

        # 4. Print status log in console for easy monitoring
        print("\n================= TRACKING STATUS (VN STOCK) =================")
        for s, st in stock_states.items():
            curr = current_prices.get(s, 0.0)
            if curr <= 0:
                status_text = "below (Waiting for price...)"
                beep_char = ""
            elif curr > st.breakout_price:
                status_text = "ABOVE 52W High"
                beep_char = "\a\a"  # Terminal sound warning for ABOVE 52W High!
            else:
                status_text = f"below ({((st.breakout_price - curr)/st.breakout_price)*100:.2f}% gap)"
                beep_char = ""
            
            scan_mode = "FAST-1m" if st.is_close else "NORMAL-300s"
            print(f"{beep_char}• {s:<10} | Stage: {st.timeframe:<3} | Consec: {st.consecutive_increases}/2 | Peak: {st.highest_price:<10.2f} | Curr: {curr:<10.2f} | 52W High: {st.breakout_price:<10.2f} | Mode: {scan_mode:<11} | Status: {status_text}")
        print("==============================================================\n")

        # Determine next loop sleep duration
        return any(st.is_close for st in stock_states.values())

    except Exception as e:
        print(f"⚠️ Error inside check_uptrend_signals: {e}")
        return False


async def main():
    await send_custom_slack_message("🤖 VN Stock Multi-Timeframe Confirmation Monitor Bot initialized.")
    
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
