import requests
import os
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.dates as mdates
from datetime import datetime, timedelta
import psycopg2
from dotenv import load_dotenv
import random
import colorsys

# Load environment variables
# Try loading from current directory first (for server), then parent directory (for local dev)
current_dir_env = os.path.join(os.path.dirname(__file__), '.env')
parent_dir_env = os.path.join(os.path.dirname(__file__), '../.env')

if os.path.exists(current_dir_env):
    load_dotenv(current_dir_env)
    print(f"Loaded .env from {current_dir_env}")
elif os.path.exists(parent_dir_env):
    load_dotenv(parent_dir_env)
    print(f"Loaded .env from {parent_dir_env}")
else:
    print("Warning: No .env file found!")

# --- CẤU HÌNH ---
def get_symbols_from_db():
    try:
        conn = psycopg2.connect(
            host=os.getenv('DB_HOST'),
            database=os.getenv('DB_NAME'),
            user=os.getenv('DB_USER'),
            password=os.getenv('DB_PASSWORD'),
            port=os.getenv('DB_PORT')
        )
        cur = conn.cursor()
        cur.execute(
            """
            SELECT DISTINCT symbol
            FROM public.symbols_watchlist
            WHERE signal_type IN (%s, %s)
            """,
            ('near_52w_ath', 'top_growth_20d')
        )
        rows = cur.fetchall()
        symbols = [row[0] for row in rows]
        cur.close()
        conn.close()
        print(
            "Loaded "
            f"{len(symbols)} symbols from database "
            "(signal_type in near_52w_ath, top_growth_20d): "
            f"{symbols}"
        )
        return symbols
    except Exception as e:
        print(f"Error fetching symbols from DB: {e}")
        # Fallback list if DB fails
        return ['PNJ', 'VCB', 'BVH', 'VNM', 'FPT', 'MSN', 'SSI', 'HPG', 'VIC', 'BCM', 'PLX', 'MWG']

SYMBOLS = get_symbols_from_db()
BENCHMARK = 'VNINDEX'
DAYS_BACK = 150
TAIL_LENGTH = 7  # Độ dài đuôi
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
OUTPUT_DIR = os.path.join(BASE_DIR, '../www')
OUTPUT_FILENAME = 'vnstock_rrgchart.png'

# Ensure output directory exists
os.makedirs(OUTPUT_DIR, exist_ok=True)

FULL_OUTPUT_PATH = os.path.join(OUTPUT_DIR, OUTPUT_FILENAME)

# Cấu hình API KBSec
KBSEC_BASE_URL = "https://kbbuddywts.kbsec.com.vn/iis-server/investment"
HEADERS = {
    'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
}

def get_date_range_strings(days=150):
    end_date = datetime.now()
    start_date = end_date - timedelta(days=days)
    start_date_buffer = start_date - timedelta(days=30) # Buffer dài hơn chút để an toàn
    
    # Format: dd-mm-yyyy
    return start_date_buffer.strftime("%d-%m-%Y"), end_date.strftime("%d-%m-%Y")

def fetch_data(symbol, start_date_str, end_date_str):
    try:
        if symbol == 'VNINDEX':
            url = f"{KBSEC_BASE_URL}/index/VNINDEX/data_day"
        else:
            url = f"{KBSEC_BASE_URL}/stocks/{symbol}/data_day"
            
        params = {
            'sdate': start_date_str,
            'edate': end_date_str
        }
        
        response = requests.get(url, params=params, headers=HEADERS)
        
        if response.status_code != 200:
            print(f"Error fetching {symbol}: HTTP {response.status_code} - {response.reason}")
            # print(f"Response content: {response.text[:200]}")
            return None

        try:
            data = response.json()
        except Exception as json_err:
            print(f"Error parsing JSON for {symbol}: {json_err}")
            return None

        if 'data_day' in data and data['data_day']:
            df = pd.DataFrame(data['data_day'])
            
            # KBSec returns data with keys: t (time), o, h, l, c (close), v (volume)
            # 't' format: "2026-01-28 07:00"
            df['date'] = pd.to_datetime(df['t'])
            
            # Convert close price to numeric (sometimes it is string)
            df['close'] = pd.to_numeric(df['c'])
            
            df.set_index('date', inplace=True)
            df = df[['close']] # Keep only close price
            return df

        return None
    except Exception as e:
        print(f"Error fetching {symbol}: {e}")
        return None

def calculate_rrg_components(stock_df, benchmark_df, window=14): # Window chuẩn thường là 10-14
    # Merge dữ liệu
    df = pd.merge(stock_df, benchmark_df, left_index=True, right_index=True, suffixes=('_stock', '_index'))
    
    # 1. RS = Stock / Index
    df['rs'] = df['close_stock'] / df['close_index']
    
    # 2. RSR = 100 + ((RS - Mean(RS)) / Std(RS)) -> Cách tính chuẩn hóa (Normalized)
    # Hoặc cách JdK xấp xỉ đơn giản: 100 * (RS / MA(RS))
    # Ở đây dùng cách xấp xỉ JdK ratio để ra số quanh 100
    df['rs_mean'] = df['rs'].rolling(window=window).mean()
    df['RSR'] = 100 * (df['rs'] / df['rs_mean'])
    
    # 3. RSM = Động lượng của RSR
    # RSM đo tốc độ thay đổi của RSR
    df['rsr_mean'] = df['RSR'].rolling(window=window).mean()
    df['RSM'] = 100 * (df['RSR'] / df['rsr_mean'])
    
    df.dropna(inplace=True)
    return df[['RSR', 'RSM']]

def get_random_dark_color():
    """Generates a random dark/bold color."""
    h = random.random()
    s = 0.8 + (random.random() * 0.2)  # High saturation (0.8 - 1.0)
    v = 0.3 + (random.random() * 0.4)  # Low-Medium brightness (0.3 - 0.7) for distinct dark colors
    r, g, b = colorsys.hsv_to_rgb(h, s, v)
    return '#{:02x}{:02x}{:02x}'.format(int(r*255), int(g*255), int(b*255))

def plot_rrg_and_save(rrg_data):
    fig, ax = plt.subplots(figsize=(12, 10))
    
    # --- TÍNH TOÁN GIỚI HẠN TRỤC TỰ ĐỘNG (AUTO SCALING) ---
    all_rsr = []
    all_rsm = []
    for df in rrg_data.values():
        tail = df.tail(TAIL_LENGTH)
        all_rsr.extend(tail['RSR'].values)
        all_rsm.extend(tail['RSM'].values)
    
    # Tìm điểm xa nhất so với tâm 100 để xác định khung hình vuông
    max_dist_x = max([abs(x - 100) for x in all_rsr]) if all_rsr else 5
    max_dist_y = max([abs(y - 100) for y in all_rsm]) if all_rsm else 5
    
    # Lấy khoảng cách lớn nhất + thêm 2 đơn vị lề (margin)
    limit = max(max_dist_x, max_dist_y) + 2
    limit = max(limit, 4) # Đảm bảo khung hình tối thiểu +/- 4
    
    min_lim = 100 - limit
    max_lim = 100 + limit
    
    # --- VẼ NỀN ---
    ax.axhline(y=100, color='gray', linestyle='--', linewidth=1)
    ax.axvline(x=100, color='gray', linestyle='--', linewidth=1)
    
    # Tô màu 4 góc
    ax.fill_between([100, max_lim], 100, max_lim, color='green', alpha=0.05) # Leading
    ax.fill_between([100, max_lim], min_lim, 100, color='yellow', alpha=0.05) # Weakening
    ax.fill_between([min_lim, 100], min_lim, 100, color='red', alpha=0.05) # Lagging
    ax.fill_between([min_lim, 100], 100, max_lim, color='blue', alpha=0.05) # Improving
    
    # Label góc
    mid_pos = limit / 2
    ax.text(100 + mid_pos, 100 + mid_pos, 'LEADING\n(Dẫn dắt)', color='green', alpha=0.3, ha='center', va='center')
    ax.text(100 + mid_pos, 100 - mid_pos, 'WEAKENING\n(Suy yếu)', color='orange', alpha=0.3, ha='center', va='center')
    ax.text(100 - mid_pos, 100 - mid_pos, 'LAGGING\n(Tụt hậu)', color='red', alpha=0.3, ha='center', va='center')
    ax.text(100 - mid_pos, 100 + mid_pos, 'IMPROVING\n(Cải thiện)', color='blue', alpha=0.3, ha='center', va='center')

    for symbol, df in rrg_data.items():
        tail = df.tail(TAIL_LENGTH)
        if tail.empty: continue
            
        c = get_random_dark_color()
        
        # Vẽ đuôi
        # ax.plot(tail['RSR'], tail['RSM'], color=c, linewidth=2, alpha=0.6, label=symbol)
        
        # Điểm hiện tại
        curr = tail.iloc[-1]
        ax.scatter(curr['RSR'], curr['RSM'], color=c, s=100, zorder=5, edgecolors='white')
        
        # Tên mã (Thêm offset thông minh để tránh đè điểm)
        ax.text(curr['RSR'], curr['RSM'] + (limit * 0.02), symbol, 
                fontsize=11, fontweight='bold', color=c, ha='center', va='bottom')
        
        # Các chấm nhỏ
        # ax.scatter(tail['RSR'][:-1], tail['RSM'][:-1], color=c, s=15, alpha=0.4)

    # UI Settings
    ax.set_title(f'Biểu đồ RRG - {TAIL_LENGTH} phiên gần nhất\nBenchmark: {BENCHMARK}', fontsize=14, fontweight='bold', y=1.02)
    
    now_str = datetime.now().strftime("%H:%M %d/%m/%Y")
    ax.text(1.0, 1.01, f'Updated: {now_str}', transform=ax.transAxes,
            ha='right', fontsize=9, color='gray', fontstyle='italic')

    ax.set_xlabel('RS-Ratio (Xu hướng)', fontsize=11)
    ax.set_ylabel('RS-Momentum (Động lượng)', fontsize=11)
    ax.grid(True, linestyle=':', alpha=0.5)
    
    # ÁP DỤNG LIMIT TỰ ĐỘNG ĐÃ TÍNH
    ax.set_xlim(min_lim, max_lim)
    ax.set_ylim(min_lim, max_lim)
    
    plt.tight_layout()
    print(f"Đang lưu file: {FULL_OUTPUT_PATH}...")
    plt.savefig(FULL_OUTPUT_PATH, dpi=150) # DPI 150 cho nhẹ và nhanh
    plt.close(fig)

def main():
    print("--- Bắt đầu xử lý ---")
    start_date_str, end_date_str = get_date_range_strings(DAYS_BACK)
    print(f"Time range: {start_date_str} to {end_date_str}")
    
    # Lấy Benchmark
    bench_df = fetch_data(BENCHMARK, start_date_str, end_date_str)
    if bench_df is None: return

    rrg_results = {}
    
    for symbol in SYMBOLS:
        print(f"Đang lấy dữ liệu: {symbol}")
        stock_df = fetch_data(symbol, start_date_str, end_date_str)
        if stock_df is not None and len(stock_df) > 20:
            rrg_df = calculate_rrg_components(stock_df, bench_df)
            rrg_results[symbol] = rrg_df
            # In ra để kiểm tra
            curr = rrg_df.iloc[-1]
            print(f" -> {symbol}: RSR={curr['RSR']:.2f}, RSM={curr['RSM']:.2f}")

    if rrg_results:
        plot_rrg_and_save(rrg_results)
        print("Xong!")
    else:
        print("Không có dữ liệu.")

if __name__ == "__main__":
    main()