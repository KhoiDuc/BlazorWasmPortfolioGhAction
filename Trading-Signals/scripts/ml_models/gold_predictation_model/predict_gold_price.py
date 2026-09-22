import pandas as pd
import numpy as np
import yfinance as yf
import warnings
import os
import json
from pycaret.regression import *
import matplotlib.pyplot as plt
import matplotlib
matplotlib.use('Agg')

warnings.filterwarnings('ignore')

print("=" * 70)
print("GOLD PRICE PREDICTION - NEXT WEEK FORECAST")
print("=" * 70)

DATA_FILE = 'gold_macro_data_full.csv'
MODEL_FILE = 'best_model_price'
NEWS_FILE = 'gold_price_model.json'

# ==========================================
# 1. LOAD DATA AND UPDATE WITH LATEST
# ==========================================
def load_and_update_data():
    """Load existing data and update with latest prices"""
    print("\n[1/5] Đang tải dữ liệu và cập nhật giá mới nhất...")
    
    if not os.path.exists(DATA_FILE):
        print("   ❌ Không tìm thấy file dữ liệu. Chạy train_gold_model.py trước.")
        return pd.DataFrame()
    
    # Load existing data
    df = pd.read_csv(DATA_FILE, index_col=0, parse_dates=True)
    df_weekly = df.resample('W-FRI').last().ffill()
    
    print(f"   ✓ Dữ liệu hiện tại: từ {df_weekly.index[0].date()} đến {df_weekly.index[-1].date()}")
    
    # Try to fetch latest data (last 2 weeks)
    try:
        print("   ⟳ Đang cập nhật giá mới nhất từ Yahoo Finance...")
        tickers = {
            'Gold': 'GC=F',
            'Silver': 'SI=F',
            'DXY': 'DX-Y.NYB',
            'US10Y': '^TNX',
            'TIPS': 'TIP',
            'SP500': '^GSPC',
            'VIX': '^VIX',
            'Miners': 'GDX',
            'Oil': 'CL=F'
        }
        
        latest_data = pd.DataFrame()
        for name, ticker in tickers.items():
            temp_df = yf.download(ticker, period='1mo', progress=False)
            if isinstance(temp_df.columns, pd.MultiIndex):
                latest_data[name] = temp_df['Close'].iloc[:, 0]
            else:
                latest_data[name] = temp_df['Close']
        
        latest_weekly = latest_data.resample('W-FRI').last().ffill()
        
        # Merge with existing data (only add new weeks)
        df_combined = pd.concat([df_weekly, latest_weekly])
        df_combined = df_combined[~df_combined.index.duplicated(keep='last')]
        df_combined = df_combined.sort_index()
        
        # Save updated data
        df_combined.to_csv(DATA_FILE)
        print(f"   ✓ Cập nhật thành công! Dữ liệu mới nhất: {df_combined.index[-1].date()}")
        
        return df_combined
        
    except Exception as e:
        print(f"   ⚠ Không thể cập nhật (offline?): {e}")
        print("   → Sử dụng dữ liệu cũ")
        return df_weekly

# ==========================================
# 2. CREATE FEATURES (SAME AS TRAINING)
# ==========================================
def create_advanced_features(df_input):
    """Create same features as training"""
    print("\n[2/5] Đang tạo features...")
    df = df_input.copy()
    
    # GEO SCORE (Historical Events)
    df['Geo_Score'] = 1.0
    df.loc['2022-02':'2022-04', 'Geo_Score'] = 9.0
    df.loc['2023-10':'2023-11', 'Geo_Score'] = 8.0
    df.loc['2020-03':'2020-05', 'Geo_Score'] = 6.0
    
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
    
    # TARGET (not used for prediction, but needed for structure)
    df['Target_Price'] = df['Gold'].shift(-1)
    
    df.dropna(inplace=True)
    
    print(f"   ✓ Tạo {len(df.columns)} features")
    return df

# ==========================================
# 3. GET CURRENT GOLD PRICE
# ==========================================
def get_current_gold_price():
    """Fetch current gold price from Yahoo Finance"""
    try:
        print("\n   ⟳ Đang lấy giá vàng realtime...")
        gold_ticker = yf.Ticker('GC=F')
        hist = gold_ticker.history(period='1d')
        
        if not hist.empty:
            current_price = hist['Close'].iloc[-1]
            print(f"   ✓ Giá vàng hiện tại: ${current_price:,.2f}")
            return float(current_price)
        else:
            print("   ⚠ Không lấy được giá realtime")
            return None
    except Exception as e:
        print(f"   ⚠ Lỗi khi lấy giá: {e}")
        return None

# ==========================================
# 4. LOAD NEWS FACTORS (OPTIONAL)
# ==========================================
def load_news_factors():
    """Load news factors if available"""
    print("\n[3/5] Đang kiểm tra news factors...")
    
    if os.path.exists(NEWS_FILE):
        try:
            with open(NEWS_FILE, 'r') as f:
                news_data = json.load(f)
            
            print(f"   ✓ Tìm thấy news factors:")
            print(f"      - Geo Score: {news_data.get('geo_score', 'N/A')}")
            print(f"      - VIX: {news_data.get('vix', 'N/A')}")
            print(f"      - DXY Change: {news_data.get('dxy_pct', 'N/A')}%")
            print(f"      - Yield Change: {news_data.get('yield_pct', 'N/A')}%")
            print(f"      - Reasoning: {news_data.get('reasoning', 'N/A')}")
            
            return news_data
        except Exception as e:
            print(f"   ⚠ Không đọc được file: {e}")
            return None
    else:
        print(f"   ℹ Không có file {NEWS_FILE}")
        print("   → Sử dụng giá trị hiện tại (không điều chỉnh)")
        return None

# ==========================================
# 5. MAKE PREDICTION
# ==========================================
def predict_next_week(df, news_factors=None, manual_current_price=None):
    """Make prediction for next week"""
    print("\n[4/5] Đang predict giá tuần tới...")
    
    if not os.path.exists(f'{MODEL_FILE}.pkl'):
        print(f"   ❌ Không tìm thấy model file '{MODEL_FILE}.pkl'")
        print("   → Chạy train_gold_model.py trước để train model")
        return None
    
    # Load model
    model = load_model(MODEL_FILE)
    print(f"   ✓ Đã load model: {type(model).__name__}")
    
    # Get latest row for prediction
    latest_row = df.iloc[-1:].copy()
    
    # Apply news factors if available
    if news_factors:
        print("\n   [Applying News Factors]")
        
        # Update Geo_Score
        if 'geo_score' in news_factors:
            latest_row['Geo_Score'] = news_factors['geo_score']
            latest_row['Fear_Factor'] = latest_row['VIX'] * news_factors['geo_score']
            latest_row['Fear_Factor_Change'] = (latest_row['Fear_Factor'] - 
                                                df.iloc[-2]['Fear_Factor']) / df.iloc[-2]['Fear_Factor']
            print(f"      ✓ Geo_Score updated: {news_factors['geo_score']}")
        
        # Simulate VIX change
        if 'vix' in news_factors:
            vix_new = news_factors['vix']
            vix_change = (vix_new - latest_row['VIX'].values[0]) / latest_row['VIX'].values[0]
            latest_row['VIX_Change'] = vix_change
            latest_row['VIX_Lag1'] = latest_row['VIX'].values[0]
            # Update VIX for Fear Factor
            latest_row['Fear_Factor'] = vix_new * latest_row['Geo_Score'].values[0]
            print(f"      ✓ VIX projected: {vix_new:.1f} (change: {vix_change*100:.2f}%)")
        
        # Simulate DXY change
        if 'dxy_pct' in news_factors:
            dxy_ret = news_factors['dxy_pct'] / 100
            latest_row['DXY_Ret'] = dxy_ret
            latest_row['DXY_Ret_Lag1'] = df.iloc[-1]['DXY_Ret']
            print(f"      ✓ DXY projected change: {news_factors['dxy_pct']:.2f}%")
    
    # Select features (exclude raw prices and target)
    exclude_cols = ['Gold', 'Silver', 'DXY', 'US10Y', 'TIPS', 'SP500', 
                    'VIX', 'Miners', 'Oil', 'Target_Price']
    feature_cols = [col for col in latest_row.columns if col not in exclude_cols]
    
    # Prepare input
    X_pred = latest_row[feature_cols]
    
    # Make prediction
    prediction = predict_model(model, data=X_pred, verbose=False)
    predicted_price = prediction['prediction_label'].values[0]
    
    # Current price - use manual override if provided
    if manual_current_price:
        current_price = manual_current_price
        print(f"   ℹ Using manual price: ${current_price:,.2f}")
    else:
        current_price = df['Gold'].iloc[-1]
        print(f"   ℹ Using latest data price: ${current_price:,.2f} ({df.index[-1].date()})")
    price_change = predicted_price - current_price
    price_change_pct = (price_change / current_price) * 100
    
    print(f"\n   {'='*60}")
    print(f"   📊 PREDICTION RESULTS")
    print(f"   {'='*60}")
    print(f"   Current Gold Price:    ${current_price:,.2f}")
    print(f"   Predicted Next Week:   ${predicted_price:,.2f}")
    print(f"   Expected Change:       ${price_change:+,.2f} ({price_change_pct:+.2f}%)")
    print(f"   {'='*60}")
    
    if price_change > 0:
        print(f"   📈 Outlook: BULLISH (BUY signal)")
    else:
        print(f"   📉 Outlook: BEARISH (SELL signal)")
    
    return {
        'current_price': float(current_price),
        'predicted_price': float(predicted_price),
        'change': float(price_change),
        'change_pct': float(price_change_pct),
        'data_date': df.index[-1].strftime('%Y-%m-%d'),
        'prediction_date': (df.index[-1] + pd.Timedelta(days=7)).strftime('%Y-%m-%d')
    }

# ==========================================
# 5B. PREDICT UNTIL END OF PERIOD (MONTH/QUARTER/YEAR)
# ==========================================
def predict_until_end_of_period(df, news_factors=None, manual_current_price=None, period='year'):
    """Make predictions from now until end of specified period
    
    Args:
        period: 'month', 'quarter', or 'year'
    """
    period_names = {
        'month': 'tháng',
        'quarter': 'quý',
        'year': 'năm'
    }
    
    print(f"\n[4/5] Đang predict giá đến hết {period_names[period]}...")
    
    if not os.path.exists(f'{MODEL_FILE}.pkl'):
        print(f"   ❌ Không tìm thấy model file '{MODEL_FILE}.pkl'")
        return None
    
    # Load model
    model = load_model(MODEL_FILE)
    print(f"   ✓ Đã load model: {type(model).__name__}")
    
    # Get current date and determine end date
    last_date = df.index[-1]
    
    if period == 'month':
        # End of current month
        if last_date.month == 12:
            end_date = pd.Timestamp(f'{last_date.year + 1}-01-01') - pd.Timedelta(days=1)
        else:
            end_date = pd.Timestamp(f'{last_date.year}-{last_date.month + 1:02d}-01') - pd.Timedelta(days=1)
    elif period == 'quarter':
        # End of current quarter
        current_quarter = (last_date.month - 1) // 3 + 1
        quarter_end_months = {1: 3, 2: 6, 3: 9, 4: 12}
        end_month = quarter_end_months[current_quarter]
        if end_month == 12:
            end_date = pd.Timestamp(f'{last_date.year}-12-31')
        else:
            end_date = pd.Timestamp(f'{last_date.year}-{end_month + 1:02d}-01') - pd.Timedelta(days=1)
    else:  # year
        end_date = pd.Timestamp(f'{last_date.year}-12-31')
    
    # Calculate number of weeks until end date
    weeks_remaining = max(1, ((end_date - last_date).days // 7) + 1)
    print(f"   ✓ Predicting {weeks_remaining} weeks until {end_date.date()}")
    
    # Prepare prediction results
    predictions = []
    df_extended = df.copy()
    
    # Current price
    if manual_current_price:
        current_price = manual_current_price
    else:
        current_price = df['Gold'].iloc[-1]
    
    print(f"   ✓ Starting price: ${current_price:,.2f}")
    print(f"\n   Predicting week by week...")
    
    for week in range(weeks_remaining):
        # Get latest row for prediction
        latest_row = df_extended.iloc[-1:].copy()
        
        # Apply news factors only to first prediction
        if week == 0 and news_factors:
            if 'geo_score' in news_factors:
                latest_row['Geo_Score'] = news_factors['geo_score']
                latest_row['Fear_Factor'] = latest_row['VIX'] * news_factors['geo_score']
            
            if 'vix' in news_factors:
                vix_new = news_factors['vix']
                vix_change = (vix_new - latest_row['VIX'].values[0]) / latest_row['VIX'].values[0]
                latest_row['VIX_Change'] = vix_change
                latest_row['Fear_Factor'] = vix_new * latest_row['Geo_Score'].values[0]
            
            if 'dxy_pct' in news_factors:
                latest_row['DXY_Ret'] = news_factors['dxy_pct'] / 100
        
        # Select features
        exclude_cols = ['Gold', 'Silver', 'DXY', 'US10Y', 'TIPS', 'SP500', 
                        'VIX', 'Miners', 'Oil', 'Target_Price']
        feature_cols = [col for col in latest_row.columns if col not in exclude_cols]
        
        # Make prediction
        X_pred = latest_row[feature_cols]
        prediction = predict_model(model, data=X_pred, verbose=False)
        predicted_price = prediction['prediction_label'].values[0]
        
        # Calculate change
        base_price = current_price if week == 0 else predictions[-1]['price']
        price_change = predicted_price - base_price
        price_change_pct = (price_change / base_price) * 100
        
        # Store prediction
        pred_date = last_date + pd.Timedelta(weeks=week+1)
        predictions.append({
            'week': week + 1,
            'date': pred_date.strftime('%Y-%m-%d'),
            'price': float(predicted_price),
            'change': float(price_change),
            'change_pct': float(price_change_pct)
        })
        
        # Print progress
        if period == 'month':
            if (week + 1) % 2 == 0 or week == 0 or week == weeks_remaining - 1:
                print(f"      Week {week+1}: ${predicted_price:,.2f} ({price_change_pct:+.2f}%)")
        elif period == 'quarter':
            if (week + 1) % 3 == 0 or week == 0 or week == weeks_remaining - 1:
                print(f"      Week {week+1}: ${predicted_price:,.2f} ({price_change_pct:+.2f}%)")
        else:  # year
            if (week + 1) % 5 == 0 or week == 0 or week == weeks_remaining - 1:
                print(f"      Week {week+1}: ${predicted_price:,.2f} ({price_change_pct:+.2f}%)")
        
        # Update df_extended with predicted values for next iteration
        new_row = df_extended.iloc[-1:].copy()
        new_row.index = [pred_date]
        new_row['Gold'] = predicted_price
        
        # Update features based on prediction
        if len(df_extended) > 0:
            prev_price = df_extended['Gold'].iloc[-1]
            new_row['Gold_Ret'] = (predicted_price - prev_price) / prev_price
            
            # Update moving averages and other features
            temp_df = pd.concat([df_extended, new_row])
            
            for window in [4, 8, 12]:
                new_row[f'Gold_MA{window}'] = temp_df['Gold'].rolling(window).mean().iloc[-1]
                new_row[f'Gold_Price_Position{window}'] = (predicted_price / new_row[f'Gold_MA{window}'].values[0]) - 1
            
            new_row['Gold_Momentum_4w'] = temp_df['Gold'].pct_change(4).iloc[-1] if len(temp_df) >= 4 else 0
            
        df_extended = pd.concat([df_extended, new_row])
    
    # Summary
    final_price = predictions[-1]['price']
    total_change = final_price - current_price
    total_change_pct = (total_change / current_price) * 100
    
    print(f"\n   {'='*60}")
    print(f"   📊 {period.upper()}-END FORECAST SUMMARY")
    print(f"   {'='*60}")
    print(f"   Current Price:              ${current_price:,.2f}")
    print(f"   Predicted End of {period_names[period].title()}: ${final_price:,.2f}")
    print(f"   Total Change:               ${total_change:+,.2f} ({total_change_pct:+.2f}%)")
    print(f"   Weeks forecasted:           {weeks_remaining}")
    print(f"   {'='*60}")
    
    if total_change > 0:
        print(f"   📈 {period_names[period].title()}-end Outlook: BULLISH")
    else:
        print(f"   📉 {period_names[period].title()}-end Outlook: BEARISH")
    
    return {
        'period': period,
        'period_name': period_names[period],
        'current_price': float(current_price),
        'current_date': last_date.strftime('%Y-%m-%d'),
        'end_date': end_date.strftime('%Y-%m-%d'),
        'predictions': predictions,
        'final_price': float(final_price),
        'total_change': float(total_change),
        'total_change_pct': float(total_change_pct),
        'weeks_forecasted': weeks_remaining
    }

# ==========================================
# 6. VISUALIZE PREDICTION
# ==========================================
def visualize_prediction(df, prediction_result, news_factors, period_forecast=None):
    """Create visualization with prediction
    
    Args:
        period_forecast: Dict with period forecast data (month/quarter/year)
    """
    print("\n[5/5] Đang tạo visualization...")
    
    # Determine figure layout based on whether we have period forecast
    if period_forecast:
        fig = plt.figure(figsize=(20, 12))
        gs = fig.add_gridspec(3, 3, hspace=0.3, wspace=0.3)
        
        # Plot 1: Full period forecast (spanning 2 columns)
        ax1 = fig.add_subplot(gs[0, :2])
        period_name = period_forecast.get('period_name', 'period')
    else:
        fig, axes = plt.subplots(2, 2, figsize=(16, 10))
        ax1 = axes[0, 0]
    
    # Plot 1: Historical Prices with Prediction(s)
    recent_data = df['Gold'].iloc[-52:]  # Last year
    ax1.plot(recent_data.index, recent_data.values, 
            color='blue', linewidth=2, label='Historical', alpha=0.8)
    
    if period_forecast:
        # Plot period predictions
        pred_dates = [pd.to_datetime(p['date']) for p in period_forecast['predictions']]
        pred_prices = [p['price'] for p in period_forecast['predictions']]
        
        ax1.plot(pred_dates, pred_prices, 
                color='red', linewidth=2, linestyle='--', 
                label=f'{period_name.title()}-end Forecast', alpha=0.7)
        ax1.scatter(pred_dates[0], pred_prices[0], 
                   color='orange', s=150, zorder=5, label='Next Week')
        ax1.scatter(pred_dates[-1], pred_prices[-1], 
                   color='red', s=200, zorder=5, label=f'End of {period_name.title()}', marker='*')
        
        # Connect current to first prediction
        ax1.plot([recent_data.index[-1], pred_dates[0]],
                [period_forecast['current_price'], pred_prices[0]],
                'orange', linewidth=2, linestyle=':', alpha=0.5)
        
        # Annotate key points
        ax1.annotate(f"${pred_prices[0]:,.0f}",
                    xy=(pred_dates[0], pred_prices[0]),
                    xytext=(10, -20), textcoords='offset points',
                    bbox=dict(boxstyle='round,pad=0.5', fc='yellow', alpha=0.7),
                    fontsize=9, fontweight='bold')
        
        ax1.annotate(f"{period_name.title()}-end\n${pred_prices[-1]:,.0f}\n({period_forecast['total_change_pct']:+.1f}%)",
                    xy=(pred_dates[-1], pred_prices[-1]),
                    xytext=(10, 10), textcoords='offset points',
                    bbox=dict(boxstyle='round,pad=0.5', fc='lightcoral', alpha=0.7),
                    fontsize=10, fontweight='bold')
        
        ax1.set_title(f'Gold Price Forecast - Until End of {period_name.title()} ({period_forecast["end_date"]})', 
                     fontsize=14, fontweight='bold')
    else:
        # Single week prediction
        pred_date = pd.to_datetime(prediction_result['prediction_date'])
        ax1.scatter([pred_date], [prediction_result['predicted_price']], 
                   color='red', s=200, zorder=5, label='Prediction')
        ax1.plot([recent_data.index[-1], pred_date],
                [prediction_result['current_price'], prediction_result['predicted_price']],
                'r--', linewidth=2, alpha=0.7)
        
        ax1.annotate(f"${prediction_result['predicted_price']:,.0f}\n({prediction_result['change_pct']:+.1f}%)",
                    xy=(pred_date, prediction_result['predicted_price']),
                    xytext=(10, 10), textcoords='offset points',
                    bbox=dict(boxstyle='round,pad=0.5', fc='yellow', alpha=0.7),
                    fontsize=10, fontweight='bold')
        
        ax1.set_title('Gold Price - Historical + Next Week Prediction', 
                     fontsize=14, fontweight='bold')
    
    ax1.set_ylabel('Price (USD)', fontsize=11)
    ax1.legend(loc='best', fontsize=9)
    ax1.grid(True, alpha=0.3)
    
    # Plot 2: Weekly changes (if period forecast)
    if period_forecast:
        ax2 = fig.add_subplot(gs[0, 2])
        weekly_changes = [p['change_pct'] for p in period_forecast['predictions']]
        colors_changes = ['green' if x > 0 else 'red' for x in weekly_changes]
        weeks = list(range(1, len(weekly_changes) + 1))
        
        ax2.bar(weeks, weekly_changes, color=colors_changes, alpha=0.7)
        ax2.axhline(y=0, color='black', linestyle='-', linewidth=0.8)
        ax2.set_title('Weekly % Changes', fontsize=12, fontweight='bold')
        ax2.set_xlabel('Week', fontsize=9)
        ax2.set_ylabel('Change (%)', fontsize=9)
        ax2.grid(True, alpha=0.3, axis='y')
    else:
        ax2 = axes[0, 1]
    # Plot 2 continued: Recent Momentum (for weekly prediction)
    if not period_forecast:
        ax2 = axes[0, 1]
        
    recent_returns = df['Gold'].pct_change().iloc[-52:] * 100
    colors = ['green' if x > 0 else 'red' for x in recent_returns]
    ax2.bar(recent_returns.index, recent_returns.values, color=colors, alpha=0.6)
    ax2.axhline(y=0, color='black', linestyle='-', linewidth=0.8)
    ax2.set_title('Weekly Returns (Last Year)', fontsize=12 if period_forecast else 14, fontweight='bold')
    ax2.set_ylabel('Return (%)', fontsize=9 if period_forecast else 11)
    ax2.grid(True, alpha=0.3)
    
    # Plot 3: Key Indicators
    if period_forecast:
        ax3 = fig.add_subplot(gs[1, :])
    else:
        ax3 = axes[1, 0]
        
    indicators = df[['VIX', 'DXY', 'US10Y']].iloc[-52:]
    ax3_twin1 = ax3.twinx()
    ax3_twin2 = ax3.twinx()
    ax3_twin2.spines['right'].set_position(('outward', 60))
    
    p1, = ax3.plot(indicators.index, indicators['VIX'], 'r-', label='VIX', linewidth=2)
    p2, = ax3_twin1.plot(indicators.index, indicators['DXY'], 'b-', label='DXY', linewidth=2)
    p3, = ax3_twin2.plot(indicators.index, indicators['US10Y'], 'g-', label='US10Y', linewidth=2)
    
    ax3.set_ylabel('VIX', color='r', fontsize=9 if period_forecast else 11)
    ax3_twin1.set_ylabel('DXY', color='b', fontsize=9 if period_forecast else 11)
    ax3_twin2.set_ylabel('US 10Y Yield (%)', color='g', fontsize=9 if period_forecast else 11)
    ax3.set_title('Market Indicators', fontsize=12 if period_forecast else 14, fontweight='bold')
    
    lines = [p1, p2, p3]
    ax3.legend(lines, [l.get_label() for l in lines], loc='upper left', fontsize=9)
    ax3.grid(True, alpha=0.3)
    
    # Plot 4: Prediction Summary Box
    if period_forecast:
        ax4 = fig.add_subplot(gs[2, :])
    else:
        ax4 = axes[1, 1]
        
    ax4.axis('off')
    
    if period_forecast:
        # Period-end forecast summary
        summary_text = f"""
    GOLD PRICE FORECAST - {period_forecast['period'].upper()} END {period_forecast['end_date'][:4]}
    {'='*70}
    
    Current Date:           {period_forecast['current_date']}
    Current Price:          ${period_forecast['current_price']:,.2f}
    
    Next Week Prediction:   ${period_forecast['predictions'][0]['price']:,.2f} ({period_forecast['predictions'][0]['change_pct']:+.2f}%)
    
    {period_name.title()}-end Prediction:    ${period_forecast['final_price']:,.2f}
    Total Expected Change:  ${period_forecast['total_change']:+,.2f} ({period_forecast['total_change_pct']:+.2f}%)
    
    Weeks Forecasted:       {period_forecast['weeks_forecasted']}
    {'='*70}
        """
        
        if news_factors:
            summary_text += f"""
    NEWS FACTORS (Applied to first week):
    - Geopolitical Score: {news_factors.get('geo_score', 'N/A')}
    - VIX Projection: {news_factors.get('vix', 'N/A')}
    - DXY Change: {news_factors.get('dxy_pct', 'N/A')}%
    - Yield Change: {news_factors.get('yield_pct', 'N/A')}%
    {'='*70}
            """
        
        outlook = "📈 BULLISH" if period_forecast['total_change'] > 0 else "📉 BEARISH"
        summary_text += f"\n    {period_name.title()}-end Outlook: {outlook}\n"
        summary_text += f"\n    Note: Long-term predictions have higher uncertainty"
        
    else:
        # Weekly prediction summary
        summary_text = f"""
    GOLD PRICE FORECAST
    {'='*50}
    
    Data as of:        {prediction_result['data_date']}
    Forecast for:      {prediction_result['prediction_date']}
    
    Current Price:     ${prediction_result['current_price']:,.2f}
    Predicted Price:   ${prediction_result['predicted_price']:,.2f}
    
    Expected Change:   ${prediction_result['change']:+,.2f}
    Percentage:        {prediction_result['change_pct']:+.2f}%
    
    {'='*50}
        """
        
        if news_factors:
            summary_text += f"""
    NEWS FACTORS APPLIED:
    - Geopolitical Score: {news_factors.get('geo_score', 'N/A')}
    - VIX Projection: {news_factors.get('vix', 'N/A')}
    - DXY Change: {news_factors.get('dxy_pct', 'N/A')}%
    - Yield Change: {news_factors.get('yield_pct', 'N/A')}%
    
    Reasoning: {news_factors.get('reasoning', 'N/A')}
    {'='*50}
            """
        
        outlook = "📈 BULLISH" if prediction_result['change'] > 0 else "📉 BEARISH"
        summary_text += f"\n    Outlook: {outlook}\n"
    
    ax4.text(0.05, 0.5, summary_text, fontsize=10 if period_forecast else 11, 
            family='monospace', verticalalignment='center', 
            bbox=dict(boxstyle='round', facecolor='lightblue', alpha=0.3, pad=1))
    
    plt.tight_layout()
    
    # Save visualization with appropriate filename
    if period_forecast:
        output_file = f"gold_{period_forecast['period']}_end_prediction.png"
    else:
        output_file = 'gold_next_week_prediction.png'
    
    plt.savefig(output_file, dpi=150, bbox_inches='tight')
    print(f"   ✓ Đã lưu visualization: '{output_file}'")
    plt.close()

# ==========================================
# MAIN EXECUTION
# ==========================================
if __name__ == "__main__":
    import sys
    
    # Parse command line arguments
    run_week = '--week' in sys.argv or '-w' in sys.argv
    run_month = '--month' in sys.argv or '-m' in sys.argv
    run_quarter = '--quarter' in sys.argv or '-q' in sys.argv
    run_year = '--year' in sys.argv or '-y' in sys.argv
    
    # If no specific flags, run all predictions
    if not any([run_week, run_month, run_quarter, run_year]):
        run_week = run_month = run_quarter = run_year = True
    
    # 1. Load and update data
    df_raw = load_and_update_data()
    
    if not df_raw.empty:
        # 2. Create features
        df = create_advanced_features(df_raw)
        
        # 3. Get current gold price (realtime)
        current_gold_price = get_current_gold_price()
        
        # Allow manual override via command line argument
        for arg in sys.argv[1:]:
            if arg not in ['--week', '-w', '--month', '-m', '--quarter', '-q', '--year', '-y']:
                try:
                    current_gold_price = float(arg)
                    print(f"\n💡 Manual price override: ${current_gold_price:,.2f}")
                    break
                except ValueError:
                    continue
        
        # 4. Load news factors (optional)
        news_factors = load_news_factors()
        
        # ========================================
        # RUN REQUESTED PREDICTIONS
        # ========================================
        all_results = {}
        prediction_count = sum([run_week, run_month, run_quarter, run_year])
        current_step = 0
        
        # 5a. NEXT WEEK
        if run_week:
            current_step += 1
            print("\n" + "="*70)
            print(f"📅 [{current_step}/{prediction_count}] PREDICTING NEXT WEEK")
            print("="*70)
            next_week_result = predict_next_week(df, news_factors, current_gold_price)
            if next_week_result:
                visualize_prediction(df, next_week_result, news_factors, period_forecast=None)
                with open('latest_prediction.json', 'w') as f:
                    json.dump(next_week_result, f, indent=2)
                all_results['next_week'] = next_week_result
                print(f"   ✅ Next week: ${next_week_result['predicted_price']:,.2f} ({next_week_result['change_pct']:+.2f}%)")
        
        # 5b. MONTH-END
        if run_month:
            current_step += 1
            print("\n" + "="*70)
            print(f"📅 [{current_step}/{prediction_count}] PREDICTING MONTH-END")
            print("="*70)
            month_result = predict_until_end_of_period(df, news_factors, current_gold_price, period='month')
            if month_result:
                visualize_prediction(df, None, news_factors, period_forecast=month_result)
                with open('month_end_prediction.json', 'w') as f:
                    json.dump(month_result, f, indent=2)
                all_results['month_end'] = month_result
                print(f"   ✅ Month-end: ${month_result['final_price']:,.2f} ({month_result['total_change_pct']:+.2f}%)")
        
        # 5c. QUARTER-END
        if run_quarter:
            current_step += 1
            print("\n" + "="*70)
            print(f"📅 [{current_step}/{prediction_count}] PREDICTING QUARTER-END")
            print("="*70)
            quarter_result = predict_until_end_of_period(df, news_factors, current_gold_price, period='quarter')
            if quarter_result:
                visualize_prediction(df, None, news_factors, period_forecast=quarter_result)
                with open('quarter_end_prediction.json', 'w') as f:
                    json.dump(quarter_result, f, indent=2)
                all_results['quarter_end'] = quarter_result
                print(f"   ✅ Quarter-end: ${quarter_result['final_price']:,.2f} ({quarter_result['total_change_pct']:+.2f}%)")
        
        # 5d. YEAR-END
        if run_year:
            current_step += 1
            print("\n" + "="*70)
            print(f"📅 [{current_step}/{prediction_count}] PREDICTING YEAR-END")
            print("="*70)
            year_result = predict_until_end_of_period(df, news_factors, current_gold_price, period='year')
            if year_result:
                visualize_prediction(df, None, news_factors, period_forecast=year_result)
                with open('year_end_prediction.json', 'w') as f:
                    json.dump(year_result, f, indent=2)
                all_results['year_end'] = year_result
                print(f"   ✅ Year-end: ${year_result['final_price']:,.2f} ({year_result['total_change_pct']:+.2f}%)")
        
        # ========================================
        # SUMMARY
        # ========================================
        if all_results:
            print("\n" + "="*70)
            print("📊 SUMMARY - PREDICTIONS")
            print("="*70)
            print(f"Current Gold Price: ${current_gold_price:,.2f}")
            print("-"*70)
            
            if 'next_week' in all_results:
                r = all_results['next_week']
                trend = "📈" if r['change_pct'] > 0 else "📉"
                print(f"{trend} Next Week:    ${r['predicted_price']:,.2f}  ({r['change_pct']:+.2f}%)")
            
            if 'month_end' in all_results:
                r = all_results['month_end']
                trend = "📈" if r['total_change_pct'] > 0 else "📉"
                print(f"{trend} Month-end:    ${r['final_price']:,.2f}  ({r['total_change_pct']:+.2f}%)  [{r['weeks_forecasted']} weeks]")
            
            if 'quarter_end' in all_results:
                r = all_results['quarter_end']
                trend = "📈" if r['total_change_pct'] > 0 else "📉"
                print(f"{trend} Quarter-end:  ${r['final_price']:,.2f}  ({r['total_change_pct']:+.2f}%)  [{r['weeks_forecasted']} weeks]")
            
            if 'year_end' in all_results:
                r = all_results['year_end']
                trend = "📈" if r['total_change_pct'] > 0 else "📉"
                print(f"{trend} Year-end:     ${r['final_price']:,.2f}  ({r['total_change_pct']:+.2f}%)  [{r['weeks_forecasted']} weeks]")
            
            print("="*70)
            print("✅ HOÀN TẤT! Đã lưu tất cả predictions")
            print("="*70)
            print("\n💡 Usage: python3 predict_gold_price.py [--week|-w] [--month|-m] [--quarter|-q] [--year|-y] [price]")
            print("   Không có arg = chạy tất cả predictions")
    else:
        print("\n❌ Không thể thực hiện prediction do lỗi dữ liệu.")
