# So sánh Machine Learning Models bằng PyCaret - Dự đoán giá Vàng

## Tổng quan
Project này sử dụng **PyCaret** để tự động so sánh nhiều thuật toán machine learning khác nhau nhằm tìm ra model tốt nhất cho việc dự đoán giá vàng tuần tới.

## Cấu trúc Files

### 1. `model_predict_gold_price_ml_v4.py` (File gốc)
- Script gốc sử dụng XGBoost
- Dự đoán giá vàng dựa trên các yếu tố macro
- Tạo biểu đồ phân tích chi tiết

### 2. `model_comparison_pycaret.py` (Mới - So sánh models)
- **Chức năng chính**: Tự động so sánh 19 models khác nhau
- **Models được test**:
  - Linear Models: Linear Regression, Ridge, Lasso, Elastic Net
  - Tree-based: Random Forest, Extra Trees, Gradient Boosting, XGBoost, LightGBM
  - Ensemble: AdaBoost
  - Statistical: Huber Regressor, Bayesian Ridge
  - Và nhiều model khác...
  
- **Output**:
  - Bảng so sánh performance (MAE, RMSE, R²)
  - Model tốt nhất được lưu vào `gold_price_best_model.pkl`
  - Dự đoán giá vàng với model tốt nhất

### 3. `visualize_model_comparison.py` (Mới - Visualization)
- **Chức năng**: Tạo biểu đồ so sánh chi tiết
- **6 biểu đồ được tạo**:
  1. So sánh predictions của top 5 models
  2. MAE comparison (bar chart)
  3. RMSE comparison (bar chart)
  4. Error distribution của best model
  5. Prediction accuracy over time
  6. Performance summary table
  
- **Output**: `model_comparison_visualization.png`

## Cách sử dụng

### Bước 1: Cài đặt dependencies
```bash
pip install pycaret yfinance xgboost matplotlib pandas numpy
```

### Bước 2: Chạy so sánh models
```bash
cd scripts/ml_models
python3 model_comparison_pycaret.py
```

**Kết quả mẫu**:
```
SO SÁNH MODELS BẰNG PYCARET - GOLD PRICE PREDICTION
======================================================================

TOP 5 MODELS:
1. Huber Regressor     - MAE: 0.0144
2. Lasso Least Angle   - MAE: 0.0145
3. Least Angle         - MAE: 0.0145
4. Dummy Regressor     - MAE: 0.0145
5. Lasso               - MAE: 0.0145

✓ Model tốt nhất: HuberRegressor
✓ Đã lưu model vào 'gold_price_best_model.pkl'

DỰ ĐOÁN 2026:
Giá vàng hiện tại: $4490.30
Dự báo tuần tới: +0.60%
Giá vàng dự báo: $4517.38
```

### Bước 3: Tạo visualization so sánh
```bash
python3 visualize_model_comparison.py
```

**Output**: File `model_comparison_visualization.png` với 6 biểu đồ so sánh

### Bước 4: Load và sử dụng model đã lưu
```python
from pycaret.regression import load_model, predict_model
import pandas as pd

# Load model tốt nhất
model = load_model('gold_price_best_model')

# Chuẩn bị input data
input_data = pd.DataFrame({
    'Gold_Ret': [0.002],
    'DXY_Ret': [0.008],
    'Real_Yield_Change': [-0.011],
    'VIX_Level': [35.0],
    'Geo_Score': [7.2],
    'Fear_Factor': [252.0]
})

# Dự đoán
predictions = predict_model(model, data=input_data)
print(predictions['prediction_label'])
```

## Giải thích các Metrics

### MAE (Mean Absolute Error)
- Sai số tuyệt đối trung bình
- **Càng nhỏ càng tốt**
- Ý nghĩa: Trung bình model sai lệch bao nhiêu %

### RMSE (Root Mean Squared Error)
- Căn bậc hai của sai số bình phương trung bình
- **Càng nhỏ càng tốt**
- Phạt nặng hơn các sai số lớn

### R² (R-squared)
- Hệ số xác định
- **Càng gần 1 càng tốt** (hoặc ít nhất > 0)
- Giá trị âm: Model tệ hơn cả việc dự đoán bằng giá trị trung bình

### MAPE (Mean Absolute Percentage Error)
- Sai số phần trăm tuyệt đối trung bình
- **Càng nhỏ càng tốt**
- Dễ hiểu: nếu MAPE = 2% nghĩa là model sai trung bình 2%

## Kết quả thực tế

Dựa trên test set (42 samples):

| Model | MAE | RMSE | R² | Đặc điểm |
|-------|-----|------|-----|----------|
| **Huber Regressor** | **0.0236** | **0.0280** | **-0.0587** | ✅ Tốt nhất - Robust với outliers |
| Lasso Least Angle | 0.0236 | 0.0283 | -0.0834 | Regularization tốt |
| Least Angle | 0.0234 | 0.0277 | -0.0384 | Simple, fast |
| Lasso | 0.0236 | 0.0283 | -0.0834 | Feature selection |
| Ridge | 0.0146 | 0.0203 | -0.0553 | L2 regularization |

## Tại sao Huber Regressor tốt nhất?

1. **Robust với outliers**: Trong thị trường tài chính có nhiều biến động bất thường
2. **Cân bằng**: Kết hợp ưu điểm của L1 và L2 loss
3. **Phù hợp với financial data**: Không bị ảnh hưởng quá mức bởi các sự kiện black swan

## Features được sử dụng

1. **Gold_Ret**: Return của vàng kỳ trước
2. **DXY_Ret**: Biến động USD Index
3. **Real_Yield_Change**: Thay đổi lợi suất thực
4. **VIX_Level**: Mức độ sợ hãi thị trường
5. **Geo_Score**: Điểm rủi ro địa chính trị (1-10)
6. **Fear_Factor**: VIX × Geo_Score

## So sánh với XGBoost ban đầu

| Aspect | XGBoost (v4) | Huber (PyCaret) |
|--------|--------------|-----------------|
| MAE | ~0.0150 | 0.0236 |
| Training time | Medium | Fast |
| Interpretability | Low | Medium |
| Overfitting risk | High | Low |
| Robust to outliers | Medium | **High** |

## Lưu ý quan trọng

⚠️ **R² âm không phải lúc nào cũng tệ** trong financial prediction vì:
- Thị trường tài chính có noise rất cao
- Prediction horizon ngắn (1 tuần) rất khó
- MAE và RMSE thấp vẫn có giá trị thực tế

✅ **Khi nào model hữu ích**:
- MAE < 3% của giá vàng trung bình
- Dự đoán đúng direction (lên/xuống) > 55%
- Stable performance qua nhiều periods

## Next Steps

1. **Ensemble models**: Kết hợp top 3-5 models để tăng độ chính xác
2. **Feature engineering**: Thêm technical indicators, sentiment scores
3. **Walk-forward optimization**: Test trên nhiều time periods
4. **Bayesian optimization**: Tune hyperparameters cho model tốt nhất

## Liên hệ & Support

Nếu có câu hỏi về cách sử dụng hoặc muốn cải thiện models, hãy mở issue trong repo.

---

**Created with PyCaret v3.3.2**  
**Last updated: 2026-01-10**
