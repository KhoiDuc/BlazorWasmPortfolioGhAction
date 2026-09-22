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
print("Training GOLD Price Prediction Model")
print("=" * 70)

DATA_FILE = 'gold_macro_data_full.csv'

# ==========================================
# 1. LOAD DATA
# ==========================================
def load_data():
    if os.path.exists(DATA_FILE):
        print(f"[1/6] Tìm thấy file dữ liệu '{DATA_FILE}'. Đang đọc...")
        df = pd.read_csv(DATA_FILE, index_col=0, parse_dates=True)
        df_weekly = df.resample('W-FRI').last().ffill()
        return df_weekly
    else:
        print("❌ Không tìm thấy file dữ liệu.")
        return pd.DataFrame()

# ==========================================
# 2. ADVANCED FEATURE ENGINEERING
# ==========================================
def create_advanced_features(df_input):
    print("[2/6] Đang tạo Advanced Features...")
    df = df_input.copy()
    
    # ===== GEO SCORE (Historical Events) =====
    df['Geo_Score'] = 1.0
    df.loc['2022-02':'2022-04', 'Geo_Score'] = 9.0  # Russia-Ukraine
    df.loc['2023-10':'2023-11', 'Geo_Score'] = 8.0  # Israel-Hamas
    df.loc['2020-03':'2020-05', 'Geo_Score'] = 6.0  # COVID-19
    
    # ===== BASIC RETURNS =====
    df['Gold_Ret'] = df['Gold'].pct_change()
    df['DXY_Ret'] = df['DXY'].pct_change()
    df['SP500_Ret'] = df['SP500'].pct_change()
    df['Oil_Ret'] = df['Oil'].pct_change()
    df['Silver_Ret'] = df['Silver'].pct_change()
    
    # ===== LAGGED FEATURES (1-4 weeks ago) =====
    for lag in [1, 2, 3, 4]:
        df[f'Gold_Ret_Lag{lag}'] = df['Gold_Ret'].shift(lag)
        df[f'DXY_Ret_Lag{lag}'] = df['DXY_Ret'].shift(lag)
        df[f'VIX_Lag{lag}'] = df['VIX'].shift(lag)
    
    # ===== ROLLING STATISTICS (4, 8, 12 weeks) =====
    for window in [4, 8, 12]:
        # Rolling Mean
        df[f'Gold_MA{window}'] = df['Gold'].rolling(window).mean()
        df[f'Gold_Deviation_MA{window}'] = (df['Gold'] - df[f'Gold_MA{window}']) / df[f'Gold_MA{window}']
        
        # Rolling Std (Volatility)
        df[f'Gold_Std{window}'] = df['Gold_Ret'].rolling(window).std()
        df[f'VIX_MA{window}'] = df['VIX'].rolling(window).mean()
        
        # Price relative to moving average
        df[f'Gold_Price_Position{window}'] = df['Gold'] / df[f'Gold_MA{window}'] - 1
    
    # ===== MOMENTUM INDICATORS =====
    # RSI-like indicator
    df['Gold_Momentum_4w'] = df['Gold'].pct_change(4)
    df['Gold_Momentum_8w'] = df['Gold'].pct_change(8)
    df['Gold_Momentum_12w'] = df['Gold'].pct_change(12)
    
    # Price acceleration
    df['Gold_Acceleration'] = df['Gold_Ret'] - df['Gold_Ret'].shift(1)
    
    # ===== VOLATILITY INDICATORS =====
    df['VIX_Change'] = df['VIX'].pct_change()
    df['VIX_Spike'] = (df['VIX'] > df['VIX'].rolling(12).mean() * 1.5).astype(int)
    
    # ===== REAL YIELD =====
    df['Real_Yield_Proxy'] = df['US10Y'] / df['TIPS']
    df['Real_Yield_Change'] = df['Real_Yield_Proxy'].pct_change()
    df['Real_Yield_MA4'] = df['Real_Yield_Proxy'].rolling(4).mean()
    
    # ===== FEAR FACTOR =====
    df['Fear_Factor'] = df['VIX'] * df['Geo_Score']
    df['Fear_Factor_Change'] = df['Fear_Factor'].pct_change()
    
    # ===== CORRELATION FEATURES =====
    # Gold vs Silver ratio (historical relationship)
    df['Gold_Silver_Ratio'] = df['Gold'] / df['Silver']
    df['Gold_Silver_Ratio_Change'] = df['Gold_Silver_Ratio'].pct_change()
    
    # Gold vs DXY inverse relationship
    df['Gold_DXY_Divergence'] = df['Gold_Ret'] + df['DXY_Ret']  # Should be negative
    
    # ===== MARKET REGIME INDICATORS =====
    # Risk-On vs Risk-Off
    df['Risk_On'] = ((df['SP500_Ret'] > 0) & (df['VIX'] < 20)).astype(int)
    df['Risk_Off'] = ((df['SP500_Ret'] < 0) & (df['VIX'] > 25)).astype(int)
    
    # ===== TARGET: Try multiple options =====
    # Option 1: Simple return (original)
    df['Target_Return'] = df['Gold'].pct_change().shift(-1)
    
    # Option 2: Log return (better for percentages)
    df['Target_LogReturn'] = np.log(df['Gold'] / df['Gold'].shift(1)).shift(-1)
    
    # Option 3: Price direction (classification-style)
    df['Target_Direction'] = np.sign(df['Target_Return'])
    
    # Option 4: Direct price prediction
    df['Target_Price'] = df['Gold'].shift(-1)
    
    # ===== CLEAN DATA =====
    df.dropna(inplace=True)
    
    print(f"   ✓ Created {len(df.columns)} features")
    print(f"   ✓ Data points: {len(df)}")
    
    return df

# ==========================================
# 3. FEATURE SELECTION
# ==========================================
def select_features(df):
    """Select most important features"""
    # Exclude targets and raw prices
    exclude_cols = ['Gold', 'Silver', 'DXY', 'US10Y', 'TIPS', 'SP500', 
                    'VIX', 'Miners', 'Oil', 'Target_Return', 'Target_LogReturn',
                    'Target_Direction', 'Target_Price', 'Geo_Score']
    
    feature_cols = [col for col in df.columns if col not in exclude_cols]
    
    # Include Geo_Score but remove raw prices
    feature_cols.append('Geo_Score')
    
    return feature_cols

# ==========================================
# 4. COMPARE MODELS WITH DIFFERENT TARGETS
# ==========================================
def compare_models_multiple_targets(df, feature_cols):
    print("[3/6] Đang thử nghiệm với 3 loại target khác nhau...")
    
    results = {}
    
    # ===== EXPERIMENT 1: Predict Return (Original) =====
    print("\n   [1] Experiment: Predict Return (pct_change)")
    df_exp1 = df[feature_cols + ['Target_Return']].copy()
    df_exp1.rename(columns={'Target_Return': 'target'}, inplace=True)
    
    split_idx = int(len(df_exp1) * 0.85)
    train_data = df_exp1.iloc[:split_idx]
    test_data = df_exp1.iloc[split_idx:]
    
    exp1 = setup(
        data=train_data,
        target='target',
        train_size=0.85,
        session_id=42,
        verbose=False,
        fold=5,
        normalize=True,
        transformation=True,  # Try transformation
        remove_outliers=True,  # Remove outliers
        n_jobs=-1
    )
    
    top_models_exp1 = compare_models(sort='RMSE', n_select=3, verbose=False)
    results['Return'] = (top_models_exp1[0], test_data, split_idx)
    
    # ===== EXPERIMENT 2: Predict Log Return =====
    print("\n   [2] Experiment: Predict Log Return")
    df_exp2 = df[feature_cols + ['Target_LogReturn']].copy()
    df_exp2.rename(columns={'Target_LogReturn': 'target'}, inplace=True)
    
    train_data = df_exp2.iloc[:split_idx]
    test_data = df_exp2.iloc[split_idx:]
    
    exp2 = setup(
        data=train_data,
        target='target',
        train_size=0.85,
        session_id=42,
        verbose=False,
        fold=5,
        normalize=True,
        transformation=True,
        remove_outliers=True
    )
    
    top_models_exp2 = compare_models(sort='RMSE', n_select=3, verbose=False)
    results['LogReturn'] = (top_models_exp2[0], test_data, split_idx)
    
    # ===== EXPERIMENT 3: Predict Price Directly =====
    print("\n   [3] Experiment: Predict Price Directly")
    df_exp3 = df[feature_cols + ['Target_Price']].copy()
    df_exp3.rename(columns={'Target_Price': 'target'}, inplace=True)
    
    train_data = df_exp3.iloc[:split_idx]
    test_data = df_exp3.iloc[split_idx:]
    
    exp3 = setup(
        data=train_data,
        target='target',
        train_size=0.85,
        session_id=42,
        verbose=False,
        fold=5,
        normalize=True,
        transformation=False,  # Don't transform prices
        remove_outliers=True
    )
    
    top_models_exp3 = compare_models(sort='RMSE', n_select=3, verbose=False)
    results['Price'] = (top_models_exp3[0], test_data, split_idx)
    
    return results, df

# ==========================================
# 5. EVALUATE AND COMPARE RESULTS
# ==========================================
def evaluate_results(results, raw_df):
    print("\n[4/6] So sánh kết quả các experiments...")
    print("=" * 70)
    
    comparison = []
    
    for exp_name, (model, test_data, split_idx) in results.items():
        predictions = predict_model(model, data=test_data, verbose=False)
        
        # Calculate metrics
        mae = np.abs(predictions['target'] - predictions['prediction_label']).mean()
        rmse = np.sqrt(((predictions['target'] - predictions['prediction_label'])**2).mean())
        r2 = 1 - (np.sum((predictions['target'] - predictions['prediction_label'])**2) / 
                  np.sum((predictions['target'] - predictions['target'].mean())**2))
        
        comparison.append({
            'Experiment': exp_name,
            'Model': type(model).__name__,
            'MAE': mae,
            'RMSE': rmse,
            'R²': r2
        })
        
        print(f"\n{exp_name} - {type(model).__name__}")
        print(f"   MAE:  {mae:.6f}")
        print(f"   RMSE: {rmse:.6f}")
        print(f"   R²:   {r2:.6f}")
    
    comparison_df = pd.DataFrame(comparison)
    
    # Find best experiment
    best_exp = comparison_df.loc[comparison_df['R²'].idxmax(), 'Experiment']
    print(f"\n{'='*70}")
    print(f"🏆 BEST EXPERIMENT: {best_exp}")
    print(f"{'='*70}")
    
    return comparison_df, best_exp

# ==========================================
# 6. VISUALIZE BEST MODEL
# ==========================================
def visualize_best_model(results, best_exp, raw_df, df):
    print(f"\n[5/6] Visualizing best model ({best_exp})...")
    
    model, test_data, split_idx = results[best_exp]
    predictions = predict_model(model, data=test_data, verbose=False)
    
    # Get actual gold prices
    gold_prices = raw_df['Gold'].loc[df.index]
    actual_prices_test = gold_prices.iloc[split_idx:split_idx+len(test_data)]
    
    # Convert predictions to prices based on experiment type
    if best_exp == 'Return':
        pred_returns = predictions['prediction_label'].values
        current_prices = actual_prices_test.shift(1).fillna(method='bfill').values
        pred_prices = current_prices * (1 + pred_returns)
    elif best_exp == 'LogReturn':
        pred_log_returns = predictions['prediction_label'].values
        current_prices = actual_prices_test.shift(1).fillna(method='bfill').values
        pred_prices = current_prices * np.exp(pred_log_returns)
    else:  # Price
        pred_prices = predictions['prediction_label'].values
    
    # Create visualization
    fig = plt.figure(figsize=(20, 12))
    
    # Plot 1: Actual vs Predicted Prices
    ax1 = plt.subplot(2, 3, 1)
    ax1.plot(actual_prices_test.index, actual_prices_test.values, 
             label='Actual', color='black', linewidth=2.5, alpha=0.8)
    ax1.plot(actual_prices_test.index, pred_prices, 
             label='Predicted', color='red', linewidth=2, alpha=0.7, linestyle='--')
    ax1.set_title(f'Gold Price: Actual vs Predicted ({best_exp})', 
                 fontsize=14, fontweight='bold')
    ax1.set_ylabel('Price (USD)')
    ax1.legend(loc='best')
    ax1.grid(True, alpha=0.3)
    
    # Plot 2: Prediction Error
    ax2 = plt.subplot(2, 3, 2)
    errors = actual_prices_test.values - pred_prices
    ax2.plot(actual_prices_test.index, errors, color='red', linewidth=1.5)
    ax2.axhline(y=0, color='black', linestyle='--', linewidth=1)
    ax2.fill_between(actual_prices_test.index, 0, errors, 
                     where=(errors > 0), color='red', alpha=0.3)
    ax2.fill_between(actual_prices_test.index, 0, errors, 
                     where=(errors <= 0), color='green', alpha=0.3)
    ax2.set_title('Prediction Errors Over Time', fontsize=14, fontweight='bold')
    ax2.set_ylabel('Error (USD)')
    ax2.grid(True, alpha=0.3)
    
    # Plot 3: Error Distribution
    ax3 = plt.subplot(2, 3, 3)
    ax3.hist(errors, bins=25, color='skyblue', edgecolor='black', alpha=0.7)
    ax3.axvline(x=np.mean(errors), color='red', linestyle='--', linewidth=2,
               label=f'Mean: ${np.mean(errors):.2f}')
    ax3.axvline(x=np.median(errors), color='green', linestyle='--', linewidth=2,
               label=f'Median: ${np.median(errors):.2f}')
    ax3.set_title('Error Distribution', fontsize=14, fontweight='bold')
    ax3.set_xlabel('Error (USD)')
    ax3.set_ylabel('Frequency')
    ax3.legend()
    ax3.grid(True, alpha=0.3)
    
    # Plot 4: Percentage Error
    ax4 = plt.subplot(2, 3, 4)
    pct_errors = (errors / actual_prices_test.values) * 100
    ax4.plot(actual_prices_test.index, pct_errors, color='purple', 
            linewidth=1.5, marker='o', markersize=4)
    ax4.axhline(y=0, color='black', linestyle='--', linewidth=1)
    ax4.axhline(y=np.mean(pct_errors), color='red', linestyle='--', linewidth=2,
               label=f'Mean: {np.mean(pct_errors):.2f}%')
    ax4.set_title('Percentage Errors Over Time', fontsize=14, fontweight='bold')
    ax4.set_ylabel('Error (%)')
    ax4.legend()
    ax4.grid(True, alpha=0.3)
    
    # Plot 5: Feature Importance (Top 15)
    ax5 = plt.subplot(2, 3, 5)
    try:
        # Get feature importance from model
        if hasattr(model, 'feature_importances_'):
            importances = model.feature_importances_
            feature_names = test_data.drop('target', axis=1).columns
            indices = np.argsort(importances)[-15:]  # Top 15
            
            ax5.barh(range(len(indices)), importances[indices], color='steelblue')
            ax5.set_yticks(range(len(indices)))
            ax5.set_yticklabels([feature_names[i] for i in indices], fontsize=8)
            ax5.set_title('Top 15 Feature Importances', fontsize=14, fontweight='bold')
            ax5.set_xlabel('Importance')
        else:
            ax5.text(0.5, 0.5, 'Feature importance\nnot available', 
                    ha='center', va='center', fontsize=12)
            ax5.set_title('Feature Importances', fontsize=14, fontweight='bold')
    except:
        ax5.text(0.5, 0.5, 'Feature importance\nnot available', 
                ha='center', va='center', fontsize=12)
        ax5.set_title('Feature Importances', fontsize=14, fontweight='bold')
    
    # Plot 6: Metrics Summary
    ax6 = plt.subplot(2, 3, 6)
    ax6.axis('off')
    
    mae = np.abs(errors).mean()
    rmse = np.sqrt((errors**2).mean())
    mape = np.mean(np.abs(pct_errors))
    r2 = 1 - (np.sum(errors**2) / np.sum((actual_prices_test.values - actual_prices_test.values.mean())**2))
    
    metrics_text = f"""
    MODEL PERFORMANCE SUMMARY
    {'='*40}
    
    Model: {type(model).__name__}
    Target Type: {best_exp}
    
    Mean Absolute Error (MAE): ${mae:.2f}
    Root Mean Squared Error: ${rmse:.2f}
    Mean Absolute % Error: {mape:.2f}%
    R² Score: {r2:.4f}
    
    Median Error: ${np.median(errors):.2f}
    Max Error: ${np.max(np.abs(errors)):.2f}
    
    Test Samples: {len(test_data)}
    """
    
    ax6.text(0.1, 0.5, metrics_text, fontsize=11, family='monospace',
            verticalalignment='center')
    
    plt.tight_layout()
    output_file = 'train_gold_model_results.png'
    plt.savefig(output_file, dpi=150, bbox_inches='tight')
    print(f"   ✓ Đã lưu visualization vào '{output_file}'")
    plt.close()

# ==========================================
# 7. SAVE BEST MODEL
# ==========================================
def save_best_model(results, best_exp):
    print("\n[6/6] Lưu best model...")
    model, _, _ = results[best_exp]
    
    model_filename = f'best_model_{best_exp.lower()}'
    save_model(model, model_filename)
    print(f"   ✓ Đã lưu model vào '{model_filename}.pkl'")

# ==========================================
# MAIN EXECUTION
# ==========================================
if __name__ == "__main__":
    # 1. Load Data
    raw_df = load_data()
    
    if not raw_df.empty:
        # 2. Create Advanced Features
        df = create_advanced_features(raw_df)
        
        # 3. Select Features
        feature_cols = select_features(df)
        print(f"\n   Selected {len(feature_cols)} features for modeling")
        
        # 4. Compare Models with Different Targets
        results, df = compare_models_multiple_targets(df, feature_cols)
        
        # 5. Evaluate Results
        comparison_df, best_exp = evaluate_results(results, raw_df)
        
        # 6. Visualize Best Model
        visualize_best_model(results, best_exp, raw_df, df)
        
        # 7. Save Best Model
        save_best_model(results, best_exp)
        
        
        print("\n" + "="*70)
        print("✅ HOÀN TẤT! Check 'train_gold_model_results.png'")
        print("="*70)
        
        # Print comparison table
        print("\n📊 COMPARISON OF ALL EXPERIMENTS:")
        print(comparison_df.to_string(index=False))
        
    else:
        print("❌ Không thể chạy do lỗi dữ liệu.")
