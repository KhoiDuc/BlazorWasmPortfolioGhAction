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
- **Auth**: Auth0 / OIDC via MSAL
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

### Auth0

Set `Auth0:Authority` (with `https://`) and `ClientId`. Login appears in the navbar; wiki edit mode requires authentication.

---
© 2026 Khoi Nguyen Minh Duc. All rights reserved.
