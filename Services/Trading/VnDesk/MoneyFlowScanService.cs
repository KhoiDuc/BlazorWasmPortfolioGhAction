using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public sealed class MoneyFlowScanService
{
    private const int HistorySessions = 90;
    private const int MaxParallel = 8;

    private readonly IVnMarketClient _market;

    public MoneyFlowScanService(IVnMarketClient market) => _market = market;

    public async Task<MoneyFlowSnapshot?> ScanSymbolAsync(string symbol, CancellationToken ct = default)
    {
        symbol = symbol.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(symbol)) return null;

        try
        {
            var hist = await _market.GetHistoricalAsync(symbol, HistorySessions, ct: ct);
            var snap = MoneyFlowCalculator.Compute(symbol, hist);
            return snap.HasData ? snap : null;
        }
        catch
        {
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
            .OrderByDescending(s => s.MediumWaveSignal)
            .ThenByDescending(s => s.Heat30D)
            .ThenByDescending(s => s.SLong)
            .ToList();
    }
}
