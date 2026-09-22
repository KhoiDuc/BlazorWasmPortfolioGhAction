import pandas as pd
import numpy as np
import warnings
import os
from pycaret.regression import *
import matplotlib.pyplot as plt
import matplotlib
matplotlib.use('Agg')

warnings.filterwarnings('ignore')

print("=" * 70)
print("SO SÁNH MODELS BẰNG PYCARET - GOLD PRICE PREDICTION")
print("=" * 70)

DATA_FILE = 'gold_macro_data_full.csv'

# ==========================================
# 1. LOAD DATA
# ==========================================
def load_data():
    """Load and prepare data"""
    if os.path.exists(DATA_FILE):
        print(f"\n[1/5] Đang đọc dữ liệu từ '{DATA_FILE}'...")
        df = pd.read_csv(DATA_FILE, index_col=0, parse_dates=True)
        df_weekly = df.resample('W-FRI').last().ffill()
        print(f"   ✓ Đã load {len(df_weekly)} weeks of data")
        return df_weekly
    else:
        print("❌ Không tìm thấy file dữ liệu.")
        return pd.DataFrame()

# ==========================================
# 2. CREATE FEATURES
# ==========================================
def create_features(df_input):
    """Create features for modeling"""
    print("\n[2/5] Đang tạo features...")
    df = df_input.copy()
    
    # GEO SCORE (Historical Events)
    df['Geo_Score'] = 1.0
    df.loc['2022-02':'2022-04', 'Geo_Score'] = 9.0  # Russia-Ukraine
    df.loc['2023-10':'2023-11', 'Geo_Score'] = 8.0  # Israel-Hamas
    df.loc['2020-03':'2020-05', 'Geo_Score'] = 6.0  # COVID-19
    
    # BASIC RETURNS
    df['Gold_Ret'] = df['Gold'].pct_change()
    df['DXY_Ret'] = df['DXY'].pct_change()
    df['SP500_Ret'] = df['SP500'].pct_change()
    df['Oil_Ret'] = df['Oil'].pct_change()
    df['Silver_Ret'] = df['Silver'].pct_change()
    
    # LAGGED FEATURES
    for lag in [1, 2, 3, 4]:
        df[f'Gold_Ret_Lag{lag}'] = df['Gold_Ret'].shift(lag)
        df[f'DXY_Ret_Lag{lag}'] = df['DXY_Ret'].shift(lag)
        df[f'VIX_Lag{lag}'] = df['VIX'].shift(lag)
    
    # ROLLING STATISTICS
    for window in [4, 8, 12]:
        df[f'Gold_MA{window}'] = df['Gold'].rolling(window).mean()
        df[f'Gold_Deviation_MA{window}'] = (df['Gold'] - df[f'Gold_MA{window}']) / df[f'Gold_MA{window}']
        df[f'Gold_Std{window}'] = df['Gold_Ret'].rolling(window).std()
        df[f'VIX_MA{window}'] = df['VIX'].rolling(window).mean()
        df[f'Gold_Price_Position{window}'] = df['Gold'] / df[f'Gold_MA{window}'] - 1
    
    # MOMENTUM INDICATORS
    df['Gold_Momentum_4w'] = df['Gold'].pct_change(4)
    df['Gold_Momentum_8w'] = df['Gold'].pct_change(8)
    df['Gold_Momentum_12w'] = df['Gold'].pct_change(12)
    df['Gold_Acceleration'] = df['Gold_Ret'] - df['Gold_Ret'].shift(1)
    
    # VOLATILITY INDICATORS
    df['VIX_Change'] = df['VIX'].pct_change()
    df['VIX_Spike'] = (df['VIX'] > df['VIX'].rolling(12).mean() * 1.5).astype(int)
    
    # REAL YIELD
    df['Real_Yield_Proxy'] = df['US10Y'] / df['TIPS']
    df['Real_Yield_Change'] = df['Real_Yield_Proxy'].pct_change()
    df['Real_Yield_MA4'] = df['Real_Yield_Proxy'].rolling(4).mean()
    
    # FEAR FACTOR
    df['Fear_Factor'] = df['VIX'] * df['Geo_Score']
    df['Fear_Factor_Change'] = df['Fear_Factor'].pct_change()
    
    # CORRELATION FEATURES
    df['Gold_Silver_Ratio'] = df['Gold'] / df['Silver']
    df['Gold_Silver_Ratio_Change'] = df['Gold_Silver_Ratio'].pct_change()
    df['Gold_DXY_Divergence'] = df['Gold_Ret'] + df['DXY_Ret']
    
    # MARKET REGIME
    df['Risk_On'] = ((df['SP500_Ret'] > 0) & (df['VIX'] < 20)).astype(int)
    df['Risk_Off'] = ((df['SP500_Ret'] < 0) & (df['VIX'] > 25)).astype(int)
    
    # TARGET: Direct price prediction
    df['Target_Price'] = df['Gold'].shift(-1)
    
    df.dropna(inplace=True)
    
    print(f"   ✓ Tạo {len(df.columns)} features")
    print(f"   ✓ Data points: {len(df)}")
    
    return df

# ==========================================
# 3. SELECT FEATURES
# ==========================================
def select_features(df):
    """Select features for modeling"""
    # Exclude targets and raw prices
    exclude_cols = ['Gold', 'Silver', 'DXY', 'US10Y', 'TIPS', 'SP500', 
                    'VIX', 'Miners', 'Oil', 'Target_Price']
    
    feature_cols = [col for col in df.columns if col not in exclude_cols]
    
    return feature_cols

# ==========================================
# 4. COMPARE MODELS
# ==========================================
def compare_all_models(df, feature_cols):
    """Compare multiple models using PyCaret"""
    print("\n[3/5] Đang setup PyCaret và so sánh models...")
    
    # Prepare data
    df_model = df[feature_cols + ['Target_Price']].copy()
    df_model.rename(columns={'Target_Price': 'target'}, inplace=True)
    
    # Split train/test
    split_idx = int(len(df_model) * 0.85)
    train_data = df_model.iloc[:split_idx]
    test_data = df_model.iloc[split_idx:]
    
    print(f"   ✓ Train samples: {len(train_data)}")
    print(f"   ✓ Test samples: {len(test_data)}")
    
    # Setup PyCaret
    exp = setup(
        data=train_data,
        target='target',
        train_size=0.85,
        session_id=42,
        verbose=False,
        fold=5,
        normalize=True,
        transformation=False,
        remove_outliers=True,
        n_jobs=-1
    )
    
    # Compare models
    print("\n   Đang so sánh models (có thể mất vài phút)...")
    top_models = compare_models(sort='RMSE', n_select=5, verbose=False)
    
    # Get comparison results
    comparison_df = pull()
    
    print("\n" + "="*70)
    print("TOP 5 MODELS:")
    print("="*70)
    for i, model in enumerate(top_models[:5], 1):
        model_name = type(model).__name__
        mae = comparison_df.loc[comparison_df.index[i-1], 'MAE']
        rmse = comparison_df.loc[comparison_df.index[i-1], 'RMSE']
        r2 = comparison_df.loc[comparison_df.index[i-1], 'R2']
        print(f"{i}. {model_name:25s} - MAE: {mae:.4f}  RMSE: {rmse:.4f}  R²: {r2:.4f}")
    
    return top_models[0], test_data, comparison_df, df

# ==========================================
# 5. EVALUATE BEST MODEL
# ==========================================
def evaluate_best_model(best_model, test_data, df):
    """Evaluate best model on test set"""
    print("\n[4/5] Đang đánh giá best model trên test set...")
    
    # Make predictions
    predictions = predict_model(best_model, data=test_data, verbose=False)
    
    # Calculate metrics
    mae = np.abs(predictions['target'] - predictions['prediction_label']).mean()
    rmse = np.sqrt(((predictions['target'] - predictions['prediction_label'])**2).mean())
    r2 = 1 - (np.sum((predictions['target'] - predictions['prediction_label'])**2) / 
              np.sum((predictions['target'] - predictions['target'].mean())**2))
    mape = np.mean(np.abs((predictions['target'] - predictions['prediction_label']) / predictions['target'])) * 100
    
    print(f"\n   {'='*60}")
    print(f"   PERFORMANCE METRICS")
    print(f"   {'='*60}")
    print(f"   Model: {type(best_model).__name__}")
    print(f"   MAE:   ${mae:.2f}")
    print(f"   RMSE:  ${rmse:.2f}")
    print(f"   R²:    {r2:.4f}")
    print(f"   MAPE:  {mape:.2f}%")
    print(f"   {'='*60}")
    
    return predictions

# ==========================================
# 6. SAVE BEST MODEL
# ==========================================
def save_best_model(best_model):
    """Save best model to file"""
    print("\n[5/5] Đang lưu best model...")
    
    model_filename = 'gold_price_best_model'
    save_model(best_model, model_filename)
    print(f"   ✓ Đã lưu model vào '{model_filename}.pkl'")

# ==========================================
# 7. MAKE PREDICTION WITH BEST MODEL
# ==========================================
def make_prediction(best_model, df):
    """Make prediction with best model"""
    print("\n" + "="*70)
    print("DỰ ĐOÁN GIÁ VÀNG")
    print("="*70)
    
    # Get latest features
    latest_row = df.iloc[-1:].copy()
    
    # Prepare features
    exclude_cols = ['Gold', 'Silver', 'DXY', 'US10Y', 'TIPS', 'SP500', 
                    'VIX', 'Miners', 'Oil', 'Target_Price']
    feature_cols = [col for col in latest_row.columns if col not in exclude_cols]
    
    X_pred = latest_row[feature_cols]
    
    # Make prediction
    prediction = predict_model(best_model, data=X_pred, verbose=False)
    predicted_price = prediction['prediction_label'].values[0]
    
    # Current price
    current_price = df['Gold'].iloc[-1]
    price_change = predicted_price - current_price
    price_change_pct = (price_change / current_price) * 100
    
    print(f"\nGiá vàng hiện tại: ${current_price:,.2f}")
    print(f"Dự báo tuần tới: {price_change_pct:+.2f}%")
    print(f"Giá vàng dự báo: ${predicted_price:,.2f}")
    
    if price_change > 0:
        print("\n📈 Outlook: BULLISH")
    else:
        print("\n📉 Outlook: BEARISH")

# ==========================================
# MAIN EXECUTION
# ==========================================
if __name__ == "__main__":
    # 1. Load Data
    raw_df = load_data()
    
    if not raw_df.empty:
        # 2. Create Features
        df = create_features(raw_df)
        
        # 3. Select Features
        feature_cols = select_features(df)
        print(f"\n   Selected {len(feature_cols)} features for modeling")
        
        # 4. Compare Models
        best_model, test_data, comparison_df, df_full = compare_all_models(df, feature_cols)
        
        # 5. Evaluate Best Model
        predictions = evaluate_best_model(best_model, test_data, df_full)
        
        # 6. Save Best Model
        save_best_model(best_model)
        
        # 7. Make Prediction
        make_prediction(best_model, df_full)
        
        print("\n" + "="*70)
        print("✅ HOÀN TẤT! Best model đã được lưu")
        print("="*70)
        print("\n💡 Tip: Chạy 'python3 visualize_model_comparison.py' để xem biểu đồ so sánh")
        
        # Save comparison results for visualization
        comparison_df.to_csv('model_comparison_results.csv')
        print("\n💾 Đã lưu kết quả so sánh vào 'model_comparison_results.csv'")
        
    else:
        print("❌ Không thể chạy do lỗi dữ liệu.")
