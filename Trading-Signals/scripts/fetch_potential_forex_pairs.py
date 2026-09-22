import asyncio
import sys
from datetime import datetime, timedelta
import yfinance as yf
import pandas as pd
import asyncpg
import os
import httpx
from dotenv import load_dotenv
from price_alert_utils import check_multiple_alerts

# Load environment variables from .env file
load_dotenv()


async def send_slack_message(recommendations, currency_strength, from_date, to_date):
    """Send forex trading recommendations to Slack"""
    slack_enabled = os.environ.get('SLACK_NOTIFICATIONS_ENABLED', 'false').lower() == 'true'
    if not slack_enabled:
        print("Slack notifications disabled, skipping")
        return
    
    slack_webhook_url = os.environ.get('SLACK_WEBHOOK_URL')
    if not slack_webhook_url:
        print("SLACK_WEBHOOK_URL not set, skipping Slack notification")
        return
    
    if not recommendations:
        print("No forex recommendations to report")
        return
    
    # Sort currencies by strength for summary
    sorted_currencies = sorted(currency_strength.items(), key=lambda x: x[1], reverse=True)
    
    # Build message
    message_parts = [
        f"💱 *Forex Trading Recommendations* ({from_date} to {to_date})\n",
        f"\n*Currency Strength:*"
    ]
    
    # Show top 3 strongest and weakest
    strongest = sorted_currencies[:3]
    weakest = sorted_currencies[-3:]
    
    message_parts.append("\n📈 *Strongest:*")
    for curr, score in strongest:
        message_parts.append(f"\n• {curr}: {score:+.2f}%")
    
    message_parts.append("\n\n📉 *Weakest:*")
    for curr, score in weakest:
        message_parts.append(f"\n• {curr}: {score:+.2f}%")
    
    # Show top recommendations (limit to 10)
    top_recs = recommendations[:10]
    message_parts.append(f"\n\n*Top Trading Setups ({len(top_recs)}):*")
    
    for rec in top_recs:
        setup_line = f"\n• *{rec['action']}* {rec['pair']} (Score: {rec['score_diff']:+.2f}%)"
        if rec['position_note']:
            # Replace emoji with text for better Slack compatibility
            note = rec['position_note'].replace('📍', '⚠️')
            setup_line += f"\n  {note}"
        message_parts.append(setup_line)
    
    if len(recommendations) > 10:
        message_parts.append(f"\n\n_...and {len(recommendations) - 10} more setups_")
    
    message = {"text": "".join(message_parts)}
    
    try:
        async with httpx.AsyncClient() as client:
            response = await client.post(
                slack_webhook_url,
                json=message,
                timeout=10.0
            )
            if response.status_code == 200:
                print(f"Slack notification sent successfully ({len(recommendations)} recommendations)")
            else:
                print(f"Failed to send Slack notification: {response.status_code}")
    except Exception as e:
        print(f"Error sending Slack message: {e}")

# Database configuration (will be loaded from environment or parameters)
DB_CONFIG = {
    'host': os.getenv('DB_HOST', 'localhost'),
    'database': os.getenv('DB_NAME', 'trading'),
    'user': os.getenv('DB_USER', 'postgres'),
    'password': os.getenv('DB_PASSWORD', ''),
    'port': os.getenv('DB_PORT', '5432')
}

# Currency pairs configuration (yfinance format)
CURRENCY_PAIRS = {
    'USD': ['EURUSD=X', 'USDJPY=X', 'GBPUSD=X', 'USDCHF=X', 'AUDUSD=X', 'USDCAD=X', 'GC=F', 'CL=F', 'DX-Y.NYB'],
    'EUR': ['EURUSD=X'],
    'JPY': ['USDJPY=X'],
    'GBP': ['GBPUSD=X'],
    'AUD': ['AUDUSD=X'],
    'CHF': ['USDCHF=X'],
    'CAD': ['USDCAD=X'],
    'XAU': ['GC=F'],
    'WTI': ['CL=F'],
    'DXY': ['DX-Y.NYB'],
    'US02Y': ['^IRX'],
    'US10Y': ['^TNX'],
    'US30Y': ['^TYX']
}

# Mapping back to standard names for display
PAIR_DISPLAY_NAMES = {
    'EURUSD=X': 'EURUSD',
    'USDJPY=X': 'USDJPY',
    'GBPUSD=X': 'GBPUSD',
    'USDCHF=X': 'USDCHF',
    'AUDUSD=X': 'AUDUSD',
    'USDCAD=X': 'USDCAD',
    'GC=F': 'XAUUSD',
    'CL=F': 'WTI',
    'DX-Y.NYB': 'DXY',
    '^IRX': 'US02Y',
    '^TNX': 'US10Y',
    '^TYX': 'US30Y'
}


async def get_forex_data(pair, from_date, to_date):
    """
    Fetch forex data from Yahoo Finance (supports macOS)
    """
    try:
        # Add one day to end date to include the last day
        date_to = datetime.strptime(to_date, "%Y-%m-%d") + timedelta(days=1)
        to_date_str = date_to.strftime("%Y-%m-%d")
        
        # Add delay to avoid rate limiting
        await asyncio.sleep(0.5)
        
        # Download data from Yahoo Finance - let YF handle session
        ticker = yf.Ticker(pair)
        df = ticker.history(start=from_date, end=to_date_str, timeout=10)
        
        if df is not None and len(df) > 0:
            # Get first and last close prices
            first_price = float(df.iloc[0]['Close'])
            last_price = float(df.iloc[-1]['Close'])
            
            # Normalize CBOE interest rate indices (e.g. ^IRX, ^TNX, ^TYX) from basis points / 10 to actual percentage
            if pair in ['^IRX', '^TNX', '^TYX']:
                if first_price > 10.0:
                    first_price /= 10.0
                if last_price > 10.0:
                    last_price /= 10.0
            
            return first_price, last_price
        else:
            print(f"No data available for {pair}")
            return None, None
        
    except Exception as e:
        print(f"Error fetching {pair}: {e}")
        return None, None


async def get_52week_data(pair):
    """
    Get 52-week high and low data for a pair
    """
    try:
        # Get last 52 weeks of data
        end_date = datetime.now()
        start_date = end_date - timedelta(weeks=52)
        
        # Add delay to avoid rate limiting
        await asyncio.sleep(0.5)
        
        # Let YF handle session
        ticker = yf.Ticker(pair)
        df = ticker.history(start=start_date, end=end_date, timeout=10)
        
        if df is not None and len(df) > 0:
            week_52_high = float(df['High'].max())
            week_52_low = float(df['Low'].min())
            current_price = float(df.iloc[-1]['Close'])
            
            # Normalize CBOE interest rate indices (e.g. ^IRX, ^TNX, ^TYX) from basis points / 10 to actual percentage
            if pair in ['^IRX', '^TNX', '^TYX']:
                if week_52_high > 10.0:
                    week_52_high /= 10.0
                if week_52_low > 10.0:
                    week_52_low /= 10.0
                if current_price > 10.0:
                    current_price /= 10.0
            
            # Calculate distance from 52w high and low
            distance_from_high = ((current_price - week_52_high) / week_52_high) * 100
            distance_from_low = ((current_price - week_52_low) / week_52_low) * 100
            
            return {
                'high_52w': week_52_high,
                'low_52w': week_52_low,
                'current': current_price,
                'dist_from_high': distance_from_high,
                'dist_from_low': distance_from_low
            }
        
        return None
        
    except Exception as e:
        print(f"Error fetching 52w data for {pair}: {e}")
        return None


async def calculate_pair_strength(pair, from_date, to_date):
    """
    Calculate percentage change for a forex pair
    """
    first_price, last_price = await get_forex_data(pair, from_date, to_date)
    
    if first_price and last_price and first_price > 0:
        pct_change = ((last_price - first_price) / first_price) * 100
        # Get display name
        display_name = PAIR_DISPLAY_NAMES.get(pair, pair)
        return {
            'pair': display_name,
            'yf_pair': pair,
            'first_price': first_price,
            'last_price': last_price,
            'pct_change': pct_change
        }
    return None


def calculate_currency_strength(pair_results):
    """
    Calculate individual currency strength based on pair movements
    
    For each currency:
    - If currency is base and pair goes up -> currency strengthens
    - If currency is base and pair goes down -> currency weakens
    - If currency is quote and pair goes up -> currency weakens
    - If currency is quote and pair goes down -> currency strengthens
    """
    currency_scores = {
        'USD': [], 'EUR': [], 'JPY': [], 'GBP': [], 
        'AUD': [], 'CAD': [], 'CHF': [], 'XAU': [], 
        'WTI': [], 'DXY': []
    }
    
    for result in pair_results:
        if not result:
            continue
            
        pair = result['pair']
        pct_change = result['pct_change']
        
        # Handle US Treasury Yields separately - they do not impact currency strength scores
        if pair in ['US02Y', 'US10Y', 'US30Y']:
            continue
            
        # Handle DXY index separately - DXY up = USD strengthens, DXY down = USD weakens
        if pair == 'DXY':
            currency_scores['USD'].append(pct_change)
            currency_scores['DXY'].append(pct_change)
            continue
        
        # Handle special commodity displays
        if pair == 'WTI':
            base = 'WTI'
            quote = 'USD'
        else:
            base = pair[:3]
            quote = pair[3:]
        
        # Base currency: positive change = strength
        if base in currency_scores:
            currency_scores[base].append(pct_change)
        
        # Quote currency: positive change = weakness (inverse)
        if quote in currency_scores:
            currency_scores[quote].append(-pct_change)
    
    # Calculate average strength for each currency
    currency_strength = {}
    for currency, scores in currency_scores.items():
        if scores:
            currency_strength[currency] = sum(scores) / len(scores)
        else:
            currency_strength[currency] = 0
    
    return currency_strength


def generate_recommendations(currency_strength, valid_results, pair_52w_data):
    """
    Generate trading recommendations based on currency strength
    Buy strong currency against weak currency
    Also include neutral currencies against very weak ones (≤ -2%)
    Show all potential pairs, with note if near 52-week high or low (within 1%)
    """
    # Sort currencies by strength
    sorted_currencies = sorted(currency_strength.items(), key=lambda x: x[1], reverse=True)
    
    # Get top 3 strongest
    strongest = [c[0] for c in sorted_currencies[:3]]
    
    # Get neutral currencies (-1% to 1%, not in top 3)
    neutral = [c[0] for c in sorted_currencies if -1 <= c[1] <= 1 and c[0] not in strongest]
    
    # Get very weak currencies (≤ -2%)
    very_weak = [c[0] for c in sorted_currencies if c[1] <= -2]
    
    # Get all weak currencies (for strong pairs)
    weakest = [c[0] for c in sorted_currencies[-3:]]
    
    recommendations = []
    added_setups = set()
    
    # Get all available pairs
    available_pairs = {r['pair'] for r in valid_results}
    
    # Function to add recommendation
    def try_add_recommendation(base, quote, action, base_type, quote_type):
        pair = f"{base}{quote}"
        if pair == 'WTIUSD':
            pair = 'WTI'
            
        if pair in available_pairs:
            setup_key = (pair, action)
            if setup_key in added_setups:
                return
                
            # Check 52w data
            week_data = pair_52w_data.get(pair)
            
            # Determine position info
            position_note = ""
            near_extreme = False
            dist_from_high = None
            dist_from_low = None
            
            if week_data:
                dist_from_high = week_data['dist_from_high']
                dist_from_low = week_data['dist_from_low']
                
                # Check if near 52w high or low (within 1%)
                near_high = dist_from_high >= -1.0
                near_low = dist_from_low <= 1.0
                
                if near_high:
                    position_note = f"📍 Near 52W HIGH ({dist_from_high:+.2f}%)"
                    near_extreme = True
                elif near_low:
                    position_note = f"📍 Near 52W LOW ({dist_from_low:+.2f}%)"
                    near_extreme = True
            
            # Calculate score_diff as the absolute setup strength (always positive for the correct action)
            score_diff = currency_strength[base] - currency_strength[quote] if action == 'Buy' else currency_strength[quote] - currency_strength[base]

            recommendations.append({
                'action': action,
                'pair': pair,
                'reason': f"{base} is {base_type}, {quote} is {quote_type}",
                'base_score': currency_strength[base],
                'quote_score': currency_strength[quote],
                'score_diff': score_diff,
                'position_note': position_note,
                'near_extreme': near_extreme,
                'dist_from_high': dist_from_high,
                'dist_from_low': dist_from_low
            })
            added_setups.add(setup_key)
    
    # 1. Majors vs Majors (Divergence Threshold)
    MAJORS = {'USD', 'EUR', 'JPY', 'GBP', 'AUD', 'CAD', 'CHF'}
    for base in MAJORS:
        for quote in MAJORS:
            if base != quote:
                diff = currency_strength[base] - currency_strength[quote]
                if diff >= 3.0: # 3% divergence threshold
                    try_add_recommendation(base, quote, 'Buy', 'strong major', 'weak major')
    
    # 2. Overall Strong vs Weak (including commodities/exotics)
    for strong in strongest:
        for weak in weakest:
            try_add_recommendation(strong, weak, 'Buy', 'strong', 'weak')
            try_add_recommendation(weak, strong, 'Sell', 'weak', 'strong')
    
    # 3. Neutral vs Very Weak (≤ -2%)
    for neut in neutral:
        for vw in very_weak:
            try_add_recommendation(neut, vw, 'Buy', 'neutral', 'very weak')
            try_add_recommendation(vw, neut, 'Sell', 'very weak', 'neutral')
            
    # 4. Near 52-Week High or Low (Breakout Setups)
    for result in valid_results:
        pair = result['pair']
        if pair in ['US02Y', 'US10Y', 'US30Y', 'DXY']:
            continue
            
        week_data = pair_52w_data.get(pair)
        if week_data:
            dist_from_high = week_data['dist_from_high']
            dist_from_low = week_data['dist_from_low']
            
            if dist_from_high >= -1.0: # Within 1% of 52W High
                if pair == 'WTI':
                    base, quote = 'WTI', 'USD'
                elif pair == 'XAUUSD':
                    base, quote = 'XAU', 'USD'
                else:
                    base, quote = pair[:3], pair[3:]
                try_add_recommendation(base, quote, 'Buy', '52w high breakout base', '52w high breakout quote')
                
            elif dist_from_low <= 1.0: # Within 1% of 52W Low
                if pair == 'WTI':
                    base, quote = 'WTI', 'USD'
                elif pair == 'XAUUSD':
                    base, quote = 'XAU', 'USD'
                else:
                    base, quote = pair[:3], pair[3:]
                try_add_recommendation(base, quote, 'Sell', '52w low breakout base', '52w low breakout quote')
    
    # Sort by score difference (highest first)
    recommendations.sort(key=lambda x: x['score_diff'], reverse=True)
    
    return recommendations


async def save_recommendations_to_db(recommendations, from_date, to_date, pair_52w_data):
    """
    Save recommendations to PostgreSQL database
    Truncates existing data and inserts fresh recommendations
    """
    try:
        # Connect to database
        conn = await asyncpg.connect(**DB_CONFIG)
        
        # Truncate existing data
        await conn.execute("TRUNCATE TABLE forex_watchlist;")
        
        # Prepare data for insertion
        updated_at = datetime.now()
        data_to_insert = []
        
        for rec in recommendations:
            data_to_insert.append((
                rec['pair'],
                rec['action'],
                rec['score_diff'],
                rec['position_note'] if rec['position_note'] else None,
                updated_at
            ))
        
        # Insert data
        insert_query = """
            INSERT INTO forex_watchlist (
                pair, action, score_diff, note, updated_at
            ) VALUES ($1, $2, $3, $4, $5)
        """
        
        await conn.executemany(insert_query, data_to_insert)
        
        await conn.close()
        
        return True, len(data_to_insert)
        
    except Exception as e:
        return False, str(e)


async def update_yield_alerts_in_db(pair_52w_data):
    """
    Update yield breakout alerts in public.triggered_alerts database.
    - If a yield (US02Y, US10Y, US30Y) is near its 52-week high (within 1%), upsert/insert the alert.
    - If it is not, delete any existing alert for that yield.
    """
    try:
        conn = await asyncpg.connect(**DB_CONFIG)
        
        yield_pairs = ['US02Y', 'US10Y', 'US30Y']
        for pair in yield_pairs:
            data = pair_52w_data.get(pair)
            if not data:
                continue
                
            curr = data.get('current')
            high_52w = data.get('high_52w')
            dist_high = data.get('dist_from_high', -999.0)
            
            # Normalize CBOE interest rate indices if needed
            if curr > 10.0:
                curr /= 10.0
                high_52w /= 10.0
                
            # If yield is near its 52W High (dist_from_high >= -1.0)
            if dist_high >= -1.0:
                message = f"Cảnh báo Lợi suất: Lợi suất trái phiếu {pair} đã tiệm cận hoặc vượt đỉnh 52 tuần tại mức {curr:.3f}%."
                
                # Delete existing alert for the same symbol first to avoid duplicates
                await conn.execute("DELETE FROM public.triggered_alerts WHERE symbol = $1;", pair)
                
                # Insert the new alert
                await conn.execute("""
                    INSERT INTO public.triggered_alerts (asset_type, symbol, price, message, is_read)
                    VALUES ($1, $2, $3, $4, false);
                """, 'yield', pair, curr, message)
                
                print(f"💾 Đã cập nhật cảnh báo vượt đỉnh cho {pair} vào website database!")
            else:
                # Delete alert for the symbol if it's no longer near 52W high
                result = await conn.execute("DELETE FROM public.triggered_alerts WHERE symbol = $1;", pair)
                if result != "DELETE 0":
                    print(f"🧹 Đã xóa cảnh báo cho {pair} vì không còn gần đỉnh 52 tuần.")
                    
        await conn.close()
    except Exception as e:
        print(f"❌ Lỗi cập nhật triggered_alerts cho Lợi suất: {e}")


def configure_db(host=None, database=None, user=None, password=None, port=None):
    """
    Configure database connection parameters
    """
    if host:
        DB_CONFIG['host'] = host
    if database:
        DB_CONFIG['database'] = database
    if user:
        DB_CONFIG['user'] = user
    if password:
        DB_CONFIG['password'] = password
    if port:
        DB_CONFIG['port'] = port


async def main():
    print("=== Currency Strength Calculator (Yahoo Finance) ===\n")
    
    # Get date from command line argument (optional)
    if len(sys.argv) >= 2:
        from_date = sys.argv[1]
        try:
            start_date = datetime.strptime(from_date, "%Y-%m-%d")
        except ValueError:
            print(f"Error: Invalid date format '{from_date}'. Please use yyyy-mm-dd format.")
            return
    else:
        # Default to 1 month ago
        start_date = datetime.now() - timedelta(days=30)
        from_date = start_date.strftime("%Y-%m-%d")
        print(f"No date provided, using default: 1 month ago ({from_date})\n")
    
    to_date = datetime.now().strftime("%Y-%m-%d")
    
    print(f"Calculating currency strength from {from_date} to {to_date}...\n")
    
    # Get all unique pairs
    all_pairs = set()
    for pairs in CURRENCY_PAIRS.values():
        all_pairs.update(pairs)
    
    print(f"Fetching data for {len(all_pairs)} currency pairs from Yahoo Finance...")
    
    # Fetch data for all pairs
    pair_results = []
    for pair in all_pairs:
        result = await calculate_pair_strength(pair, from_date, to_date)
        pair_results.append(result)
    
    # Filter valid results
    valid_results = [r for r in pair_results if r]
    
    if not valid_results:
        print("\n⚠️  Warning: Could not fetch forex data.")
        print("Please check:")
        print("  - Your internet connection")
        print("  - The date range is valid")
        print("  - Yahoo Finance service is available")
        return
    
    print(f"✅ Successfully fetched data for {len(valid_results)} pairs\n")
    
    # Fetch 52-week data for all pairs
    print("Fetching 52-week high/low data...")
    pair_52w_data = {}
    for result in valid_results:
        yf_pair = result['yf_pair']
        pair_name = result['pair']
        week_data = await get_52week_data(yf_pair)
        if week_data:
            pair_52w_data[pair_name] = week_data
    
    print(f"✅ Successfully fetched 52w data for {len(pair_52w_data)} pairs\n")
    
    # Calculate currency strength
    currency_strength = calculate_currency_strength(valid_results)
    
    # Sort currencies by strength
    sorted_currencies = sorted(currency_strength.items(), key=lambda x: x[1], reverse=True)
    
    # Display results
    print(f"\n{'='*60}")
    print(f"Currency Strength Ranking")
    print(f"{'='*60}")
    print(f"{'Rank':<6} {'Currency':<10} {'Strength Score':<15} {'Status':<15}")
    print(f"{'-'*60}")
    
    for idx, (currency, strength) in enumerate(sorted_currencies, 1):
        status = "Strong 💪" if strength > 1 else "Weak 📉" if strength < -1 else "Neutral ➡️"
        print(f"{idx:<6} {currency:<10} {strength:>13.2f}%  {status:<15}")
    
    # Display pair details
    print(f"\n{'='*60}")
    print(f"Currency Pair Performance")
    print(f"{'='*60}")
    print(f"{'Pair':<10} {'% Change':<12} {'Start Price':<15} {'End Price':<15}")
    print(f"{'-'*60}")
    
    for result in sorted(valid_results, key=lambda x: x['pct_change'], reverse=True):
        # Highlight DXY index
        prefix = "📊" if result['pair'] == 'DXY' else " "
        print(f"{prefix} {result['pair']:<9} {result['pct_change']:>10.2f}%  {result['first_price']:>13.2f}  {result['last_price']:>13.2f}")
    
    # Display DXY Index separate section
    dxy_result = next((r for r in valid_results if r['pair'] == 'DXY'), None)
    dxy_52w = pair_52w_data.get('DXY')
    if dxy_result and dxy_52w:
        print(f"\n{'='*60}")
        print(f"📊 USD Dollar Index (DXY)")
        print(f"{'='*60}")
        print(f"  Current:     {dxy_result['last_price']:>10.2f}")
        print(f"  52W High:    {dxy_52w['high_52w']:>10.2f}  ({dxy_52w['dist_from_high']:+.2f}%)")
        print(f"  52W Low:     {dxy_52w['low_52w']:>10.2f}  ({dxy_52w['dist_from_low']:+.2f}%)")
        print(f"  Period Chg:  {dxy_result['pct_change']:>10.2f}%")
        dxy_status = "Bullish ↗️" if dxy_result['pct_change'] > 1 else "Bearish ↘️" if dxy_result['pct_change'] < -1 else "Neutral ➡️"
        print(f"  Signal:      {dxy_status}")
        print(f"{'='*60}")
        
    # Display US Treasury Yields separate section
    yield_results = [r for r in valid_results if r['pair'] in ['US02Y', 'US10Y', 'US30Y']]
    if yield_results:
        print(f"\n{'='*65}")
        print(f"📈 US Treasury Yields")
        print(f"{'='*65}")
        print(f"  {'Yield':<10} {'Current':<10} {'52W High':<18} {'52W Low':<18} {'Period Chg':<12}")
        print(f"  {'-'*61}")
        for r in yield_results:
            pair_name = r['pair']
            w_data = pair_52w_data.get(pair_name)
            if w_data:
                curr = w_data['current']
                high_52w = w_data['high_52w']
                low_52w = w_data['low_52w']
                dist_high = w_data['dist_from_high']
                dist_low = w_data['dist_from_low']
                print(f"  {pair_name:<10} {curr:>8.3f}%  {high_52w:>8.3f}% ({dist_high:+.2f}%)  {low_52w:>8.3f}% ({dist_low:+.2f}%)  {r['pct_change']:>9.2f}%")
        print(f"{'='*65}")
    
    print(f"\n{'='*60}")
    print(f"Analysis Period: {from_date} to {to_date}")
    print(f"Total Pairs Analyzed: {len(valid_results)}")
    print(f"Data Source: Yahoo Finance")
    print(f"{'='*60}\n")
    
    # Generate and display recommendations
    recommendations = generate_recommendations(currency_strength, valid_results, pair_52w_data)
    
    if recommendations:
        print(f"{'='*70}")
        print(f"TRADING RECOMMENDATIONS")
        print(f"{'='*70}")
        print(f"Based on currency strength divergence:\n")
        print("Setup:\n")
        
        # Display recommendations
        for rec in recommendations:
            setup_line = f"{rec['action']:4} {rec['pair']:8} (Score Diff: {rec['score_diff']:+.2f}%)"
            
            # Add note if near 52W extreme
            if rec['position_note']:
                setup_line += f"  {rec['position_note']}"
            
            print(setup_line)
        
        print(f"\n{'='*70}")
        print("Legend:")
        print("  📍 = Near 52-week HIGH or LOW (within 1%) - High priority setup")
        print("\nNote: These are suggestions based on relative strength.")
        print("Always use proper risk management and confirm with your analysis.")
        print(f"{'='*70}\n")
        
        # Save to database if configured
        if DB_CONFIG.get('password'):  # Only try to save if DB is configured
            print("Saving recommendations to database...")
            success, result = await save_recommendations_to_db(recommendations, from_date, to_date, pair_52w_data)
            
            if success:
                print(f"✅ Successfully saved {result} recommendations to database\n")
            else:
                print(f"⚠️  Failed to save to database: {result}\n")
                
            # Update US Treasury Yield alerts on homepage
            print("Updating yield alerts in database...")
            await update_yield_alerts_in_db(pair_52w_data)
        else:
            print("💡 Database not configured. Set DB_HOST, DB_NAME, DB_USER, DB_PASSWORD to enable saving.\n")
        
        # Send Slack notification
        await send_slack_message(recommendations, currency_strength, from_date, to_date)
        
        # Check price alerts for forex pairs
        print("🔹 Checking price alerts for forex pairs...")
        price_data = {}
        for pair, data in pair_52w_data.items():
            current_price = data.get('current')  # Changed from 'current_price' to 'current'
            if current_price and current_price > 0:
                # Use standard format without =X: EURUSD, GBPUSD, etc
                price_data[pair] = current_price
        
        if price_data:
            triggered = check_multiple_alerts("forex", price_data)
            print(f"🔔 Triggered {triggered} forex price alerts")
    else:
        print("\nNo clear trading setups found based on current currency strength.\n")


if __name__ == "__main__":
    asyncio.run(main())
