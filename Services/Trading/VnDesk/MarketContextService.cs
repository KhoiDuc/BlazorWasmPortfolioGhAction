using System.Collections.Concurrent;
using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public sealed class MarketContextService
{
    private readonly IVnMarketClient _market;
    private readonly VnDeskDataService _data;
    private readonly IndicatorService _indicators;
    private readonly ConcurrentDictionary<string, MarketContext> _cache = new();

    public MarketContextService(IVnMarketClient market, VnDeskDataService data, IndicatorService indicators)
    {
        _market = market;
        _data = data;
        _indicators = indicators;
    }

    public async Task<MarketContext> GetAsync(string symbol, CancellationToken ct = default)
    {
        symbol = symbol.Trim().ToUpperInvariant();
        if (_cache.TryGetValue(symbol, out var cached))
            return cached;

        var ctx = new MarketContext();
        var sectors = await _data.GetSectorsAsync();
        ctx.SectorName = sectors.FirstOrDefault(kv => kv.Value.Contains(symbol)).Key;

        var indexHist = await _market.GetMarketIndexHistoryAsync("VNINDEX", 250, ct);
        if (indexHist.Count >= 21)
        {
            ctx.VnIndexChange20d = CalcChangePct(indexHist, 20);
            if (indexHist.Count >= 200)
            {
                var idxTa = _indicators.Calculate("VNINDEX", indexHist);
                if (idxTa is not null)
                {
                    ctx.VnIndexTrend = idxTa.Trend;
                    ctx.VnIndexSupport = idxTa.SupportResistance.SupportLevels.OrderByDescending(s => s).FirstOrDefault();
                    ctx.VnIndexResistance = idxTa.SupportResistance.ResistanceLevels.OrderBy(r => r).FirstOrDefault();
                }
            }
        }

        if (!string.IsNullOrEmpty(ctx.SectorName) && sectors.TryGetValue(ctx.SectorName, out var peers))
        {
            var sample = peers.Where(s => s != symbol).Take(14).Prepend(symbol).Distinct().ToList();
            ctx.SectorRelativeStrength = await CalcSectorRelativeStrengthAsync(sample, indexHist, ct);
        }

        _cache[symbol] = ctx;
        return ctx;
    }

    private async Task<decimal> CalcSectorRelativeStrengthAsync(
        IReadOnlyList<string> symbols, List<StockData> indexHist, CancellationToken ct)
    {
        var indexChange = indexHist.Count >= 21 ? CalcChangePct(indexHist, 20) : 0m;
        var changes = new List<decimal>();
        using var sem = new SemaphoreSlim(12);
        var tasks = symbols.Select(async sym =>
        {
            await sem.WaitAsync(ct);
            try
            {
                var hist = await _market.GetHistoricalAsync(sym, 21, "daily", ct);
                if (hist.Count >= 21)
                    changes.Add(CalcChangePct(hist, 20));
            }
            finally { sem.Release(); }
        });
        await Task.WhenAll(tasks);

        if (changes.Count == 0) return 0m;
        return Math.Round(changes.Average() - indexChange, 2);
    }

    private static decimal CalcChangePct(List<StockData> hist, int sessions)
    {
        var ordered = hist.OrderBy(d => d.Date).ToList();
        if (ordered.Count <= sessions) return 0m;
        var first = ordered[^(sessions + 1)].Close;
        var last = ordered[^1].Close;
        if (first <= 0) return 0m;
        return Math.Round((last - first) / first * 100m, 2);
    }
}
