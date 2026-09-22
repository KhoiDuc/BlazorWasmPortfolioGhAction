import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from sklearn.ensemble import GradientBoostingRegressor
import json
import joblib

# 1. Tải dữ liệu JSON
file_path = 'hcm_real_estate_cpi_2018_2026.json'
with open(file_path, 'r', encoding='utf-8') as f:
    data = json.load(f)
df = pd.DataFrame(data)

# 2. Xử lý dữ liệu (Điền khuyết CPI cho năm 2026)
df = df.sort_values(by=['year', 'quarter']).reset_index(drop=True)
df_hist = df[df['year'] < 2026].copy()
df_future = df[df['year'] == 2026].copy()

# Tính tốc độ tăng CPI trung bình các quý gần nhất để dự phóng cho 2026
cpi_trend = np.mean(np.diff(df_hist.tail(4)['cpi_index']))
last_cpi = df_hist.iloc[-1]['cpi_index']

future_cpis = []
for i in range(len(df_future)):
    last_cpi += cpi_trend
    future_cpis.append(round(last_cpi, 1))

df_future['cpi_index'] = future_cpis
df_full = pd.concat([df_hist, df_future], ignore_index=True)

# 3. Huấn luyện mô hình GradientBoostingRegressor
features = ['year', 'quarter', 'cpi_index']
target = 'apt_price_avg_m2'

# Nếu bạn có cài pycaret, có thể dùng: model = load_model('hcmc_real_estate_price_model')
# Ở đây ta dùng sklearn trực tiếp để đảm bảo chạy được mọi nơi:
gb_model = GradientBoostingRegressor(
    n_estimators=100, 
    learning_rate=0.1, 
    max_depth=3, 
    random_state=42
)

# Huấn luyện trên dữ liệu lịch sử (có giá apt_price_avg_m2)
X_train = df_hist[features]
y_train = df_hist[target]
gb_model.fit(X_train, y_train)

# 4. Dự báo
df_full['predicted_price'] = gb_model.predict(df_full[features])

# In kết quả năm 2026
print("Dự báo giá năm 2026:")
print(df_full[df_full['year'] == 2026][['year', 'quarter', 'predicted_price']])

# 5. Vẽ biểu đồ
plt.figure(figsize=(10, 6))
plt.plot(df_hist['year'].astype(str) + '-Q' + df_hist['quarter'].astype(str), 
         df_hist['apt_price_avg_m2'], label='Thực tế', marker='o')
plt.plot(df_full.iloc[-5:]['year'].astype(str) + '-Q' + df_full.iloc[-5:]['quarter'].astype(str), 
         df_full.iloc[-5:]['predicted_price'], label='Dự báo (GradientBoosting)', 
         marker='x', linestyle='--', color='red')
plt.title('Dự báo Giá Bất Động Sản TP.HCM')
plt.xticks(rotation=45)
plt.legend()
plt.tight_layout()
plt.show()