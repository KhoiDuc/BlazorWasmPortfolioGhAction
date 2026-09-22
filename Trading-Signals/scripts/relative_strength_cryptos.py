import asyncio
import httpx
import time
import sys
from datetime import datetime

EXCLUDE_KEYWORDS = ["USDC", "USDE", "FDUSD", "USD1", "TUSD", "USDD", "USDP", "DAI"]

# ================================
#  API FUNCTIONS
# ================================
async def get_historical_price(symbol, from_timestamp, to_timestamp):
    """
    Get historical price data from Binance API
    """
    url = "https://api.binance.com/api/v3/klines"
    params = {
        "symbol": symbol,
        "interval": "1d",
        "startTime": from_timestamp,
        "endTime": to_timestamp,
        "limit": 1000
    }
    
    async with httpx.AsyncClient(timeout=30) as client:
        try:
            res = await client.get(url, params=params)
            res.raise_for_status()
            data = res.json()
            if data:
                # Return first candle open price and last candle close price
                first_price = float(data[0][1])  # Open price of first candle
                last_price = float(data[-1][4])   # Close price of last candle
                return first_price, last_price
            return None, None
        except Exception as e:
            print(f"Error fetching {symbol}: {e}")
            return None, None


async def get_top_coins():
    """
    Get top coins by market cap from CoinGecko
    """
    url = "https://api.coingecko.com/api/v3/coins/markets"
    async with httpx.AsyncClient(timeout=30) as client:
        res = await client.get(url, params={
            "vs_currency": "usd",
            "order": "market_cap_desc",
            "per_page": 250,
            "page": 1
        })
        res.raise_for_status()
        coins = res.json()
        # Filter out stablecoins
        return [
            coin for coin in coins
            if not any(keyword in coin['symbol'].upper() for keyword in EXCLUDE_KEYWORDS)
        ]


async def calculate_coin_performance(coin_symbol, from_timestamp, to_timestamp):
    """
    Calculate percentage change for a coin
    """
    symbol = f"{coin_symbol}USDT"
    first_price, last_price = await get_historical_price(symbol, from_timestamp, to_timestamp)
    
    if first_price and last_price and first_price > 0:
        pct_change = ((last_price - first_price) / first_price) * 100
        return {
            'symbol': coin_symbol,
            'first_price': first_price,
            'last_price': last_price,
            'pct_change': pct_change
        }
    return None


# ================================
#  MAIN ENTRY
# ================================
async def main():
    print("=== Crypto Relative Strength Calculator ===\n")
    
    # Parse command line arguments
    base_coin = "BTC"  # Default base coin
    date_input = None
    
    if len(sys.argv) < 2:
        print("Usage: python3 relative_strength_cryptos.py <start_date> [coin_symbol]")
        print("Examples:")
        print("  python3 relative_strength_cryptos.py 2025-10-05           (uses BTC as base)")
        print("  python3 relative_strength_cryptos.py 2025-10-05 BCH      (uses BCH as base)")
        return
    
    # Check if user provided coin symbol or just date
    if len(sys.argv) == 2:
        # Only date provided, use default BTC
        date_input = sys.argv[1]
    elif len(sys.argv) == 3:
        # Both date and coin provided
        date_input = sys.argv[1]
        base_coin = sys.argv[2].upper()
    else:
        print("Error: Too many arguments")
        print("Usage: python3 relative_strength_cryptos.py <start_date> [coin_symbol]")
        return
    
    try:
        start_date = datetime.strptime(date_input, "%Y-%m-%d")
    except ValueError:
        print(f"Error: Invalid date format '{date_input}'. Please use yyyy-mm-dd format.")
        print("Example: python3 relative_strength_cryptos.py 2025-10-05")
        return
    
    # Convert dates to timestamps (milliseconds)
    from_timestamp = int(start_date.timestamp() * 1000)
    to_timestamp = int(datetime.now().timestamp() * 1000)
    
    print(f"\nCalculating performance from {date_input} to now...")
    print(f"Base coin: {base_coin}\n")
    
    # Get top coins from CoinGecko
    print("Fetching top coins from CoinGecko...")
    coins = await get_top_coins()
    print(f"Found {len(coins)} coins\n")
    
    # Calculate base coin performance first
    print(f"Calculating {base_coin} performance...")
    base_result = await calculate_coin_performance(base_coin, from_timestamp, to_timestamp)
    
    if not base_result:
        print(f"Error: Could not fetch {base_coin} data")
        return
    
    base_pct_change = base_result['pct_change']
    print(f"{base_coin} Performance: {base_pct_change:.2f}%\n")
    
    # Calculate performance for all coins
    print("Calculating performance for all coins...")
    tasks = []
    for coin in coins[:150]:  # Process top 150 coins
        symbol = coin['symbol'].upper()
        if symbol == base_coin:
            continue
        tasks.append(calculate_coin_performance(symbol, from_timestamp, to_timestamp))
    
    results = await asyncio.gather(*tasks)
    
    # Filter valid results and calculate relative strength
    valid_results = []
    for result in results:
        if result and result['pct_change'] > base_pct_change:
            # Calculate Relative Strength vs base coin
            rs = result['pct_change'] / base_pct_change if base_pct_change != 0 else 0
            result['rs'] = rs
            valid_results.append(result)
    
    # Sort by % Change descending
    valid_results.sort(key=lambda x: x['pct_change'], reverse=True)
    
    # Get top 50
    top_50 = valid_results[:50]
    
    print(f"\n{'='*80}")
    print(f"Top 50 Coins with Performance > {base_coin} ({base_pct_change:.2f}%)")
    print(f"{'='*80}")
    print(f"{'Rank':<6} {'Symbol':<10} {'% Change':<12} {'RS vs ' + base_coin:<12} {'Start Price':<15} {'Current Price':<15}")
    print(f"{'-'*80}")
    
    for idx, coin in enumerate(top_50, 1):
        print(f"{idx:<6} {coin['symbol']:<10} {coin['pct_change']:>10.2f}%  {coin['rs']:>10.2f}x  ${coin['first_price']:>13.8f}  ${coin['last_price']:>13.8f}")
    
    print(f"\n{'='*80}")
    print(f"Found {len(valid_results)} coins outperforming {base_coin}")
    print(f"Showing top 50 by % Change (descending)")
    print(f"{'='*80}\n")


if __name__ == "__main__":
    asyncio.run(main())
