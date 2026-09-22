# Price Alert System

## Overview
The price alert system allows users to set price alerts for various assets (crypto, stocks, gold, silver, forex) and receive Slack notifications when prices reach their target levels.

## Features
- ✅ Set price alerts for multiple asset types
- ✅ Automatic notifications when price >= (alert_price - 5%)
- ✅ Notifications sent via Slack regardless of SLACK_NOTIFICATIONS_ENABLED env var
- ✅ Rate limiting: notifications sent max once per hour per alert
- ✅ Enable/disable alerts via UI
- ✅ RESTful API for managing alerts

## Database Schema

```sql
CREATE TABLE public.price_alerts (
    id SERIAL PRIMARY KEY,
    user_id varchar NOT NULL,
    symbol varchar NOT NULL,
    asset_type varchar(20) NOT NULL CHECK (asset_type IN ('crypto', 'stock', 'gold', 'silver', 'forex')),
    alert_price numeric(20, 8) NOT NULL,
    is_active bool DEFAULT true NOT NULL,
    last_notified_at timestamptz NULL,
    created_at timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT uq_user_symbol_alert UNIQUE (user_id, symbol, asset_type)
);
```

## API Endpoints

### List User Alerts
```
GET /priceAlerts?user_id={user_id}
```

Response:
```json
[
  {
    "id": 1,
    "user_id": "user123",
    "symbol": "BTCUSDT",
    "asset_type": "crypto",
    "alert_price": 100000.0,
    "is_active": true,
    "last_notified_at": null,
    "created_at": "2025-12-28T10:00:00Z",
    "updated_at": "2025-12-28T10:00:00Z"
  }
]
```

### Create Alert
```
POST /priceAlerts
Content-Type: application/json

{
  "user_id": "user123",
  "symbol": "BTCUSDT",
  "asset_type": "crypto",
  "alert_price": 100000.0
}
```

Response:
```json
{
  "id": 1,
  "message": "Alert created successfully"
}
```

### Update Alert
```
PUT /priceAlerts/{alert_id}
Content-Type: application/json

{
  "alert_price": 105000.0,
  "is_active": true
}
```

### Delete Alert
```
DELETE /priceAlerts/{alert_id}
```

## Python Scripts Integration

### Crypto Alerts
The `fetch_potential_cryptos.py` script automatically checks crypto price alerts after updating the watchlist.

### Stock Alerts
The `fetch_potential_stocks.py` script checks stock price alerts after fetching potential stocks.

### Forex Alerts
The `fetch_potential_forex_pairs.py` script checks forex pair alerts after analyzing currency strength.

### Gold/Silver Alerts
Run `check_gold_silver_alerts.py` periodically (e.g., via cron) to check gold and silver alerts.

## Usage Example

### Setting up a cron job for gold/silver alerts:
```bash
# Check every hour
0 * * * * cd /path/to/scripts && python3 check_gold_silver_alerts.py
```

## Environment Variables

Required:
- `DB_HOST` - Database host
- `DB_PORT` - Database port
- `DB_USER` - Database user
- `DB_PASSWORD` - Database password
- `DB_NAME` - Database name
- `SLACK_WEBHOOK_URL` - Slack webhook URL for notifications

Note: Price alerts will send notifications regardless of `SLACK_NOTIFICATIONS_ENABLED` setting.

## Alert Triggering Logic

1. Alert is triggered when: `current_price >= (alert_price * 0.95)`
2. Notifications are rate-limited to once per hour per alert
3. Alert remains active until manually disabled by user
4. Each notification includes:
   - Symbol and asset type
   - Alert price vs current price
   - Percentage difference
   - Timestamp

## Notification Example

```
🔔 *Price Alert Triggered!*
Asset: *BTCUSDT* (CRYPTO)
Alert Price: *$100,000.00*
Current Price: *$98,500.00* (-1.50%)
User: user123
Time: 2025-12-28 10:30:45
```

## Web UI Integration

To integrate alerts into your Vue.js frontend:

1. Add alert management UI component
2. Call API endpoints to create/read/update/delete alerts
3. Display user's active alerts
4. Allow toggling alert on/off
5. Show notification history

Example API call from Vue:
```javascript
// Create alert
const createAlert = async (symbol, assetType, alertPrice) => {
  const response = await fetch('/priceAlerts', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      user_id: 'user123',
      symbol: symbol,
      asset_type: assetType,
      alert_price: alertPrice
    })
  });
  return await response.json();
};

// Toggle alert
const toggleAlert = async (alertId, isActive) => {
  const response = await fetch(`/priceAlerts/${alertId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ is_active: isActive })
  });
  return await response.json();
};
```

## Testing

Test the alert system:
```bash
cd scripts
python3 price_alert_utils.py
```

## Troubleshooting

### Alerts not triggering
1. Check if alert is active: `is_active = true`
2. Verify price threshold: current_price >= (alert_price * 0.95)
3. Check last notification time (max 1 notification per hour)
4. Verify SLACK_WEBHOOK_URL is set correctly

### Database connection errors
1. Verify all DB_* environment variables are set
2. Check database connectivity
3. Ensure price_alerts table exists

### Slack notifications not sending
1. Verify SLACK_WEBHOOK_URL is valid
2. Check Slack webhook status
3. Review script logs for error messages
