import pandas as pd
import os
import yfinance as yf
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
import matplotlib.patheffects as PathEffects
from datetime import datetime, timedelta
import psycopg2
import random
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# --- 1. CONFIGURATION: WATCHLIST & DB ---
# Default tickers (BTC pairs mostly)
tickers = ['ETH-BTC', 'BNB-BTC', 'XRP-BTC', 'SOL-BTC']
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
OUTPUT_DIR = os.path.join(BASE_DIR, '../www')
OUTPUT_FILENAME = 'crypto_rrgchart.png'

# DB Connection to fetch watchlist
try:
    print("Connecting to database to fetch watchlist...")
    conn = psycopg2.connect(
        host=os.environ.get('DB_HOST'),
        database=os.environ.get('DB_NAME'),
        user=os.environ.get('DB_USER'),
        password=os.environ.get('DB_PASSWORD'),
        port=os.environ.get('DB_PORT')
    )
    cur = conn.cursor()
    cur.execute("SELECT DISTINCT crypto FROM public.cryptos_watchlist;")
    rows = cur.fetchall()
    
    db_tickers = []
    print(f"Found {len(rows)} tickers in watchlist.")
    
    for row in rows:
        raw_symbol = row[0] # e.g. BCHUSDT
        # Convert to Yahoo format: e.g. BCH-USD
        if raw_symbol.endswith('USDT'):
            symbol = raw_symbol.replace('USDT', '-USD')
        elif raw_symbol.endswith('USD'):
            symbol = raw_symbol.replace('USD', '-USD')
        else:
            # Fallback or skip if format is unexpected
            symbol = f"{raw_symbol}-USD"
            
        if symbol not in tickers and symbol not in db_tickers:
            db_tickers.append(symbol)
            
    print(f"Added from DB: {db_tickers}")
    tickers.extend(db_tickers)
    
    cur.close()
    conn.close()
except Exception as e:
    print(f"Error fetching from DB: {e}")
    print("Using default tickers only.")

# Ensure output directory exists
os.makedirs(OUTPUT_DIR, exist_ok=True)

image_filename = os.path.join(OUTPUT_DIR, OUTPUT_FILENAME)

# Lấy dữ liệu (Cần khoảng 130 ngày để đủ số liệu tính toán)
start_date = (datetime.now() - timedelta(days=150)).strftime('%Y-%m-%d')
print("Start date:", start_date)
end_date = None 

print("Đang tải dữ liệu từ Yahoo Finance (USD pairs)...")
data_raw = yf.download(tickers, start=start_date, end=end_date, progress=False)['Close']

# Xử lý MultiIndex (làm phẳng bảng dữ liệu)
if isinstance(data_raw.columns, pd.MultiIndex):
    data = data_raw.columns.droplevel(0) if 'Close' in data_raw.columns else data_raw
else:
    data = data_raw

# --- 2. HÀM TÍNH TOÁN RRG (SMOOTHED) ---
def calculate_rrg_smoothed(series, window_ratio=100, window_mom=25, smooth_window=3):
    # Logic: Với cặp USD, 'series' chính là giá USD của coin.
    # Ta so sánh Momentum của chính nó so với quá khứ.
    
    rs_scaled = 100 * series # Ở đây RS chính là giá (Relative so với USD=1)
    
    # RS-Ratio
    mean_r = rs_scaled.rolling(window=window_ratio).mean()
    std_r = rs_scaled.rolling(window=window_ratio).std(ddof=0)
    rsr_raw = 100 + ((rs_scaled - mean_r) / std_r)
    
    # ROC của RS-Ratio
    roc = 100 * ((rsr_raw / rsr_raw.shift(1)) - 1)
    
    # RS-Momentum
    mean_m = roc.rolling(window=window_mom).mean()
    std_m = roc.rolling(window=window_mom).std(ddof=0)
    rsm_raw = 100 + ((roc - mean_m) / std_m)
    
    # Làm mượt (Smoothing)
    rsr_smoothed = rsr_raw.rolling(window=smooth_window).mean()
    rsm_smoothed = rsm_raw.rolling(window=smooth_window).mean()
    
    return pd.DataFrame({'RSR': rsr_smoothed, 'RSM': rsm_smoothed}).dropna()

# --- 3. TÍNH TOÁN ---
rrg_data = {}
# Định nghĩa màu để dễ phân biệt
colors = {
    'BTC': '#006400', # Bitcoin màu Cam
    'ETH': '#0018a8',
    'BNB': '#ffbf00',
    'XRP': '#9370db',
    'SOL': '#8b8589',
    'DOGE': '#ff4f00',
    'ADA': '#5a4fcf'
}

# Generate random colors for new tickers
def get_random_color():
    return "#{:06x}".format(random.randint(0, 0xFFFFFF))

for ticker in tickers:
    # Key matching logic: extracting base symbol
    # Ticker format: ETH-BTC or BCH-USD
    base = ticker.split('-')[0]
    if base not in colors:
        colors[base] = get_random_color()

print("\n--- Sức mạnh so với USD ---")
for ticker in tickers:
    col_name = next((c for c in data.columns if ticker.split('-')[0] in c), None)
    if col_name is None: continue

    try:
        df_res = calculate_rrg_smoothed(data[col_name])
        rrg_data[ticker] = df_res
        print(f"{ticker:<8} | RSR: {df_res['RSR'].iloc[-1]:.2f} | RSM: {df_res['RSM'].iloc[-1]:.2f}")
    except Exception as e:
        pass

# ... (Giữ nguyên phần 1, 2, 3 ở trên) ...

# --- 4. VẼ BIỂU ĐỒ (ĐÃ NÂNG CẤP AUTO-ZOOM) ---
fig, ax = plt.subplots(figsize=(12, 12))

# Vẽ trục trung tâm
ax.axhline(100, color='black', lw=1, zorder=1)
ax.axvline(100, color='black', lw=1, zorder=1)

# Biến để lưu min/max phục vụ Auto-Zoom
all_x = []
all_y = []

# Vẽ đường đi
tail_length = 7
for ticker, df_res in rrg_data.items():
    if len(df_res) < tail_length: continue
    recent = df_res.tail(tail_length)
    x, y = recent['RSR'], recent['RSM']
    
    # Lưu dữ liệu để tính khung hình
    all_x.extend(x.values)
    all_y.extend(y.values)
    
    # Logic fix: Extract base symbol (e.g. "ETH" from "ETH-BTC")
    base_symbol = ticker.split('-')[0]
    c = colors.get(base_symbol, 'black') # Màu mặc định là đen nếu ko tìm thấy
    
    # Vẽ đuôi (mỏng hơn chút để đỡ rối)
    ax.plot(x, y, color=c, alpha=0.5, lw=1.5, zorder=3)
    
    # Vẽ đầu (to rõ)
    ax.scatter(x.iloc[-1], y.iloc[-1], s=200, color=c, edgecolors='white', linewidth=2, zorder=5)
    
    # Vẽ nhãn tên (Thêm logic để chữ ko đè lên điểm)
    offset = 0.05 # Khoảng cách chữ so với điểm
    txt = ax.text(x.iloc[-1] + offset, y.iloc[-1] + offset, ticker.split('-')[0], 
                  fontsize=12, fontweight='bold', color=c, zorder=6)
    txt.set_path_effects([PathEffects.withStroke(linewidth=3, foreground='white')])

# --- LOGIC AUTO-ZOOM THÔNG MINH ---
# Tìm biên độ dữ liệu thực tế
if len(all_x) > 0:
    min_x, max_x = min(all_x), max(all_x)
    min_y, max_y = min(all_y), max(all_y)
    
    # Thêm khoảng đệm (padding) 10% để điểm không sát mép
    pad_x = (max_x - min_x) * 0.1 if max_x != min_x else 1.0
    pad_y = (max_y - min_y) * 0.1 if max_y != min_y else 1.0
    
    # Đảm bảo khung hình luôn vuông vức (tỉ lệ 1:1) để không méo hình
    center_x = (max_x + min_x) / 2
    center_y = (max_y + min_y) / 2
    max_range = max(max_x - min_x, max_y - min_y) / 2 + max(pad_x, pad_y)
    
    # Nếu biến động quá nhỏ (< 2 đơn vị), ép zoom tối thiểu +/- 2 đơn vị để chart ko bị quá to
    max_range = max(max_range, 2.0)

    ax.set_xlim(center_x - max_range, center_x + max_range)
    ax.set_ylim(center_y - max_range, center_y + max_range)
    
    # Vẽ màu nền dựa trên khung hình động này
    xlim = ax.get_xlim()
    ylim = ax.get_ylim()
    alpha_quad = 0.05
    
    # Tô màu 4 góc
    ax.fill_between([100, xlim[1]], 100, ylim[1], color='green', alpha=alpha_quad)  # Leading
    ax.fill_between([100, xlim[1]], ylim[0], 100, color='#B8860B', alpha=alpha_quad)# Weakening
    ax.fill_between([xlim[0], 100], ylim[0], 100, color='red', alpha=alpha_quad)    # Lagging
    ax.fill_between([xlim[0], 100], 100, ylim[1], color='blue', alpha=alpha_quad)   # Improving
    
    # Cập nhật vị trí nhãn 4 góc (động theo zoom)
    ax.text(xlim[1]*0.99, ylim[1]*0.99, 'LEADING', color='green', ha='right', va='top', alpha=0.3, fontweight='bold', fontsize=14)
    ax.text(xlim[1]*0.99, ylim[0]*1.01, 'WEAKENING', color='#B8860B', ha='right', va='bottom', alpha=0.3, fontweight='bold', fontsize=14)
    ax.text(xlim[0]*1.01, ylim[0]*1.01, 'LAGGING', color='red', ha='left', va='bottom', alpha=0.3, fontweight='bold', fontsize=14)
    ax.text(xlim[0]*1.01, ylim[1]*0.99, 'IMPROVING', color='blue', ha='left', va='top', alpha=0.3, fontweight='bold', fontsize=14)

else:
    # Fallback nếu không có data
    ax.set_xlim(90, 110); ax.set_ylim(90, 110)

# Trang trí
now_str = datetime.now().strftime('%Y-%m-%d %H:%M')
ax.set_title('RRG - Crypto (Top + Watchlist)', fontsize=16, fontweight='bold')
ax.text(1, 1.01, f'Updated: {now_str}', transform=ax.transAxes, ha='right', color='#555', fontsize=10)
ax.set_xlabel('Trend (RS-Ratio)', fontsize=12)
ax.set_ylabel('Momentum (RS-Momentum)', fontsize=12)
ax.grid(True, linestyle='--', alpha=0.5)

plt.tight_layout()
plt.savefig(image_filename, dpi=120, bbox_inches='tight')
print(f'Chart saved as {image_filename}')