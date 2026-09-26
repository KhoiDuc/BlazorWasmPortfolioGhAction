# Khoi Duc - Blazor WebAssembly Portfolio

A feature-rich personal portfolio and developer utility suite built with **Blazor WebAssembly** (.NET 10), Fluxor state management, and modern Web APIs.

## Key Features

- **Personal Resume & CV**: Interactive multi-lingual resume (English & Vietnamese).
- **Developer Utilities**: JWT debugger, converters, QR generator, WebGL, Gemini spell checker, and 30+ tools.
- **Trading desk**: VN market tools, broker portfolio, and TCBS via [broker-api](https://github.com/KhoiDuc/BROKER-API).
- **Wiki CMS**: Knowledge base with read-only and authenticated edit modes.
- **State Management**: Fluxor with Redux DevTools support (DEBUG).
- **GitHub Integration**: REST DevOps tools + GraphQL user search.

## Tech Stack

- **Framework**: Blazor WebAssembly (.NET 10)
- **State**: [Fluxor](https://github.com/mrpmorris/Fluxor)
- **UI**: Bootstrap 5 + Bootstrap Icons + CSS isolation (`.tool-*` design system)
- **Auth**: broker-api JWT. The same sign-in unlocks the broker desk and wiki editing. `ALLOWED_ORIGINS` on the API must include `https://khoiduc.github.io`.
- **Nested folders**: `Survey/`, `Trading-Signals/`, `StockPrj/`, `ZxmsToolbox/`, and `Meziantou.OnlineTools/` are not part of the WASM build.
- **Deploy**: GitHub Pages CI/CD

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run locally

```bash
dotnet run --project BlazorWasmPortfolioGhAction.csproj
```

Open `http://localhost:5000` in your browser.

## Configuration (`wwwroot/appsettings.json`)

### GitHub DevOps tools (real data)

By default `MockApi: true` returns fake GraphQL data for user search. For REST tools (search repos, inspector, trending, gist):

1. Set `"MockApi": false` for GraphQL user search (optional).
2. Add a GitHub Personal Access Token to `"DevOps:GitHubToken"`.
3. Do not publish that token. `wwwroot/appsettings.json` is shipped to the browser, so a value committed there is visible to anyone who loads the site.

### Wiki CMS (GitHub commit)

Fill the `Wiki` section with your repo owner/name. Saving edits calls the GitHub API from the browser and needs `DevOps:GitHubToken` with `repo` scope. Treat that token as public if it is in the published app.

### EmailJS

Fill `EmailJs:ServiceId`, `TemplateId`, and `PublicKey` to enable send from the Email Composer utility.

### Wiki and broker sign-in

Wiki editing and the broker desk share one broker-api account. There is no local `admin` / `admin` password.

1. Set `BrokerApi:BaseUrl` in `wwwroot/appsettings.json`.
2. Open `/admin` for wiki editing, or `/trading/login` for the broker desk.
3. Sign in with the broker-api username and password. The same JWT unlocks both.
4. From `/admin`, use **Edit wiki**, or open `/wiki/edit/...` directly after signing in.

Login is **not shown in the navbar**. Microsoft/Azure AD (MSAL) was removed — it was only used for learning.

<!--
### Microsoft login (Azure AD / MSA) — disabled

Previously used MSAL for wiki editing. Re-enable by restoring `AddMsalAuthentication` in `Program.cs`,
`AuthenticationService.js` in `index.html`, and `AzureAd` in `appsettings.json`.
-->

---
© 2026 Khoi Nguyen Minh Duc. All rights reserved.
