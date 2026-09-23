namespace BlazorWasmPortfolioGhAction.Services;

public static class FinanceMath
{
    public static double Compound(double principal, double annualRate, int years, int compoundsPerYear = 1)
    {
        if (principal < 0 || years < 0 || compoundsPerYear <= 0) return 0;
        var n = compoundsPerYear;
        return principal * Math.Pow(1 + annualRate / n, n * years);
    }

    public static double LoanPayment(double principal, double annualRate, int months)
    {
        if (principal <= 0 || months <= 0) return 0;
        if (annualRate <= 0) return principal / months;
        var r = annualRate / 12;
        return principal * r / (1 - Math.Pow(1 + r, -months));
    }

    public static int PositionShares(double equity, double riskPct, double entry, double stop)
    {
        var risk = equity * riskPct;
        var perShare = Math.Abs(entry - stop);
        if (risk <= 0 || perShare <= 0) return 0;
        return (int)Math.Floor(risk / perShare);
    }

    public static double BreakEven(double entry, double feeRate, double sellTax)
    {
        if (entry <= 0) return 0;
        var cost = entry * (1 + feeRate);
        var net = 1 - feeRate - sellTax;
        return net <= 0 ? 0 : cost / net;
    }

    public static double InflationAdjusted(double amount, double inflationRate, int years)
    {
        if (years < 0) return 0;
        return amount / Math.Pow(1 + inflationRate, years);
    }
}
