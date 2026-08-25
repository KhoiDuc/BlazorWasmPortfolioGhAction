# Khoi Duc - Blazor WebAssembly Portfolio

A feature-rich personal portfolio and developer utility suite built with **Blazor WebAssembly** (.NET 9), Fluxor state management, and modern Web APIs.

## Key Features

- **Personal Resume & CV**: Interactive multi-lingual resume (English & Vietnamese).
- **Developer Utilities**: JWT debugger, converters, QR generator, SQLite Wasm, WebGL, Gemini spell checker, and 30+ tools.
- **Wiki CMS**: Knowledge base with read-only and authenticated edit modes.
- **State Management**: Fluxor with Redux DevTools support (DEBUG).
- **GitHub Integration**: REST DevOps tools + GraphQL user search.

## Tech Stack

- **Framework**: Blazor WebAssembly (.NET 9)
- **State**: [Fluxor](https://github.com/mrpmorris/Fluxor)
- **Database**: EF Core + SQLite Wasm
- **UI**: Bootstrap 5 + Bootstrap Icons + CSS isolation (`.tool-*` design system)
- **Auth**: Microsoft account (Azure AD) via MSAL
- **Deploy**: GitHub Pages CI/CD

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)

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
3. Never commit tokens — use user secrets locally or GitHub Actions secrets in CI.

### Wiki CMS (GitHub commit)

Fill the `Wiki` section with your repo owner/name. Editing requires `DevOps:GitHubToken` with `repo` scope.

### EmailJS

Fill `EmailJs:ServiceId`, `TemplateId`, and `PublicKey` to enable send from the Email Composer utility.

### Microsoft login (Azure AD / MSA)

1. Register an app in [Azure Portal → App registrations](https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade).
2. Set **Supported account types** to *Accounts in any organizational directory and personal Microsoft accounts*.
3. Add **Single-page application** redirect URIs (match your dev/prod origins):
   - `https://localhost:7255/authentication/login-callback`
   - `http://localhost:5288/authentication/login-callback`
   - `https://<your-github-pages-domain>/authentication/login-callback`
4. Copy the **Application (client) ID** into `wwwroot/appsettings.json`:

```json
"AzureAd": {
  "Authority": "https://login.microsoftonline.com/common",
  "ClientId": "YOUR_CLIENT_ID",
  "ValidateAuthority": true
}
```

Login appears in the navbar; wiki edit mode requires authentication.

---
© 2026 Khoi Nguyen Minh Duc. All rights reserved.
