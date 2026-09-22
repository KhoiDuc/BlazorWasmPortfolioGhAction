import asyncio
import asyncpg
import httpx
import os
import re
from datetime import datetime
from dotenv import load_dotenv

# Các hằng số tín hiệu
SIGNAL_NEAR_52W_HIGH = 'near_52w_high'
SIGNAL_EMA9_ABOVE_EMA21 = 'ema9_above_ema21'

# Lấy top 25 hợp đồng có thanh khoản cao nhất để đảm bảo an toàn (Tier 1 & 2)
TOP_FUTURES_LIMIT = 25

# ================================
#  COINGECKO MARKET CAP HELPERS
# ================================
async def get_coingecko_market_caps():
    """Lấy danh sách vốn hoá của top 250 coin từ CoinGecko để làm map tra cứu"""
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
                # Tạo map từ base_symbol (viết hoa) -> market_cap
                return {c.get('symbol', '').upper(): float(c.get('market_cap') or 0) for c in coins}
            else:
                print(f"   ⚠️  Không thể lấy vốn hoá từ CoinGecko: status_code={res.status_code}")
    except Exception as e:
        print(f"⚠️ Lỗi fetch CoinGecko market caps: {e}")
    return {}

def get_base_symbol(symbol):
    """Trích xuất symbol cơ bản từ tên hợp đồng Futures (ví dụ BTCUSDT -> BTC, 1000SHIBUSDT -> SHIB)"""
    if symbol.endswith('USDT'):
        base = symbol[:-4]
    else:
        base = symbol
    
    # Loại bỏ các số ở đầu (như 1000, 1000000, vv.)
    base = re.sub(r'^\d+', '', base)
    return base.upper()

load_dotenv()

# ================================
#  INDICATOR FUNCTIONS
# ================================
def _calc_ema(closes, period):
    if len(closes) < period: return None
    k = 2.0 / (period + 1)
    ema = sum(closes[:period]) / period
    for price in closes[period:]:
        ema = price * k + ema * (1 - k)
    return ema

def check_ema9_above_ema21(closes):
    if not closes or len(closes) < 21: return False
    ema9 = _calc_ema(closes, 9)
    ema21 = _calc_ema(closes, 21)
    if ema9 is None or ema21 is None: return False
    return ema9 >= ema21

# ================================
#  BINANCE FUTURES (FAPI) FUNCTIONS
# ================================
async def get_top_futures_by_volume(limit=TOP_FUTURES_LIMIT):
    """Lấy danh sách các hợp đồng USDT-M Perpetual có Volume 24h cao nhất"""
    try:
        url = "https://fapi.binance.com/fapi/v1/ticker/24hr"
        async with httpx.AsyncClient(timeout=10) as client:
            res = await client.get(url)
            if res.status_code != 200: return []
            
            data = res.json()
            # Lọc chỉ lấy cặp USDT và bỏ qua các token bị giới hạn
            usdt_pairs = [p for p in data if p['symbol'].endswith('USDT') and '_' not in p['symbol']]
            
            # Sắp xếp theo quoteVolume (Khối lượng giao dịch bằng USDT) giảm dần
            usdt_pairs.sort(key=lambda x: float(x['quoteVolume']), reverse=True)
            
            symbols = [pair['symbol'] for pair in usdt_pairs[:limit]]
            print(f"🔹 Đã lấy Top {len(symbols)} Futures theo Volume: {', '.join(symbols[:5])}...")
            return symbols
    except Exception as e:
        print(f"⚠️ Lỗi khi lấy danh sách Futures: {e}")
        return []

async def fetch_futures_daily_data(symbol, days=365):
    """Lấy dữ liệu nến 1D từ Binance Futures"""
    try:
        url = "https://fapi.binance.com/fapi/v1/klines"
        params = {"symbol": symbol, "interval": "1d", "limit": days}
        async with httpx.AsyncClient(timeout=10) as client:
            res = await client.get(url, params=params)
            if res.status_code != 200: return None
            
            klines = res.json()
            if not klines: return None
            
            # Format: [open_time, open, high, low, close, volume...]
            closes = [float(k[4]) for k in klines]
            highs = [float(k[2]) for k in klines]
            return {"closes": closes, "highs": highs}
    except Exception as e:
        print(f"⚠️ Lỗi fetch data cho {symbol}: {e}")
        return None

# ================================
#  SCAN LOGIC
# ================================
async def scan_futures():
    symbols = await get_top_futures_by_volume()
    
    print("🔹 Đang tải dữ liệu vốn hoá từ CoinGecko...")
    market_caps = await get_coingecko_market_caps()
    
    results = []
    
    print("🔹 Bắt đầu quét dữ liệu kĩ thuật...")
    for symbol in symbols:
        base_symbol = get_base_symbol(symbol)
        mcap = market_caps.get(base_symbol, 0.0)
        
        # Chỉ quét các hợp đồng có vốn hoá lớn hơn $2B (2,000,000,000 USD)
        if mcap < 2000000000.0:
            print(f"   ⏩ {symbol} bỏ qua do vốn hoá thấp (${mcap:,.0f} USD < $2B)")
            continue
            
        data = await fetch_futures_daily_data(symbol, 365)
        if not data: continue
        
        closes = data["closes"]
        highs = data["highs"]
        current_price = closes[-1]
        max_52w = max(highs)
        
        # Check EMA
        if check_ema9_above_ema21(closes[-50:]): # Chỉ cần 50 nến gần nhất để tính EMA21
            print(f"   📈 {symbol}: EMA9 >= EMA21 ✓")
            results.append({
                "symbol": symbol,
                "signal_type": SIGNAL_EMA9_ABOVE_EMA21,
                "highest_price": max_52w,
                "market_cap": mcap
            })
            
        # Check 52W High (Giá cách đỉnh dưới 10%)
        if max_52w > 0:
            diff = (max_52w - current_price) / max_52w
            if diff <= 0.10:
                print(f"   🔥 {symbol}: Gần đỉnh 52W (Cách {diff:.2%}) ✓")
                results.append({
                    "symbol": symbol,
                    "signal_type": SIGNAL_NEAR_52W_HIGH,
                    "highest_price": max_52w,
                    "market_cap": mcap
                })
                
        await asyncio.sleep(0.1) # Tránh Rate Limit của Binance
        
    return results

# ================================
#  MAIN DATABASE UPDATE
# ================================
async def main():
    conn = None
    try:
        conn = await asyncpg.connect(
            user=os.environ.get('DB_USER'),
            password=os.environ.get('DB_PASSWORD'),
            database=os.environ.get('DB_NAME'),
            host=os.environ.get('DB_HOST'),
            port=int(os.environ.get('DB_PORT'))
        )
        
        signals = await scan_futures()
        
        if signals:
            print(f"🔹 Cập nhật {len(signals)} tín hiệu vào Database...")
            # Giả định bạn tạo một bảng mới tên là 'futures_watchlist' để tách biệt với Spot
            await conn.execute("DELETE FROM futures_watchlist")
            await conn.executemany(
                """
                INSERT INTO futures_watchlist (symbol, signal_type, highest_price, market_cap)
                VALUES ($1, $2, $3, $4)
                """,
                [(d["symbol"], d["signal_type"], d["highest_price"], d["market_cap"]) for d in signals]
            )
            print("✅ Cập nhật Database thành công!")
            
    except Exception as e:
        print("Lỗi hệ thống:", e)
    finally:
        if conn: await conn.close()

if __name__ == "__main__":
    asyncio.run(main())