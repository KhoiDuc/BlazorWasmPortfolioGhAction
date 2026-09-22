#!/usr/bin/env python3
"""
Price Alert Utility
Checks price alerts from database and sends Slack notifications
"""

import os
import psycopg2
import requests
from datetime import datetime, timedelta, timezone
import logging
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)


def get_db_connection():
    """Create database connection"""
    return psycopg2.connect(
        host=os.getenv('DB_HOST', 'localhost'),
        port=os.getenv('DB_PORT', 5432),
        user=os.getenv('DB_USER', 'postgres'),
        password=os.getenv('DB_PASSWORD', ''),
        database=os.getenv('DB_NAME', 'trading')
    )


def send_slack_notification(message, webhook_url=None):
    """Send notification to Slack - ALWAYS sends regardless of env var"""
    if webhook_url is None:
        webhook_url = os.getenv('SLACK_WEBHOOK_URL')
    
    if not webhook_url:
        logger.warning("SLACK_WEBHOOK_URL not set, skipping notification")
        return False
    
    try:
        payload = {
            "text": message,
            "username": "Price Alert Bot",
            "icon_emoji": ":bell:"
        }
        response = requests.post(webhook_url, json=payload, timeout=10)
        if response.status_code == 200:
            logger.info(f"Slack notification sent: {message[:100]}")
            return True
        else:
            logger.error(f"Failed to send Slack notification: {response.status_code}")
            return False
    except Exception as e:
        logger.error(f"Error sending Slack notification: {e}")
        return False


def check_price_alerts(asset_type, symbol, current_price):
    """
    Check if any alerts should be triggered for this symbol
    
    Args:
        asset_type: Type of asset (crypto, stock, gold, silver, forex)
        symbol: Symbol/ticker name
        current_price: Current price of the asset
    
    Returns:
        Number of alerts triggered
    """
    try:
        conn = get_db_connection()
        cursor = conn.cursor()
        
        # Get active alerts for this symbol
        cursor.execute("""
            SELECT alert_price, operator, last_notified_at
            FROM price_alerts
            WHERE asset_type = %s 
            AND symbol = %s 
            AND is_active = true
        """, (asset_type, symbol))
        
        alerts = cursor.fetchall()
        triggered_count = 0
        
        for alert_price, operator, last_notified_at in alerts:
            # Convert Decimal to float if needed
            alert_price = float(alert_price)
            
            # Check if alert condition is met based on operator with 1% buffer
            alert_triggered = False
            if operator == '<=':
                # Alert when price drops to or below (alert_price + 1%)
                threshold = alert_price * 1.01
                alert_triggered = current_price <= threshold
            elif operator == '>=':
                # Alert when price rises to or above (alert_price - 1%)
                threshold = alert_price * 0.99
                alert_triggered = current_price >= threshold
            
            if alert_triggered:
                # Check if we should send notification (avoid spam)
                should_notify = True
                if last_notified_at:
                    # Only notify once per hour
                    time_since_last = datetime.now(timezone.utc) - last_notified_at
                    if time_since_last < timedelta(hours=1):
                        should_notify = False
                
                if should_notify:
                    # Send notification
                    price_diff = ((current_price - alert_price) / alert_price) * 100
                    
                    # Choose emoji based on operator and price movement
                    if operator == '<=':
                        emoji = "🔻"  # Price dropped
                        condition = "dropped to or below"
                    else:  # >=
                        emoji = "🚀"  # Price increased
                        condition = "rose to or above"
                    
                    message = (
                        f"{emoji} *Price Alert Triggered!*\n"
                        f"Asset: *{symbol}* ({asset_type.upper()})\n"
                        f"Condition: Price {condition} alert\n"
                        f"Alert Price: *${alert_price:,.2f}* ({operator})\n"
                        f"Current Price: *${current_price:,.2f}* "
                        f"({price_diff:+.2f}%)\n"
                        f"Time: {datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M:%S')}"
                    )
                    
                    if send_slack_notification(message):
                        # Update last_notified_at
                        cursor.execute("""
                            UPDATE price_alerts
                            SET last_notified_at = CURRENT_TIMESTAMP
                            WHERE symbol = %s AND asset_type = %s
                        """, (symbol, asset_type))
                        conn.commit()
                        triggered_count += 1
                        logger.info(f"Alert triggered for {symbol}")
        
        cursor.close()
        conn.close()
        
        return triggered_count
        
    except Exception as e:
        logger.error(f"Error checking price alerts: {e}")
        return 0


def check_multiple_alerts(asset_type, price_data):
    """
    Check alerts for multiple symbols at once
    
    Args:
        asset_type: Type of asset (crypto, stock, gold, silver, forex)
        price_data: Dictionary of {symbol: current_price}
    
    Returns:
        Total number of alerts triggered
    """
    total_triggered = 0
    for symbol, price in price_data.items():
        if price and price > 0:
            triggered = check_price_alerts(asset_type, symbol, price)
            total_triggered += triggered
    
    return total_triggered


if __name__ == "__main__":
    # Test the alert system
    print("Testing price alert system...")
    
    # Example: Check a crypto alert
    check_price_alerts("crypto", "BTCUSDT", 95000.0)
    
    print("Test complete!")
