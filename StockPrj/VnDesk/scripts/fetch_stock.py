import sys
import io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')
import json
from vnstock import Vnstock

def fetch_intraday(symbol):
    try:
        stock = Vnstock().stock(symbol=symbol, source='TCBS')
        df = stock.quote.intraday()
        df['time'] = df['time'].apply(lambda x: x.strftime('%Y-%m-%d %H:%M:%S'))
        return df.to_dict(orient='records')
    except Exception as e:
        return {'error': str(e)}

if __name__ == "__main__":
    symbol = sys.argv[1]
    result = fetch_intraday(symbol)
    print(json.dumps(result, ensure_ascii=False))
    if len(sys.argv) > 2:
        out = sys.argv[2]
        with open(out, 'w', encoding='utf-8') as f:
            json.dump(result, f, ensure_ascii=False, indent=4)
