import asyncio
import csv
import logging
import random
import os
import httpx
import asyncpg
from bs4 import BeautifulSoup
from datetime import datetime
import re

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


# Constants
BASE_URL = "https://mogi.vn"

# Regions map: Display Name -> URL slug
REGIONS = {
    "Hồ Chí Minh": "ho-chi-minh",
    "Bình Dương": "binh-duong",
    "Bà Rịa - Vũng Tàu": "ba-ria-vung-tau",
    "Đồng Nai": "dong-nai"
}

# Categories map: Type -> URL slug
CATEGORIES = {
    "House": "mua-nha",
    "Land": "mua-dat",
    "Apartment": "mua-can-ho"
}

USER_AGENTS = [
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36",
    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36"
]

def parse_price_to_numeric(price_text):
    """
    Parse price text to integer VND.
    Examples: "5 tỷ" -> 5000000000, "500 triệu" -> 500000000
    Returns None if parsing fails or complex format (e.g. /m2)
    """
    if not price_text:
        return None
    
    clean_text = price_text.lower().replace(',', '.').strip()
    
    # Check if price is per m2, ignore for total price (or handle separately)
    # User might want total price mostly.
    if "/m2" in clean_text:
        return None # Difficult to compare total prices with per m2 prices without area
    
    multiplier = 1
    if "tỷ" in clean_text:
        multiplier = 1_000_000_000
        val_str = clean_text.split("tỷ")[0].strip()
    elif "triệu" in clean_text:
        multiplier = 1_000_000
        val_str = clean_text.split("triệu")[0].strip()
    else:
        return None

    try:
        # Extract number from val_str
        # e.g. "5.2" -> 5.2
        # Use regex to find number
        import re
        match = re.search(r"(\d+(\.\d+)?)", val_str)
        if match:
            val = float(match.group(1))
            return int(val * multiplier)
    except:
        pass
    
    return None

def parse_area(text):
    """
    Parse area text to float (m2).
    """
    if not text:
        return 0.0
    try:
        match = re.search(r"(\d+(\.\d+)?)", text.replace(',', '.'))
        if match:
            return float(match.group(1))
    except:
        pass
    return 0.0

async def fetch_url(client, url):
    """Fetch URL with random User-Agent"""
    headers = {
        "User-Agent": random.choice(USER_AGENTS),
        "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
        "Accept-Language": "en-US,en;q=0.5",
        "Referer": "https://google.com"
    }
    try:
        response = await client.get(url, headers=headers)
        if response.status_code == 200:
            return response.text
        else:
            logger.error(f"Failed to fetch {url}: Status {response.status_code}")
            return None
    except Exception as e:
        logger.error(f"Error fetching {url}: {e}")
        return None

async def parse_listing_page(client, region_name, property_type, url):
    """Parse listing data from page"""
    html = await fetch_url(client, url)
    if not html:
        return []

    soup = BeautifulSoup(html, 'html.parser')
    data = []
    
    items = soup.find_all(class_="prop-info")
    if not items:
        # Fallback
        items = soup.find_all(class_="prop-item")
    
    if not items:
        titles = soup.find_all(class_="prop-title")
        items = [t.find_parent(class_="prop-info") or t.parent for t in titles]

    for item in items:
        if not item: continue
        
        # Title
        title_tag = item.find(class_="prop-title")
        title = title_tag.get_text(strip=True) if title_tag else "N/A"
        
        # Price
        price_tag = item.find(class_="price")
        if not price_tag:
             price_tag = item.find(class_="property-top-price")
        
        price_text = price_tag.get_text(strip=True) if price_tag else "N/A"
        price_numeric = parse_price_to_numeric(price_text)
        
        # Address
        addr_tag = item.find(class_="prop-addr")
        location = addr_tag.get_text(strip=True) if addr_tag else "N/A"
        
        # Area
        area = 0.0
        attr_list = item.find(class_="prop-attr")
        if attr_list:
             for li in attr_list.find_all('li'):
                 # Get text, handling sup tag for m2
                 # "82 m2" or "82 m 2"
                 txt = li.get_text(strip=True)
                 clean_txt = txt.lower().replace(' ', '').replace('\n', '')
                 
                 if 'm2' in clean_txt or 'm²' in clean_txt:
                     try:
                         # Extract valid number from start
                         match = re.search(r"(\d+(\.\d+)?)", clean_txt)
                         if match:
                             area = float(match.group(1))
                             break
                     except: pass
        if area == 0:
             # Fallback: search in full text if specific tag not found
             # Often in title or other text, but risky.
             # The markdown showed "- 82 m2", usually in a list.
             # Let's try to find any text with m2 in the item
             matches = re.findall(r"(\d+(?:[.,]\d+)?)\s*m2", item.get_text())
             if matches:
                 # Take the first one found ?
                 try:
                     area = float(matches[0].replace(',', '.'))
                 except: pass

        data.append({
            "region": region_name,
            "property_type": property_type,
            "title": title,
            "location": location,
            "price_text": price_text,
            "price_numeric": price_numeric,
            "area": area,
            "url": url,
            "fetched_at": datetime.now()
        })
    
    logger.info(f"Extracted {len(data)} items for {region_name} - {property_type}")
    return data

async def save_to_db(items):
    """Save items to database"""
    if not items:
        return

    conn = None
    try:
        db_port_str = os.environ.get('DB_PORT')
        if not db_port_str:
            logger.error("DB_PORT not set")
            return

        conn = await asyncpg.connect(
            user=os.environ.get('DB_USER'),
            password=os.environ.get('DB_PASSWORD'),
            database=os.environ.get('DB_NAME'),
            host=os.environ.get('DB_HOST'),
            port=int(db_port_str)
        )
        
        # Insert data
        query = """
            INSERT INTO real_estate_prices 
            (region, location, price_text, price_numeric, property_type, url, fetched_at, area)
            VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
        """
        
        values = [
            (
                item['region'],
                item['location'],
                item['price_text'],
                item['price_numeric'],
                item['property_type'],
                item['url'],
                item['fetched_at'],
                item['area']
            )
            for item in items
        ]
        
        await conn.executemany(query, values)
        logger.info(f"Saved {len(items)} records to DB")

    except Exception as e:
        logger.error(f"Database error: {e}")
    finally:
        if conn:
            await conn.close()

async def main():
    # Load env
    from dotenv import load_dotenv
    load_dotenv()

    async with httpx.AsyncClient(timeout=15, follow_redirects=True) as client:
        all_data = []
        
        for region_name, region_slug in REGIONS.items():
            for type_name, type_slug in CATEGORIES.items():
                url = f"{BASE_URL}/{region_slug}/{type_slug}"
                
                logger.info(f"Scraping {region_name} - {type_name} from {url}...")
                items = await parse_listing_page(client, region_name, type_name, url)
                all_data.extend(items)
                
                # Rate limit
                await asyncio.sleep(2)

        if all_data:
            await save_to_db(all_data)
        else:
            logger.error("No data extracted.")

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        pass
