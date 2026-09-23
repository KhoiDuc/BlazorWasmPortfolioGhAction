using BlazorWasmPortfolioGhAction.Resources;
using Microsoft.Extensions.Localization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Models.Trading.Broker;
using BlazorWasmPortfolioGhAction.Services.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public interface IBrokerApiClient
{
    Task<BrokerPortfolio?> GetPortfolioAsync(CancellationToken ct = default);

    // CRUD Position
    Task<(bool Ok, string? Error)> CreatePositionAsync(BrokerPosition pos, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UpdatePositionAsync(string symbol, BrokerPosition pos, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> ArchivePositionAsync(string symbol, bool isArchived, BrokerPositionStatus? status = null, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> DeletePositionAsync(string symbol, CancellationToken ct = default);

    // CRUD Lot
    Task<(bool Ok, string? Error)> AddLotAsync(string symbol, BrokerLot lot, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UpdateLotAsync(string symbol, BrokerLot lot, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> DeleteLotAsync(string symbol, string lotId, CancellationToken ct = default);

    // CRUD Sell
    Task<(bool Ok, string? Error)> AddSellAsync(string symbol, BrokerSell sell, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UpdateSellAsync(string symbol, BrokerSell sell, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> DeleteSellAsync(string symbol, string sellId, CancellationToken ct = default);

    // CRUD Note
    Task<(bool Ok, string? Error)> AddNoteAsync(string symbol, BrokerNote note, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UpdateNoteAsync(string symbol, BrokerNote note, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> DeleteNoteAsync(string symbol, string noteId, CancellationToken ct = default);

    // CRUD Dividend
    Task<(bool Ok, string? Error)> AddDividendAsync(string symbol, BrokerDividend div, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UpdateDividendAsync(string symbol, BrokerDividend div, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> DeleteDividendAsync(string symbol, string divId, CancellationToken ct = default);

    // Import (giữ nguyên PUT toàn bộ — cho ImportJson)
    Task<(bool Ok, string? Error)> ImportPortfolioAsync(BrokerPortfolio portfolio, CancellationToken ct = default);

    // Legacy — kept temporarily for backward compat
    Task<(bool Ok, string? Error)> SavePortfolioAsync(BrokerPortfolio portfolio, CancellationToken ct = default);

    string? LastError { get; }
    Task<(bool Ok, string? Body, string? Error)> GetTextAsync(string path, CancellationToken ct = default);
    Task<(bool Ok, string? Body, string? Error)> SendTextAsync(HttpMethod method, string path, string? body, string? contentType, CancellationToken ct = default);
}

public sealed class BrokerApiClient : IBrokerApiClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly IStringLocalizer<SharedResources> _L;
    private readonly IBrokerAuthService _auth;
    private readonly NavigationManager _navigation;
    private readonly ICultureService _culture;

    public BrokerApiClient(
        HttpClient http,
        IConfiguration config,
        IStringLocalizer<SharedResources> L,
        IBrokerAuthService auth,
        NavigationManager navigation,
        ICultureService culture)
    {
        _http = http;
        _config = config;
        _L = L;
        _auth = auth;
        _navigation = navigation;
        _culture = culture;
    }

    private string? BaseUrl => _config["BrokerApi:BaseUrl"]?.Trim().TrimEnd('/');

    public string? LastError { get; private set; }

    public async Task<BrokerPortfolio?> GetPortfolioAsync(CancellationToken ct = default)
    {
        LastError = null;
        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            LastError = _L["Trading_BrokerApi_BaseUrlMissing"];
            return null;
        }

        try
        {
            using var request = await CreateAuthorizedRequestAsync(HttpMethod.Get, $"/api/portfolio?v={DateTime.UtcNow.Ticks}", ct);
            if (request is null)
            {
                LastError = _L["Trading_BrokerApi_TokenMissing"];
                return null;
            }

            using var resp = await _http.SendAsync(request, ct);
            if (await HandleUnauthorizedAsync(resp))
            {
                LastError = _L["Trading_BrokerApi_Unauthorized"];
                return null;
            }

            if (!resp.IsSuccessStatusCode)
            {
                LastError = $"HTTP {(int)resp.StatusCode}";
                return null;
            }

            var json = await resp.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(json))
            {
                LastError = "Empty portfolio response.";
                return null;
            }

            return JsonSerializer.Deserialize<BrokerPortfolio>(json, BrokerJson.Options);
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
    }

    public Task<(bool Ok, string? Body, string? Error)> GetTextAsync(string path, CancellationToken ct = default) =>
        SendTextAsync(HttpMethod.Get, path, null, null, ct);

    public async Task<(bool Ok, string? Body, string? Error)> SendTextAsync(HttpMethod method, string path, string? body, string? contentType, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            return (false, null, _L["Trading_BrokerApi_BaseUrlMissing"]);

        try
        {
            using var request = await CreateAuthorizedRequestAsync(method, path, ct);
            if (request is null)
                return (false, null, _L["Trading_BrokerApi_TokenMissing"]);
            if (body is not null)
                request.Content = new StringContent(body, System.Text.Encoding.UTF8, contentType ?? "application/json");

            using var resp = await _http.SendAsync(request, ct);
            var text = await resp.Content.ReadAsStringAsync(ct);
            if (await HandleUnauthorizedAsync(resp))
                return (false, text, _L["Trading_BrokerApi_Unauthorized"]);
            if (!resp.IsSuccessStatusCode)
                return (false, text, $"HTTP {(int)resp.StatusCode} — {TruncateBody(text)}");
            return (true, text, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    // ── Position CRUD ──

    public Task<(bool Ok, string? Error)> CreatePositionAsync(BrokerPosition pos, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Post, "/api/positions", body: pos, ct: ct);

    public Task<(bool Ok, string? Error)> UpdatePositionAsync(string symbol, BrokerPosition pos, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Put, $"/api/positions/{Uri.EscapeDataString(symbol)}", body: pos, ct: ct);

    public Task<(bool Ok, string? Error)> ArchivePositionAsync(string symbol, bool isArchived, BrokerPositionStatus? status = null, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Patch, $"/api/positions/{Uri.EscapeDataString(symbol)}/archive", new { isArchived, status = status?.ToString() }, ct);

    public Task<(bool Ok, string? Error)> DeletePositionAsync(string symbol, CancellationToken ct = default) =>
        SendNoContentAsync(HttpMethod.Delete, $"/api/positions/{Uri.EscapeDataString(symbol)}", ct: ct);

    // ── Lot CRUD ──

    public Task<(bool Ok, string? Error)> AddLotAsync(string symbol, BrokerLot lot, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Post, $"/api/positions/{Uri.EscapeDataString(symbol)}/lots", body: lot, ct: ct);

    public Task<(bool Ok, string? Error)> UpdateLotAsync(string symbol, BrokerLot lot, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Put, $"/api/positions/{Uri.EscapeDataString(symbol)}/lots/{Uri.EscapeDataString(lot.Id)}", body: lot, ct: ct);

    public Task<(bool Ok, string? Error)> DeleteLotAsync(string symbol, string lotId, CancellationToken ct = default) =>
        SendNoContentAsync(HttpMethod.Delete, $"/api/positions/{Uri.EscapeDataString(symbol)}/lots/{Uri.EscapeDataString(lotId)}", ct: ct);

    // ── Sell CRUD ──

    public Task<(bool Ok, string? Error)> AddSellAsync(string symbol, BrokerSell sell, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Post, $"/api/positions/{Uri.EscapeDataString(symbol)}/sells", body: sell, ct: ct);

    public Task<(bool Ok, string? Error)> UpdateSellAsync(string symbol, BrokerSell sell, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Put, $"/api/positions/{Uri.EscapeDataString(symbol)}/sells/{Uri.EscapeDataString(sell.Id)}", body: sell, ct: ct);

    public Task<(bool Ok, string? Error)> DeleteSellAsync(string symbol, string sellId, CancellationToken ct = default) =>
        SendNoContentAsync(HttpMethod.Delete, $"/api/positions/{Uri.EscapeDataString(symbol)}/sells/{Uri.EscapeDataString(sellId)}", ct: ct);

    // ── Note CRUD ──

    public Task<(bool Ok, string? Error)> AddNoteAsync(string symbol, BrokerNote note, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Post, $"/api/positions/{Uri.EscapeDataString(symbol)}/notes", body: note, ct: ct);

    public Task<(bool Ok, string? Error)> UpdateNoteAsync(string symbol, BrokerNote note, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Put, $"/api/positions/{Uri.EscapeDataString(symbol)}/notes/{Uri.EscapeDataString(note.Id)}", body: note, ct: ct);

    public Task<(bool Ok, string? Error)> DeleteNoteAsync(string symbol, string noteId, CancellationToken ct = default) =>
        SendNoContentAsync(HttpMethod.Delete, $"/api/positions/{Uri.EscapeDataString(symbol)}/notes/{Uri.EscapeDataString(noteId)}", ct: ct);

    // ── Dividend CRUD ──

    public Task<(bool Ok, string? Error)> AddDividendAsync(string symbol, BrokerDividend div, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Post, $"/api/positions/{Uri.EscapeDataString(symbol)}/dividends", body: div, ct: ct);

    public Task<(bool Ok, string? Error)> UpdateDividendAsync(string symbol, BrokerDividend div, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Put, $"/api/positions/{Uri.EscapeDataString(symbol)}/dividends/{Uri.EscapeDataString(div.Id)}", body: div, ct: ct);

    public Task<(bool Ok, string? Error)> DeleteDividendAsync(string symbol, string divId, CancellationToken ct = default) =>
        SendNoContentAsync(HttpMethod.Delete, $"/api/positions/{Uri.EscapeDataString(symbol)}/dividends/{Uri.EscapeDataString(divId)}", ct: ct);

    // ── Import (PUT toàn bộ — cho ImportJson) ──

    public Task<(bool Ok, string? Error)> ImportPortfolioAsync(BrokerPortfolio portfolio, CancellationToken ct = default) =>
        SavePortfolioAsync(portfolio, ct);

    public async Task<(bool Ok, string? Error)> SavePortfolioAsync(BrokerPortfolio portfolio, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            return (false, _L["Trading_BrokerApi_BaseUrlMissing"].Value);

        try
        {
            portfolio.UpdatedAt = DateTime.Now;
            using var request = await CreateAuthorizedRequestAsync(HttpMethod.Put, "/api/portfolio", ct);
            if (request is null)
                return (false, _L["Trading_BrokerApi_TokenMissing"].Value);

            request.Content = JsonContent.Create(portfolio, options: BrokerJson.Options);

            using var resp = await _http.SendAsync(request, ct);
            if (await HandleUnauthorizedAsync(resp))
                return (false, _L["Trading_BrokerApi_Unauthorized"].Value);

            if (resp.IsSuccessStatusCode)
                return (true, null);

            var body = await resp.Content.ReadAsStringAsync(ct);
            var snippet = TruncateBody(body);
            return (false, $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase} — {snippet}");
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            return (false, _L["Trading_BrokerApi_Timeout", _http.Timeout.TotalSeconds, ex.Message].Value);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"HttpRequestException: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    // ── Helpers ──

    private async Task<(bool Ok, string? Error)> SendJsonAsync<T>(HttpMethod method, string path, T body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            return (false, _L["Trading_BrokerApi_BaseUrlMissing"].Value);

        try
        {
            using var request = await CreateAuthorizedRequestAsync(method, path, ct);
            if (request is null)
                return (false, _L["Trading_BrokerApi_TokenMissing"].Value);

            request.Content = JsonContent.Create(body, options: BrokerJson.Options);

            using var resp = await _http.SendAsync(request, ct);
            if (await HandleUnauthorizedAsync(resp))
                return (false, _L["Trading_BrokerApi_Unauthorized"].Value);

            if (resp.IsSuccessStatusCode)
                return (true, null);

            var respBody = await resp.Content.ReadAsStringAsync(ct);
            return (false, $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase} — {TruncateBody(respBody)}");
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            return (false, _L["Trading_BrokerApi_Timeout", _http.Timeout.TotalSeconds, ex.Message].Value);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"HttpRequestException: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private async Task<(bool Ok, string? Error)> SendNoContentAsync(HttpMethod method, string path, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            return (false, _L["Trading_BrokerApi_BaseUrlMissing"].Value);

        try
        {
            using var request = await CreateAuthorizedRequestAsync(method, path, ct);
            if (request is null)
                return (false, _L["Trading_BrokerApi_TokenMissing"].Value);

            using var resp = await _http.SendAsync(request, ct);
            if (await HandleUnauthorizedAsync(resp))
                return (false, _L["Trading_BrokerApi_Unauthorized"].Value);

            if (resp.IsSuccessStatusCode)
                return (true, null);

            var respBody = await resp.Content.ReadAsStringAsync(ct);
            return (false, $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase} — {TruncateBody(respBody)}");
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            return (false, _L["Trading_BrokerApi_Timeout", _http.Timeout.TotalSeconds, ex.Message].Value);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"HttpRequestException: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private async Task<HttpRequestMessage?> CreateAuthorizedRequestAsync(HttpMethod method, string path, CancellationToken ct)
    {
        var token = await _auth.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var request = new HttpRequestMessage(method, $"{BaseUrl}{path}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private async Task<bool> HandleUnauthorizedAsync(HttpResponseMessage resp)
    {
        if (resp.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            return false;

        await _auth.LogoutAsync();
        var returnUrl = Uri.EscapeDataString(_navigation.Uri);
        _navigation.NavigateTo(
            CulturePath.AppRouteWithQuery(_culture.UrlLang, "trading/login", $"returnUrl={returnUrl}"),
            forceLoad: false);
        return true;
    }

    private static string TruncateBody(string? body)
    {
        if (string.IsNullOrWhiteSpace(body)) return "(empty body)";
        return body.Length > 500 ? body[..500] + "…" : body;
    }
}
