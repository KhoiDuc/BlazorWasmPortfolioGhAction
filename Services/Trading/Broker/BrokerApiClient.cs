using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Models.Trading.Broker;
using Microsoft.Extensions.Configuration;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public interface IBrokerApiClient
{
    Task<BrokerPortfolio?> GetPortfolioAsync(CancellationToken ct = default);
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
            var snippet = string.IsNullOrWhiteSpace(body) ? "(empty body)" : body.Length > 500 ? body[..500] + "…" : body;
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
}
