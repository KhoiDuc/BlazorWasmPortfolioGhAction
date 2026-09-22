using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using VnDesk.Models;

namespace VnDesk.Clients;

public sealed class CafeFClient
{
    private readonly HttpClient _http;
    private readonly AppConfig _cfg;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public CafeFClient(AppConfig cfg)
    {
        _cfg = cfg;
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
        _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
    }

    public async Task<List<MarketIndex>> FetchIndicesAsync()
    {
        try
        {
            var json = await _http.GetStringAsync(_cfg.CafefIndexUrl);
            return JsonSerializer.Deserialize<List<MarketIndex>>(json, JsonOpts) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<string>> FetchSymbolsAsync()
    {
        try
        {
            var json = await _http.GetStringAsync(_cfg.CafefAllStocksUrl);
            var types = JsonSerializer.Deserialize<List<CafeFStockType>>(json, JsonOpts);
            return types?.Select(x => x.Symbol).Where(s => !string.IsNullOrWhiteSpace(s)).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    private sealed class CafeFStockType
    {
        [JsonPropertyName("a")]
        public string Symbol { get; set; } = "";
    }
}
