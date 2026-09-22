import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import matplotlib
matplotlib.use('Agg')
import warnings
import os
from pycaret.regression import *

warnings.filterwarnings('ignore')

print("=" * 70)
print("VISUALIZATION - MODEL COMPARISON")
print("=" * 70)

# ==========================================
# LOAD REQUIRED FILES
# ==========================================
def load_comparison_data():
    """Load model comparison results and best model"""
    print("\n[1/3] Đang load data và models...")
    
    # Check if comparison results exist
    if not os.path.exists('model_comparison_results.csv'):
        print("   ❌ Không tìm thấy 'model_comparison_results.csv'")
        print("   → Chạy 'python3 model_comparison_pycaret.py' trước")
        return None, None
    
    # Check if best model exists
    if not os.path.exists('gold_price_best_model.pkl'):
        print("   ❌ Không tìm thấy 'gold_price_best_model.pkl'")
        print("   → Chạy 'python3 model_comparison_pycaret.py' trước")
        return None, None
    
    # Load comparison results
    comparison_df = pd.read_csv('model_comparison_results.csv', index_col=0)
    print(f"   ✓ Đã load {len(comparison_df)} model results")
    
    # Load best model
    best_model = load_model('gold_price_best_model')
    print(f"   ✓ Đã load best model: {type(best_model).__name__}")
    
    return comparison_df, best_model

# ==========================================
# LOAD GOLD DATA FOR PREDICTIONS
# ==========================================
def load_gold_data():
    """Load gold data for making predictions"""
    if not os.path.exists('gold_macro_data_full.csv'):
        return None
    
    df = pd.read_csv('gold_macro_data_full.csv', index_col=0, parse_dates=True)
    df_weekly = df.resample('W-FRI').last().ffill()
    
    # Create features (simplified version)
    df_weekly['Geo_Score'] = 1.0
    df_weekly['Gold_Ret'] = df_weekly['Gold'].pct_change()
    df_weekly['DXY_Ret'] = df_weekly['DXY'].pct_change()
    df_weekly['Target_Price'] = df_weekly['Gold'].shift(-1)
    
    for lag in [1, 2, 3, 4]:
        df_weekly[f'Gold_Ret_Lag{lag}'] = df_weekly['Gold_Ret'].shift(lag)
        df_weekly[f'VIX_Lag{lag}'] = df_weekly['VIX'].shift(lag)
    
    df_weekly['VIX_Change'] = df_weekly['VIX'].pct_change()
    df_weekly['Real_Yield_Proxy'] = df_weekly['US10Y'] / df_weekly['TIPS']
    df_weekly['Fear_Factor'] = df_weekly['VIX'] * df_weekly['Geo_Score']
    
    df_weekly.dropna(inplace=True)
    
    return df_weekly

# ==========================================
# CREATE VISUALIZATION
# ==========================================
def create_visualization(comparison_df, best_model):
    """Create comprehensive visualization of model comparison"""
    print("\n[2/3] Đang tạo visualizations...")
    
    fig = plt.figure(figsize=(20, 12))
    
    # ===== PLOT 1: Top Models Comparison (MAE) =====
    ax1 = plt.subplot(2, 3, 1)
    top_5 = comparison_df.head(5).copy()
    models = [idx[:20] + '...' if len(idx) > 20 else idx for idx in top_5.index]
    mae_values = top_5['MAE'].values
    
    colors = ['#2ecc71' if i == 0 else '#3498db' for i in range(len(mae_values))]
    bars = ax1.barh(models, mae_values, color=colors, alpha=0.7)
    
    # Annotate values
    for i, bar in enumerate(bars):
        width = bar.get_width()
        ax1.text(width, bar.get_y() + bar.get_height()/2, 
                f'{width:.4f}', ha='left', va='center', fontsize=9, fontweight='bold')
    
    ax1.set_xlabel('MAE (Mean Absolute Error)', fontsize=11, fontweight='bold')
    ax1.set_title('Top 5 Models - MAE Comparison', fontsize=13, fontweight='bold')
    ax1.invert_yaxis()
    ax1.grid(True, alpha=0.3, axis='x')
    
    # ===== PLOT 2: Top Models Comparison (RMSE) =====
    ax2 = plt.subplot(2, 3, 2)
    rmse_values = top_5['RMSE'].values
    
    bars = ax2.barh(models, rmse_values, color=colors, alpha=0.7)
    
    for i, bar in enumerate(bars):
        width = bar.get_width()
        ax2.text(width, bar.get_y() + bar.get_height()/2, 
                f'{width:.4f}', ha='left', va='center', fontsize=9, fontweight='bold')
    
    ax2.set_xlabel('RMSE (Root Mean Squared Error)', fontsize=11, fontweight='bold')
    ax2.set_title('Top 5 Models - RMSE Comparison', fontsize=13, fontweight='bold')
    ax2.invert_yaxis()
    ax2.grid(True, alpha=0.3, axis='x')
    
    # ===== PLOT 3: R² Comparison =====
    ax3 = plt.subplot(2, 3, 3)
    r2_values = top_5['R2'].values
    
    colors_r2 = ['#2ecc71' if r2 >= 0 else '#e74c3c' for r2 in r2_values]
    bars = ax3.barh(models, r2_values, color=colors_r2, alpha=0.7)
    
    for i, bar in enumerate(bars):
        width = bar.get_width()
        ax3.text(width + 0.01 if width >= 0 else width - 0.01, 
                bar.get_y() + bar.get_height()/2, 
                f'{width:.4f}', ha='left' if width >= 0 else 'right', 
                va='center', fontsize=9, fontweight='bold')
    
    ax3.axvline(x=0, color='black', linestyle='-', linewidth=1)
    ax3.set_xlabel('R² Score', fontsize=11, fontweight='bold')
    ax3.set_title('Top 5 Models - R² Score', fontsize=13, fontweight='bold')
    ax3.invert_yaxis()
    ax3.grid(True, alpha=0.3, axis='x')
    
    # ===== PLOT 4: Performance Metrics Table =====
    ax4 = plt.subplot(2, 3, 4)
    ax4.axis('off')
    
    # Create table data
    table_data = []
    table_data.append(['Rank', 'Model', 'MAE', 'RMSE', 'R²'])
    
    for i, (idx, row) in enumerate(top_5.iterrows(), 1):
        model_name = idx[:25] + '...' if len(idx) > 25 else idx
        table_data.append([
            str(i),
            model_name,
            f"{row['MAE']:.4f}",
            f"{row['RMSE']:.4f}",
            f"{row['R2']:.4f}"
        ])
    
    table = ax4.table(cellText=table_data, cellLoc='left', loc='center',
                     colWidths=[0.08, 0.35, 0.15, 0.15, 0.15])
    
    table.auto_set_font_size(False)
    table.set_fontsize(9)
    table.scale(1, 2)
    
    # Style header row
    for i in range(5):
        table[(0, i)].set_facecolor('#3498db')
        table[(0, i)].set_text_props(weight='bold', color='white')
    
    # Style best model row
    for i in range(5):
        table[(1, i)].set_facecolor('#2ecc71')
        table[(1, i)].set_alpha(0.3)
    
    ax4.set_title('Performance Summary - Top 5 Models', 
                 fontsize=13, fontweight='bold', pad=20)
    
    # ===== PLOT 5: All Models Overview =====
    ax5 = plt.subplot(2, 3, 5)
    
    all_mae = comparison_df['MAE'].head(10)
    model_names = [idx[:15] + '...' if len(idx) > 15 else idx for idx in all_mae.index]
    
    ax5.bar(range(len(all_mae)), all_mae.values, 
           color=['#2ecc71' if i == 0 else '#95a5a6' for i in range(len(all_mae))],
           alpha=0.7)
    ax5.set_xticks(range(len(all_mae)))
    ax5.set_xticklabels(model_names, rotation=45, ha='right', fontsize=8)
    ax5.set_ylabel('MAE', fontsize=11, fontweight='bold')
    ax5.set_title('Top 10 Models - MAE Overview', fontsize=13, fontweight='bold')
    ax5.grid(True, alpha=0.3, axis='y')
    
    # ===== PLOT 6: Model Statistics =====
    ax6 = plt.subplot(2, 3, 6)
    ax6.axis('off')
    
    best_model_name = comparison_df.index[0]
    best_mae = comparison_df.iloc[0]['MAE']
    best_rmse = comparison_df.iloc[0]['RMSE']
    best_r2 = comparison_df.iloc[0]['R2']
    
    stats_text = f"""
    🏆 BEST MODEL SUMMARY
    {'='*50}
    
    Model:           {best_model_name}
    
    Performance Metrics:
    • MAE:           {best_mae:.4f}
    • RMSE:          {best_rmse:.4f}
    • R² Score:      {best_r2:.4f}
    
    Total Models:    {len(comparison_df)}
    Top 5 Average:   MAE = {comparison_df['MAE'].head(5).mean():.4f}
    
    {'='*50}
    
    Interpretation:
    • MAE: Average error in predictions
    • RMSE: Penalizes large errors more
    • R²: Variance explained (closer to 1 is better)
    
    Note: Negative R² means model is worse than
    predicting the mean, common in noisy financial data
    """
    
    ax6.text(0.1, 0.5, stats_text, fontsize=10, family='monospace',
            verticalalignment='center',
            bbox=dict(boxstyle='round', facecolor='wheat', alpha=0.5))
    
    plt.tight_layout()
    
    # Save
    output_file = 'model_comparison_visualization.png'
    plt.savefig(output_file, dpi=150, bbox_inches='tight')
    print(f"   ✓ Đã lưu visualization: '{output_file}'")
    plt.close()

# ==========================================
# MAIN EXECUTION
# ==========================================
if __name__ == "__main__":
    # 1. Load comparison data
    comparison_df, best_model = load_comparison_data()
    
    if comparison_df is not None and best_model is not None:
        # 2. Create visualization
        create_visualization(comparison_df, best_model)
        
        print("\n[3/3] Tổng kết:")
        print("="*70)
        print(f"✅ Best Model: {comparison_df.index[0]}")
        print(f"✅ MAE: {comparison_df.iloc[0]['MAE']:.4f}")
        print(f"✅ RMSE: {comparison_df.iloc[0]['RMSE']:.4f}")
        print(f"✅ R²: {comparison_df.iloc[0]['R2']:.4f}")
        print("="*70)
        print("\n📊 Visualization đã được lưu: 'model_comparison_visualization.png'")
        
    else:
        print("\n❌ Không thể tạo visualization. Chạy 'model_comparison_pycaret.py' trước.")
