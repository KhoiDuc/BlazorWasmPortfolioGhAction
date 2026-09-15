using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Models.Trading.Broker;
using Microsoft.Extensions.Configuration;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public interface IBrokerApiClient
{
    Task<BrokerPortfolio?> GetPortfolioAsync(CancellationToken ct = default);

    // CRUD Position
    Task<(bool Ok, string? Error)> CreatePositionAsync(BrokerPosition pos, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UpdatePositionAsync(string symbol, BrokerPosition pos, CancellationToken ct = default);
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
    Task<(bool Ok, string? Error)> DeleteDividendAsync(string symbol, string divId, CancellationToken ct = default);

    // Import (giữ nguyên PUT toàn bộ — cho ImportJson)
    Task<(bool Ok, string? Error)> ImportPortfolioAsync(BrokerPortfolio portfolio, CancellationToken ct = default);

    // Legacy — kept temporarily for backward compat
    Task<(bool Ok, string? Error)> SavePortfolioAsync(BrokerPortfolio portfolio, CancellationToken ct = default);
}

public sealed class BrokerApiClient : IBrokerApiClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public BrokerApiClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    private string? BaseUrl => _config["BrokerApi:BaseUrl"]?.Trim().TrimEnd('/');
    private string? ApiKey => _config["BrokerApi:ApiKey"]?.Trim();

    public async Task<BrokerPortfolio?> GetPortfolioAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            return null;

        try
        {
            var url = $"{BaseUrl}/api/portfolio?v={DateTime.UtcNow.Ticks}";
            using var resp = await _http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode)
                return null;

            var json = await resp.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<BrokerPortfolio>(json, BrokerJson.Options);
        }
        catch
        {
            return null;
        }
    }

    // ── Position CRUD ──

    public Task<(bool Ok, string? Error)> CreatePositionAsync(BrokerPosition pos, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Post, "/api/positions", body: pos, ct: ct);

    public Task<(bool Ok, string? Error)> UpdatePositionAsync(string symbol, BrokerPosition pos, CancellationToken ct = default) =>
        SendJsonAsync(HttpMethod.Put, $"/api/positions/{Uri.EscapeDataString(symbol)}", body: pos, ct: ct);

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

    public Task<(bool Ok, string? Error)> DeleteDividendAsync(string symbol, string divId, CancellationToken ct = default) =>
        SendNoContentAsync(HttpMethod.Delete, $"/api/positions/{Uri.EscapeDataString(symbol)}/dividends/{Uri.EscapeDataString(divId)}", ct: ct);

    // ── Import (PUT toàn bộ — cho ImportJson) ──

    public Task<(bool Ok, string? Error)> ImportPortfolioAsync(BrokerPortfolio portfolio, CancellationToken ct = default) =>
        SavePortfolioAsync(portfolio, ct);

    public async Task<(bool Ok, string? Error)> SavePortfolioAsync(BrokerPortfolio portfolio, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            return (false, "BrokerApi:BaseUrl trống trong appsettings.json");
        if (string.IsNullOrWhiteSpace(ApiKey))
            return (false, "BrokerApi:ApiKey trống trong appsettings.json");

        try
        {
            portfolio.UpdatedAt = DateTime.Now;
            using var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/api/portfolio");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
            request.Content = JsonContent.Create(portfolio, options: BrokerJson.Options);

            using var resp = await _http.SendAsync(request, ct);

            if (resp.IsSuccessStatusCode)
                return (true, null);

            var body = await resp.Content.ReadAsStringAsync(ct);
            var snippet = TruncateBody(body);
            return (false, $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase} — {snippet}");
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            return (false, $"Timeout sau {_http.Timeout.TotalSeconds:F0}s — {ex.Message}");
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
            return (false, "BrokerApi:BaseUrl trống trong appsettings.json");
        if (string.IsNullOrWhiteSpace(ApiKey))
            return (false, "BrokerApi:ApiKey trống trong appsettings.json");

        try
        {
            using var request = new HttpRequestMessage(method, $"{BaseUrl}{path}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
            request.Content = JsonContent.Create(body, options: BrokerJson.Options);

            using var resp = await _http.SendAsync(request, ct);
            if (resp.IsSuccessStatusCode)
                return (true, null);

            var respBody = await resp.Content.ReadAsStringAsync(ct);
            return (false, $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase} — {TruncateBody(respBody)}");
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            return (false, $"Timeout sau {_http.Timeout.TotalSeconds:F0}s — {ex.Message}");
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
            return (false, "BrokerApi:BaseUrl trống trong appsettings.json");
        if (string.IsNullOrWhiteSpace(ApiKey))
            return (false, "BrokerApi:ApiKey trống trong appsettings.json");

        try
        {
            using var request = new HttpRequestMessage(method, $"{BaseUrl}{path}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            using var resp = await _http.SendAsync(request, ct);
            if (resp.IsSuccessStatusCode)
                return (true, null);

            var respBody = await resp.Content.ReadAsStringAsync(ct);
            return (false, $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase} — {TruncateBody(respBody)}");
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            return (false, $"Timeout sau {_http.Timeout.TotalSeconds:F0}s — {ex.Message}");
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

    private static string TruncateBody(string? body)
    {
        if (string.IsNullOrWhiteSpace(body)) return "(empty body)";
        return body.Length > 500 ? body[..500] + "…" : body;
    }
}