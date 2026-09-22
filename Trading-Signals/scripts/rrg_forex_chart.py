import pandas as pd
import os
import yfinance as yf
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
import matplotlib.patheffects as PathEffects
from datetime import datetime, timedelta

# --- 1. CONFIGURATION ---
# Tickers from Yahoo Finance
# Direct pairs (Base=Currency, Quote=USD): EURUSD=X, GBPUSD=X, AUDUSD=X, NZDUSD=X
# Inverse pairs (Base=USD, Quote=Currency): JPY=X (USD/JPY), CAD=X (USD/CAD), CHF=X (USD/CHF)
tickers = ['EURUSD=X', 'JPY=X', 'GBPUSD=X', 'AUDUSD=X', 'CAD=X', 'NZDUSD=X', 'CHF=X']

# Mapping for display names and logic handling
# Key: Yahoo Ticker, Value: {'label': Display Name, 'type': 'direct'/'inverse'}
ticker_info = {
    'EURUSD=X': {'label': 'EUR', 'type': 'direct'},
    'JPY=X':    {'label': 'JPY', 'type': 'inverse'}, # Yahoo returns USD/JPY -> Need 1/(USD/JPY) = JPY/USD
    'GBPUSD=X': {'label': 'GBP', 'type': 'direct'},
    'AUDUSD=X': {'label': 'AUD', 'type': 'direct'},
    'CAD=X':    {'label': 'CAD', 'type': 'inverse'}, # Yahoo returns USD/CAD -> Need 1/(USD/CAD) = CAD/USD
    'NZDUSD=X': {'label': 'NZD', 'type': 'direct'},
    'CHF=X':    {'label': 'CHF', 'type': 'inverse'}  # Yahoo returns USD/CHF -> Need 1/(USD/CHF) = CHF/USD
}

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
OUTPUT_DIR = os.path.join(BASE_DIR, '../www')
OUTPUT_FILENAME = 'forex_rrgchart.png'

# Ensure output directory exists
os.makedirs(OUTPUT_DIR, exist_ok=True)

image_filename = os.path.join(OUTPUT_DIR, OUTPUT_FILENAME)

# Fetch data (Need enough history for RRG calculation)
start_date = (datetime.now() - timedelta(days=400)).strftime('%Y-%m-%d')
print("Start date:", start_date)
end_date = None 

print("Downloading Forex data from Yahoo Finance...")
data_raw = yf.download(tickers, start=start_date, end=end_date, progress=False)['Close']

# Handle MultiIndex if present
if isinstance(data_raw.columns, pd.MultiIndex):
    data = data_raw.columns.droplevel(0) if 'Close' in data_raw.columns else data_raw
else:
    data = data_raw

# --- 2. RRG CALCULATION FUNCTION (SMOOTHED) ---
def calculate_rrg_smoothed(series, window_ratio=100, window_mom=25, smooth_window=3):
    # Logic: RRG compares relative strength and momentum.
    # standard RRG usually compares to a benchmark. 
    # Here we assume 'series' IS the relative strength metric (Price against USD).
    # Since we normalized everything to be "Currency per 1 USD" or "USD per 1 Currency",
    # actually we want "USD Value of the Currency".
    # Direct pairs (EURUSD) -> Value is already in USD.
    # Inverse pairs (USDJPY) -> Value is JPY per USD. Inverting it gives USD per JPY.
    
    rs_scaled = 100 * series # Base value
    
    # RS-Ratio
    mean_r = rs_scaled.rolling(window=window_ratio).mean()
    std_r = rs_scaled.rolling(window=window_ratio).std(ddof=0)
    rsr_raw = 100 + ((rs_scaled - mean_r) / std_r)
    
    # ROC of RS-Ratio
    roc = 100 * ((rsr_raw / rsr_raw.shift(1)) - 1)
    
    # RS-Momentum
    mean_m = roc.rolling(window=window_mom).mean()
    std_m = roc.rolling(window=window_mom).std(ddof=0)
    rsm_raw = 100 + ((roc - mean_m) / std_m)
    
    # Smoothing
    rsr_smoothed = rsr_raw.rolling(window=smooth_window).mean()
    rsm_smoothed = rsm_raw.rolling(window=smooth_window).mean()
    
    return pd.DataFrame({'RSR': rsr_smoothed, 'RSM': rsm_smoothed}).dropna()

# --- 3. PROCESSING ---
rrg_data = {}

# Definitive Colors for Forex
colors = {
    'EUR': '#003399', # EU Blue
    'JPY': '#bc002d', # Japan Red
    'GBP': '#cf142b', # UK Red/Blue (using a distinct red/purple) - Let's use Purple to distinguish from JPY
    'GBP': '#800080', # Purple for UK
    'AUD': '#00843D', # Aussie Green
    'CAD': '#FF0000', # Canada Red (Warning: JPY is also red-ish). Let's use a Dark Orange or different Red.
    'CAD': '#d95f02', # Dark Orange
    'NZD': '#000000', # Kiwi Black (Silver Fern)
    'CHF': '#D52B1E'  # Swiss Red... Too many reds.
}

# Adjusted colors for clarity on chart
colors = {
    'EUR': '#1f77b4', # Blue
    'JPY': '#d62728', # Red
    'GBP': '#9467bd', # Purple
    'AUD': '#2ca02c', # Green
    'CAD': '#ff7f0e', # Orange
    'NZD': '#7f7f7f', # Gray (Black tends to hide in dark mode or axes, gray is safer)
    'CHF': '#8c564b'  # Brown
}


print("\n--- Strength vs USD ---")
for ticker in tickers:
    if ticker not in data.columns:
        print(f"Missing data for {ticker}")
        continue
        
    series = data[ticker].dropna()
    info = ticker_info.get(ticker)
    
    if info['type'] == 'inverse':
        # Invert the pair to get Currency value in USD
        # e.g. USD/JPY=150 -> JPY/USD = 1/150
        series = 1 / series
        
    label = info['label']
    
    try:
        df_res = calculate_rrg_smoothed(series)
        rrg_data[label] = df_res
        print(f"{label:<8} | RSR: {df_res['RSR'].iloc[-1]:.2f} | RSM: {df_res['RSM'].iloc[-1]:.2f}")
    except Exception as e:
        print(f"Error calculating {label}: {e}")
        pass

# --- 4. PLOTTING ---
fig, ax = plt.subplots(figsize=(10, 10))

# Axes
ax.axhline(100, color='black', lw=1, zorder=1)
ax.axvline(100, color='black', lw=1, zorder=1)

# Auto-Zoom variables
all_x = []
all_y = []

tail_length = 7 # Halved from 25 to make the chart less cluttered and much easier to read

for label, df_res in rrg_data.items():
    if len(df_res) < tail_length: continue
    recent = df_res.tail(tail_length)
    x, y = recent['RSR'], recent['RSM']
    
    all_x.extend(x.values)
    all_y.extend(y.values)
    
    c = colors.get(label, 'black')
    
    # Tail
    ax.plot(x, y, color=c, alpha=0.5, lw=1.5, zorder=3)
    
    # Head
    ax.scatter(x.iloc[-1], y.iloc[-1], s=150, color=c, edgecolors='white', linewidth=2, zorder=5)
    
    # Text
    offset = 0.05
    txt = ax.text(x.iloc[-1] + offset, y.iloc[-1] + offset, label, 
                  fontsize=11, fontweight='bold', color=c, zorder=6)
    txt.set_path_effects([PathEffects.withStroke(linewidth=3, foreground='white')])

# Auto-Zoom Logic
if len(all_x) > 0:
    min_x, max_x = min(all_x), max(all_x)
    min_y, max_y = min(all_y), max(all_y)
    
    pad_x = (max_x - min_x) * 0.15 if max_x != min_x else 1.0
    pad_y = (max_y - min_y) * 0.15 if max_y != min_y else 1.0
    
    center_x = (max_x + min_x) / 2
    center_y = (max_y + min_y) / 2
    
    # Square aspect ratio logic
    max_range = max(max_x - min_x, max_y - min_y) / 2 + max(pad_x, pad_y)
    max_range = max(max_range, 1.5) # Minimum range
    
    ax.set_xlim(center_x - max_range, center_x + max_range)
    ax.set_ylim(center_y - max_range, center_y + max_range)
    
    # Quadrant colors
    xlim = ax.get_xlim()
    ylim = ax.get_ylim()
    alpha_quad = 0.05
    
    ax.fill_between([100, xlim[1]], 100, ylim[1], color='green', alpha=alpha_quad)  # Leading
    ax.fill_between([100, xlim[1]], ylim[0], 100, color='#B8860B', alpha=alpha_quad)# Weakening
    ax.fill_between([xlim[0], 100], ylim[0], 100, color='red', alpha=alpha_quad)    # Lagging
    ax.fill_between([xlim[0], 100], 100, ylim[1], color='blue', alpha=alpha_quad)   # Improving
    
    # Labels
    ax.text(xlim[1]*0.99, ylim[1]*0.99, 'LEADING', color='green', ha='right', va='top', alpha=0.3, fontweight='bold', fontsize=12)
    ax.text(xlim[1]*0.99, ylim[0]*1.01, 'WEAKENING', color='#B8860B', ha='right', va='bottom', alpha=0.3, fontweight='bold', fontsize=12)
    ax.text(xlim[0]*1.01, ylim[0]*1.01, 'LAGGING', color='red', ha='left', va='bottom', alpha=0.3, fontweight='bold', fontsize=12)
    ax.text(xlim[0]*1.01, ylim[1]*0.99, 'IMPROVING', color='blue', ha='left', va='top', alpha=0.3, fontweight='bold', fontsize=12)
else:
    ax.set_xlim(95, 105)
    ax.set_ylim(95, 105)

# Styling
now_str = datetime.now().strftime('%Y-%m-%d %H:%M')
ax.set_title('RRG - Major Forex Pairs (vs USD)', fontsize=14, fontweight='bold')
ax.text(1, 1.01, f'Updated: {now_str}', transform=ax.transAxes, ha='right', color='#555', fontsize=9)
ax.set_xlabel('Trend (RS-Ratio)', fontsize=10)
ax.set_ylabel('Momentum (RS-Momentum)', fontsize=10)
ax.grid(True, linestyle='--', alpha=0.5)

plt.tight_layout()
plt.savefig(image_filename, dpi=120, bbox_inches='tight')
print(f'Chart saved as {image_filename}')
