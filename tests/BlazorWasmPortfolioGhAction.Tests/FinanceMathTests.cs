using BlazorWasmPortfolioGhAction.Services;
using Xunit;

namespace BlazorWasmPortfolioGhAction.Tests;

public class FinanceMathTests
{
    [Fact]
    public void Compound_doubles_with_known_inputs()
    {
        var value = FinanceMath.Compound(1000, 0.10, 2);
        Assert.Equal(1210, value, 2);
    }

    [Fact]
    public void Position_size_uses_risk_per_share()
    {
        Assert.Equal(100, FinanceMath.PositionShares(100_000, 0.01, 10, 0));
    }

    [Fact]
    public void Break_even_covers_fees_and_tax()
    {
        var price = FinanceMath.BreakEven(100, 0.0015, 0.001);
        Assert.True(price > 100);
    }
}
