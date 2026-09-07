using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

/// <summary>
/// Pivot (swing high/low) detection — ported & simplified from Survey/StockLib/Utils/ExtensionMethod.cs GetTopBottom*.
/// Pure functions on StockData, no external deps.
/// </summary>
public static class PatternPivots
{
    /// <summary>Close-based pivot detection (fractal 5-bar). minRate = min % move between pivots to keep.</summary>
    public static List<Pivot> GetTopBottom(List<StockData> data, double minRatePct = 5)
    {
        var result = new List<Pivot>();
        if (data.Count < 5) return result;

        // first 5 placeholders (not pivots)
        for (var i = 0; i < 5; i++)
            result.Add(new Pivot(data[i].Date, false, false, data[i].Close));

        var lastPivot = (Pivot?)null;
        for (var i = 2; i < data.Count - 2; i++)
        {
            var c = data[i].Close;
            // swing low: lower than 2 prev + 2 next
            if (c < data[i - 1].Close && c < data[i - 2].Close && c < data[i + 1].Close && c < data[i + 2].Close)
            {
                var p = new Pivot(data[i].Date, false, true, c);
                if (lastPivot is { } lp && lp.Value > 0)
                {
                    var rate = Math.Abs(100 * (-1 + (double)(p.Value / lp.Value)));
                    if (rate < minRatePct)
                    {
                        // too small a move — keep the deeper bottom
                        if (lp.IsBot && p.Value < lp.Value)
                        {
                            var idx = result.IndexOf(lp);
                            result[idx] = lp with { IsBot = false };
                            lastPivot = p;
                            result.Add(p);
                        }
                        else
                        {
                            result.Add(new Pivot(data[i].Date, false, false, c));
                        }
                    }
                    else
                    {
                        lastPivot = p;
                        result.Add(p);
                    }
                }
                else
                {
                    lastPivot = p;
                    result.Add(p);
                }
            }
            // swing high: higher than 2 prev + 2 next
            else if (c > data[i - 1].Close && c > data[i - 2].Close && c > data[i + 1].Close && c > data[i + 2].Close)
            {
                var p = new Pivot(data[i].Date, true, false, c);
                if (lastPivot is { } lp && lp.Value > 0)
                {
                    var rate = Math.Abs(100 * (-1 + (double)(p.Value / lp.Value)));
                    if (rate < minRatePct)
                    {
                        if (lp.IsTop && p.Value > lp.Value)
                        {
                            var idx = result.IndexOf(lp);
                            result[idx] = lp with { IsTop = false };
                            lastPivot = p;
                            result.Add(p);
                        }
                        else
                        {
                            result.Add(new Pivot(data[i].Date, false, false, c));
                        }
                    }
                    else
                    {
                        lastPivot = p;
                        result.Add(p);
                    }
                }
                else
                {
                    lastPivot = p;
                    result.Add(p);
                }
            }
            else
            {
                result.Add(new Pivot(data[i].Date, false, false, c));
            }
        }
        return result;
    }

    /// <summary>Filtered pivots — only Top/Bot, alternating, dedup adjacent same-type.</summary>
    public static List<Pivot> GetTopBottomClean(List<StockData> data, double minRatePct = 5)
    {
        var raw = GetTopBottom(data, minRatePct);
        var pivots = raw.Where(p => p.IsTop || p.IsBot).ToList();
        if (pivots.Count == 0) return pivots;

        // drop leading top (must start with bot)
        if (pivots[0].IsTop) pivots.RemoveAt(0);

        // dedup: keep higher top / lower bot when same type adjacent
        var cleaned = new List<Pivot>();
        foreach (var p in pivots)
        {
            if (cleaned.Count > 0)
            {
                var last = cleaned[^1];
                if (last.IsTop == p.IsTop)
                {
                    if (p.IsTop && p.Value > last.Value) cleaned[^1] = p;
                    else if (p.IsBot && p.Value < last.Value) cleaned[^1] = p;
                    continue;
                }
            }
            cleaned.Add(p);
        }
        return cleaned;
    }

    /// <summary>High/low-based pivot detection (fractal 5-bar on High/Low instead of Close).</summary>
    public static List<Pivot> GetTopBottomHL(List<StockData> data, double minRatePct = 5)
    {
        var result = new List<Pivot>();
        if (data.Count < 5) return result;

        for (var i = 0; i < 5; i++)
            result.Add(new Pivot(data[i].Date, false, false, data[i].Low));

        var lastPivot = (Pivot?)null;
        for (var i = 2; i < data.Count - 2; i++)
        {
            var low = data[i].Low;
            var high = data[i].High;
            // swing low on Low
            if (low < data[i - 1].Low && low < data[i - 2].Low && low < data[i + 1].Low && low < data[i + 2].Low)
            {
                var p = new Pivot(data[i].Date, false, true, low);
                if (lastPivot is { } lp && lp.Value > 0)
                {
                    var rate = Math.Abs(100 * (-1 + (double)(p.Value / lp.Value)));
                    if (rate < minRatePct)
                    {
                        if (lp.IsBot && p.Value < lp.Value)
                        {
                            var idx = result.IndexOf(lp);
                            result[idx] = lp with { IsBot = false };
                            lastPivot = p;
                            result.Add(p);
                        }
                        else result.Add(new Pivot(data[i].Date, false, false, low));
                    }
                    else { lastPivot = p; result.Add(p); }
                }
                else { lastPivot = p; result.Add(p); }
            }
            // swing high on High
            else if (high > data[i - 1].High && high > data[i - 2].High && high > data[i + 1].High && high > data[i + 2].High)
            {
                var p = new Pivot(data[i].Date, true, false, high);
                if (lastPivot is { } lp && lp.Value > 0)
                {
                    var rate = Math.Abs(100 * (-1 + (double)(p.Value / lp.Value)));
                    if (rate < minRatePct)
                    {
                        if (lp.IsTop && p.Value > lp.Value)
                        {
                            var idx = result.IndexOf(lp);
                            result[idx] = lp with { IsTop = false };
                            lastPivot = p;
                            result.Add(p);
                        }
                        else result.Add(new Pivot(data[i].Date, false, false, high));
                    }
                    else { lastPivot = p; result.Add(p); }
                }
                else { lastPivot = p; result.Add(p); }
            }
            else result.Add(new Pivot(data[i].Date, false, false, low));
        }
        return result;
    }

    /// <summary>Filtered HL pivots — alternating, dedup.</summary>
    public static List<Pivot> GetTopBottomCleanHL(List<StockData> data, double minRatePct = 5)
    {
        var raw = GetTopBottomHL(data, minRatePct);
        var pivots = raw.Where(p => p.IsTop || p.IsBot).ToList();
        if (pivots.Count == 0) return pivots;
        if (pivots[0].IsTop) pivots.RemoveAt(0);

        var cleaned = new List<Pivot>();
        foreach (var p in pivots)
        {
            if (cleaned.Count > 0)
            {
                var last = cleaned[^1];
                if (last.IsTop == p.IsTop)
                {
                    if (p.IsTop && p.Value > last.Value) cleaned[^1] = p;
                    else if (p.IsBot && p.Value < last.Value) cleaned[^1] = p;
                    continue;
                }
            }
            cleaned.Add(p);
        }
        return cleaned;
    }
}

public record Pivot(DateTime Date, bool IsTop, bool IsBot, decimal Value);