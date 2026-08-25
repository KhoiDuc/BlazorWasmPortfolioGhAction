# Phân tích dự án Trading-Signals

Dự án gốc nằm trong thư mục `Trading-Signals/` — một **trading dashboard** full-stack do người khác xây dựng. Portfolio Blazor của bạn đã **port UI sang `/trading`**, nhưng backend và workers vẫn dùng chung kiến trúc này.

---

## Tổng quan — Dự án làm gì?

**Trading-Signals** là nền tảng theo dõi thị trường tài chính đa tài sản:

- **Crypto, Futures, Stock VN/World, Forex, Commodities, Real Estate**
- **Watchlist tín hiệu kỹ thuật** (EMA cross, gần ATH, top growth…)
- **Biểu đồ RRG** (Relative Rotation Graph) tự generate
- **Macro Intel Hub** — nhóm tin tức, AI phân tích, world state
- **OSINT** — scrape Telegram, trích xuất signal bằng LLM
- **Community** — bài viết, comment, like
- **Trading Journal** + **Price Alerts**
- **Tích hợp DNSE (Entrade)** — login, xem deal, đặt lệnh, OTP
- **Overlay toàn cục** — News panel, Chat AI, Alert bell

Không phải bot trade tự động end-to-end, mà là **dashboard + data pipeline + alert + broker hook**.

---

## Kiến trúc 4 lớp

```
┌─────────────────────────────────────────────────────────────┐
│  Lớp 1: Frontend (Vue 3 SPA)                                │
│  Deploy: Vercel (vercel.json rewrites)                      │
│  src/ — 12 routes, NavBar, overlays                         │
└──────────────────────────┬──────────────────────────────────┘
                           │ HTTP (same-origin qua Vercel proxy)
┌──────────────────────────▼──────────────────────────────────┐
│  Lớp 2: Go API (trading_api)                                │
│  Deploy: Fly.io → trading-api-dark-sunset-2092.fly.dev      │
│  Postgres, REST handlers, SSH trigger, reverse proxy        │
└──────────────┬────────────────────────────┬─────────────────┘
               │                            │
┌──────────────▼──────────────┐  ┌──────────▼──────────────────┐
│  Lớp 3: Python workers      │  │  Lớp 4: OSINT service       │
│  VPS (152.53.208.182)       │  │  VPS :8080 + :8081          │
│  scripts/*.py — scan, RRG   │  │  osint_ai_worker/           │
│  Cron + SSH từ Go API       │  │  Telegram + LLM + scheduler │
└─────────────────────────────┘  └─────────────────────────────┘
               │
┌──────────────▼──────────────┐
│  Postgres (shared DB)       │
│  watchlist, journal, OSINT  │
└─────────────────────────────┘
```

### Vì sao Blazor không chạy Vue?

GitHub Pages **không có server-side rewrite** như Vercel. Blazor WASM gọi thẳng Fly.io API; proxy paths (TCBS, DNSE, RRG PNG…) được chuyển vào Go API thay cho `vercel.json`.

---

## Cấu trúc thư mục

```
Trading-Signals/
├── src/                    # Vue 3 frontend (gốc)
│   ├── router/index.js     # 12 routes
│   ├── components/         # CryptoView, Stock, HomeView, overlays…
│   └── views/MacroIntelHub.vue
├── trading_api/            # Go REST API → Fly.io
│   ├── cmd/api/main.go
│   ├── internal/handlers/  # Business logic
│   ├── internal/proxy/     # Thay vercel.json rewrites
│   ├── internal/middleware/cors.go
│   ├── migrations/         # 000–019 SQL migrations
│   ├── Dockerfile
│   └── fly.toml
├── scripts/                # Python data pipeline (chạy trên VPS)
│   ├── fetch_potential_*.py
│   ├── rrg_*_chart.py
│   ├── alert.py, price alerts…
│   ├── deploy.sh           # Upload scripts lên VPS qua SCP
│   └── ml_models/          # ML gold/real estate (không có UI Vue)
├── osint_ai_worker/        # Telegram scraper + LLM scheduler
│   └── osint_ai_worker.py
├── EA/                     # MetaTrader 5 Expert Advisor
│   └── PriceActionEA.mq5
├── vercel.json             # Proxy config cho Vue trên Vercel
├── package.json            # Vue 3 + Chart.js + ECharts
└── .env.example            # DB + SSH deploy vars
```

---

## Lớp 1 — Vue Frontend

### Tech stack

| Thành phần | Công nghệ |
|---|---|
| Framework | Vue 3 + Vue Router 4 |
| UI | Bootstrap 5 |
| Charts | Chart.js, ECharts, vue-chartjs |
| HTTP | Axios |
| Deploy gốc | Vercel (`vercel.json`) |

### Routes (`src/router/index.js`)

| Path | Component | Auth |
|---|---|---|
| `/` | HomeView | — |
| `/crypto` | CryptoView | — |
| `/futures` | FuturesView | — |
| `/stock` | StockMarket | — |
| `/forex` | ForexView | — |
| `/commodities` | CommoditiesView | — |
| `/real-estate` | RealEstateView | — |
| `/central-banks` | CentralBanksView | — |
| `/others` | OthersView | — |
| `/macro` | MacroIntelHub | ✅ token |
| `/my-portfolio` | MyPortfolio | ✅ token |
| `/community` | CommunityView | ✅ token |
| `/login` | LoginPage | — |

Auth: `localStorage.token` (DNSE), guard trong `router.beforeEach`.

### Overlay toàn cục (`src/App.vue`)

Luôn hiển thị trên mọi trang:

- **AlertOverlay** — triggered alerts, script status
- **NewsPanel** — Telegram news (slide từ phải)
- **Chatbox** — AI assistant (Gemini/Groq qua Go API)

Blazor đã port tương đương: `TradingNewsPanel`, `TradingChatbox`, `TradingAlertOverlay` trong `TradingLayout.razor`.

### Vercel rewrites (`vercel.json`)

Vue trên Vercel **không gọi API trực tiếp** — mọi request same-origin được rewrite:

| Loại | Ví dụ destination |
|---|---|
| Go API Fly.io | `/getPotentialCoins`, `/journal`, `/api/chat` |
| DNSE Entrade | `/dnse-auth-service/login` |
| TCBS / VNDirect | `/tcanalysis/...`, `/v4/stocks...` |
| External JSON | `/ff_calendar_thisweek.json`, `/api/rates` |
| RRG PNG | `/assets_rrgchart` → alwaysdata.net |
| OSINT VPS | `/api/osint/*`, `/api/news/telegram` → `152.53.208.182:8080` |

**Blazor thay thế:** Go API `internal/proxy/proxy.go` + gọi trực tiếp `TradingApi:BaseUrl`.

---

## Lớp 2 — Go API (`trading_api/`)

### Deploy

| Item | Giá trị |
|---|---|
| Platform | Fly.io |
| App name | `trading-api-dark-sunset-2092` |
| Region | `sin` (Singapore) |
| Port | `8080` |
| Docker | Multi-stage Go 1.x build |

### Database

Postgres qua biến môi trường riêng lẻ (không dùng `DATABASE_URL` trong Go):

- `DB_HOST`, `DB_PORT`, `DB_USER`, `DB_PASSWORD`, `DB_NAME`

**19 migration files** (`migrations/000` → `019`) tạo schema:

| Migration | Nội dung |
|---|---|
| 000 | Schema init |
| 001 | Journal |
| 002 | Community posts/comments |
| 003 | Real estate prices |
| 004–006 | Journal currency, stock signals, volume |
| 007–008 | Macro Intel Hub (news groups/items) |
| 009–012 | Signal types, EMA9, crypto ATH |
| 013–014 | Triggered alerts, journal current price |
| 015–019 | Futures watchlist, system settings, market cap, score_diff |

### API endpoints chính

#### Watchlist (đọc từ Postgres, data do Python ghi)

| Endpoint | Mô tả |
|---|---|
| `GET /getPotentialCoins` | Crypto signals |
| `GET /getPotentialFuturesCoins` | Futures signals |
| `GET /getPotentialSymbols` | VN stock signals |
| `GET /getPotentialWorldSymbols` | World stocks |
| `GET /getPotentialForexPairs` | Forex pairs |
| `GET /getRealEstate` | BĐS prices |

#### User & trading

| Endpoint | Mô tả |
|---|---|
| `POST /inputOTP` | Lưu OTP DNSE |
| `GET/POST/DELETE /journal` | Trading journal |
| `GET/POST /priceAlerts` | Price alerts |
| `GET/POST /community/*` | Community |

#### Alerts & automation

| Endpoint | Mô tả |
|---|---|
| `GET /triggeredAlerts` | Alerts đã kích hoạt |
| `POST /triggeredAlerts/read` | Mark read |
| `GET /scriptStatus` | Trạng thái Python scanner |
| `POST /restartScript` | Restart scanner qua SSH |
| `POST /runSSHScript` | Chạy script cụ thể trên VPS |

`runSSHScript` map `script_type` → lệnh Python trên VPS:

| script_type | Script |
|---|---|
| `crypto_potential` | `fetch_potential_cryptos.py` |
| `crypto_rrg` | `rrg_crypto_chart.py` |
| `futures_potential` | `fetch_potential_cryptofutures.py` |
| `futures_rrg` | `rrg_cryptofutures_chart.py` |
| `forex_potential` | `fetch_potential_forex_pairs.py` |
| `forex_rrg` | `rrg_forex_chart.py` |
| `vnstock_potential` | `fetch_potential_stocks.py` |
| `vnstock_rrg` | `rrg_vnstock_chart.py` |
| `assets_rrg` | `rrg_assets_chart.py` |
| `world_potential` | `fetch_potential_world_stocks.py` |

#### Macro Intel Hub

| Endpoint | Mô tả |
|---|---|
| `GET/POST/PUT/DELETE /api/news-groups` | Nhóm chiến lược |
| `GET/POST/PUT/DELETE /api/news-items` | Tin trong nhóm |
| `GET /api/news-groups/generate-prompt` | AI strategy prompt |
| `GET/POST /api/settings` | System settings (AI on/off, scan toggle) |

#### AI Chat

| Endpoint | Mô tả |
|---|---|
| `POST /api/chat` | Gemini (ưu tiên) → Groq fallback |

Secrets: `GEMINI_API_KEY`, `GROQ_API_KEY`

#### Reverse proxy (thay Vercel)

Xem đầy đủ trong `internal/proxy/proxy.go` — proxy tới TCBS, DNSE, CoinGecko, Yahoo, SJC, Petrolimex, alwaysdata RRG PNG, OSINT VPS.

Env: `OSINT_API_URL` (default `http://152.53.208.182:8080`)

---

## Lớp 3 — Python Workers (`scripts/`)

Chạy trên **VPS** (`/home/thehaohcm/scripts/`), không chạy trong Fly hay Blazor.

### Pipeline chính

| Script | Chức năng |
|---|---|
| `fetch_potential_cryptos.py` | Quét crypto, ghi Postgres |
| `fetch_potential_cryptofutures.py` | Quét futures |
| `fetch_potential_stocks.py` | Quét cổ phiếu VN |
| `fetch_potential_world_stocks.py` | Quét world stocks |
| `fetch_potential_forex_pairs.py` | Quét forex |
| `rrg_*_chart.py` | Generate PNG RRG → upload alwaysdata.net |
| `alert.py` / `alert_script/` | Price alert monitoring → Slack |
| `check_gold_silver_alerts.py` | Alert vàng/bạc |
| `fetch_housing_prices.py` / `real_estate_hcm.py` | Dữ liệu BĐS |
| `relative_strength_*.py` | Relative strength vs VNINDEX/VN30 |

### Cron shells

| File | Chạy |
|---|---|
| `run_crypto.sh` | crypto fetch + RRG |
| `run_forex.sh` | forex fetch + RRG |
| `run_vnstock.sh` | stock fetch + RRG |

### Deploy scripts lên VPS

`scripts/deploy.sh` — SCP flat upload + git backup trên VPS. Cần `.env`:

```
DEPLOY_HOST, DEPLOY_USER, DEPLOY_PASSWORD, DEPLOY_PORT, DEPLOY_PATH
```

### ML (không có UI Vue)

- `scripts/ml_models/` — model gold/BĐS
- Không được expose trong frontend gốc → **Blazor cũng chưa port**

### Trading bot Go

- `scripts/trading_bot.go` — bot riêng, không liên quan trực tiếp UI

---

## Lớp 4 — OSINT AI Worker (`osint_ai_worker/`)

Service Python chạy liên tục trên VPS, port **8080** (REST) và **8081** (trigger).

### Chức năng

1. **Telegram scraper** (`collectors/telegram_scraper.py`) — thu tin từ channels
2. **Signal extraction** — LLM trích signal từ news → `osint_signals`
3. **Thesis update** — tổng hợp macro theses → `osint_theses`
4. **World state** — cập nhật trạng thái vĩ mô → `osint_world_state`
5. **Adaptive scheduling** — tần suất extraction tùy volume tin mới
6. **Cleanup** — xóa news > 14 ngày

### Scheduler (APScheduler)

| Job | Interval |
|---|---|
| Signal extraction | 3–20 phút (adaptive) |
| Thesis update | 4 giờ |
| World state update | 4 giờ |
| DB cleanup | 2:00 AM daily |

### HTTP trigger (port 8081)

`POST /trigger-thesis-update` — cập nhật thủ công theses + world state.

### Env cần

| Biến | Mô tả |
|---|---|
| `DATABASE_URL` | Postgres (OSINT worker dùng URL, khác Go API) |
| LLM keys | Trong worker code (Gemini/OpenAI tùy config) |
| Telegram | API credentials cho Pyrogram scraper |

---

## EA MetaTrader (`EA/`)

- `PriceActionEA.mq5` — Expert Advisor MT5, **độc lập** với web app
- Không được tích hợp vào Vue hay Blazor

---

## Mapping sang Portfolio Blazor

| Trading-Signals (Vue) | Portfolio Blazor |
|---|---|
| Vue SPA + Vercel | `Pages/Trading/*` + GitHub Pages |
| `vercel.json` rewrites | Go `proxy.go` + `TradingApiClient` |
| `localStorage.token` | `TradingAuthService` + `trading.js` |
| NavBar.vue | `Navbar` + `TradingNav` |
| App.vue overlays | `TradingNewsPanel`, `TradingChatbox`, `TradingAlertOverlay` |
| `/crypto` route | `/trading/crypto` (+ Live Ticker từ CryptoManager) |
| DNSE login | `/trading/login` |

Config: `wwwroot/appsettings.json` → `TradingApi:BaseUrl`

---

## Bạn cần gì để chạy đầy đủ?

### Bắt buộc

| Thành phần | Việc cần làm |
|---|---|
| **Fly.io Go API** | Deploy + secrets DB |
| **Postgres** | Chạy migrations, có data |
| **Python workers VPS** | Cron scripts fetch/RRG |
| **Blazor config** | `TradingApi:BaseUrl` đúng |

### Khuyến nghị (nhiều tính năng hơn)

| Thành phần | Việc cần làm |
|---|---|
| SSH secrets trên Fly | Refresh Assets từ UI |
| OSINT VPS :8080 | Telegram news, macro signals |
| `GEMINI_API_KEY` / `GROQ_API_KEY` | Chat AI |
| DNSE account | Login, portfolio, orders |
| `fly deploy` bản mới | CORS + proxy (GitHub Pages cần) |

### Không bắt buộc cho Blazor

| Thành phần | Ghi chú |
|---|---|
| Vue frontend | Đã thay bằng Blazor `/trading` |
| Vercel | Không dùng nếu deploy GitHub Pages |
| `yarn serve` Vue | Chỉ để tham khảo UI gốc |
| MT5 EA | Riêng biệt |
| ML models scripts | Không có UI |

---

## Hai host API (quan trọng)

Blazor/UI chỉ biết **một** `BaseUrl` (Fly.io), nhưng backend thực tế có **2 server**:

| Host | Port | Vai trò |
|---|---|---|
| `trading-api-dark-sunset-2092.fly.dev` | 443 | Watchlist, journal, community, chat, proxy gateway |
| `152.53.208.182` | 8080 | OSINT REST API (news, signals, theses) |
| `152.53.208.182` | 8081 | OSINT manual trigger |
| VPS scripts | SSH | Python scan + RRG generation |

Fly.io **proxy** request `/api/osint/*` và `/api/news/telegram` sang VPS OSINT.

---

## Phụ thuộc bên ngoài

| Dịch vụ | Dùng cho |
|---|---|
| Binance WebSocket | Live crypto ticker (Blazor client-side) |
| TCBS API | Stock analysis VN |
| VNDirect API | Stock data |
| Entrade/DNSE | Broker login & orders |
| CoinGecko | Crypto metadata |
| Yahoo Finance | Market data |
| SJC / Phú Quý | Giá vàng VN |
| Petrolimex | Giá xăng |
| alwaysdata.net | Host PNG biểu đồ RRG |
| faireconomy.media | Economic calendar |
| live-rates.com | FX rates |
| Gemini / Groq | AI chat & OSINT LLM |
| Slack (optional) | Price alert notifications |

---

## Blazor — URL config (`appsettings.json`)

```json
"TradingApi": {
  "BaseUrl": "https://trading-api-dark-sunset-2092.fly.dev",
  "OsintBaseUrl": "http://152.53.208.182:8080",
  "RrgBaseUrl": "https://thehaohcm.alwaysdata.net",
  "CalendarUrl": "https://nfs.faireconomy.media/ff_calendar_thisweek.json",
  "FxRatesUrl": "https://live-rates.com/rates"
}
```

Đổi link trực tiếp trong config khi endpoint die — không có flag bật/tắt.

Routing (`Services/Trading/TradingEndpointResolver.cs`):

| Loại request | Config key |
|---|---|
| Watchlist, journal, community, chat, alerts | `BaseUrl` |
| OSINT, Telegram news, macro news CRUD | `OsintBaseUrl` |
| RRG PNG | `RrgBaseUrl` |
| Calendar, FX rates | `CalendarUrl`, `FxRatesUrl` |
| Gold, petrolimex, yahoo, DNSE… | URL ngoài cố định (giống vercel.json) |

**Lưu ý GitHub Pages (HTTPS):** `OsintBaseUrl` là HTTP — browser có thể chặn mixed content trên production. Test trên `http://localhost` hoặc đổi sang HTTPS gateway khi có.

---

## Trạng thái tích hợp hiện tại

| Phần | Trạng thái |
|---|---|
| UI Blazor `/trading` | ✅ Port từ Vue, design sync portfolio |
| `TradingApiClient` | ✅ Gọi đủ endpoints chính |
| Go API trên Fly (core) | ✅ Watchlist/community hoạt động |
| Go API proxy + CORS | ⚠️ Cần redeploy (xem checklist deploy) |
| Python workers | ⚠️ Phụ thuộc VPS + cron của owner gốc |
| OSINT VPS | ⚠️ Phụ thuộc `152.53.208.182` online |
| Vue frontend gốc | 📦 Giữ trong repo để tham khảo, không build trong CI portfolio |

---

## Tài liệu liên quan

- [Checklist deploy Fly.io](./trading-api-deploy-checklist.md)
- Go API entry: `Trading-Signals/trading_api/cmd/api/main.go`
- Proxy routes: `Trading-Signals/trading_api/internal/proxy/proxy.go`
- Vercel rewrites gốc: `Trading-Signals/vercel.json`
- Blazor module: `Pages/Trading/`
- Price alerts: `Trading-Signals/scripts/PRICE_ALERTS_README.md`
