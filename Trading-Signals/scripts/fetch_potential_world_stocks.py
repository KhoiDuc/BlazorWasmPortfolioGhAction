import asyncio
import os
import pandas as pd
import asyncpg
import httpx
from tradingview_screener import Query, Column
from dotenv import load_dotenv

# Load environment variables
load_dotenv() 

async def send_slack_message(symbols_list):
    """Gửi danh sách cổ phiếu tiềm năng lên Slack"""
    slack_enabled = os.environ.get('SLACK_NOTIFICATIONS_ENABLED', 'false').lower() == 'true'
    if not slack_enabled:
        print("Slack notifications disabled, skipping")
        return
    
    slack_webhook_url = os.environ.get('SLACK_WEBHOOK_URL')
    if not slack_webhook_url or not symbols_list:
        return
    
    # SỬA LỖI TRUY XUẤT: Dùng key thay vì index
    formatted_lines = []
    # Giới hạn hiển thị khoảng 20-30 mã trên Slack để tránh quá tải tin nhắn
    for s in symbols_list[:30]: 
        line = f"• *{s['symbol']}* ({s['country']}) | Giá: `{s['price']:.2f}` | Cách đỉnh: *{s['diff_str']}*"
        formatted_lines.append(line)
    
    symbols_text = "\n".join(formatted_lines)
    message = {
        "text": f"🚀 *Cổ phiếu gần đỉnh 52 tuần ({len(symbols_list)} mã)*\n\n{symbols_text}"
    }
    
    try:
        async with httpx.AsyncClient() as client:
            await client.post(slack_webhook_url, json=message, timeout=15.0)
            print(f"✅ Đã gửi Slack thông báo cho {len(symbols_list)} mã.")
    except Exception as e:
        print(f"❌ Lỗi gửi Slack: {e}")

async def scan_market(market_code, country_name):
    print(f"🔍 Quét: {country_name}...")
    try:
        q = (Query()
              .select('name', 'close', 'price_52_week_high', 'market_cap_basic', 'description', 'exchange')
              .set_markets(market_code) 
              .where(
                  Column('market_cap_basic') > 1000000000, 
                  Column('price_52_week_high') > 0
              )
              .limit(30))
            
        raw_data = q.get_scanner_data()
        df = raw_data[1] if isinstance(raw_data, tuple) else raw_data

        results = []
        if df is not None and not df.empty:
            for _, row in df.iterrows():
                price = row.get('close')
                high_52 = row.get('price_52_week_high')
                
                if price and high_52 and high_52 > 0:
                    diff = (high_52 - price) / high_52
                    if diff <= 0.10: 
                        results.append({
                            "country": country_name,
                            "symbol": f"{row.get('exchange')}:{row.get('name')}",
                            "company_name": row.get('description'),
                            "price": price,
                            "high_52": high_52,
                            "diff_val": diff,
                            "diff_str": f"{diff:.2%}"
                        })
        return results
    except Exception as e:
        print(f"❌ Lỗi tại {country_name}: {e}")
        return []

async def main():
    # PHỤC HỒI ĐẦY ĐỦ 13 THỊ TRƯỜNG
    markets_to_scan = [
        ('vietnam', 'Việt Nam'),
        ('hongkong', 'Hồng Kông'),
        ('japan', 'Nhật Bản'),
        ('china', 'Trung Quốc'),
        ('india', 'Ấn Độ'),
        ('uk', 'Anh'),             
        ('france', 'Pháp'),        
        ('germany', 'Đức'),        
        ('netherlands', 'Hà Lan'), 
        ('switzerland', 'Thụy Sĩ'),
        ('italy', 'Ý'),            
        ('spain', 'Tây Ban Nha'),  
        ('america', 'Mỹ')
    ]

    all_stocks = []
    print(f"🚀 Bắt đầu quét {len(markets_to_scan)} thị trường...\n")

    for market, name in markets_to_scan:
        data = await scan_market(market, name)
        all_stocks.extend(data)
        await asyncio.sleep(0.5) 

    if all_stocks:
        # Hiển thị console
        df_display = pd.DataFrame(all_stocks).sort_values(by=['country', 'diff_val'])
        print("\n" + "="*80)
        print(df_display[['country', 'symbol', 'price', 'diff_str']].to_string(index=False))
        print("="*80)

        # Lưu Database
        conn = None
        try:
            conn = await asyncpg.connect(
                user=os.environ.get('DB_USER'),
                password=os.environ.get('DB_PASSWORD'),
                database=os.environ.get('DB_NAME'),
                host=os.environ.get('DB_HOST'),
                port=int(os.environ.get('DB_PORT', 5432))
            )
            async with conn.transaction():
                await conn.execute("DELETE FROM world_symbols_watchlist")
                await conn.executemany(
                    "INSERT INTO world_symbols_watchlist (country, symbol) VALUES ($1, $2)",
                    [(d["country"], d["symbol"]) for d in all_stocks]
                )
            print("✅ Đã cập nhật Database.")
            
            # Gửi Slack
            await send_slack_message(all_stocks)
            
        except Exception as e:
            print(f"❌ Lỗi xử lý DB: {e}")
        finally:
            if conn: await conn.close()
    else:
        print("Không có dữ liệu.")

if __name__ == "__main__":
    asyncio.run(main())