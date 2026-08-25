using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorWasmPortfolioGhAction.Extensions;
using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public interface IVnMarketClient
{
    Task<List<MarketIndex>> FetchIndicesAsync(CancellationToken ct = default);
    Task<List<string>> FetchSymbolsAsync(CancellationToken ct = default);
    Task<List<StockData>> GetHistoricalAsync(string symbol, int sessions = 250, string timeframe = "daily", CancellationToken ct = default);
    Task<List<StockData>> GetLatestManyAsync(IReadOnlyList<string> symbols, IProgress<int>? progress = null, int maxParallel = 12, CancellationToken ct = default);
    Task<List<IntradayData>> FetchIntradayAsync(string symbol, CancellationToken ct = default);
    void ClearCache();
}

public sealed class VnMarketClient : IVnMarketClient
{
    private readonly HttpClient _vndHttp;
    private readonly HttpClient _cafefHttp;
    private readonly HttpClient _proxyHttp;
    private readonly TradingEndpointResolver _endpoints;
    private readonly VnDeskOptions _options;
    private readonly ConcurrentDictionary<string, List<StockData>> _cache = new();
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public VnMarketClient(
        IHttpClientFactory factory,
        TradingEndpointResolver endpoints,
        VnDeskOptions options)
    {
        _endpoints = endpoints;
        _options = options;
        _vndHttp = factory.CreateClient(TradingServiceExtensions.VnMarketClientName);
        _cafefHttp = factory.CreateClient(TradingServiceExtensions.VnCafeFClientName);
        _proxyHttp = factory.CreateClient(TradingServiceExtensions.TradingApiClientName);
    }

    public void ClearCache() => _cache.Clear();

    public async Task<List<MarketIndex>> FetchIndicesAsync(CancellationToken ct = default)
    {
        try
        {
            var url = _endpoints.ResolveProxyUrl("cafef/stockhandler.ashx?index=true");
            var json = await _cafefHttp.GetStringAsync(url, ct);
            return JsonSerializer.Deserialize<List<MarketIndex>>(json, JsonOpts) ?? [];
        }
        catch
        {
            try
            {
                var json = await _cafefHttp.GetStringAsync(_options.CafefIndexUrl, ct);
                return JsonSerializer.Deserialize<List<MarketIndex>>(json, JsonOpts) ?? [];
            }
            catch
            {
                return [];
            }
        }
    }

    public async Task<List<string>> FetchSymbolsAsync(CancellationToken ct = default)
    {
        try
        {
            var url = _endpoints.ResolveProxyUrl("cafef/stockhandler.ashx?allstocks=true");
            var json = await _cafefHttp.GetStringAsync(url, ct);
            var types = JsonSerializer.Deserialize<List<CafeFStockType>>(json, JsonOpts);
            return types?.Select(x => x.Symbol).Where(s => !string.IsNullOrWhiteSpace(s)).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<StockData>> GetHistoricalAsync(string symbol, int sessions = 250, string timeframe = "daily", CancellationToken ct = default)
    {
        var cacheKey = $"{symbol}_{timeframe}_{sessions}";
        if (_cache.TryGetValue(cacheKey, out var cached) && cached.Count > 0)
            return cached.OrderBy(d => d.Date).TakeLast(sessions).ToList();

        try
        {
            var path = $"v4/stock_prices?sort=date:desc&q=code:{Uri.EscapeDataString(symbol)}&size={sessions}&type={timeframe}";
            var url = _endpoints.ResolveFetchUrl(path);
            using var response = await _vndHttp.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
                return FallbackCache(cacheKey, sessions);

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var api = await JsonSerializer.DeserializeAsync<VnDirectResponse>(stream, JsonOpts, ct);
            if (api?.data is null || api.data.Count == 0)
                return FallbackCache(cacheKey, sessions);

            var history = api.data.Select(x => new StockData
            {
                Symbol = x.code,
                Date = x.date,
                Open = x.open,
                High = x.high,
                Low = x.low,
                Close = x.close,
                Volume = x.nmVolume,
                Change = x.change,
                PercentChange = x.pctChange
            }).OrderBy(d => d.Date).ToList();

            _cache[cacheKey] = history;
            return history.TakeLast(sessions).ToList();
        }
        catch
        {
            return FallbackCache(cacheKey, sessions);
        }
    }

    public async Task<List<StockData>> GetLatestManyAsync(IReadOnlyList<string> symbols, IProgress<int>? progress = null, int maxParallel = 12, CancellationToken ct = default)
    {
        var bag = new ConcurrentBag<StockData>();
        using var sem = new SemaphoreSlim(maxParallel);
        int done = 0;
        var tasks = symbols.Select(async symbol =>
        {
            await sem.WaitAsync(ct);
            try
            {
                var data = await GetHistoricalAsync(symbol, 1, "summary", ct);
                var first = data.FirstOrDefault();
                if (first is not null)
                    bag.Add(first);
            }
            finally
            {
                Interlocked.Increment(ref done);
                progress?.Report(done);
                sem.Release();
            }
        });
        await Task.WhenAll(tasks);
        return bag.ToList();
    }

    public async Task<List<IntradayData>> FetchIntradayAsync(string symbol, CancellationToken ct = default)
    {
        try
        {
            var url = _endpoints.ResolveFetchUrl($"stock-insight/v1/stock/bars/{Uri.EscapeDataString(symbol.ToUpperInvariant())}?timeframe=1&count=200");
            var resp = await _proxyHttp.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return [];

            using var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Array)
                return [];

            var list = new List<IntradayData>();
            foreach (var el in root.EnumerateArray())
            {
                if (!TryGetDecimal(el, "close", out var price) && !TryGetDecimal(el, "c", out price))
                    continue;
                var time = el.TryGetProperty("tradingDate", out var td) ? ParseTime(td) :
                    el.TryGetProperty("time", out var t) ? ParseTime(t) : DateTime.Now;
                var vol = TryGetDecimal(el, "volume", out var v) || TryGetDecimal(el, "vol", out v) ? v : 0;
                list.Add(new IntradayData { Time = time, Price = price, Volume = vol });
            }
            return list;
        }
        catch
        {
            return [];
        }
    }

    private List<StockData> FallbackCache(string cacheKey, int sessions)
    {
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached.OrderBy(d => d.Date).TakeLast(Math.Min(sessions, cached.Count)).ToList();
        return [];
    }

    private static DateTime ParseTime(JsonElement el)
    {
        if (el.ValueKind == JsonValueKind.String && DateTime.TryParse(el.GetString(), out var dt))
            return dt;
        if (el.ValueKind == JsonValueKind.Number && el.TryGetInt64(out var ms))
            return DateTimeOffset.FromUnixTimeMilliseconds(ms).LocalDateTime;
        return DateTime.Now;
    }

    private static bool TryGetDecimal(JsonElement el, string prop, out decimal value)
    {
        value = 0;
        if (!el.TryGetProperty(prop, out var p)) return false;
        if (p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out value)) return true;
        if (p.ValueKind == JsonValueKind.String && decimal.TryParse(p.GetString(), out value)) return true;
        return false;
    }

    private sealed class CafeFStockType
    {
        [JsonPropertyName("a")]
        public string Symbol { get; set; } = "";
    }
}
