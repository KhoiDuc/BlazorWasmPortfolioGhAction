import time
from vnstock import Quote
import winsound
import os
from dotenv import load_dotenv
import psycopg2

load_dotenv()

def play_alert(symbol):
    # Phát âm thanh cảnh báo
    winsound.Beep(800, 200)  # Tần số 800Hz, thời lượng 200ms
    print(f">>> CẢNH BÁO: PHÁT HIỆN LỆNH LỚN CHO {symbol}! <<<")

def get_watchlist_symbols():
    conn = None
    try:
        conn = psycopg2.connect(
            host=os.getenv('DB_HOST'),
            port=int(os.getenv('DB_PORT', 5432)),
            database=os.getenv('DB_NAME'),
            user=os.getenv('DB_USER'),
            password=os.getenv('DB_PASSWORD')
        )
        cur = conn.cursor()
        query = """
        SELECT symbol
        FROM public.symbols_watchlist
        WHERE signal_type IN ('ema9_above_ema21', 'near_52w_ath', 'top_growth_20d')
        GROUP BY symbol
        HAVING COUNT(DISTINCT signal_type) = 3;
        """
        cur.execute(query)
        symbols = [row[0] for row in cur.fetchall()]
        cur.close()
        return symbols
    except Exception as e:
        print(f"Lỗi kết nối DB: {e}")
        return ['HCM', 'GEX', 'VHM']  # Fallback to default list
    finally:
        if conn:
            conn.close()


def normalize_symbols(symbols):
    normalized = []
    for symbol in symbols:
        if not symbol:
            continue
        if not isinstance(symbol, str):
            symbol = str(symbol)
        symbol = symbol.strip().upper()
        if symbol:
            normalized.append(symbol)
    return normalized


def merge_symbols(db_symbols, extra_symbols):
    merged = []
    seen = set()
    for symbol in normalize_symbols(db_symbols) + normalize_symbols(extra_symbols):
        if symbol not in seen:
            seen.add(symbol)
            merged.append(symbol)
    return merged


def monitor_stocks(symbols, threshold=5000):
    last_processed_time = {symbol: set() for symbol in symbols}
    
    print(f"Bắt đầu theo dõi {symbols} với ngưỡng lệnh > {threshold}...")
    
    while True:
        try:
            for symbol in symbols:
                # Khởi tạo Quote cho từng symbol
                q = Quote(symbol=symbol, source='kbs')
                
                # Lấy dữ liệu khớp lệnh trong ngày (intraday) - lấy 50 ticks gần nhất để có đủ dữ liệu
                df = q.intraday(page_size=50, show_log=False)
                
                if df.empty:
                    continue
                
                # Lấy các giao dịch gần nhất (giả sử df được sắp xếp theo thời gian tăng dần)
                recent_trades = df.tail(10)  # Kiểm tra 10 giao dịch gần nhất
                
                for _, trade in recent_trades.iterrows():
                    current_time = trade['time']
                    volume = trade['volume']
                    price = trade['price']
                    side = trade.get('side', 'N/A')
                    
                    # Kiểm tra nếu là lệnh mới và đạt ngưỡng khối lượng
                    if current_time not in last_processed_time[symbol] and volume >= threshold:
                        print(f"[{current_time}] {symbol}: {side} {volume:,} cp tại giá {price}")
                        play_alert(symbol)
                        last_processed_time[symbol].add(current_time)
                        
                        # Giới hạn bộ nhớ, chỉ giữ 100 thời gian gần nhất
                        if len(last_processed_time[symbol]) > 100:
                            last_processed_time[symbol].pop()
            
            # Nghỉ 15 giây trước khi quét lượt tiếp theo để tránh bị khóa API (Rate limit)
            time.sleep(15)
            
        except Exception as e:
            print(f"Lỗi kết nối: {e}")
            time.sleep(5)

if __name__ == "__main__":
    manual_symbols = [
        # Thêm các symbol muốn theo dõi thủ công tại đây.
        # Ví dụ: 'HCM', 'VHM'
    ]
    db_symbols = get_watchlist_symbols()
    watch_list = merge_symbols(db_symbols, manual_symbols)
    print(f"Danh sách symbol theo dõi (không trùng): {watch_list}")
    monitor_stocks(watch_list, threshold=5000)