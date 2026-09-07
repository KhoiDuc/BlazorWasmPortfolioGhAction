using System.Net.Http.Json;
using System.Text.Json;

namespace BlazorWasmPortfolioGhAction.Services.Trading;

public interface ITradingApiClient
{
    Task<JsonElement?> GetFxRatesAsync(CancellationToken ct = default);
    Task<JsonElement?> GetProxyJsonAsync(string path, CancellationToken ct = default);
    string ProxyUrl(string path);
}

/// <summary>
/// Proxy-external URL rewriter + FX rates fetch. No internal backend (Fly Go / OSINT VPS) — removed.
/// </summary>
public class TradingApiClient : ITradingApiClient
{
    private readonly HttpClient _http;
    private readonly TradingEndpointResolver _endpoints;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public TradingApiClient(HttpClient http, TradingEndpointResolver endpoints)
    {
        _http = http;
        _endpoints = endpoints;
    }

    public string ProxyUrl(string path) => _endpoints.ResolveProxyUrl(path);

    public Task<JsonElement?> GetFxRatesAsync(CancellationToken ct = default) =>
        GetAbsoluteJsonAsync(_endpoints.ResolveFetchUrl("api/rates"), ct);

    public Task<JsonElement?> GetProxyJsonAsync(string path, CancellationToken ct = default) =>
        GetJsonAsync(path, ct);

    private async Task<JsonElement?> GetJsonAsync(string path, CancellationToken ct)
    {
        var url = _endpoints.ResolveFetchUrl(path);
        // Absolute external URL (vndirect, cafef, yahoo, petrolimex, …) — fetch directly.
        if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            return await GetAbsoluteJsonAsync(url, ct);

        // ponytail: no Fly gateway anymore — relative paths unsupported.
        return null;
    }

    private async Task<JsonElement?> GetAbsoluteJsonAsync(string absoluteUrl, CancellationToken ct)
    {
        try
        {
            var resp = await _http.GetAsync(absoluteUrl, ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOpts, ct);
        }
        catch
        {
            return null;
        }
    }
}