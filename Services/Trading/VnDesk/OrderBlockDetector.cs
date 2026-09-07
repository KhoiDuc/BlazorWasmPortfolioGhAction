using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

/// <summary>
/// OrderBlock detector — ported from Survey/StockLib PartternService.OrderBlock.cs.
/// 4 modes: TopPinbar, TopInsideBar, BotPinbar, BotInsideBar. Uses HL pivots.
/// </summary>
public enum OrderBlockMode { TopPinbar, TopInsideBar, BotPinbar, BotInsideBar }

public static class OrderBlockDetector
{
    /// <summary>
    /// Detect order blocks. Returns the most recent signal or null.
    /// </summary>
    public static OrderBlockSignal? Detect(List<StockData> data, int lookback = 300)
    {
        if (data.Count < 20) return null;
        var window = data.OrderBy(d => d.Date).TakeLast(lookback).ToList();
        var pivots = PatternPivots.GetTopBottomCleanHL(window, 5);
        var tops = pivots.Where(p => p.IsTop).ToList();
        var bots = pivots.Where(p => p.IsBot).ToList();

        var signals = new List<OrderBlockSignal>();

        foreach (var top in tops)
        {
            var idx = window.FindIndex(d => d.Date == top.Date);
            if (idx < 0 || idx + 1 >= window.Count) continue;
            var item = window[idx];
            var avg = window.Where(d => d.Date <= top.Date).TakeLast(5).Average(d => d.High - d.Low);
            if (avg == 0) continue;

            var uplen = item.High - Math.Max(item.Open, item.Close);
            var len = item.High - item.Low;

            // TopPinbar: upper wick ≥ 60% range, range ≥ 1.3× avg
            if (uplen / len >= 0.6m && len >= 1.3m * avg)
            {
                var entry = item.High - uplen / 4m;
                var sl = entry + uplen;
                var tp1 = entry - (sl - entry) * 2m;
                signals.Add(new OrderBlockSignal(nameof(OrderBlockMode.TopPinbar), item.Date, entry, sl, [tp1], "Râu trên dài — short"));
            }
            else if (idx + 1 < window.Count)
            {
                var next = window[idx + 1];
                // TopInsideBar: next bearish, close ≤ min(open,close) of item, open ≥ max, range ≥ 1.3× avg
                if (next.Open > next.Close
                    && next.Close <= Math.Min(item.Open, item.Close)
                    && next.Open >= Math.Max(item.Open, item.Close)
                    && (next.High - next.Low) >= 1.3m * avg)
                {
                    var entry = Math.Min(item.Open, item.Close) + 3m * Math.Abs(item.Open - item.Close) / 4m;
                    var sl = Math.Max(item.High, next.High) + Math.Abs(item.Open - item.Close) / 4m;
                    var tp1 = entry - (sl - entry) * 2m;
                    signals.Add(new OrderBlockSignal(nameof(OrderBlockMode.TopInsideBar), item.Date, entry, sl, [tp1], "Inside bar bearish — short"));
                }
            }
        }

        foreach (var bot in bots)
        {
            var idx = window.FindIndex(d => d.Date == bot.Date);
            if (idx < 0 || idx + 1 >= window.Count) continue;
            var item = window[idx];
            var avg = window.Where(d => d.Date <= bot.Date).TakeLast(5).Average(d => d.High - d.Low);
            if (avg == 0) continue;

            var downlen = Math.Min(item.Open, item.Close) - item.Low;
            var len = item.High - item.Low;

            // BotPinbar: lower wick ≥ 60% range, range ≥ 1.3× avg
            if (downlen / len >= 0.6m && len >= 1.3m * avg)
            {
                var entry = downlen / 4m + item.Low;
                var sl = entry - downlen;
                var tp1 = entry + (entry - sl) * 2m;
                signals.Add(new OrderBlockSignal(nameof(OrderBlockMode.BotPinbar), item.Date, entry, sl, [tp1], "Râu dưới dài — long"));
            }
            else if (idx + 1 < window.Count)
            {
                var next = window[idx + 1];
                // BotInsideBar: next bullish, close ≥ max(open,close) of item, open ≤ min, range ≥ 1.3× avg
                if (next.Open < next.Close
                    && next.Close >= Math.Max(item.Open, item.Close)
                    && next.Open <= Math.Min(item.Open, item.Close)
                    && (next.High - next.Low) >= 1.3m * avg)
                {
                    var entry = Math.Min(item.Open, item.Close) + Math.Abs(item.Open - item.Close) / 4m;
                    var sl = Math.Min(item.Low, next.Low) - Math.Abs(item.Open - item.Close) / 4m;
                    var tp1 = entry + (entry - sl) * 2m;
                    signals.Add(new OrderBlockSignal(nameof(OrderBlockMode.BotInsideBar), item.Date, entry, sl, [tp1], "Inside bar bullish — long"));
                }
            }
        }

        return signals.Count > 0 ? signals[^1] : null;
    }
}