import json
import pandas as pd
from pycaret.regression import *

# 1. Load và Chuẩn bị dữ liệu
def get_data():
    try:
        with open('hcm_real_estate_cpi_2018_2026.json', 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        df = pd.DataFrame(data)
        
        # Chọn cột cần thiết
        df = df[['year', 'quarter', 'cpi_index', 'apt_price_avg_m2']]
        
        # Tách dữ liệu huấn luyện: Bỏ các dòng CPI bị null (năm 2026)
        train_data = df.dropna(subset=['cpi_index', 'apt_price_avg_m2'])
        
        return train_data
    except FileNotFoundError:
        print("Không tìm thấy file json!")
        return None

# 2. Chạy mô hình và Dự đoán
def run_prediction():
    # --- BƯỚC 1: HUẤN LUYỆN ---
    train_data = get_data()
    if train_data is None: return

    print("Dang setup PyCaret...")
    s = setup(data=train_data, target='apt_price_avg_m2', session_id=123, verbose=False)
    
    # Lấy mô hình tốt nhất (ví dụ: Linear Regression, Random Forest...)
    best_model = compare_models(sort='R2', n_select=1)
    
    # "Finalize" là bước quan trọng để chốt mô hình sau khi train xong
    final_model = finalize_model(best_model)
    
    print(f"\nModel được chọn: {best_model}")

    # --- BƯỚC 2: DỰ ĐOÁN NĂM 2026 ---
    print("\n--- ĐANG DỰ ĐOÁN GIÁ NĂM 2026 ---")
    
    # Tạo dữ liệu giả lập cho năm 2026 (Vì trong JSON gốc CPI đang là null)
    # Giả sử CPI tiếp tục tăng nhẹ trong năm 2026
    future_data = pd.DataFrame([
        {'year': 2026, 'quarter': 1, 'cpi_index': 119.0}, # Giả định CPI
        {'year': 2026, 'quarter': 2, 'cpi_index': 119.8},
        {'year': 2026, 'quarter': 3, 'cpi_index': 120.5},
        {'year': 2026, 'quarter': 4, 'cpi_index': 121.2}
    ])

    # Thực hiện dự đoán
    predictions = predict_model(final_model, data=future_data)
    
    # --- BƯỚC 3: HIỂN THỊ KẾT QUẢ ---
    # PyCaret thường trả về cột 'prediction_label' là giá dự đoán
    
    # Format lại số tiền cho dễ đọc
    pd.options.display.float_format = '{:,.0f}'.format
    
    # Đổi tên cột cho dễ hiểu
    results = predictions.rename(columns={'prediction_label': 'Gia_Du_Doan_VND'})
    
    print("\nKẾT QUẢ DỰ BÁO (Dựa trên giả định CPI):")
    print(results[['year', 'quarter', 'cpi_index', 'Gia_Du_Doan_VND']])

    # Save model
    save_model(final_model, 'hcmc_real_estate_price_model')

if __name__ == "__main__":
    run_prediction()