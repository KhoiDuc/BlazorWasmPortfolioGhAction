import pandas as pd
import numpy as np
import yfinance as yf
import xgboost as xgb
import warnings
import os
import json
import matplotlib.pyplot as plt
import matplotlib
matplotlib.use('Agg')  # Non-interactive backend

# ASK AI TO COMPLETE THE FOLLOWING PROMPT:

# Role: You are a senior financial analyst specializing in Gold (XAU/USD), Macroeconomics, and Geopolitics.

# Task: Analyze the user's provided news text and quantify its potential impact on the market for the NEXT WEEK into 4 numeric variables.

# Rules for scoring:

# 1. geo_score (0.0 to 10.0) - Geopolitical Risk:
#    - 0-2: Peace, trade agreements, stability.
#    - 3-4: Mild tension, sanctions, diplomatic disputes.
#    - 5-6: Local conflict, riots, military drills (e.g., Taiwan drills).
#    - 7-8: WAR / Direct armed conflict (e.g., Russia-Ukraine, Israel-Gaza).
#    - 9-10: Major Crisis / World War risk (Nuclear threats, Superpower collision).

# 2. vix (10.0 to 80.0) - Market Fear Sentiment:
#    - 10-15: Euphoria / Complacency (Stock market booming).
#    - 16-20: Normal market conditions.
#    - 21-30: Nervous / Anxiety (Inflation fears, bad earnings).
#    - 31-50: PANIC (Crash, Pandemic, Black Swan events).
#    - >50: Total Collapse (Financial system failure).

# 3. dxy_pct (-2.0 to +2.0) - USD Strength Change (%):
#    - Positive (+): USD strengthens (Fed hikes rates, strong US economy) -> Gold DOWN.
#    - Negative (-): USD weakens (Fed cuts rates, US recession) -> Gold UP.
#    - Range: Normal news is +/- 0.1 to 0.5. Major monetary policy shifts are +/- 1.0 to 2.0.

# 4. yield_pct (-5.0 to +5.0) - US 10Y Bond Yield Change (%):
#    - Positive (+): Yields rise (Bond sell-off, inflation spikes) -> Gold DOWN.
#    - Negative (-): Yields fall (Bond rally, flight to safety) -> Gold UP.
#    - Range: Normal fluctuation is +/- 0.5 to 1.0. Extreme events are +/- 3.0 to 5.0.

# Input News: "{USER_INPUT_NEWS}"

# Output Requirement:
# - Return ONLY a valid JSON object. Do not include markdown formatting (```json).
# - Estimate values based on the logic above.

# JSON Format:
# {
#   "geo_score": <float>,
#   "vix": <float>,
#   "dxy_pct": <float>,
#   "yield_pct": <float>,
#   "reasoning": "<short explanation under 30 words>"
# }

# Paste the result to gold_price_model.json in the same directory as the script and run python.

# Tắt cảnh báo
warnings.filterwarnings('ignore')

print("=" * 70)
print("GOLD PRICE PREDICTION V8 - WEEKLY FORECAST")
print("=" * 70)

# Cấu hình đường dẫn file data
DATA_FILE = 'gold_macro_data_full.csv'

# ==========================================
# 1. HÀM TẢI DỮ LIỆU THÔNG MINH (SMART LOADER)
# ==========================================
def load_data():
    # Kiểm tra nếu file đã tồn tại thì load lên cho nhanh
    if os.path.exists(DATA_FILE):
        print(f"[1/5] Tìm thấy file dữ liệu cũ '{DATA_FILE}'. Đang đọc...")
        df = pd.read_csv(DATA_FILE, index_col=0, parse_dates=True)
        # Resample lại để chắc chắn đúng format weekly
        df_weekly = df.resample('W-FRI').last().ffill()
        return df_weekly
    
    # Nếu chưa có file thì tải mới
    print("[1/5] Chưa có dữ liệu. Đang tải từ Yahoo Finance (Mất khoảng 10-20s)...")
    
    tickers = {
        'Gold': 'GC=F',           # Vàng
        'Silver': 'SI=F',         # Bạc
        'DXY': 'DX-Y.NYB',        # USD Index
        'US10Y': '^TNX',          # Lợi suất trái phiếu 10 năm
        'TIPS': 'TIP',            # Quỹ trái phiếu chống lạm phát (Real Yield Proxy)
        'SP500': '^GSPC',         # Chứng khoán Mỹ
        'VIX': '^VIX',            # Chỉ số sợ hãi
        'Miners': 'GDX',          # Cổ phiếu mỏ vàng
        'Oil': 'CL=F'             # Dầu thô
    }

    data = pd.DataFrame()
    try:
        for name, ticker in tickers.items():
            print(f"  - Downloading {name} ({ticker})...")
            df = yf.download(ticker, start='2018-01-01', progress=False)
            
            # Xử lý format mới của yfinance (MultiIndex)
            if isinstance(df.columns, pd.MultiIndex):
                data[name] = df['Close'].iloc[:, 0]
            else:
                data[name] = df['Close']
                
        # Resample Weekly
        df_weekly = data.resample('W-FRI').last().ffill()
        
        # # Lưu xuống file để lần sau đỡ phải tải lại
        df_weekly.to_csv(DATA_FILE)
        print(f"  ✓ Đã lưu dữ liệu vào '{DATA_FILE}'")
        
        return df_weekly

    except Exception as e:
        print(f"❌ Lỗi tải dữ liệu: {e}")
        return pd.DataFrame()

# ==========================================
# 2. FEATURE ENGINEERING
# ==========================================
def create_features(df_input):
    print("[2/5] Đang tạo features (Feature Engineering)...")
    df = df_input.copy()
    
    # Nạp dữ liệu sự kiện lịch sử (Hardcode cho mục đích training)
    df['Geo_Score'] = 1.0
    df.loc['2022-02':'2022-04', 'Geo_Score'] = 9.0  # Nga-Ukraine
    df.loc['2023-10':'2023-11', 'Geo_Score'] = 8.0  # Israel-Hamas
    
    # Target: Return tuần tới
    df['Target_Return'] = df['Gold'].pct_change().shift(-1)
    
    # Tính toán các chỉ số
    df['Gold_Ret'] = df['Gold'].pct_change()
    df['DXY_Ret'] = df['DXY'].pct_change()
    
    # REAL YIELD: (Yield danh nghĩa / Giá TIPS)
    df['Real_Yield_Proxy'] = df['US10Y'] / df['TIPS']
    df['Real_Yield_Change'] = df['Real_Yield_Proxy'].pct_change()
    
    df['VIX_Level'] = df['VIX']
    df['Fear_Factor'] = df['VIX_Level'] * df['Geo_Score']
    
    df.dropna(inplace=True)
    return df

# ==========================================
# 3. TRAINING
# ==========================================
def train_model(df):
    print("[3/5] Đang train model XGBoost...")
    
    features = ['Gold_Ret', 'DXY_Ret', 'Real_Yield_Change', 'VIX_Level', 'Geo_Score', 'Fear_Factor']
    X = df[features]
    y = df['Target_Return']
    
    # Chia train/test
    split = int(len(X) * 0.9)
    X_train, y_train = X.iloc[:split], y.iloc[:split]
    
    # Trọng số cho sự kiện chiến tranh
    weights = np.where(X_train['Geo_Score'] > 5, 5.0, 1.0)
    
    model = xgb.XGBRegressor(
        n_estimators=500, 
        learning_rate=0.04, 
        max_depth=5,
        objective='reg:squarederror', 
        n_jobs=-1
    )
    
    model.fit(X_train, y_train, sample_weight=weights)
    return model, features, split

# ==========================================
# 4. VISUALIZATION FUNCTION
# ==========================================
def generate_visualization(model, df, features, split, raw_df):
    print("[4/5] Đang tạo biểu đồ phân tích...")
    
    X = df[features]
    y = df['Target_Return']
    
    # Predictions
    X_test = X.iloc[split:]
    y_pred_test = model.predict(X_test)
    
    # Convert returns to prices
    gold_prices = raw_df['Gold'].loc[df.index]
    actual_prices_test = gold_prices.iloc[split:]
    predicted_prices_test = actual_prices_test.shift(1) * (1 + y_pred_test)
    
    # Setup figure
    fig = plt.figure(figsize=(18, 12))
    
    # 1. Actual vs Predicted (Test Set)
    ax1 = plt.subplot(2, 3, 1)
    valid_mask = ~(actual_prices_test.isna() | predicted_prices_test.isna())
    ax1.plot(actual_prices_test[valid_mask].index, actual_prices_test[valid_mask].values, 
             label='Actual', color='blue', linewidth=2)
    ax1.plot(predicted_prices_test[valid_mask].index, predicted_prices_test[valid_mask].values, 
             label='Predicted', color='red', linewidth=2, alpha=0.7)
    ax1.set_title('Gold Price: Actual vs Predicted (Test Set)', fontsize=12, fontweight='bold')
    ax1.set_ylabel('Gold Price (USD)')
    ax1.legend()
    ax1.grid(True, alpha=0.3)
    
    # 2. Prediction Error Over Time
    ax2 = plt.subplot(2, 3, 2)
    errors = (actual_prices_test - predicted_prices_test).dropna()
    ax2.fill_between(errors.index, errors.values, alpha=0.5, color='salmon')
    ax2.plot(errors.index, errors.values, color='red', linewidth=1)
    ax2.set_title('Prediction Error Over Time', fontsize=12, fontweight='bold')
    ax2.set_ylabel('Error (USD)')
    ax2.grid(True, alpha=0.3)
    ax2.axhline(y=0, color='black', linestyle='--', linewidth=1)
    
    # 3. Feature Importance
    ax3 = plt.subplot(2, 3, 3)
    importance = model.feature_importances_
    feature_importance = pd.DataFrame({'feature': features, 'importance': importance})
    feature_importance = feature_importance.sort_values('importance', ascending=True)
    ax3.barh(feature_importance['feature'], feature_importance['importance'], color='navy')
    ax3.set_title('Feature Importance', fontsize=12, fontweight='bold')
    ax3.set_xlabel('Importance')
    
    # 4. Scatter Plot
    ax4 = plt.subplot(2, 3, 4)
    actual_clean = actual_prices_test[valid_mask]
    predicted_clean = predicted_prices_test[valid_mask]
    ax4.scatter(actual_clean, predicted_clean, alpha=0.6, s=50)
    if len(actual_clean) > 0:
        min_val = min(actual_clean.min(), predicted_clean.min())
        max_val = max(actual_clean.max(), predicted_clean.max())
        ax4.plot([min_val, max_val], [min_val, max_val], 'r--', linewidth=2, label='Perfect Prediction')
    ax4.set_title('Actual vs Predicted Scatter Plot', fontsize=12, fontweight='bold')
    ax4.set_xlabel('Actual Gold Price')
    ax4.set_ylabel('Predicted Gold Price')
    ax4.legend()
    ax4.grid(True, alpha=0.3)
    
    # 5. Complete History
    ax5 = plt.subplot(2, 3, 5)
    train_prices = gold_prices.iloc[:split]
    ax5.plot(train_prices.index, train_prices.values, label='Train Data', color='blue', linewidth=1.5)
    ax5.plot(actual_prices_test[valid_mask].index, actual_prices_test[valid_mask].values, 
             label='Test Data (Actual)', color='green', linewidth=1.5)
    ax5.plot(predicted_prices_test[valid_mask].index, predicted_prices_test[valid_mask].values, 
             label='Test Data (Predicted)', color='red', linestyle='--', linewidth=1.5)
    ax5.axvline(x=gold_prices.index[split], color='black', linestyle=':', linewidth=2, label='Train/Test Split')
    ax5.set_title('Complete History with Predictions', fontsize=12, fontweight='bold')
    ax5.set_ylabel('Gold Price (USD)')
    ax5.legend()
    ax5.grid(True, alpha=0.3)
    
    # 6. Error Distribution
    ax6 = plt.subplot(2, 3, 6)
    if len(errors) > 0:
        ax6.hist(errors, bins=30, color='skyblue', edgecolor='black', alpha=0.7)
        ax6.axvline(x=errors.mean(), color='red', linestyle='--', linewidth=2, 
                   label=f'Mean: ${errors.mean():.2f}')
        mae = np.abs(errors).mean()
        ax6.set_title(f'Error Distribution (MAE: ${mae:.2f})', fontsize=12, fontweight='bold')
        ax6.set_xlabel('Prediction Error (USD)')
        ax6.set_ylabel('Frequency')
        ax6.legend()
    ax6.grid(True, alpha=0.3)
    
    plt.tight_layout()
    output_file = 'gold_prediction_analysis.png'
    plt.savefig(output_file, dpi=150, bbox_inches='tight')
    print(f"   ✓ Đã lưu biểu đồ vào '{output_file}'")
    plt.close()

# ==========================================
# 5. PREDICTION FUNCTION
# ==========================================
def predict_scenario(model, df_last_row, scenario_name, params, current_gold_price):
    # Lấy dữ liệu nền tảng mới nhất
    input_row = df_last_row.copy()
    
    # Gán thông số từ kịch bản
    input_row['DXY_Ret'] = params['dxy_pct'] / 100
    input_row['VIX_Level'] = params['vix']
    input_row['Geo_Score'] = params['geo_score']
    input_row['Real_Yield_Change'] = params['yield_pct'] / 100
    input_row['Fear_Factor'] = params['vix'] * params['geo_score']
    
    # Predict
    pred_ret = model.predict(input_row)[0]
    
    # Panic Mode Logic
    panic_add = 0.0
    if params['geo_score'] >= 7.0: panic_add += 0.01
    if params['vix'] > 30: panic_add += 0.005
    
    final_ret = pred_ret + panic_add
    
    # Tính giá vàng dự báo
    predicted_gold_price = current_gold_price * (1 + final_ret)
    
    print(f"\n📌 {scenario_name.upper()}")
    print(f"   Input: Geo={params['geo_score']} | VIX={params['vix']} | DXY={params['dxy_pct']}% | Yield={params['yield_pct']}%")
    print(f"   Giá vàng hiện tại: ${current_gold_price:.2f}")
    print(f"   Dự báo tuần tới: {final_ret*100:+.2f}%")
    print(f"   Giá vàng dự báo: ${predicted_gold_price:.2f}")
    
    return final_ret

# ==========================================
# MAIN EXECUTION
# ==========================================
if __name__ == "__main__":
    # 1. Load Data
    raw_df = load_data()
    
    if not raw_df.empty:
        # 2. Create Features
        processed_df = create_features(raw_df)
        
        # 3. Train
        model, feature_names, split_idx = train_model(processed_df)
        
        # 4. Generate Visualization
        generate_visualization(model, processed_df, feature_names, split_idx, raw_df)
        
        # 5. Run Scenarios (Hardcoded examples for Demo)
        print("\n[5/5] CHẠY DỰ BÁO DEMO:")
        last_row = processed_df[feature_names].iloc[[-1]] # Lấy dòng dữ liệu cuối cùng làm nền
        current_gold_price = raw_df['Gold'].iloc[-1]  # Giá vàng hiện tại
        
        # # Kịch bản 1: Bình thường
        # predict_scenario(model, last_row, "Thị trường yên bình", 
        #                  {'dxy_pct': 0.1, 'vix': 15, 'geo_score': 1, 'yield_pct': 0.0}, current_gold_price)
        
        # # Kịch bản 2: Chiến tranh
        # predict_scenario(model, last_row, "Chiến tranh / Khủng hoảng", 
        #                  {'dxy_pct': -0.5, 'vix': 35, 'geo_score': 9, 'yield_pct': -2.0}, current_gold_price)

        # Đọc parameters từ file JSON
        json_file = 'gold_price_model.json'
        if os.path.exists(json_file):
            print(f"\n[6/6] Đọc parameters từ '{json_file}'...")
            with open(json_file, 'r', encoding='utf-8') as f:
                params_data = json.load(f)
            print(f"   ✓ Đã load parameters: {params_data}")
        else:
            print(f"\n⚠️  Không tìm thấy '{json_file}', sử dụng giá trị mặc định...")
            params_data = {'dxy_pct': 0.4, 'vix': 28.0, 'geo_score': 6.5, 'yield_pct': -0.6}

        predict_scenario(model, last_row, "2026 prediction", params_data, current_gold_price)

        print("\n✅ XONG! File data đã được lưu tại:", DATA_FILE)
    else:
        print("❌ Không thể chạy tiếp do lỗi dữ liệu.")