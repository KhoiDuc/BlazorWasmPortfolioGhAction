# 🏆 Gold Price Prediction with Machine Learning

Hệ thống dự đoán giá vàng sử dụng Machine Learning với PyCaret, đạt R² = 0.97+ trên test set.

## 📋 Tổng quan

Hệ thống bao gồm 5 scripts chính:

1. **`train_gold_model.py`** - Train model với advanced features
2. **`predict_gold_price.py`** - Predict giá vàng (tuần/tháng/quý/năm)
3. **`explain_prediction.py`** - Giải thích chi tiết prediction
4. **`model_comparison_pycaret.py`** - So sánh 19+ models với PyCaret
5. **`visualize_model_comparison.py`** - Visualization so sánh models

## 🚀 Cài đặt

### Requirements

```bash
pip install pandas numpy yfinance pycaret matplotlib scikit-learn
```

### Data Files

- **`gold_macro_data_full.csv`** - Historical data (2018-2026, weekly)
- **`gold_price_model.json`** - News factors configuration (optional)

## 📚 Hướng dẫn sử dụng

### 1️⃣ Train Model (Bước đầu tiên - chạy 1 lần)

```bash
python3 train_gold_model.py
```

**Output:**
- `best_model_price.pkl` - Trained model file
- `train_gold_model_results.png` - Model performance visualization

**Thời gian:** ~2-5 phút (tùy máy)

**Khi nào cần train lại:**
- Có data mới (>1 tháng)
- Muốn thử features mới
- Model performance giảm

---

### 2️⃣ Predict Giá Vàng

#### 🔹 Chạy TẤT CẢ predictions (tuần/tháng/quý/năm)

```bash
python3 predict_gold_price.py
```

#### 🔹 Chạy prediction cụ thể

```bash
# Chỉ tuần tới
python3 predict_gold_price.py --week
python3 predict_gold_price.py -w

# Chỉ hết tháng
python3 predict_gold_price.py --month
python3 predict_gold_price.py -m

# Chỉ hết quý
python3 predict_gold_price.py --quarter
python3 predict_gold_price.py -q

# Chỉ hết năm
python3 predict_gold_price.py --year
python3 predict_gold_price.py -y
```

#### 🔹 Kết hợp nhiều predictions

```bash
# Tuần + Tháng
python3 predict_gold_price.py -w -m

# Tháng + Quý + Năm
python3 predict_gold_price.py -m -q -y
```

#### 🔹 Override giá vàng hiện tại

```bash
# Predict với giá $4,500
python3 predict_gold_price.py -w 4500

# Predict tháng + quý với giá $4,600
python3 predict_gold_price.py -m -q 4600
```

**Output Files:**
- `gold_next_week_prediction.png` - Weekly forecast visualization
- `gold_month_end_prediction.png` - Month-end forecast
- `gold_quarter_end_prediction.png` - Quarter-end forecast  
- `gold_year_end_prediction.png` - Year-end forecast
- `latest_prediction.json` - Weekly results (JSON)
- `month_end_prediction.json` - Month-end results
- `quarter_end_prediction.json` - Quarter-end results
- `year_end_prediction.json` - Year-end results

---

### 3️⃣ Giải thích Prediction (Optional)

```bash
python3 explain_prediction.py
```

**Output:**
- Chi tiết về các features ảnh hưởng đến prediction
- Lý do tại sao model predict giá tăng/giảm
- Phân tích moving averages, momentum, volatility

---

### 4️⃣ So sánh Models với PyCaret (Advanced)

#### 🔍 So sánh 19+ Machine Learning Models

```bash
python3 model_comparison_pycaret.py
```

**Chức năng:**
- Tự động test 19+ regression models (Linear, Ridge, Lasso, XGBoost, LightGBM, Random Forest, v.v.)
- So sánh performance metrics (MAE, RMSE, R²)
- Tự động chọn best model
- Lưu best model vào `gold_price_best_model.pkl`
- Predict giá vàng với best model

**Output:**
```
TOP 5 MODELS:
======================================================================
1. Lasso                     - MAE: 26.4616  RMSE: 35.0725  R²: 0.9890
2. LassoLars                 - MAE: 26.7963  RMSE: 35.9191  R²: 0.9884
3. ExtraTreesRegressor       - MAE: 27.1088  RMSE: 36.7383  R²: 0.9880
4. OrthogonalMatchingPursuit - MAE: 27.7487  RMSE: 36.8432  R²: 0.9879
5. GradientBoostingRegressor - MAE: 31.0798  RMSE: 41.5043  R²: 0.9847

✓ Best Model: Lasso
✓ Đã lưu model vào 'gold_price_best_model.pkl'

DỰ ĐOÁN:
Giá vàng hiện tại: $4490.30
Dự báo tuần tới: -2.20%
Giá vàng dự báo: $4391.46
```

**Files created:**
- `gold_price_best_model.pkl` - Best model file
- `model_comparison_results.csv` - Comparison table

**Thời gian:** ~5-10 phút (tùy máy)

---

#### 📊 Tạo Visualization So sánh

```bash
python3 visualize_model_comparison.py
```

**Chức năng:**
- Tạo 6 biểu đồ so sánh chi tiết:
  1. **MAE Comparison** - Top 5 models
  2. **RMSE Comparison** - Top 5 models
  3. **R² Score** - Performance comparison
  4. **Performance Table** - Summary metrics
  5. **All Models Overview** - Top 10 models
  6. **Best Model Statistics** - Detailed info

**Output:**
- `model_comparison_visualization.png` - Comprehensive visualization (20x12 inches)

**Khi nào sử dụng:**
- ✅ Muốn so sánh nhiều algorithms để tìm best model
- ✅ Thử nghiệm với different models thay vì chỉ dùng Lasso
- ✅ Nghiên cứu performance của các model families (Tree-based, Linear, Ensemble)
- ✅ Báo cáo kết quả với visualization đẹp

**Note:** Script này yêu cầu đã chạy `model_comparison_pycaret.py` trước.

---

### 5️⃣ Sử dụng News Factors (Advanced)

Điều chỉnh prediction dựa trên tin tức/sự kiện bằng cách edit file **`gold_price_model.json`**.

#### 📋 Parameter Guidelines - Trước khi chạy predict_gold_price.py

**1. geo_score (0.0 to 10.0) - Geopolitical Risk:**
- **0-2**: Peace, trade agreements, stability
- **3-4**: Mild tension, sanctions, diplomatic disputes
- **5-6**: Local conflict, riots, military drills (e.g., Taiwan drills)
- **7-8**: WAR / Direct armed conflict (e.g., Russia-Ukraine, Israel-Gaza)
- **9-10**: Major Crisis / World War risk (Nuclear threats, Superpower collision)

**2. vix (10.0 to 80.0) - Market Fear Sentiment:**
- **10-15**: Euphoria / Complacency (Stock market booming)
- **16-20**: Normal market conditions
- **21-30**: Nervous / Anxiety (Inflation fears, bad earnings)
- **31-50**: PANIC (Crash, Pandemic, Black Swan events)
- **>50**: Total Collapse (Financial system failure)

**3. dxy_pct (-2.0 to +2.0) - USD Strength Change (%):**
- **Positive (+)**: USD strengthens (Fed hikes rates, strong US economy) → Gold DOWN
- **Negative (-)**: USD weakens (Fed cuts rates, US recession) → Gold UP
- **Range**: Normal news is ±0.1 to 0.5. Major monetary policy shifts are ±1.0 to 2.0

**4. yield_pct (-5.0 to +5.0) - US 10Y Bond Yield Change (%):**
- **Positive (+)**: Yields rise (Bond sell-off, inflation spikes) → Gold DOWN
- **Negative (-)**: Yields fall (Bond rally, flight to safety) → Gold UP
- **Range**: Normal fluctuation is ±0.5 to 1.0. Extreme events are ±3.0 to 5.0

#### 📝 Cách sử dụng

**🤖 OPTION 1: Tự động generate với AI (Khuyên dùng)**

Hỏi ChatGPT/Gemini/Copilot prompt này:

```
1. geo_score (0.0 to 10.0) - Geopolitical Risk:
   - 0-2: Peace, trade agreements, stability.
   - 3-4: Mild tension, sanctions, diplomatic disputes.
   - 5-6: Local conflict, riots, military drills (e.g., Taiwan drills).
   - 7-8: WAR / Direct armed conflict (e.g., Russia-Ukraine, Israel-Gaza).
   - 9-10: Major Crisis / World War risk (Nuclear threats, Superpower collision).

2. vix (10.0 to 80.0) - Market Fear Sentiment:
   - 10-15: Euphoria / Complacency (Stock market booming).
   - 16-20: Normal market conditions.
   - 21-30: Nervous / Anxiety (Inflation fears, bad earnings).
   - 31-50: PANIC (Crash, Pandemic, Black Swan events).
   - >50: Total Collapse (Financial system failure).

3. dxy_pct (-2.0 to +2.0) - USD Strength Change (%):
   - Positive (+): USD strengthens (Fed hikes rates, strong US economy) -> Gold DOWN.
   - Negative (-): USD weakens (Fed cuts rates, US recession) -> Gold UP.
   - Range: Normal news is +/- 0.1 to 0.5. Major monetary policy shifts are +/- 1.0 to 2.0.

4. yield_pct (-5.0 to +5.0) - US 10Y Bond Yield Change (%):
   - Positive (+): Yields rise (Bond sell-off, inflation spikes) -> Gold DOWN.
   - Negative (-): Yields fall (Bond rally, flight to safety) -> Gold UP.
   - Range: Normal fluctuation is +/- 0.5 to 1.0. Extreme events are +/- 3.0 to 5.0.

Input News: "{PASTE TIN TỨC CỦA BẠN Ở ĐÂY}"

Output Requirement:
- Return ONLY a valid JSON object. Do not include markdown formatting (```json).
- Estimate values based on the logic above.

JSON Format:
{
  "geo_score": <float>,
  "vix": <float>,
  "dxy_pct": <float>,
  "yield_pct": <float>,
  "reasoning": "<short explanation under 30 words>"
}
```

**Sau đó:**
1. Copy JSON response từ AI
2. Paste vào file `gold_price_model.json`
3. Chạy `python3 predict_gold_price.py`

---

**📝 OPTION 2: Điều chỉnh manual**

**Bước 1:** Đọc tin tức và đánh giá impact theo bảng parameter guidelines ở trên

**Bước 2:** Edit file **`gold_price_model.json`** với values phù hợp:

```json
{
  "geo_score": 7.2,
  "vix": 35.0,
  "dxy_pct": 0.8,
  "yield_pct": -1.1,
  "reasoning": "Tensions in Middle East + Fed rate uncertainty"
}
```

**Bước 3:** Chạy prediction

```bash
python3 predict_gold_price.py
```

#### 🎯 Ví dụ Scenarios

**Parameters:**
- `geo_score` (0-10): Điểm địa chính trị
- `vix` (10-80): VIX dự kiến (volatility index)
- `dxy_pct` (-2 to +2): % thay đổi Dollar Index dự kiến
- `yield_pct` (-5 to +5): % thay đổi US 10Y Yield dự kiến
- `reasoning`: Lý do điều chỉnh (dưới 30 từ)

**Ví dụ:**

```json
// Scenario 1: Chiến tranh bùng nổ
{
  "geo_score": 9.5,
  "vix": 45.0,
  "dxy_pct": -1.5,
  "yield_pct": -0.5,
  "reasoning": "Major conflict outbreak - flight to safety"
}

// Scenario 2: Fed tăng lãi suất mạnh
{
  "geo_score": 3.0,
  "vix": 20.0,
  "dxy_pct": 2.0,
  "yield_pct": 1.5,
  "reasoning": "Aggressive Fed rate hikes - bearish for gold"
}

// Scenario 3: Bình thường
{
  "geo_score": 1.0,
  "vix": 15.0,
  "dxy_pct": 0.0,
  "yield_pct": 0.0,
  "reasoning": "Normal market conditions"
}
```

Sau khi edit, chạy lại `predict_gold_price.py` để thấy tác động.

---

## 📊 Hiểu kết quả

### Console Output

```
======================================================================
📊 SUMMARY - PREDICTIONS
======================================================================
Current Gold Price: $4,490.30
----------------------------------------------------------------------
📉 Next Week:    $4,391.46  (-2.20%)
📉 Month-end:    $4,370.54  (-2.67%)  [5 weeks]
📉 Quarter-end:  $4,354.62  (-3.02%)  [13 weeks]
📉 Year-end:     $4,258.47  (-5.16%)  [52 weeks]
======================================================================
```

### Visualization Files

**4 panel layout:**
1. **Historical + Forecast** - Giá lịch sử + dự đoán
2. **Weekly Returns/Changes** - % thay đổi từng tuần
3. **Market Indicators** - VIX, DXY, US10Y
4. **Summary Box** - Thông tin chi tiết prediction

### JSON Output

```json
{
  "current_price": 4490.30,
  "predicted_price": 4391.46,
  "change": -98.84,
  "change_pct": -2.20,
  "data_date": "2026-01-09",
  "prediction_date": "2026-01-16"
}
```

---

## 🎯 Model Features (59 features)

### 1. Price-based Features
- Lagged returns (1-4 weeks)
- Moving averages (MA4, MA8, MA12)
- Price position vs MA
- Momentum indicators (4w, 8w, 12w)

### 2. Macro Indicators
- DXY (Dollar Index)
- US 10Y Yield
- TIPS (Treasury Inflation-Protected Securities)
- Real yield proxy

### 3. Market Sentiment
- VIX (Fear Index)
- VIX spikes detection
- S&P 500 returns
- Gold/Silver ratio

### 4. Geopolitical
- Geo Score (historical events tagged)
- Fear Factor (VIX × Geo Score)

### 5. Risk Regime
- Risk-On indicator
- Risk-Off indicator
- Volatility measures

---

## ⚠️ Lưu ý quan trọng

### Độ chính xác theo thời gian

- **Tuần tới**: Cao nhất (~2-3% error)
- **Hết tháng**: Khá tốt (~3-4% error)
- **Hết quý**: Trung bình (~4-5% error)
- **Hết năm**: Thấp (~5-7% error)

⚡ **Predictions dài hạn (>3 tháng) có uncertainty cao!**

### Best Practices

1. ✅ Chạy prediction **hàng tuần** để có kết quả mới nhất
2. ✅ Update `gold_price_model.json` khi có tin tức quan trọng
3. ✅ So sánh predictions với thực tế để đánh giá model
4. ✅ Train lại model **mỗi 2-3 tháng** khi có đủ data mới
5. ❌ **KHÔNG** tin tưởng 100% vào predictions dài hạn
6. ❌ **KHÔNG** sử dụng làm lời khuyên đầu tư duy nhất

---

## 🔧 Troubleshooting

### Lỗi: "Không tìm thấy model file"

```bash
# Train model trước
python3 train_gold_model.py
```

### Lỗi: "Không lấy được giá realtime"

```bash
# Override manual
python3 predict_gold_price.py -w 4500
```

### Lỗi: "Module not found"

```bash
# Cài đặt dependencies
pip install pycaret pandas numpy yfinance matplotlib
```

### Model performance giảm

```bash
# Train lại model với data mới
python3 train_gold_model.py
```

---

## 📈 Model Performance

- **Model type**: Lasso Regression (PyCaret AutoML)
- **R² Score**: 0.9728
- **MAE**: $72.73
- **RMSE**: $90.48
- **Training data**: 2018-2026 (weekly, 419 weeks)
- **Test split**: 85/15
- **Cross-validation**: 5-fold

---

## 📞 Support

Nếu có vấn đề:
1. Check console output để xem error message
2. Verify data files tồn tại
3. Ensure model đã được train
4. Check internet connection (để fetch realtime prices)

---

## 📝 License

MIT License - Free to use and modify

**Disclaimer**: Predictions chỉ mang tính tham khảo, không phải lời khuyên đầu tư. Trade at your own risk!
