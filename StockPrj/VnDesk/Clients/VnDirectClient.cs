using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using VnDesk.Models;

namespace VnDesk.Clients;

public sealed class VnDirectClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly ConcurrentDictionary<string, List<StockData>> _cache = new();
    private readonly object _cacheLock = new();
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public VnDirectClient(AppConfig cfg)
    {
        _baseUrl = cfg.VndirectBaseUrl.TrimEnd('/');
        var handler = new HttpClientHandler
        {
            MaxConnectionsPerServer = 20,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
        _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.vndirect.com.vn/");
    }

    public void ClearCache() => _cache.Clear();

    public async Task<List<StockData>> GetHistoricalAsync(string symbol, int sessions = 250, string timeframe = "daily")
    {
        var cacheKey = $"{symbol}_{timeframe}_{sessions}";
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(cacheKey, out var cached) && cached.Count > 0)
                return cached.OrderBy(d => d.Date).TakeLast(sessions).ToList();
        }

        try
        {
            var url = $"{_baseUrl}/v4/stock_prices?sort=date:desc&q=code:{symbol}&size={sessions}&type={timeframe}";
            using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync();
            var api = await JsonSerializer.DeserializeAsync<VnDirectResponse>(stream, JsonOpts);
            if (api?.data is null || api.data.Count == 0)
                return [];

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

            lock (_cacheLock)
                _cache[cacheKey] = history;

            return history.TakeLast(sessions).ToList();
        }
        catch
        {
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(cacheKey, out var cached))
                    return cached.OrderBy(d => d.Date).TakeLast(Math.Min(sessions, cached.Count)).ToList();
            }
            return [];
        }
    }

    public async Task<List<StockData>> GetLatestManyAsync(IReadOnlyList<string> symbols, IProgress<int>? progress = null, int maxParallel = 20)
    {
        var bag = new ConcurrentBag<StockData>();
        using var sem = new SemaphoreSlim(maxParallel);
        int done = 0;
        var tasks = symbols.Select(async symbol =>
        {
            await sem.WaitAsync();
            try
            {
                var data = await GetHistoricalAsync(symbol, 1, "summary");
                var first = data.FirstOrDefault();
                if (first is not null)
                    bag.Add(first);
            }
            finally
            {
                var n = Interlocked.Increment(ref done);
                progress?.Report(n);
                sem.Release();
            }
        });
        await Task.WhenAll(tasks);
        return bag.ToList();
    }
}
