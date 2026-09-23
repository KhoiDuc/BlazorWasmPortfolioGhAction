namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public sealed record BacktestCostOptions(
    decimal FeeRate = 0.0015m,
    decimal SellTaxRate = 0.001m,
    decimal SlippageBps = 0,
    int LotSize = 100,
    decimal CapitalPerTrade = 100_000_000m);

public static class BacktestCosts
{
    /// <summary>
    /// Net VND P/L for one round trip. Entry and exit prices are in thousands of VND.
    /// </summary>
    public static (decimal NetPnl, double NetPnlPct, int Shares) Apply(decimal entryPrice, decimal exitPrice, BacktestCostOptions? options)
    {
        var cost = options ?? new BacktestCostOptions();
        var lot = Math.Max(1, cost.LotSize);
        var slip = cost.SlippageBps / 10_000m;
        var entry = entryPrice * (1 + slip);
        var exit = exitPrice * (1 - slip);
        if (entry <= 0) return (0, 0, 0);
        var rawShares = (int)Math.Floor(cost.CapitalPerTrade / (entry * 1000m) / lot) * lot;
        var shares = Math.Max(lot, rawShares);
        var buyNotional = entry * 1000m * shares;
        var sellNotional = exit * 1000m * shares;
        var net = sellNotional - sellNotional * cost.FeeRate - sellNotional * cost.SellTaxRate
                  - buyNotional - buyNotional * cost.FeeRate;
        var pct = buyNotional == 0 ? 0 : (double)(net / buyNotional * 100m);
        return (decimal.Round(net, 0), pct, shares);
    }
}
