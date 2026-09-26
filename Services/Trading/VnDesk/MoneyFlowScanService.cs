using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;
using BlazorWasmPortfolioGhAction.Services.Trading.Tcbs;
using Microsoft.Extensions.Logging;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public sealed class MoneyFlowScanService
{
    private const int HistorySessions = 90;
    private const int MaxParallel = 8;

    private readonly IVnMarketClient _market;
    private readonly ITcbsApiClient _tcbs;
    private readonly ILogger<MoneyFlowScanService> _logger;

    public MoneyFlowScanService(IVnMarketClient market, ITcbsApiClient tcbs, ILogger<MoneyFlowScanService> logger)
    {
        _market = market;
        _tcbs = tcbs;
        _logger = logger;
    }

    public async Task<MoneyFlowSnapshot?> ScanSymbolAsync(string symbol, CancellationToken ct = default)
    {
        symbol = symbol.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(symbol)) return null;

        try
        {
            var hist = await _market.GetHistoricalAsync(symbol, HistorySessions, ct: ct);
            var snap = MoneyFlowCalculator.Compute(symbol, hist);
            if (!snap.HasData) return null;
            await AttachTcbsAsync(snap, ct);
            return snap;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Money-flow scan failed for {Symbol}", symbol);
            return null;
        }
    }

    public async Task<List<MoneyFlowSnapshot>> ScanManyAsync(
        IReadOnlyList<string> symbols,
        IProgress<int>? progress = null,
        CancellationToken ct = default)
    {
        var distinct = symbols
            .Select(s => s.Trim().ToUpperInvariant())
            .Where(s => s.Length >= 2)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var results = new List<MoneyFlowSnapshot>();
        using var sem = new SemaphoreSlim(MaxParallel);
        var done = 0;

        var tasks = distinct.Select(async sym =>
        {
            await sem.WaitAsync(ct);
            try
            {
                var snap = await ScanSymbolAsync(sym, ct);
                if (snap is not null)
                    lock (results) results.Add(snap);
            }
            finally
            {
                Interlocked.Increment(ref done);
                progress?.Report(done);
                sem.Release();
            }
        });

        await Task.WhenAll(tasks);

        MoneyFlowCalculator.ApplyHeatPercentiles(results);

        return results
            .OrderByDescending(s => s.BuySellRatio ?? 0)
            .ThenByDescending(s => s.MediumWaveSignal)
            .ThenByDescending(s => s.Heat30D)
            .ThenByDescending(s => s.SLong)
            .ToList();
    }

    private async Task AttachTcbsAsync(MoneyFlowSnapshot snap, CancellationToken ct)
    {
        try
        {
            var status = await _tcbs.GetStatusAsync(ct);
            if (!status.Connected) return;
            var day = await _tcbs.GetAsync($"market/supply-demand?ticker={Uri.EscapeDataString(snap.Symbol)}&window=day", ct);
            var month = await _tcbs.GetAsync($"market/supply-demand?ticker={Uri.EscapeDataString(snap.Symbol)}&window=month", ct);
            var room = await _tcbs.GetAsync("market/foreign-room?index=1", ct);
            var points = day.Ok ? TcbsMapper.ReadSupply(day.Json) : [];
            if (points.Count == 0 && month.Ok) points = TcbsMapper.ReadSupply(month.Json);
            var last = points.LastOrDefault();
            if (last is not null) snap.BuySellRatio = last.Ratio;
            if (room.Ok)
            {
                var quote = TcbsMapper.ReadQuotes(room.Json).FirstOrDefault(q => q.Symbol.Equals(snap.Symbol, StringComparison.OrdinalIgnoreCase));
                if (quote is not null) snap.ForeignNet = quote.BuyForeign - quote.SellForeign;
            }
            if (snap.BuySellRatio is not null || snap.ForeignNet is not null)
                snap.TcbsFlowNote = $"Cung cầu {snap.BuySellRatio:N2} · NN {snap.ForeignNet:N0}";
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "TCBS flow overlay skipped for {Symbol}", snap.Symbol);
        }
    }
}
