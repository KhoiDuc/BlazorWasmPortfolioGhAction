#!/usr/bin/env python3
"""
Script để quét toàn bộ danh sách cổ phiếu và tính Relative Strength so với VNINDEX.

Tính năng:
- Lấy danh sách tất cả cổ phiếu niêm yết từ VNDirect API
- Lọc các mã có 3 ký tự
- Kiểm tra accumulatedVolume, chỉ xử lý cổ phiếu có volume > 100,000
- Tính % thay đổi từ ngày nhập đến hiện tại cho từng cổ phiếu
- Chỉ hiển thị các cổ phiếu có % thay đổi > % thay đổi của VNINDEX
- Sắp xếp theo % thay đổi từ cao xuống thấp (mạnh nhất trước)

Lưu ý:
- Mỗi request có độ trễ 500ms để tránh quá tải API
- Quá trình quét toàn bộ có thể mất vài phút

Usage:
    python relative-strength.py <DATE> [INDEX_SYMBOL] [TOP_N]
    
    DATE: Ngày theo định dạng yyyy-mm-dd (ví dụ: 2024-11-11)
    INDEX_SYMBOL: (Tùy chọn) Mã chỉ số để so sánh (mặc định: VNINDEX)
    TOP_N: (Tùy chọn) Số lượng cổ phiếu hiển thị (mặc định: 20, dùng 0 để hiện tất cả)
    
Example:
    python relative-strength.py 2024-11-11
    # Output: Top 20 cổ phiếu có % thay đổi cao hơn VNINDEX
    
    python relative-strength.py 2024-10-01 VNINDEX 50
    # Output: Top 50 cổ phiếu có % thay đổi cao hơn VNINDEX
    
    python relative-strength.py 2024-11-11 VNINDEX 0
    # Output: Tất cả cổ phiếu có % thay đổi cao hơn VNINDEX
"""

import sys
import requests
from datetime import datetime
import calendar
import time

def date_to_unix_timestamp(date_str):
    """Chuyển đổi ngày yyyy-mm-dd sang Unix timestamp (giây) tại 00:00:00 UTC"""
    try:
        dt = datetime.strptime(date_str, '%Y-%m-%d')
        # Chuyển sang timestamp UTC tại 00:00:00 (dùng timegm để convert sang UTC)
        return int(calendar.timegm(dt.timetuple()))
    except ValueError as e:
        print(f"❌ Lỗi định dạng ngày: {e}")
        print("Vui lòng nhập ngày theo định dạng yyyy-mm-dd (ví dụ: 2024-12-24)")
        sys.exit(1)

def fetch_stock_list():
    """
    Lấy danh sách tất cả các mã cổ phiếu từ VNDirect API
    
    Returns:
        list: Danh sách các mã cổ phiếu (3 ký tự)
    """
    print("📋 Đang lấy danh sách cổ phiếu từ VNDirect API...")
    headers = {
        "User-Agent": "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36"
    }
    
    try:
        response = requests.get(
            "https://api-finfo.vndirect.com.vn/v4/stocks?q=type:STOCK~status:LISTED&fields=code&size=3000",
            headers=headers,
            timeout=10
        )
        response.raise_for_status()
        stocks_data = response.json().get('data', [])
        stocks_data = [item['code'] for item in stocks_data if len(item.get('code', '')) == 3]
        print(f"✅ Tìm thấy {len(stocks_data)} mã cổ phiếu")
        return stocks_data
    except requests.exceptions.RequestException as e:
        print(f"❌ Lỗi khi lấy danh sách cổ phiếu: {e}")
        sys.exit(1)

def check_accumulated_volume(symbol):
    """
    Kiểm tra accumulatedVolume của symbol từ VietCap API
    
    Args:
        symbol: Mã cổ phiếu (VD: VIC)
    
    Returns:
        float: accumulatedVolume hoặc 0 nếu lỗi/không tìm thấy
    """
    url = 'https://trading.vietcap.com.vn/api/market-data-service/v1/tickers/price/top-stock'
    headers = {
        'Accept': 'application/json',
        'Content-Type': 'application/json',
        'Origin': 'https://trading.vietcap.com.vn',
        'Referer': 'https://trading.vietcap.com.vn/',
        'User-Agent': 'Mozilla/5.0'
    }
    payload = {"tickers": [symbol]}
    
    try:
        response = requests.post(url, json=payload, headers=headers, timeout=10)
        response.raise_for_status()
        data = response.json()
        
        if data.get('success') and data.get('data'):
            item = data['data'][0]
            volume = item.get('accumulatedVolume', 0)
            return volume if volume is not None else 0
        return 0
    except Exception:
        return 0

def fetch_price_data(symbol, target_date, silent=False):
    """
    Gọi API VietCap và lấy item có tradingTime khớp với ngày đã nhập
    
    Args:
        symbol: Mã cổ phiếu (VD: MWG)
        target_date: Ngày cần lấy dữ liệu (yyyy-mm-dd)
        silent: Không hiển thị log nếu True
    
    Returns:
        tuple: (dữ liệu giá của ngày đó, dữ liệu item cuối cùng) hoặc (None, None) nếu không tìm thấy
    """
    # Chuyển ngày sang Unix timestamp
    target_timestamp = date_to_unix_timestamp(target_date)
    if not silent:
        print(f"🔍 Tìm kiếm dữ liệu cho {symbol} vào ngày {target_date}")
        print(f"   Unix timestamp: {target_timestamp}")
    
    # Gọi API VietCap
    url = f'https://iq.vietcap.com.vn/api/iq-insight-service/v1/company/{symbol}/price-chart'
    params = {
        'lengthReport': 10,  # Dùng 10 để tránh API trả về null
        'toCurrent': 'true'
    }
    headers = {
        'Accept': 'application/json',
        'User-Agent': 'Mozilla/5.0',
        'Origin': 'https://trading.vietcap.com.vn',
        'Referer': 'https://trading.vietcap.com.vn/'
    }
    
    try:
        if not silent:
            print(f"📡 Đang gọi API VietCap...")
        response = requests.get(url, params=params, headers=headers, timeout=10)
        response.raise_for_status()
        
        data = response.json()
        
        if not data.get('successful'):
            if not silent:
                print(f"❌ API trả về lỗi: {data.get('msg')}")
            return None, None
        
        items = data.get('data', [])
        if items is None:
            items = []
        if not silent:
            print(f"✅ Nhận được {len(items)} mục dữ liệu")
        
        # Lấy item cuối cùng (ngày gần nhất)
        latest_item = items[-1] if items else None
        
        # Tìm item có tradingTime khớp với target_timestamp
        target_item = None
        for item in items:
            if item.get('tradingTime') == target_timestamp:
                target_item = item
                break
        
        if not target_item:
            if not silent:
                print(f"⚠️  Không tìm thấy dữ liệu cho ngày {target_date}")
                print(f"   Có thể ngày này không có giao dịch hoặc nằm ngoài phạm vi dữ liệu")
            return None, None
        
        return target_item, latest_item
        
    except requests.exceptions.RequestException as e:
        if not silent:
            print(f"❌ Lỗi khi gọi API: {e}")
        return None, None

def main():
    if len(sys.argv) < 2 or len(sys.argv) > 4:
        print(__doc__)
        sys.exit(1)
    
    target_date = sys.argv[1]
    index_symbol = sys.argv[2].upper() if len(sys.argv) >= 3 else 'VNINDEX'
    top_n = int(sys.argv[3]) if len(sys.argv) == 4 else 50
    
    print("=" * 80)
    print(f"Relative Strength Scanner - VietCap Data")
    print("=" * 80)
    
    # Bước 1: Lấy dữ liệu chỉ số
    print(f"\n🔹 Bước 1: Lấy dữ liệu {index_symbol}")
    vnindex_target_data, vnindex_latest_data = fetch_price_data(index_symbol, target_date, silent=False)
    
    if not vnindex_target_data:
        print(f"\n❌ Không lấy được dữ liệu {index_symbol}.")
        sys.exit(1)
    
    vnindex_closing_price = vnindex_target_data.get('closingPrice')
    vnindex_latest_price = vnindex_latest_data.get('closingPrice')
    vnindex_change_pct = ((vnindex_latest_price / vnindex_closing_price) - 1) * 100
    
    print(f"✅ {index_symbol} ngày {target_date}: {vnindex_closing_price:,.2f}")
    print(f"✅ {index_symbol} ngày gần nhất: {vnindex_latest_price:,.2f}")
    print(f"📊 Thay đổi: {vnindex_change_pct:+.2f}%")
    
    # Bước 2: Lấy danh sách cổ phiếu
    print(f"\n🔹 Bước 2: Lấy danh sách cổ phiếu")
    stock_list = fetch_stock_list()
    
    # Bước 3: Quét từng cổ phiếu
    print(f"\n🔹 Bước 3: Quét {len(stock_list)} cổ phiếu (có thể mất vài phút)...")
    results = []
    processed = 0
    failed = 0
    skipped_volume = 0
    
    for i, symbol in enumerate(stock_list, 1):
        # Hiển thị tiến độ
        if i % 50 == 0 or i == len(stock_list):
            print(f"   📊 Đã xử lý: {i}/{len(stock_list)} ({processed} thành công, {failed} thất bại, {skipped_volume} bỏ qua do volume thấp)")
        
        # Kiểm tra accumulatedVolume trước
        accumulated_volume = check_accumulated_volume(symbol)
        if accumulated_volume is None or accumulated_volume <= 100000:
            skipped_volume += 1
            time.sleep(0.5)  # Vẫn delay để tránh quá tải
            continue
        
        target_data, latest_data = fetch_price_data(symbol, target_date, silent=True)
        
        if target_data and latest_data:
            target_closing = target_data.get('closingPrice')
            latest_closing = latest_data.get('closingPrice')
            
            if target_closing and latest_closing and target_closing > 0:
                symbol_change_pct = ((latest_closing / target_closing) - 1) * 100
                relative_strength = symbol_change_pct - vnindex_change_pct
                
                results.append({
                    'symbol': symbol,
                    'target_price': target_closing,
                    'latest_price': latest_closing,
                    'change_pct': symbol_change_pct,
                    'relative_strength': relative_strength
                })
                processed += 1
        else:
            failed += 1
        
        # Delay 500ms để tránh quá tải API
        time.sleep(0.5)
    
    print(f"\n✅ Hoàn thành! Đã xử lý {processed}/{len(stock_list)} cổ phiếu")
    
    # Bước 4: Lọc chỉ những cổ phiếu có % thay đổi lớn hơn VNINDEX
    strong_stocks = [r for r in results if r['change_pct'] > vnindex_change_pct]
    
    if not strong_stocks:
        print(f"\n❌ Không có cổ phiếu nào mạnh hơn {index_symbol} ({vnindex_change_pct:+.2f}%) trong kỳ này")
        sys.exit(0)
    
    # Sắp xếp theo % thay đổi từ cao xuống thấp
    strong_stocks_sorted = sorted(strong_stocks, key=lambda x: x['change_pct'], reverse=True)
    
    # Hiển thị tất cả cổ phiếu mạnh hơn VNINDEX (hoặc giới hạn theo top_n)
    display_count = min(top_n, len(strong_stocks_sorted)) if top_n > 0 else len(strong_stocks_sorted)
    
    print("\n" + "=" * 80)
    print(f"📈 CỔ PHIẾU MẠNH HƠN {index_symbol} (% thay đổi > {vnindex_change_pct:.2f}%)")
    print(f"Hiển thị: Top {display_count}/{len(strong_stocks_sorted)}")
    print("=" * 80)
    print(f"{'STT':<5} {'Mã':<8} {'Giá ' + target_date:<15} {'Giá hiện tại':<15} {'% Thay đổi':<12} {'RS':<10}")
    print("-" * 80)
    
    for i, stock in enumerate(strong_stocks_sorted[:display_count], 1):
        print(f"{i:<5} {stock['symbol']:<8} {stock['target_price']:>12,.0f}   "
              f"{stock['latest_price']:>12,.0f}   {stock['change_pct']:>10.2f}%  {stock['relative_strength']:>8.2f}%")
    
    # Thống kê tổng quan
    print("\n" + "=" * 80)
    print("📊 THỐNG KÊ TỔNG QUAN")
    print("=" * 80)
    print(f"Tổng số cổ phiếu quét:              {len(stock_list)}")
    print(f"Số cổ phiếu bỏ qua (volume ≤ 100k): {skipped_volume}")
    print(f"Số cổ phiếu có dữ liệu:             {processed}")
    print(f"Số cổ phiếu mạnh hơn {index_symbol}:        {len(strong_stocks)} ({len(strong_stocks)/processed*100:.1f}%)")
    print(f"Số cổ phiếu yếu hơn {index_symbol}:         {processed - len(strong_stocks)}")
    print(f"{index_symbol} thay đổi:                    {vnindex_change_pct:+.2f}%")
    print("=" * 80)

if __name__ == '__main__':
    main()

