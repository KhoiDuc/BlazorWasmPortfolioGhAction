using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

/// <summary>
/// SuperTrend + T3 indicators — ported from Survey/StockLib PartternService.SuperTrend.cs + T3.cs.
/// Reimplemented pure C# (no Skender.Stock.Indicators dependency).
/// </summary>
public sealed partial class IndicatorService
{
    /// <summary>Public RSI history accessor for backtest use.</summary>
    public List<decimal> GetRsiHistory(decimal[] closes, int period = 14) => CalculateRsiHistory(closes, period);
    /// <summary>SuperTrend: ATR(period) × multiplier upper/lower band, trend flips when close crosses band.</summary>
    public SuperTrendResult CalculateSuperTrend(List<StockData> data, int period = 10, decimal multiplier = 3m)
    {
        var n = data.Count;
        var upper = new decimal[n];
        var lower = new decimal[n];
        var line = new decimal[n];
        var isUp = new bool[n];

        var atr = CalculateAtrSeries(data, period);
        var closes = data.Select(d => d.Close).ToArray();

        for (var i = 0; i < n; i++)
        {
            if (i < period || atr[i] == 0)
            {
                upper[i] = 0; lower[i] = 0; line[i] = closes[i]; isUp[i] = false;
                continue;
            }

            var hl2 = (data[i].High + data[i].Low) / 2m;
            var basicUpper = hl2 + multiplier * atr[i];
            var basicLower = hl2 - multiplier * atr[i];

            // final upper = min(basicUpper, prevUpper) if close <= prevUpper else basicUpper
            upper[i] = i > 0 && upper[i - 1] != 0 && closes[i - 1] <= upper[i - 1]
                ? Math.Min(basicUpper, upper[i - 1])
                : basicUpper;

            // final lower = max(basicLower, prevLower) if close >= prevLower else basicLower
            lower[i] = i > 0 && lower[i - 1] != 0 && closes[i - 1] >= lower[i - 1]
                ? Math.Max(basicLower, lower[i - 1])
                : basicLower;

            // trend: switch to up when close > prevUpper; switch to down when close < prevLower; else keep
            if (i > 0)
            {
                if (closes[i] > upper[i - 1]) isUp[i] = true;
                else if (closes[i] < lower[i - 1]) isUp[i] = false;
                else isUp[i] = isUp[i - 1];
            }

            // superTrend line = isUp ? lower : upper
            line[i] = isUp[i] ? lower[i] : upper[i];
        }

        return new SuperTrendResult(upper, lower, line, isUp);
    }

    /// <summary>SuperTrend Phrase2: 5-candle LowerBand rising confirmation + green candle.</summary>
    public bool CheckSuperTrendPhrase2(List<StockData> data)
    {
        if (data.Count < 10) return false;
        var st = CalculateSuperTrend(data);
        var n = data.Count;
        var last = st.LowerBand[n - 1];
        var near = st.LowerBand[n - 2];
        var near2 = st.LowerBand[n - 3];
        var near3 = st.LowerBand[n - 4];
        var near4 = st.LowerBand[n - 5];
        var near5 = st.LowerBand[n - 6];

        if (near == 0 || near2 == 0 || near3 == 0 || near4 == 0 || near5 == 0) return false;
        if (near2 != near3 || near2 != near4 || near2 != near5) return false;

        var itemLast = data[n - 1];
        var itemNear = data[n - 2];
        if (near > near2 && itemNear.Close > itemNear.Open) return true;
        if (near == near2 && last > near && itemLast.Close > itemLast.Open) return true;
        return false;
    }

    /// <summary>Tillson T3 — 5×EMA with volume factor. Pure recursive EMA.</summary>
    public decimal[] CalculateT3(List<StockData> data, int period = 5, decimal volumeFactor = 0.7m)
    {
        var closes = data.Select(d => d.Close).ToArray();
        return CalculateT3Series(closes, period, volumeFactor);
    }

    private static decimal[] CalculateT3Series(decimal[] closes, int period, decimal vFactor)
    {
        var e1 = EmaSeries(closes, period);
        var e2 = EmaSeries(e1, period);
        var e3 = EmaSeries(e2, period);
        var e4 = EmaSeries(e3, period);
        var e5 = EmaSeries(e4, period);
        var e6 = EmaSeries(e5, period);

        var c1 = -vFactor * vFactor * vFactor;
        var c2 = 3m * vFactor * vFactor * (1m + vFactor);
        var c3 = -3m * vFactor * (1m + vFactor) * (1m + vFactor);
        var c4 = 1m + 3m * vFactor + 3m * vFactor * vFactor + vFactor * vFactor * vFactor;

        var t3 = new decimal[closes.Length];
        for (var i = 0; i < closes.Length; i++)
            t3[i] = c1 * e6[i] + c2 * e5[i] + c3 * e4[i] + c4 * e3[i];
        return t3;
    }

    /// <summary>EMA series — recursive, seeded with SMA of first `period` values.</summary>
    private static decimal[] EmaSeries(decimal[] values, int period)
    {
        var n = values.Length;
        var ema = new decimal[n];
        if (n == 0) return ema;
        var k = 2m / (period + 1);

        if (n < period)
        {
            // seed with running average
            var sum = 0m;
            for (var i = 0; i < n; i++) { sum += values[i]; ema[i] = sum / (i + 1); }
            return ema;
        }

        var seed = 0m;
        for (var i = 0; i < period; i++) seed += values[i];
        ema[period - 1] = seed / period;

        for (var i = period; i < n; i++)
            ema[i] = values[i] * k + ema[i - 1] * (1m - k);

        // backfill first period-1 with seed
        for (var i = 0; i < period - 1; i++) ema[i] = ema[period - 1];
        return ema;
    }

    /// <summary>ATR series (Wilder smoothing) — returns ATR per bar.</summary>
    private static decimal[] CalculateAtrSeries(List<StockData> data, int period)
    {
        var n = data.Count;
        var atr = new decimal[n];
        if (n < period + 1) return atr;

        var tr = new decimal[n];
        for (var i = 0; i < n; i++)
        {
            if (i == 0) tr[i] = data[i].High - data[i].Low;
            else tr[i] = Math.Max(
                data[i].High - data[i].Low,
                Math.Max(
                    Math.Abs(data[i].High - data[i - 1].Close),
                    Math.Abs(data[i].Low - data[i - 1].Close)));
        }

        // seed ATR = average of first `period` TRs
        var sum = 0m;
        for (var i = 0; i < period; i++) sum += tr[i];
        atr[period - 1] = sum / period;

        for (var i = period; i < n; i++)
            atr[i] = (atr[i - 1] * (period - 1) + tr[i]) / period;

        return atr;
    }
}