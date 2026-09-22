import asyncio
import os
import asyncpg
from dotenv import load_dotenv


def get_latest_migration_file():
    migrations_dir = os.path.join(os.path.dirname(__file__), '../trading_api/migrations')
    migration_files = sorted(
        file_name for file_name in os.listdir(migrations_dir)
        if file_name.endswith('.sql')
    )
    if not migration_files:
        return None
    return os.path.join(migrations_dir, migration_files[-1])


async def run_migration():
    load_dotenv()
    
    migration_file = get_latest_migration_file()
    if not migration_file or not os.path.exists(migration_file):
        print(f"Error: {migration_file} does not exist")
        return

    print(f"Applying migration: {migration_file}")
    with open(migration_file, 'r') as f:
        sql = f.read()

    try:
        conn = await asyncpg.connect(
            user=os.environ.get('DB_USER'),
            password=os.environ.get('DB_PASSWORD'),
            database=os.environ.get('DB_NAME'),
            host=os.environ.get('DB_HOST'),
            port=int(os.environ.get('DB_PORT', 5432))
        )
        await conn.execute(sql)
        print("Migration applied successfully.")
        await conn.close()
    except Exception as e:
        print(f"Migration failed: {e}")

if __name__ == "__main__":
    asyncio.run(run_migration())
