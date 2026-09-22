#!/usr/bin/env python3
"""
Check Gold and Silver Price Alerts
Run this script periodically (e.g., via cron) to check gold/silver alerts
"""

import asyncio
import httpx
from price_alert_utils import check_price_alerts
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)


async def get_gold_price():
    """Fetch current gold price (XAUUSD)"""
    try:
        # Using a free forex API
        url = "https://api.exchangerate-api.com/v4/latest/USD"
        async with httpx.AsyncClient(timeout=10) as client:
            response = await client.get(url)
            if response.status_code == 200:
                # For demo - in production use real gold price API
                # This is a placeholder - replace with actual gold price API
                logger.info("Gold price API call placeholder")
                return 2050.0  # Placeholder
    except Exception as e:
        logger.error(f"Error fetching gold price: {e}")
    return None


async def get_silver_price():
    """Fetch current silver price (XAGUSD)"""
    try:
        # Using a free forex API
        url = "https://api.exchangerate-api.com/v4/latest/USD"
        async with httpx.AsyncClient(timeout=10) as client:
            response = await client.get(url)
            if response.status_code == 200:
                # For demo - in production use real silver price API
                # This is a placeholder - replace with actual silver price API
                logger.info("Silver price API call placeholder")
                return 24.50  # Placeholder
    except Exception as e:
        logger.error(f"Error fetching silver price: {e}")
    return None


async def check_gold_silver_alerts():
    """Check and trigger gold/silver price alerts"""
    logger.info("Checking gold and silver price alerts...")
    
    # Get current prices
    gold_price = await get_gold_price()
    silver_price = await get_silver_price()
    
    total_triggered = 0
    
    # Check gold alerts
    if gold_price:
        logger.info(f"Current Gold Price: ${gold_price:.2f}")
        triggered = check_price_alerts("gold", "XAUUSD", gold_price)
        total_triggered += triggered
    
    # Check silver alerts
    if silver_price:
        logger.info(f"Current Silver Price: ${silver_price:.2f}")
        triggered = check_price_alerts("silver", "XAGUSD", silver_price)
        total_triggered += triggered
    
    logger.info(f"Total alerts triggered: {total_triggered}")


if __name__ == "__main__":
    asyncio.run(check_gold_silver_alerts())
