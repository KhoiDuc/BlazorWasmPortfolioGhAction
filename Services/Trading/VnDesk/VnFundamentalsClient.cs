using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Extensions;
using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public interface IVnFundamentalsClient
{
    Task<FundamentalSnapshot?> GetAsync(string symbol, CancellationToken ct = default);
    void ClearCache();
}

public sealed class VnFundamentalsClient : IVnFundamentalsClient
{
    private static readonly string[] RatioCodes =
    [
        "PRICE_TO_EARNINGS", "PRICE_TO_BOOK", "ROAE_TR_AVG5Q", "ROAA_TR_AVG5Q",
        "NET_PROFIT_TR_GRYOY", "PRETAX_PROFIT_TR_GRYOY", "EPS_TR", "BVPS_CR",
        "MARKETCAP", "DIVIDEND_YIELD"
    ];

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _http;
    private readonly TradingEndpointResolver _endpoints;
    private readonly ConcurrentDictionary<string, FundamentalSnapshot> _cache = new();

    public VnFundamentalsClient(IHttpClientFactory factory, TradingEndpointResolver endpoints)
    {
        _endpoints = endpoints;
        _http = factory.CreateClient(TradingServiceExtensions.VnMarketClientName);
    }

    public void ClearCache() => _cache.Clear();

    public async Task<FundamentalSnapshot?> GetAsync(string symbol, CancellationToken ct = default)
    {
        symbol = symbol.Trim().ToUpperInvariant();
        if (symbol.Length < 3) return null;

        if (_cache.TryGetValue(symbol, out var cached))
            return cached;

        try
        {
            var codes = string.Join(",", RatioCodes);
            var path = $"v4/ratios/latest?filter=ratioCode:{codes}&where=code:{Uri.EscapeDataString(symbol)}&order=reportDate&fields=ratioCode,value,reportDate";
            var url = _endpoints.ResolveFetchUrl(path);
            using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode) return null;

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var api = await JsonSerializer.DeserializeAsync<VnDirectRatioResponse>(stream, JsonOpts, ct);
            if (api?.data is null || api.data.Count == 0) return null;

            var map = api.data
                .GroupBy(x => x.ratioCode)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.reportDate).First());

            decimal? Get(string code) => map.TryGetValue(code, out var row) ? row.value : null;
            DateTime? GetDate(string code) => map.TryGetValue(code, out var row) ? row.reportDate : null;

            var snapshot = new FundamentalSnapshot
            {
                Symbol = symbol,
                Pe = Get("PRICE_TO_EARNINGS"),
                Pb = Get("PRICE_TO_BOOK"),
                Roe = Get("ROAE_TR_AVG5Q"),
                Roa = Get("ROAA_TR_AVG5Q"),
                ProfitGrowthYoY = Get("NET_PROFIT_TR_GRYOY"),
                PretaxGrowthYoY = Get("PRETAX_PROFIT_TR_GRYOY"),
                Eps = Get("EPS_TR"),
                Bvps = Get("BVPS_CR"),
                MarketCap = Get("MARKETCAP"),
                DividendYield = Get("DIVIDEND_YIELD"),
                ReportDate = GetDate("PRICE_TO_EARNINGS") ?? GetDate("ROAE_TR_AVG5Q") ?? GetDate("NET_PROFIT_TR_GRYOY")
            };

            var required = new[] { snapshot.Pe, snapshot.Pb, snapshot.Roe, snapshot.ProfitGrowthYoY };
            snapshot.IsPartial = required.Count(v => v.HasValue) < 3;

            _cache[symbol] = snapshot;
            return snapshot;
        }
        catch
        {
            return null;
        }
    }
}
