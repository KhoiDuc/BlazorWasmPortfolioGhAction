import pandas as pd
import yfinance as yf
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
import matplotlib.patheffects as PathEffects
import numpy as np
from datetime import datetime, timedelta
import os
import asyncio
import asyncpg
import io
import urllib.request
from dotenv import load_dotenv

# Load .env từ cùng thư mục với script (scripts/.env)
load_dotenv(os.path.join(os.path.dirname(os.path.abspath(__file__)), '.env'))

# --- CONFIGURATION ---
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
# Output to 'public' folder which is sibling to 'scripts'
OUTPUT_DIR = os.path.join(BASE_DIR, '../www')
OUTPUT_FILENAME = 'assets_rrgchart.png'

# Benchmark
BENCHMARK_TICKER = 'DX-Y.NYB' # US Dollar Index

# Assets List
COMMODITIES = {
    'PreciousMetals': 'GLTR', # Precious Metals Basket (Gold, Silver, Platinum, Palladium)
    'IndustrialMetals': 'DBB', # Base Metals Fund (Copper, Zinc, Aluminum)
    'WTI Crude Oil': 'CL=F'
}

CRYPTO = {
    'CryptoIndex': 'BITW' # Bitwise 10 Crypto Index Fund
}

VN_INDEX = {
    'VNIndex': 'VNM' # Using VNM (VanEck Vietnam ETF) as proxy because ^VNINDEX/^VNI is often unavailable on Yahoo
}

US_INDEX = {
    'S&P 500': '^GSPC'
}

CURRENCY_PAIR = 'VND=X' # USD/VND rate to convert VN stocks to USD

# Treasury Yields (FRED)
TREASURY_YIELDS = {
    'US02Y (FED Policy & Liquidity)': 'DGS2',
    'US10Y (Growth & Inflation)': 'DGS10'
}

# Special Asset
HOUSING_LABEL = 'HCMC Housing'

# --- HELPER FUNCTIONS ---

async def fetch_bank_rate():
    """Lấy 1 record duy nhất từ bảng highest_bank_interest_rate theo config .env.
    Trả về dict {bank, rate, term, channel} hoặc None nếu lỗi."""
    db_port_str = os.environ.get("DB_PORT")
    if db_port_str is None:
        print("⚠️  DB_PORT chưa được đặt trong .env, bỏ qua bank interest rate.")
        return None
    try:
        conn = await asyncpg.connect(
            user=os.environ.get("DB_USER"),
            password=os.environ.get("DB_PASSWORD"),
            database=os.environ.get("DB_NAME"),
            host=os.environ.get("DB_HOST"),
            port=int(db_port_str),
        )
        try:
            row = await conn.fetchrow(
                "SELECT bank, rate, term, channel "
                "FROM public.highest_bank_interest_rate "
                "ORDER BY created_at DESC LIMIT 1"
            )
            if row:
                return {"bank": row["bank"], "rate": float(row["rate"]),
                        "term": row["term"], "channel": row["channel"]}
            else:
                print("⚠️  Bảng highest_bank_interest_rate trống.")
                return None
        finally:
            await conn.close()
    except Exception as e:
        print(f"⚠️  Lỗi kết nối DB để lấy bank rate: {e}")
        return None


def create_synthetic_bank_rate_data(index_dates, annual_rate_pct):
    """Tạo chuỗi giá tổng hợp cho lãi suất ngân hàng.
    annual_rate_pct: lãi suất %/năm (ví dụ 7.3).
    Mô phỏng tài sản tích lũy theo lãi suất cố định (tương tự housing)."""
    days = len(index_dates)
    daily_rate = (1 + annual_rate_pct / 100) ** (1 / 365) - 1
    prices = [100 * ((1 + daily_rate) ** i) for i in range(days)]
    return pd.Series(prices, index=index_dates, name="BankRate")



def fetch_fred_yield(series_id, start_date):
    """Fetches constant maturity Treasury yields directly from FRED CSV."""
    url = f"https://fred.stlouisfed.org/graph/fredgraph.csv?id={series_id}"
    try:
        req = urllib.request.Request(
            url, 
            headers={'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36'}
        )
        with urllib.request.urlopen(req, timeout=10) as response:
            csv_data = response.read().decode('utf-8')
            
        df = pd.read_csv(io.StringIO(csv_data))
        df['observation_date'] = pd.to_datetime(df['observation_date'])
        df.set_index('observation_date', inplace=True)
        df[series_id] = pd.to_numeric(df[series_id], errors='coerce')
        df = df.loc[start_date:]
        df = df.ffill().bfill()
        return df[series_id]
    except Exception as e:
        print(f"Error fetching {series_id} from FRED: {e}")
        return None


def fetch_data(lookback_days=200):
    """Fetches data for all assets and benchmark."""
    start_date = (datetime.now() - timedelta(days=lookback_days)).strftime('%Y-%m-%d')
    print(f"Fetching data starting from {start_date}...")

    # 1. Fetch Benchmark
    bench_df = yf.download(BENCHMARK_TICKER, start=start_date, progress=False)['Close']
    if isinstance(bench_df, pd.DataFrame): bench_df = bench_df.iloc[:, 0] # Handle if multi-col
    bench_df.name = 'Benchmark'

    # 2. Fetch Assets (Batch where possible)
    tickers_map = {**COMMODITIES, **CRYPTO, **VN_INDEX, **US_INDEX}
    all_tickers = list(tickers_map.values()) + [CURRENCY_PAIR]
    
    print(f"Downloading {len(all_tickers)} tickers...")
    data_raw = yf.download(all_tickers, start=start_date, progress=False)['Close']
    
    # Handle Data Structure
    # yfinance might return MultiIndex columns if multiple tickers
    if isinstance(data_raw.columns, pd.MultiIndex):
        # Flatten if needed, but usually we just access by column name
        pass
        
    # Fetch US Treasury yields from FRED
    for name, series_id in TREASURY_YIELDS.items():
        print(f"Fetching {name} ({series_id}) from FRED...")
        yield_series = fetch_fred_yield(series_id, start_date)
        if yield_series is not None:
            # Handle tz-naive and tz-aware index compatibility
            if data_raw.index.tz is not None and yield_series.index.tz is None:
                yield_series.index = yield_series.index.tz_localize(data_raw.index.tz)
            elif data_raw.index.tz is None and yield_series.index.tz is not None:
                yield_series.index = yield_series.index.tz_localize(None)
                
            # Reindex to align exactly with the other financial assets
            data_raw[name] = yield_series.reindex(data_raw.index, method='ffill')
        else:
            fallback_ticker = '^IRX' if 'US02Y' in name else '^TNX'
            print(f"⚠️ FRED failed for {name}. Falling back to yfinance ticker {fallback_ticker}...")
            try:
                fallback_df = yf.download(fallback_ticker, start=start_date, progress=False)['Close']
                if isinstance(fallback_df, pd.DataFrame):
                    fallback_df = fallback_df.iloc[:, 0]
                
                if data_raw.index.tz is not None and fallback_df.index.tz is None:
                    fallback_df.index = fallback_df.index.tz_localize(data_raw.index.tz)
                elif data_raw.index.tz is None and fallback_df.index.tz is not None:
                    fallback_df.index = fallback_df.index.tz_localize(None)
                
                s_data = fallback_df.reindex(data_raw.index, method='ffill')
                if fallback_ticker == '^TNX' and s_data.mean() > 10:
                    s_data = s_data / 10.0
                
                data_raw[name] = s_data
                print(f"✅ Fallback successful for {name} using {fallback_ticker}")
            except Exception as fe:
                print(f"❌ Fallback failed for {name}: {fe}")
        
    return data_raw, bench_df

def create_synthetic_housing_data(index_dates):
    """Creates a synthetic data series for HCMC Housing."""
    # Assumption: Slow, steady growth with low volatility, slightly unconnected to DXY
    # localized trend.
    days = len(index_dates)
    # Start at arbitrary 100, grow 5-10% a year with some random noise
    base_price = 100
    daily_growth = 1.0002 # approx 7% a year
    
    prices = [base_price * (daily_growth ** i) for i in range(days)]
    # Add minimal noise
    noise = np.random.normal(0, 0.2, days)
    prices = prices + noise
    
    return pd.Series(prices, index=index_dates, name=HOUSING_LABEL)

def calculate_rrg(prices_df, benchmark_series):
    """
    Calculates RRG indicators (RS-Ratio, RS-Momentum).
    prices_df: DataFrame of asset prices in USD.
    benchmark_series: Series of benchmark prices (DXY).
    """
    
    # Align Data
    common_index = prices_df.index.intersection(benchmark_series.index)
    prices = prices_df.loc[common_index].ffill().dropna()
    bench = benchmark_series.loc[common_index].ffill().dropna()
    
    # Re-align after dropna
    valid_idx = prices.index.intersection(bench.index)
    prices = prices.loc[valid_idx]
    bench = bench.loc[valid_idx]
    
    rrg_results = {}
    
    # Parameters provided by JdK RRG standard
    window_rs = 100
    window_mom = 10 
    
    print("Calculating RRG metrics...")
    
    for col in prices.columns:
        # Relative Strength (Price / Benchmark)
        rs = 100 * (prices[col] / bench)
        
        # RRG Logic (JdK RRG uses a specific normalization, here we use a simplified robust approximation)
        # 1. RS-Ratio = (RS / MA(RS, window)) * 100 -> Standard logic is slightly more complex involving std dev
        
        # Standard JdK Approximation:
        # RS-Ratio is a normalized measure of trend
        # We use a moving average to smooth trends
        
        # Method A: Simple Ratio
        # ma_rs = rs.rolling(window=window_rs).mean()
        # rs_ratio = 100 * (rs / ma_rs)
        
        # Method B: JdK-like Normalization (More commonly cited in Python implementations)
        # RS-Ratio = 100 + ((RS - MA(RS)) / StdDev(RS)) -- standardized Z-score shifted to 100
        mean_rs = rs.rolling(window=window_rs).mean()
        std_rs = rs.rolling(window=window_rs).std()
        
        if len(std_rs) == 0: continue
        # Avoid division by zero
        if std_rs.iloc[-1] == 0: continue
            
        rs_ratio = 100 + ((rs - mean_rs) / std_rs)
        
        # RS-Momentum = ROC(RS-Ratio)
        # Momentum is the rate of change of the ratio
        # JdK uses 10-day momentum usually, normalized similarly
        mom_raw = 100 * (rs_ratio / rs_ratio.shift(1)) # Simple 1-day momentum of ratio? Or 10-day?
        # Let's use 10-day Rate of Change of RS-Ratio centered around 100
        # No, standard is: Momentum is calculated on Ratio
        
        # Improved approximation:
        # RS-Momentum = 100 + ((RS-Ratio - MA(RS-Ratio)) / StdDev(RS-Ratio))
        # Usually faster window
        
        mean_mom = rs_ratio.rolling(window=window_mom).mean()
        std_mom = rs_ratio.rolling(window=window_mom).std()
        
        rs_momentum = 100 + ((rs_ratio - mean_mom) / std_mom)

        # Smooth to reduce noise
        rs_ratio_smooth = rs_ratio.rolling(window=3).mean()
        rs_momentum_smooth = rs_momentum.rolling(window=3).mean()
        
        res = pd.DataFrame({'RSR': rs_ratio_smooth, 'RSM': rs_momentum_smooth}).dropna()
        if not res.empty:
            rrg_results[col] = res

    return rrg_results

def main():
    # Lấy bank rate từ DB (chạy async trong sync context)
    bank_info = asyncio.run(fetch_bank_rate())
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    
    # 1. Data Fetching
    data_raw, bench_df = fetch_data()
    
    # 2. Data Processing & Conversion
    # We need everything in USD terms
    
    processed_data = pd.DataFrame(index=data_raw.index)
    
    # Get USDVND Rate
    usdvnd_col = None
    if 'VND=X' in data_raw.columns:
        usdvnd_col = data_raw['VND=X']
    elif isinstance(data_raw.columns, pd.MultiIndex) and 'VND=X' in data_raw.columns.get_level_values(0):
         pass # Handle multi-index if needed
         
    # Flatten MultiIndex if necessary (Yahoo sometimes returns Price -> Ticker)
    if isinstance(data_raw.columns, pd.MultiIndex):
        # Assuming level 1 is ticker
        # Often it comes as (PriceType, Ticker) e.g. ('Close', 'AAPL')
        # We fetched only 'Close' so it might be just Ticker columns if we did ['Close']
        pass

    # Quick helper to safely get column
    def get_col(df, ticker):
        if ticker in df.columns:
            return df[ticker]
        return None

    usd_vnd = get_col(data_raw, 'VND=X')
    if usd_vnd is None:
        print("Error: Could not fetch USD/VND rate. Aborting VN30 conversion.")
        return

    # Process Commodities, Crypto & US Index (Already in USD)
    for name, ticker in {**COMMODITIES, **CRYPTO, **US_INDEX}.items():
        s = get_col(data_raw, ticker)
        if s is not None:
            processed_data[name] = s
            
    # Process VNIndex (Convert VND to USD)
    for name, ticker in VN_INDEX.items():
        s = get_col(data_raw, ticker)
        if s is not None:
             # Convert to USD. 
            # Note: Checking if VN stock data is scaled. Usually it's integer VND.
            try:
                processed_data[name] = s / usd_vnd
            except:
                pass

    # Process Treasury Yields
    for name in TREASURY_YIELDS.keys():
        s = get_col(data_raw, name)
        if s is not None:
            processed_data[name] = s

    # Process Housing
    housing_data = create_synthetic_housing_data(processed_data.index)
    processed_data[HOUSING_LABEL] = housing_data

    # Process Bank Interest Rate (synthetic series anchored to annual rate)
    bank_label = None
    if bank_info:
        bank_label = f"{bank_info['bank']} {bank_info['rate']}%({bank_info['term']}-{bank_info['channel']})"
        bank_series = create_synthetic_bank_rate_data(processed_data.index, bank_info['rate'])
        bank_series.name = bank_label
        processed_data[bank_label] = bank_series
        print(f"✅ Bank rate loaded: {bank_label}")
    else:
        print("ℹ️  Không có dữ liệu bank rate, bỏ qua.")
    
    # 3. Calculate RRG
    rrg_map = calculate_rrg(processed_data, bench_df)
    
    # 4. Plotting
    fig, ax = plt.subplots(figsize=(14, 14))
    
    # Crosshairs
    ax.axhline(100, color='black', lw=1)
    ax.axvline(100, color='black', lw=1)
    
    # Quadrant Backgrounds
    # We need to determine limits first to fill, but we can fill large area
    # Or rely on auto-zoom logic later. Let's fill large static area first or dynamic.
    
    # Collect all points to determine global limits
    all_x, all_y = [], []
    for df in rrg_map.values():
        all_x.extend(df['RSR'].values[-20:])
        all_y.extend(df['RSM'].values[-20:])
        
    if not all_x:
        print("No data to plot.")
        return

    # Dynamic Limits
    min_x, max_x = min(all_x), max(all_x)
    min_y, max_y = min(all_y), max(all_y)
    
    pad = 2
    x_lims = [min_x - pad, max_x + pad]
    y_lims = [min_y - pad, max_y + pad]
    
    # Force minimal view around 100
    if x_lims[0] > 98: x_lims[0] = 98
    if x_lims[1] < 102: x_lims[1] = 102
    if y_lims[0] > 98: y_lims[0] = 98
    if y_lims[1] < 102: y_lims[1] = 102
    
    # Quadrant Colors
    # Leading (Right-Top): Green
    ax.fill_between([100, 200], 100, 200, color='#e6f9e6', alpha=0.3)
    # Weakening (Right-Bottom): Yellow
    ax.fill_between([100, 200], 0, 100, color='#fff9e6', alpha=0.3)
    # Lagging (Left-Bottom): Red
    ax.fill_between([0, 100], 0, 100, color='#f9e6e6', alpha=0.3)
    # Improving (Left-Top): Blue
    ax.fill_between([0, 100], 100, 200, color='#e6e6f9', alpha=0.3)
    
    # Add Quadrant Labels
    ax.text(x_lims[1], y_lims[1], 'LEADING', ha='right', va='top', color='green', fontweight='bold', fontsize=14, alpha=0.5)
    ax.text(x_lims[1], y_lims[0], 'WEAKENING', ha='right', va='bottom', color='#b38f00', fontweight='bold', fontsize=14, alpha=0.5)
    ax.text(x_lims[0], y_lims[0], 'LAGGING', ha='left', va='bottom', color='red', fontweight='bold', fontsize=14, alpha=0.5)
    ax.text(x_lims[0], y_lims[1], 'IMPROVING', ha='left', va='top', color='blue', fontweight='bold', fontsize=14, alpha=0.5)

    # Plot Tails and Heads
    tail_len = 7
    
    # Color mapping categories
    colors_map = {
        'Commodities': 'gold',
        'Crypto': 'purple',
        'Housing': 'brown',
        'Stock': '#333333' # Default for stocks
    }
    
    for name, df in rrg_map.items():
        if len(df) < tail_len: continue
        recent = df.tail(tail_len)
        
        # Determine Color
        color = 'gray'
        if name == 'PreciousMetals': color = '#FFD700' # Gold
        elif name == 'IndustrialMetals': color = '#FF4500' # Orange Red
        elif name == 'WTI Crude Oil': color = '#228B22' # Forest Green
        elif name in CRYPTO: color = '#9370db' # Purple
        elif name == HOUSING_LABEL: color = '#8b4513' # Brown
        elif bank_label and name == bank_label: color = '#00ced1' # Dark Turquoise for Bank Rate
        elif name.startswith('US02Y'): color = '#008080' # Teal
        elif name.startswith('US10Y'): color = '#4682B4' # Steel Blue
        elif name == 'S&P 500': color = '#4169E1' # Royal Blue
        else: color = '#2f4f4f' # Dark Slate Gray for Stocks
        
        # Special highlight for Major Indices & Treasury Yields
        lw = 1.0
        alpha = 0.6
        zorder = 3
        if name in ['PreciousMetals', 'IndustrialMetals', 'WTI Crude Oil', 'CryptoIndex', 'VNIndex', 'S&P 500', HOUSING_LABEL] or name.startswith('US02Y') or name.startswith('US10Y') or (bank_label and name == bank_label): 
            lw = 2.5
            alpha = 1.0
            zorder = 5
        
        # Plot Tail
        ax.plot(recent['RSR'], recent['RSM'], color=color, lw=lw, alpha=alpha, zorder=zorder)
        
        # Plot Head
        head = recent.iloc[-1]
        ax.scatter(head['RSR'], head['RSM'], color=color, s=80, zorder=zorder+1, edgecolors='white')
        
        # Label
        txt_offset = 0.2
        ax.text(head['RSR'] + 0.1, head['RSM'] + 0.1, name, fontsize=9, color=color, fontweight='bold', zorder=zorder+2)\
            .set_path_effects([PathEffects.withStroke(linewidth=2, foreground='white')])

    ax.set_xlim(x_lims)
    ax.set_ylim(y_lims)
    
    ax.set_title('Relative Rotation Graph vs USD (DXY)', fontsize=16, fontweight='bold')
    ax.text(1.0, 1.01, f'updated at: {datetime.now().strftime("%Y-%m-%d %H:%M:%S")}', transform=ax.transAxes, ha='right', va='bottom', fontsize=10, color='gray')
    ax.set_xlabel('RS-Ratio (Trend)', fontsize=12)
    ax.set_ylabel('RS-Momentum (Momentum)', fontsize=12)
    ax.grid(True, linestyle='--', alpha=0.5)
    
    out_path = os.path.join(OUTPUT_DIR, OUTPUT_FILENAME)
    plt.tight_layout()
    plt.savefig(out_path, dpi=150)
    print(f"Chart saved to {out_path}")

if __name__ == "__main__":
    main()
