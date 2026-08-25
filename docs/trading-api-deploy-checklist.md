# Checklist deploy Trading API lên Fly.io

Tài liệu này dùng cho **Go API** trong `Trading-Signals/trading_api/`, phục vụ module Blazor `/trading`.

Blazor WASM deploy riêng qua **GitHub Actions → GitHub Pages**. Fly.io chỉ host backend API.

---

## Trạng thái đã kiểm tra (2026-08-25)

| Nhóm | Kết quả live trên `trading-api-dark-sunset-2092.fly.dev` |
|---|---|
| API cốt lõi (Postgres) | ✅ `/health`, watchlist, alerts, community… → 200 |
| Proxy routes (RRG, OSINT, calendar…) | ❌ → **404** (bản Fly đang chạy chưa có proxy/CORS mới) |
| CORS headers trên production | ❌ Chưa thấy `Access-Control-Allow-Origin` |

**Kết luận:** Cần `fly deploy` lại sau khi có `internal/middleware/cors.go` và `internal/proxy/proxy.go`.

---

## Phần 0 — Chuẩn bị

- [ ] Cài [flyctl](https://fly.io/docs/hands-on/install-flyctl/)
- [ ] Login: `fly auth login`
- [ ] Quyền truy cập app: `trading-api-dark-sunset-2092` (region `sin`, port nội bộ `8080`)
- [ ] Postgres đã có dữ liệu watchlist (Python workers trên VPS vẫn chạy cron như cũ)
- [ ] Blazor trỏ đúng API trong `wwwroot/appsettings.json`:

```json
"TradingApi": {
  "BaseUrl": "https://trading-api-dark-sunset-2092.fly.dev"
}
```

**Origin GitHub Pages (CORS):**

```
https://khoinguyenminhduc.github.io
```

> Header `Origin` **không** chứa path `/BlazorWasmPortfolioGhAction/` — chỉ `scheme + host`.

---

## Phần 1 — Secrets trên Fly.io

API đọc biến môi trường qua `fly secrets`, không dùng file `.env` trên production.

### 1.1 Postgres (bắt buộc)

Theo `Trading-Signals/trading_api/internal/db/db.go`:

| Secret | Ví dụ | Ghi chú |
|---|---|---|
| `DB_HOST` | `xxx.flycast` hoặc IP | Host Postgres |
| `DB_PORT` | `5432` | |
| `DB_USER` | `postgres` | |
| `DB_PASSWORD` | `***` | |
| `DB_NAME` | `trading` | |

```powershell
fly secrets list -a trading-api-dark-sunset-2092

fly secrets set `
  DB_HOST="your-db-host" `
  DB_PORT="5432" `
  DB_USER="postgres" `
  DB_PASSWORD="your-password" `
  DB_NAME="trading" `
  -a trading-api-dark-sunset-2092
```

- [ ] `fly secrets list` có đủ 5 biến DB
- [ ] Sau set secrets, Fly tự restart machine
- [ ] Log không có `Failed to connect to database`

Tham khảo mẫu local: `Trading-Signals/.env.example`

### 1.2 SSH — Refresh Assets / RRG (tùy chọn)

Theo `internal/handlers/handlers.go` → `RunSSHScript`, `RestartScript`:

| Secret | Mặc định | Mô tả |
|---|---|---|
| `DEPLOY_HOST` | — | VPS chạy Python scripts |
| `DEPLOY_PORT` | `22` | |
| `DEPLOY_USER` | — | |
| `DEPLOY_PASSWORD` | — | |

```powershell
fly secrets set `
  DEPLOY_HOST="152.53.208.182" `
  DEPLOY_PORT="22" `
  DEPLOY_USER="your-user" `
  DEPLOY_PASSWORD="your-password" `
  -a trading-api-dark-sunset-2092
```

- [ ] Nếu không set → nút Refresh trả lỗi "SSH credentials are not configured" (UI vẫn load được)

### 1.3 AI Chat (tùy chọn)

| Secret | Dùng cho |
|---|---|
| `GEMINI_API_KEY` | Chat AI (ưu tiên) |
| `GROQ_API_KEY` | Fallback khi Gemini fail |

- [ ] Ít nhất một key nếu muốn chatbot hoạt động

### 1.4 OSINT / Telegram proxy (tùy chọn)

Theo `internal/proxy/proxy.go`:

| Secret | Mặc định | Proxy path |
|---|---|---|
| `OSINT_API_URL` | `http://152.53.208.182:8080` | `/api/osint/*`, `/api/news/telegram` |

```powershell
fly secrets set OSINT_API_URL="http://152.53.208.182:8080" -a trading-api-dark-sunset-2092
```

- [ ] VPS OSINT reachable từ Fly (region `sin`)

---

## Phần 2 — CORS origins

File: `Trading-Signals/trading_api/internal/middleware/cors.go`

**Origins đã whitelist:**

| Origin | Mục đích |
|---|---|
| `http://localhost:5288` | Dev Blazor |
| `https://localhost:7255` | Dev HTTPS |
| `http://localhost:5000` | Dev alt |
| `https://localhost:5001` | Dev alt |
| `https://khoinguyenminhduc.github.io` | GitHub Pages production |

**Lưu ý:** Nếu dev ở port khác (vd. `5299`), thêm vào `allowedOrigins` rồi deploy lại:

```go
"http://localhost:5299",
```

**Fallback:** Origin không match → middleware set `Access-Control-Allow-Origin: *`.

### Checklist CORS sau deploy

```powershell
$API = "https://trading-api-dark-sunset-2092.fly.dev"
$ORIGIN = "https://khoinguyenminhduc.github.io"

curl -s -D - -o NUL -X OPTIONS "$API/getPotentialCoins" `
  -H "Origin: $ORIGIN" `
  -H "Access-Control-Request-Method: GET"

curl -s -D - -o NUL "$API/health" -H "Origin: $ORIGIN"
```

- [ ] Có `Access-Control-Allow-Origin: https://khoinguyenminhduc.github.io`
- [ ] Có `Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS, PATCH`
- [ ] OPTIONS trả `204 No Content`

---

## Phần 3 — Deploy

```powershell
cd d:\BlazorWasmPortfolioGhAction\Trading-Signals\trading_api

fly status -a trading-api-dark-sunset-2092
fly deploy -a trading-api-dark-sunset-2092
fly logs -a trading-api-dark-sunset-2092
```

- [ ] Build Docker thành công
- [ ] Log có `Server listening on :8080`
- [ ] `fly status` → machine `started`

**Cold start:** `fly.toml` có `auto_stop_machines = stop`, `min_machines_running = 0` — request đầu có thể mất ~3–10 giây.

---

## Phần 4 — Test từng endpoint

Base URL: `https://trading-api-dark-sunset-2092.fly.dev`

```powershell
$API = "https://trading-api-dark-sunset-2092.fly.dev"

function Test-Api($path, $method = "GET") {
  try {
    $r = Invoke-WebRequest -Uri "$API$path" -Method $method -TimeoutSec 15 -UseBasicParsing
    Write-Host "OK  $($r.StatusCode)  $path"
  } catch {
    $code = $_.Exception.Response.StatusCode.value__
    Write-Host "ERR $code  $path"
  }
}
```

### 4.1 Health

| Endpoint | Method | Kỳ vọng |
|---|---|---|
| `/health` | GET | `200`, body `OK` |

```powershell
Test-Api "/health"
```

### 4.2 Watchlist (Postgres)

| Endpoint | Trang Blazor |
|---|---|
| `/getPotentialCoins` | `/trading/crypto` |
| `/getPotentialCoins?signal_type=near_ath` | Crypto filter |
| `/getPotentialFuturesCoins` | `/trading/futures` |
| `/getPotentialSymbols` | `/trading/stock` |
| `/getPotentialWorldSymbols` | Stock world |
| `/getPotentialForexPairs` | `/trading/forex` |

```powershell
Test-Api "/getPotentialCoins"
Test-Api "/getPotentialFuturesCoins"
Test-Api "/getPotentialSymbols"
Test-Api "/getPotentialWorldSymbols"
Test-Api "/getPotentialForexPairs"
```

- [ ] `data: []` = workers chưa chạy / DB trống (không phải lỗi API)
- [ ] `latestUpdated` có timestamp gần đây khi workers OK

### 4.3 Alerts & scripts

| Endpoint | Method | Trang |
|---|---|---|
| `/triggeredAlerts?limit=10` | GET | Home, bell overlay |
| `/triggeredAlerts/read` | POST | Alert overlay |
| `/scriptStatus` | GET | Alert overlay |
| `/restartScript` | POST | Alert overlay |
| `/runSSHScript` | POST | Home Refresh |

```powershell
Test-Api "/triggeredAlerts?limit=5"
Test-Api "/scriptStatus"

Invoke-WebRequest -Uri "$API/runSSHScript" -Method POST `
  -ContentType "application/json" `
  -Body '{"script_type":"assets_rrg"}' -UseBasicParsing
```

### 4.4 Journal, Community, Real Estate

| Endpoint | Method | Auth |
|---|---|---|
| `/journal?user_id=demo` | GET | Bearer DNSE token |
| `/community/posts` | GET | Không |
| `/getRealEstate` | GET | Không |

```powershell
Test-Api "/community/posts"
Test-Api "/getRealEstate"
Test-Api "/journal?user_id=demo"
```

### 4.5 Macro Intel Hub

| Endpoint | Method |
|---|---|
| `/api/news-groups` | GET |
| `/api/news-groups/generate-prompt` | GET |
| `/api/news-items?group_id=1` | GET |
| `/api/settings` | GET |
| `/api/settings/update` | POST |

```powershell
Test-Api "/api/news-groups"
Test-Api "/api/settings"
Test-Api "/api/news-groups/generate-prompt"
```

### 4.6 Price alerts

```powershell
Test-Api "/priceAlerts"
```

### 4.7 Proxy — External APIs (cần deploy mới)

| Endpoint | Upstream | Trang Blazor |
|---|---|---|
| `/assets_rrgchart` | alwaysdata PNG | Home |
| `/crypto_rrgchart` | alwaysdata PNG | Crypto |
| `/futures_rrgchart` | alwaysdata PNG | Futures |
| `/vnstock_rrgchart` | alwaysdata PNG | Stock |
| `/forex_rrgchart` | alwaysdata PNG | Forex |
| `/ff_calendar_thisweek.json` | faireconomy | Home calendar |
| `/api/rates` | live-rates.com | Forex rates |
| `/goldprice/` | SJC | Commodities |
| `/silverprice/` | Phú Quý | Commodities |
| `/petrolimex/` | Petrolimex | Commodities iframe |
| `/tcanalysis/VNM` | TCBS | Stock |
| `/dnse-auth-service/login` | Entrade DNSE | Login |
| `/dnse-order-service/...` | Entrade DNSE | Portfolio |
| `/cg/...` | CoinGecko | Crypto |
| `/yahoo-finance/...` | Yahoo | Various |
| `/v4/...` | VNDirect | Stock |

```powershell
Test-Api "/assets_rrgchart"
Test-Api "/crypto_rrgchart"
Test-Api "/ff_calendar_thisweek.json"
Test-Api "/api/rates"
Test-Api "/tcanalysis/VNM"
```

- [ ] Sau deploy mới: không còn 404
- [ ] RRG trả `200` (content-type image)

### 4.8 OSINT / Telegram (proxy → VPS)

| Endpoint | Trang Blazor |
|---|---|
| `/api/osint/world-state` | Home, Macro |
| `/api/osint/signals` | Home, Macro |
| `/api/osint/theses` | Home |
| `/api/osint/theses/trigger` | POST — Update Theses |
| `/api/news/telegram` | News overlay |

```powershell
Test-Api "/api/osint/world-state"
Test-Api "/api/osint/signals"
Test-Api "/api/news/telegram"
```

- [ ] `502 proxy error` → VPS OSINT down hoặc firewall

### 4.9 Chat AI

```powershell
Invoke-WebRequest -Uri "$API/api/chat" -Method POST `
  -ContentType "application/json" `
  -Body '{"message":"Xin chào"}' -UseBasicParsing
```

### 4.10 DNSE Login

```powershell
Invoke-WebRequest -Uri "$API/dnse-auth-service/login" -Method POST `
  -ContentType "application/json" `
  -Body '{"username":"your@email.com","password":"***"}' -UseBasicParsing
```

- [ ] Không phải `404` (proxy hoạt động)

---

## Phần 5 — Test từ browser (CORS thật)

Mở DevTools trên:

```
https://khoinguyenminhduc.github.io/BlazorWasmPortfolioGhAction/trading
```

Console:

```javascript
const API = "https://trading-api-dark-sunset-2092.fly.dev";

fetch(`${API}/getPotentialCoins`)
  .then(r => r.json())
  .then(d => console.log("coins:", d.data?.length, d.latestUpdated))
  .catch(e => console.error("CORS/fetch failed:", e));

fetch(`${API}/assets_rrgchart`)
  .then(r => console.log("RRG:", r.status, r.headers.get("content-type")))
  .catch(e => console.error(e));
```

- [ ] Không có lỗi `CORS policy`
- [ ] Watchlist load trên `/trading/crypto`
- [ ] Ảnh RRG hiện trên Dashboard
- [ ] News panel load Telegram (nếu OSINT VPS OK)

---

## Phần 6 — Smoke test theo trang UI

| Trang | Kiểm tra |
|---|---|
| `/trading` | RRG chart, calendar, theses, OSINT signals |
| `/trading/crypto` | Watchlist + Live Ticker (Binance WS — không qua Fly) |
| `/trading/futures` | Futures watchlist + RRG |
| `/trading/stock` | VN + world stocks |
| `/trading/forex` | Pairs, sessions, rates |
| `/trading/commodities` | Gold spread, Petrolimex iframe |
| `/trading/real-estate` | Chart + API data |
| `/trading/macro` | News groups, world state, AI settings |
| `/trading/community` | Posts, comments, likes |
| `/trading/portfolio` | Journal, DNSE orders (cần login) |
| `/trading/login` | DNSE login hoặc Demo |

---

## Phần 7 — Troubleshooting

| Triệu chứng | Nguyên nhân | Cách xử lý |
|---|---|---|
| Proxy paths `404` | Fly chưa deploy code mới | `fly deploy` từ `trading_api/` |
| `CORS policy` trên GitHub Pages | Middleware CORS chưa deploy | Deploy lại + test OPTIONS |
| Watchlist rỗng | Python workers / DB | SSH VPS, chạy scripts cron |
| RRG 502 | alwaysdata.net down | Test URL PNG trực tiếp |
| OSINT/Telegram 502 | VPS `152.53.208.182` down | `curl http://152.53.208.182:8080/...` |
| Refresh Assets fail | Thiếu SSH secrets | Set `DEPLOY_*` trên Fly |
| API cold start chậm | `min_machines_running = 0` | Chấp nhận hoặc set `min_machines_running = 1` |
| Chat không trả lời | Thiếu Gemini/Groq key | Set secrets |
| DNSE login 404 | Proxy chưa deploy | Deploy lại |

---

## Phần 8 — Thứ tự làm việc (tóm tắt)

```
1. fly secrets list          → kiểm tra DB + SSH + AI + OSINT
2. (tuỳ chọn) thêm localhost:5299 vào cors.go
3. fly deploy                → từ Trading-Signals/trading_api/
4. Test curl health + proxy  → Part 4
5. Test browser CORS         → Part 5 trên GitHub Pages
6. Smoke test từng trang     → Part 6
7. git push master           → Blazor tự deploy GitHub Actions
```

---

## Liên quan

- Phân tích dự án gốc: [`trading-signals-analysis.md`](./trading-signals-analysis.md)
- Config Blazor: `wwwroot/appsettings.json` → `TradingApi:BaseUrl`
- Module UI: `Pages/Trading/`
