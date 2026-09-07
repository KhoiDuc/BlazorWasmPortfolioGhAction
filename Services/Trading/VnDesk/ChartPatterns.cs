using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

/// <summary>
/// Chart patterns — VCP, Head & Shoulders, Double Top/Bottom.
/// Ported & simplified from Survey/StockLib PartternService.VCP.cs + W.cs.
/// Uses PatternPivots instead of Skender ZigZag.
/// </summary>
public static class ChartPatterns
{
    /// <summary>
    /// VCP (Volatility Contraction Pattern): 252-day base, price 60-100% of 252-day high,
    /// 50-day volume slope decreasing, last 5-bar pivot width < 10%, volume dry-up below 50-day avg.
    /// </summary>
    public static PatternMatch? DetectVCP(List<StockData> data)
    {
        const int timeframe = 252;
        const int volTf = 50;
        const decimal baseLowerLimit = 0.6m;
        const int pivotLength = 5;
        const decimal pvLimit = 0.1m;

        if (data.Count < timeframe) return null;
        var ordered = data.OrderBy(d => d.Date).ToList();
        var i = ordered.Count - 1;
        var cur = ordered[i];

        var yearData = ordered.Skip(i + 1 - timeframe).Take(timeframe).ToList();
        var highPrice = yearData.MaxBy(d => d.Close)!;

        var nearHigh = cur.Close < highPrice.Close && cur.Close > baseLowerLimit * highPrice.Close;

        // 50-day volume SMA slope (linear regression last vs first)
        var volSeries = ordered.Select(d => d.Volume).ToArray();
        var volSma = SmaSeries(volSeries, volTf);
        var volSlope = volSma.Length > volTf
            ? (volSma[^1] - volSma[volSma.Length - volTf]) / volTf
            : 0;
        var volDecreasing = volSlope < 0;

        // pivot: last `pivotLength` bars width < 10% of close, start at pivot high
        var pivotSlice = ordered.Skip(i + 1 - pivotLength).Take(pivotLength).ToList();
        var pivotHigh = pivotSlice.Max(d => d.High);
        var pivotLow = pivotSlice.Min(d => d.Low);
        var pivotWidth = (pivotHigh - pivotLow) / cur.Close;
        var pivotStartHp = ordered[i + 1 - pivotLength].High;
        var isPivot = pivotWidth < pvLimit && pivotHigh == pivotStartHp;

        // volume dry-up: last `pivotLength` volumes all below 50-day SMA
        var volDryUp = true;
        for (var j = 0; j < pivotLength; j++)
        {
            var volSmaIdx = i - j;
            if (volSmaIdx < 0 || volSmaIdx >= volSma.Length) { volDryUp = false; break; }
            if (ordered[i - j].Volume >= volSma[volSmaIdx]) { volDryUp = false; break; }
        }

        if (nearHigh && volDecreasing && isPivot && volDryUp)
        {
            var entry = cur.Close;
            var stop = pivotLow * 0.98m;
            var tp1 = entry + (entry - stop) * 2;
            var tp2 = entry + (entry - stop) * 3;
            return new PatternMatch("VCP", cur.Date, entry, stop, [tp1, tp2], 0.8);
        }
        return null;
    }

    /// <summary>Head & Shoulders: 3 peaks, middle highest, shoulders ~equal. Uses HL pivots.</summary>
    public static PatternMatch? DetectHeadAndShoulders(List<StockData> data)
    {
        var pivots = PatternPivots.GetTopBottomCleanHL(data, 5);
        var tops = pivots.Where(p => p.IsTop).ToList();
        if (tops.Count < 3) return null;

        // take last 3 tops
        var r = tops[^3..];
        var left = r[0]; var head = r[1]; var right = r[2];
        if (head.Value <= left.Value || head.Value <= right.Value) return null;

        // shoulders within 5% of each other
        var shoulderDiff = Math.Abs((double)(left.Value - right.Value) / (double)left.Value);
        if (shoulderDiff > 0.05) return null;

        // neckline = troughs between shoulders
        var troughs = pivots.Where(p => p.IsBot && p.Date > left.Date && p.Date < right.Date).ToList();
        if (troughs.Count == 0) return null;
        var neckline = troughs.Min(p => p.Value);

        var cur = data[^1];
        if (cur.Close > neckline) return null; // not broken yet

        var entry = cur.Close;
        var stop = head.Value;
        var targetHeight = head.Value - neckline;
        var tp1 = entry - targetHeight;
        var tp2 = entry - targetHeight * 1.5m;
        return new PatternMatch("Head & Shoulders", cur.Date, entry, stop, [tp1, tp2], 0.7);
    }

    /// <summary>Double Top / Double Bottom via close-based pivots. Replaces IndicatorService.DetectChartPatterns.</summary>
    public static List<PatternMatch> DetectDoubleTopBottom(List<StockData> data)
    {
        var result = new List<PatternMatch>();
        var pivots = PatternPivots.GetTopBottomClean(data, 5);
        if (pivots.Count < 4) return result;

        const decimal validdiff = 0.0125m; // ~5%/4
        const int minDistance = 10;

        // Double Top: last 2 tops within validdiff, first top >= forward high
        var tops = pivots.Where(p => p.IsTop).ToList();
        if (tops.Count >= 2)
        {
            var t1 = tops[^2]; var t2 = tops[^1];
            var diff = Math.Abs((double)(t1.Value / t2.Value) - 1);
            var barsBetween = data.FindIndex(d => d.Date == t2.Date) - data.FindIndex(d => d.Date == t1.Date);
            if (diff < (double)validdiff && barsBetween > minDistance)
            {
                var cur = data[^1];
                if (cur.Close < Math.Min(t1.Value, t2.Value))
                {
                    var entry = cur.Close;
                    var stop = Math.Max(t1.Value, t2.Value);
                    var height = stop - entry;
                    result.Add(new PatternMatch("Double Top", cur.Date, entry, stop, [entry - height, entry - height * 1.5m], 0.65));
                }
            }
        }

        // Double Bottom: last 2 bots within validdiff, first bot <= forward low
        var bots = pivots.Where(p => p.IsBot).ToList();
        if (bots.Count >= 2)
        {
            var b1 = bots[^2]; var b2 = bots[^1];
            var diff = Math.Abs((double)(b1.Value / b2.Value) - 1);
            var barsBetween = data.FindIndex(d => d.Date == b2.Date) - data.FindIndex(d => d.Date == b1.Date);
            if (diff < (double)validdiff && barsBetween > minDistance)
            {
                var cur = data[^1];
                if (cur.Close > Math.Max(b1.Value, b2.Value))
                {
                    var entry = cur.Close;
                    var stop = Math.Min(b1.Value, b2.Value);
                    var height = entry - stop;
                    result.Add(new PatternMatch("Double Bottom", cur.Date, entry, stop, [entry + height, entry + height * 1.5m], 0.65));
                }
            }
        }

        return result;
    }

    private static decimal[] SmaSeries(decimal[] values, int period)
    {
        var n = values.Length;
        var sma = new decimal[n];
        for (var i = 0; i < n; i++)
        {
            if (i + 1 < period) { sma[i] = 0; continue; }
            var sum = 0m;
            for (var j = i + 1 - period; j <= i; j++) sum += values[j];
            sma[i] = sum / period;
        }
        return sma;
    }
}