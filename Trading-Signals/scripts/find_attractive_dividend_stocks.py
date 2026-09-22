import asyncio
import os
import sys
import random
from dotenv import load_dotenv
from curl_cffi.requests import AsyncSession, RequestsError

# Set standard output to UTF-8 to prevent encoding errors on Windows
sys.stdout.reconfigure(encoding='utf-8')

# Configuration Constants for Filtering
MAX_PE = 12.0               # Attractive P/E <= 12
MAX_PB = 1.5                # Attractive P/B <= 1.5
MIN_DIVIDEND_YIELD = 0.05   # Attractive Dividend Yield >= 5% (gross)
MIN_TRADE_VOLUME = 50000    # Daily volume >= 50,000 to ensure liquidity
CONCURRENCY_LIMIT = 8       # Reduced concurrency to be gentler on the API
MAX_RETRIES = 5             # Maximum retries on rate limits (429)

# File paths
ENV_FILE = os.path.join(os.path.dirname(__file__), '.env')
load_dotenv(ENV_FILE)

async def get_tcbs_token():
    """Login to TCBS and obtain authentication token."""
    login_url = "https://apipub.tcbs.com.vn/authen/v1/login"
    username = os.environ.get('TCBS_USR')
    password = os.environ.get('TCBS_PWD')
    
    if not username or not password:
        print("⚠️ Cảnh báo: Không tìm thấy TCBS_USR hoặc TCBS_PWD trong file .env")
        return None
    
    headers = {
        "User-Agent": "Mozilla/5.0",
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
            response = await client.post(login_url, json=login_data, headers=headers, timeout=10.0)
            if response.status_code == 200:
                result = response.json()
                return result.get('token')
    except Exception as e:
        print(f"⚠️ Cảnh báo: Đăng nhập TCBS thất bại: {e}")
    return None

async def get_all_listed_stocks():
    """Fetch all listed stock tickers from VNDirect."""
    url = "https://api-finfo.vndirect.com.vn/v4/stocks?q=type:STOCK~status:LISTED&fields=code&size=3000"
    headers = {
        "User-Agent": "Mozilla/5.0"
    }
    try:
        async with AsyncSession() as client:
            response = await client.get(url, headers=headers, timeout=15.0)
            response.raise_for_status()
            data = response.json().get('data', [])
            return [item['code'] for item in data if len(item.get('code', '')) == 3]
    except Exception as e:
        print(f"❌ Lỗi lấy danh sách cổ phiếu từ VNDirect: {e}")
        return []

async def fetch_with_retry(client, url, headers, semaphore, label):
    """Fetch helper that handles rate-limiting (429) using exponential backoff."""
    async with semaphore:
        backoff = 1.5
        for attempt in range(MAX_RETRIES):
            try:
                response = await client.get(url, headers=headers, timeout=12.0)
                if response.status_code == 200:
                    return response.json()
                elif response.status_code == 429:
                    # Handle rate limiting with backoff
                    sleep_time = backoff * 2.0 + random.uniform(0.2, 0.8)
                    retry_after = response.headers.get("Retry-After")
                    if retry_after and retry_after.isdigit():
                        sleep_time = int(retry_after) + 0.5
                    print(f"⚠️ Bị giới hạn tần suất (429) khi quét mã {label}. Thử lại sau {sleep_time:.1f} giây...")
                    await asyncio.sleep(sleep_time)
                    backoff *= 1.5
                    continue
                elif response.status_code == 404:
                    return None
                else:
                    # Other HTTP error codes
                    return None
            except (RequestsError, asyncio.TimeoutError):
                await asyncio.sleep(backoff)
                backoff *= 1.5
    return None

async def fetch_stock_ratio(client, symbol, token, semaphore):
    """Fetch stockratio details from TCBS with retry support."""
    url = f"https://apiextaws.tcbs.com.vn/tcanalysis/v1/ticker/{symbol}/stockratio"
    headers = {
        "Accept": "application/json",
        "Referer": "https://tcinvest.tcbs.com.vn/"
    }
    if token:
        headers["Authorization"] = f"Bearer {token}"
    return await fetch_with_retry(client, url, headers, semaphore, symbol)

async def fetch_price_volatility(client, symbol, token, semaphore):
    """Fetch 52-week price range details from TCBS with retry support."""
    url = f"https://apiextaws.tcbs.com.vn/tcanalysis/v1/ticker/{symbol}/price-volatility"
    headers = {
        "Accept": "application/json",
        "Referer": "https://tcinvest.tcbs.com.vn/"
    }
    if token:
        headers["Authorization"] = f"Bearer {token}"
    return await fetch_with_retry(client, url, headers, semaphore, symbol)

def make_price_position_bar(low, high, current, width=10):
    """Create a visual slider representing the price relative to its 52W range."""
    if low is None or high is None or current is None or high == low:
        return "N/A"
    pos = (current - low) / (high - low)
    pos = max(0.0, min(1.0, pos))
    star_idx = int(pos * (width - 1))
    bar = ["-"] * width
    bar[star_idx] = "★"
    return "[" + "".join(bar) + "]"

async def main():
    print("🚀 ĐANG KHỞI CHẠY BỘ LỌC CỔ PHIẾU CỔ TỨC HẤP DẪN...")
    print(f"Tiêu chí lọc:")
    print(f"  - P/E <= {MAX_PE}")
    print(f"  - P/B <= {MAX_PB}")
    print(f"  - Tỷ suất cổ tức >= {MIN_DIVIDEND_YIELD * 100:.1f}%")
    print(f"  - Khối lượng giao dịch tối thiểu >= {MIN_TRADE_VOLUME:,} cp")
    print("-" * 75)
    
    # 1. Login to get TCBS token
    token = await get_tcbs_token()
    if token:
        print("🔐 Đăng nhập TCBS thành công.")
    else:
        print("⚠️ Tiếp tục mà không có token đăng nhập...")

    # 2. Get listed stocks
    print("📋 Đang lấy danh sách mã cổ phiếu từ VNDirect...")
    stocks = await get_all_listed_stocks()
    if not stocks:
        print("❌ Không lấy được danh sách cổ phiếu. Thoát chương trình.")
        return
    print(f"Đã tìm thấy {len(stocks)} mã cổ phiếu niêm yết.")

    # 3. Fetch ratios in parallel
    print("⏳ Đang quét dữ liệu tài chính (ratios) từ TCBS (quá trình có thể mất khoảng 1-2 phút)...")
    semaphore = asyncio.Semaphore(CONCURRENCY_LIMIT)
    
    candidate_stocks = []
    
    async with AsyncSession(impersonate="chrome") as client:
        tasks = [fetch_stock_ratio(client, symbol, token, semaphore) for symbol in stocks]
        
        # Gather results with progress tracking
        completed = 0
        total = len(tasks)
        
        for future in asyncio.as_completed(tasks):
            result = await future
            completed += 1
            if completed % 200 == 0 or completed == total:
                print(f"  Tiến độ: {completed}/{total} cổ phiếu đã quét...")
                
            if result:
                ticker = result.get('ticker')
                pe = result.get('priceToEarning')
                pb = result.get('priceToBook')
                div = result.get('dividend')
                vol = result.get('tradeVolume')
                bvps = result.get('bookValuePerShare')
                eps = result.get('earningPerShare')
                
                # Check filtering criteria
                if (pe and pe > 0 and pe <= MAX_PE and
                    pb and pb > 0 and pb <= MAX_PB and
                    div and div >= MIN_DIVIDEND_YIELD and
                    vol and vol >= MIN_TRADE_VOLUME):
                    
                    # Estimate stock price
                    price = None
                    if bvps and pb:
                        price = bvps * pb
                    elif eps and pe:
                        price = eps * pe
                    
                    candidate_stocks.append({
                        'ticker': ticker,
                        'pe': pe,
                        'pb': pb,
                        'dividend_gross': div,
                        'dividend_net': div * 0.95, # 5% tax deduction
                        'volume': vol,
                        'estimated_price': price,
                    })
                    
    print(f"\n🎯 Đã tìm thấy {len(candidate_stocks)} cổ phiếu tiềm năng đạt tiêu chuẩn ban đầu.")
    if not candidate_stocks:
        print("ℹ️ Không tìm thấy cổ phiếu nào khớp tiêu chí lọc.")
        return

    # 4. Fetch price-volatility details for the candidates to get 52W range
    print("⏳ Đang lấy thông tin vùng giá 52 tuần cho các cổ phiếu đạt tiêu chuẩn...")
    
    # Reset semaphore for candidates fetch
    async with AsyncSession(impersonate="chrome") as client:
        tasks = [fetch_price_volatility(client, s['ticker'], token, semaphore) for s in candidate_stocks]
        volatility_results = await asyncio.gather(*tasks)
        
        # Merge 52W range data
        vol_map = {}
        for r in volatility_results:
            if r:
                vol_map[r['ticker']] = {
                    'high_52w': r.get('highestPrice'),
                    'low_52w': r.get('lowestPrice'),
                    'high_pct': r.get('highestPricePercent')
                }
                
        for s in candidate_stocks:
            ticker = s['ticker']
            vdata = vol_map.get(ticker, {})
            s['high_52w'] = vdata.get('high_52w')
            s['low_52w'] = vdata.get('low_52w')
            
            # Recalculate price if we have 52w range info
            high_pct = vdata.get('high_pct')
            if s['high_52w'] and high_pct is not None:
                s['price'] = s['high_52w'] * (1 + high_pct)
            else:
                s['price'] = s['estimated_price']
                
            # Estimated dividend values in VND
            if s['price'] and s['dividend_gross']:
                s['div_value_gross'] = s['price'] * s['dividend_gross']
                s['div_value_net'] = s['div_value_gross'] * 0.95
            else:
                s['div_value_gross'] = None
                s['div_value_net'] = None

    # Print results
    print("\n" + "=" * 120)
    print(" DANH SÁCH CỔ PHIẾU ĐỊNH GIÁ RẺ - CỔ TỨC CAO HẤP DẪN (Đã trừ 5% thuế TNCN cổ tức)")
    print("=" * 120)
    
    header_format = "{:<6} | {:<12} | {:<5} | {:<5} | {:<12} | {:<12} | {:<12} | {:<12} | {:<15} | {:<10}"
    row_format = "{:<6} | {:<12,} | {:<5.1f} | {:<5.1f} | {:<12.1f}% | {:<12.1f}% | {:<12,} | {:<12,} | {:<15} | {:<10,}"
    
    print(header_format.format(
        "Mã CP", "Thị giá (đ)", "P/E", "P/B", "C.Tức gốc/n", "C.Tức thực/n", "C.Tức VND", "Thực nhận", "Vùng giá 52T", "KLGD ngày"
    ))
    print("-" * 120)
    
    # Sort candidate stocks by Net Dividend Yield descending
    candidate_stocks.sort(key=lambda x: x['dividend_net'], reverse=True)
    
    for s in candidate_stocks:
        # Determine price position bar
        pos_bar = make_price_position_bar(s.get('low_52w'), s.get('high_52w'), s.get('price'))
        
        # Defaults for missing data formatting
        price = s.get('price') or 0
        div_gross_vnd = s.get('div_value_gross') or 0
        div_net_vnd = s.get('div_value_net') or 0
        
        print(row_format.format(
            s['ticker'],
            round(price),
            s['pe'],
            s['pb'],
            s['dividend_gross'] * 100,
            s['dividend_net'] * 100,
            round(div_gross_vnd),
            round(div_net_vnd),
            pos_bar,
            s['volume']
        ))
        
    print("-" * 120)
    print("Ghi chú:")
    print("  * C.Tức gốc/n: Tỷ suất cổ tức gốc trước thuế.")
    print("  * C.Tức thực/n: Tỷ suất cổ tức thực nhận sau khi tự động khấu trừ 5% thuế TNCN tại nguồn.")
    print("  * Thực nhận: Số tiền cổ tức thực nhận bằng VND trên mỗi cổ phiếu sở hữu (sau thuế 5%).")
    print("  * Vùng giá 52T: Vị trí giá hiện tại giữa đáy 52T và đỉnh 52T (kí hiệu ★ thể hiện vị trí hiện tại).")
    print("=" * 120 + "\n")

if __name__ == "__main__":
    asyncio.run(main())
