import os
import logging
import asyncio
from pyrogram import Client, idle
import psycopg2
from psycopg2.extras import RealDictCursor
from dotenv import load_dotenv

load_dotenv()
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

API_ID = os.getenv("TG_API_ID")
API_HASH = os.getenv("TG_API_HASH")
CHANNELS = os.getenv("TG_CHANNELS", "").split(",")
DB_URL = os.getenv("DATABASE_URL")

# Ensure session directory exists
os.makedirs("session", exist_ok=True)
app = Client("session/my_account", api_id=API_ID, api_hash=API_HASH)

def save_to_db(message):
    try:
        conn = psycopg2.connect(DB_URL)
        cur = conn.cursor()
        
        # We need to find or create a news_group for Telegram News if it doesn't exist
        # For simplicity, assuming group_id = 1 is the default for TG News, but ideally we query it.
        cur.execute("SELECT id FROM news_groups WHERE name = 'Telegram News' LIMIT 1")
        group_row = cur.fetchone()
        if not group_row:
            # Need a user_id to create a group. Assuming user_id=1 exists or is system user
            cur.execute("INSERT INTO news_groups (user_id, name, description) VALUES (1, 'Telegram News', 'Automated Telegram Feed') RETURNING id")
            group_id = cur.fetchone()[0]
        else:
            group_id = group_row[0]

        source_url = f"https://t.me/{message.chat.username}/{message.id}" if message.chat.username else ""
        content = message.text or message.caption or ""
        title = content[:100] + "..." if len(content) > 100 else content

        if not content.strip():
            return

        # Check if message already exists by source_url to prevent duplicates from polling
        if source_url:
            cur.execute("SELECT 1 FROM news_items WHERE source_url = %s LIMIT 1", (source_url,))
            if cur.fetchone():
                cur.close()
                conn.close()
                return

        cur.execute("""
            INSERT INTO news_items (group_id, title, content, source_url, importance, status, created_at)
            VALUES (%s, %s, %s, %s, %s, %s, %s)
            RETURNING id
        """, (group_id, title, content, source_url, 3, 'active', message.date))
        
        conn.commit()
        cur.close()
        conn.close()
        logger.info(f"Saved message from {message.chat.username or message.chat.title}")
    except Exception as e:
        logger.error(f"Error saving to DB: {e}")

@app.on_message()
async def my_handler(client, message):
    if not message.chat:
        return
        
    username = message.chat.username
    chat_id = str(message.chat.id)
    normalized_channels = [c.strip().lower() for c in CHANNELS if c.strip()]
    
    is_match = False
    if username and username.lower() in normalized_channels:
        is_match = True
    elif chat_id in normalized_channels:
        is_match = True
        
    if is_match:
        logger.info(f"Received message from target chat: {message.chat.title} (username: {username}, id: {chat_id})")
        save_to_db(message)

async def poll_channels():
    normalized_channels = [c.strip().lower() for c in CHANNELS if c.strip()]
    for channel in normalized_channels:
        try:
            # Active fetch of the last 10 messages from history
            async for message in app.get_chat_history(channel, limit=10):
                save_to_db(message)
        except Exception as e:
            logger.error(f"Error polling channel {channel}: {e}")

async def poll_channels_loop():
    logger.info("Starting active background polling loop...")
    while True:
        try:
            await poll_channels()
        except Exception as e:
            logger.error(f"Error in poll_channels_loop: {e}")
        await asyncio.sleep(120)  # Check every 2 minutes

async def main():
    await app.start()
    logger.info("Fetching dialogs to populate peer cache...")
    try:
        async for dialog in app.get_dialogs():
            pass
        logger.info("Cache populated successfully.")
    except Exception as e:
        logger.warning(f"Failed to fetch dialogs (cache might be incomplete): {e}")
    
    # Auto-join target channels on startup to ensure we can receive messages from them
    normalized_channels = [c.strip().lower() for c in CHANNELS if c.strip()]
    for channel in normalized_channels:
        if not channel.startswith("-") and not channel.isdigit():
            try:
                chat = await app.join_chat(channel)
                logger.info(f"Successfully joined target channel: {chat.title} ({channel})")
            except Exception as e:
                logger.warning(f"Could not automatically join channel {channel}: {e}")

    # Start the active background polling task
    asyncio.create_task(poll_channels_loop())

    logger.info("Skipping historical messages crawl as per user request...")

    logger.info("Listening for new real-time messages...")
    await idle()
    await app.stop()

def start_scraping():
    logger.info("Starting Telegram Scraper...")
    app.run(main())

if __name__ == "__main__":
    start_scraping()
