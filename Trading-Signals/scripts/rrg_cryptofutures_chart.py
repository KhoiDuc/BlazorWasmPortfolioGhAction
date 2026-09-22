import os
import requests
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
import matplotlib.patheffects as PathEffects
from datetime import datetime
import psycopg2
import random
from dotenv import load_dotenv

load_dotenv()

# --- 1. CONFIGURATION ---
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
OUTPUT_DIR = os.path.join(BASE_DIR, '../www')
OUTPUT_FILENAME = 'futures_rrgchart.png'
os.makedirs(OUTPUT_DIR, exist_ok=True)
image_filename = os.path.join(OUTPUT_DIR, OUTPUT_FILENAME)

# Danh sách mặc định nếu DB trống
tickers = ['BTCUSDT', 'ETHUSDT', 'SOLUSDT', 'BNBUSDT']

try:
    print("Kết nối DB lấy danh sách Futures Watchlist...")
    conn = psycopg2.connect(
        host=os.environ.get('DB_HOST'), database=os.environ.get('DB_NAME'),
        user=os.environ.get('DB_USER'), password=os.environ.get('DB_PASSWORD'),
        port=os.environ.get('DB_PORT')
    )
    cur = conn.cursor()
    cur.execute("SELECT DISTINCT symbol FROM public.futures_watchlist;")
    rows = cur.fetchall()
    
    db_tickers = [row[0] for row in rows if row[0] not in tickers]
    tickers.extend(db_tickers)
    print(f"Tổng cộng có {len(tickers)} mã Futures cần vẽ RRG.")
    cur.close(); conn.close()
except Exception as e:
    print(f"Lỗi DB: {e}. Sử dụng danh sách mặc định.")

# --- 2. FETCH BINANCE FUTURES DATA ---
print("Đang tải dữ liệu từ Binance Futures API...")
df_close = pd.DataFrame()

for symbol in tickers:
    try:
        url = "https://fapi.binance.com/fapi/v1/klines"
        params = {"symbol": symbol, "interval": "1d", "limit": 150} # Cần 150 ngày để RRG đủ độ mượt
        res = requests.get(url, params=params)
        data = res.json()
        
        # Chỉ lấy giá đóng cửa
        closes = [float(k[4]) for k in data]
        if len(closes) == 150:
            df_close[symbol] = closes
    except Exception as e:
        pass

if df_close.empty:
    print("Không lấy được dữ liệu. Thoát script.")
    exit()

# --- 3. TÍNH TOÁN RRG (SMOOTHED) ---
def calculate_rrg_smoothed(series, window_ratio=100, window_mom=25, smooth_window=3):
    rs_scaled = 100 * series 
    
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
    
    # Smoothing
    rsr_smoothed = rsr_raw.rolling(window=smooth_window).mean()
    rsm_smoothed = rsm_raw.rolling(window=smooth_window).mean()
    
    return pd.DataFrame({'RSR': rsr_smoothed, 'RSM': rsm_smoothed}).dropna()

rrg_data = {}
colors = {'BTCUSDT': '#f7931a', 'ETHUSDT': '#627eea', 'BNBUSDT': '#f3ba2f', 'SOLUSDT': '#14f195'}

for col in df_close.columns:
    df_res = calculate_rrg_smoothed(df_close[col])
    if not df_res.empty:
        rrg_data[col] = df_res
        if col not in colors:
            colors[col] = "#{:06x}".format(random.randint(0, 0xFFFFFF))

# --- 4. VẼ BIỂU ĐỒ (AUTO-ZOOM) ---
fig, ax = plt.subplots(figsize=(12, 12))
ax.axhline(100, color='black', lw=1, zorder=1)
ax.axvline(100, color='black', lw=1, zorder=1)

all_x, all_y = [], []
tail_length = 7

for ticker, df_res in rrg_data.items():
    if len(df_res) < tail_length: continue
    recent = df_res.tail(tail_length)
    x, y = recent['RSR'], recent['RSM']
    
    all_x.extend(x.values)
    all_y.extend(y.values)
    
    c = colors.get(ticker, 'black')
    
    # Vẽ đuôi và điểm hiện tại
    ax.plot(x, y, color=c, alpha=0.5, lw=1.5, zorder=3)
    ax.scatter(x.iloc[-1], y.iloc[-1], s=200, color=c, edgecolors='white', linewidth=2, zorder=5)
    
    # Nhãn tên (Cắt bỏ đuôi USDT cho gọn)
    display_name = ticker.replace("USDT", "")
    txt = ax.text(x.iloc[-1] + 0.05, y.iloc[-1] + 0.05, display_name, 
                  fontsize=12, fontweight='bold', color=c, zorder=6)
    txt.set_path_effects([PathEffects.withStroke(linewidth=3, foreground='white')])

# Logic Auto-Zoom
if all_x:
    min_x, max_x = min(all_x), max(all_x)
    min_y, max_y = min(all_y), max(all_y)
    
    pad_x = (max_x - min_x) * 0.1 if max_x != min_x else 1.0
    pad_y = (max_y - min_y) * 0.1 if max_y != min_y else 1.0
    
    center_x = (max_x + min_x) / 2
    center_y = (max_y + min_y) / 2
    max_range = max(max_x - min_x, max_y - min_y) / 2 + max(pad_x, pad_y)
    max_range = max(max_range, 2.0)

    ax.set_xlim(center_x - max_range, center_x + max_range)
    ax.set_ylim(center_y - max_range, center_y + max_range)
    
    xlim = ax.get_xlim()
    ylim = ax.get_ylim()
    alpha_quad = 0.05
    
    ax.fill_between([100, xlim[1]], 100, ylim[1], color='green', alpha=alpha_quad)  # Leading
    ax.fill_between([100, xlim[1]], ylim[0], 100, color='#B8860B', alpha=alpha_quad)# Weakening
    ax.fill_between([xlim[0], 100], ylim[0], 100, color='red', alpha=alpha_quad)    # Lagging
    ax.fill_between([xlim[0], 100], 100, ylim[1], color='blue', alpha=alpha_quad)   # Improving

now_str = datetime.now().strftime('%Y-%m-%d %H:%M')
ax.set_title('RRG - Perpetual Futures', fontsize=16, fontweight='bold')
ax.text(1, 1.01, f'Updated: {now_str}', transform=ax.transAxes, ha='right', color='#555', fontsize=10)
ax.set_xlabel('Trend (RS-Ratio)', fontsize=12)
ax.set_ylabel('Momentum (RS-Momentum)', fontsize=12)
ax.grid(True, linestyle='--', alpha=0.5)

plt.tight_layout()
plt.savefig(image_filename, dpi=120, bbox_inches='tight')
print(f'✅ Đã lưu chart Futures tại: {image_filename}')