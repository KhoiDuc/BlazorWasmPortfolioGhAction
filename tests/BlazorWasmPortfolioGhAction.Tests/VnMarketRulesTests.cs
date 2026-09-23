using BlazorWasmPortfolioGhAction.Services.Trading;
using Xunit;
using BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Tests;

public class VnMarketRulesTests
{
    [Fact]
    public void Tick_follows_hose_and_hnx_steps()
    {
        Assert.Equal(10, VnMarketRules.TickVnd("HOSE", 9_500));
        Assert.Equal(50, VnMarketRules.TickVnd("HOSE", 10_000));
        Assert.Equal(100, VnMarketRules.TickVnd("HOSE", 50_000));
        Assert.Equal(100, VnMarketRules.TickVnd("UPCOM", 9_500));
        Assert.True(VnMarketRules.OnTick("HOSE", 60.5m));
        Assert.False(VnMarketRules.OnTick("HOSE", 60.55m));
    }

    [Fact]
    public void Band_uses_exchange_percent()
    {
        var hose = VnMarketRules.Band("HOSE", 20m);
        Assert.Equal(18.6m, hose.Floor);
        Assert.Equal(21.4m, hose.Ceil);
        Assert.Equal(0.10m, VnMarketRules.BandPercent("HNX"));
        Assert.Equal(0.15m, VnMarketRules.BandPercent("UPCOM"));
    }

    [Fact]
    public void Settlement_skips_weekend_and_holiday()
    {
        var friday = new DateTime(2026, 1, 2);
        Assert.Equal(new DateTime(2026, 1, 6, 13, 0, 0), VnMarketRules.SettledAt(friday));

        var beforeNewYear = new DateTime(2025, 12, 31);
        Assert.Equal(new DateTime(2026, 1, 5, 13, 0, 0), VnMarketRules.SettledAt(beforeNewYear));
    }

    [Fact]
    public void Sellable_quantity_waits_for_t2()
    {
        var lots = new[] { (new DateTime(2026, 3, 2), 100m), (new DateTime(2026, 3, 4), 200m) };
        var now = new DateTime(2026, 3, 4, 14, 0, 0);
        Assert.Equal(100m, VnMarketRules.SellableQuantity(lots, 0, now));
        Assert.Equal(200m, VnMarketRules.PendingQuantity(lots, now));
        Assert.Equal(0m, VnMarketRules.SellableQuantity(lots, 100, now));
    }

    [Fact]
    public void Session_and_odd_lot()
    {
        var ato = new DateTime(2026, 3, 2, 9, 5, 0);
        Assert.Equal(VnSession.Ato, VnMarketRules.SessionAt("HOSE", ato));
        Assert.Equal(new[] { "ATO" }, VnMarketRules.PriceTypes("HOSE", VnSession.Ato));
        Assert.Null(VnMarketRules.QuantityError(50, "LO"));
        Assert.NotNull(VnMarketRules.QuantityError(50, "MP"));
        Assert.NotNull(VnMarketRules.QuantityError(150, "LO"));
    }

    [Fact]
    public void Backtest_cost_keeps_fee_and_tax()
    {
        var (net, _, shares) = BacktestCosts.Apply(20m, 22m, new BacktestCostOptions());
        Assert.Equal(5000, shares);
        Assert.Equal(9_575_000m, net);
    }
}
