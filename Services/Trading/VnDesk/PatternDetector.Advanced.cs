using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

/// <summary>
/// PinBar + Elliott Wave detectors — ported from Survey/StockLib PartternService.OrderBlock.cs (Pinbar) + Coin.Eliot.cs.
/// </summary>
public static partial class CandlestickPatternDetector
{
    /// <summary>
    /// PinBar: body ≤ 1/3 of range, long nose (≥ 60% of range on one side).
    /// Bullish PinBar = long lower wick (hammer-like); Bearish = long upper wick (shooting-star-like).
    /// </summary>
    public static List<CandlestickPattern> DetectPinBar(List<StockData> historyData)
    {
        var result = new List<CandlestickPattern>();
        if (historyData.Count < 1) return result;

        var window = historyData.TakeLast(20).ToList();
        var avgRange = window.Average(d => d.Range()) > 0 ? window.Average(d => d.Range()) : 1m;

        foreach (var d in historyData.TakeLast(50))
        {
            var range = d.Range();
            if (range <= 0 || range < 0.8m * avgRange) continue;

            var body = d.BodySize();
            if (body > range / 3m) continue;

            var upperWick = d.UpperWickSize();
            var lowerWick = d.LowerWickSize();

            // Bullish pinbar: long lower wick
            if (lowerWick >= 0.6m * range)
            {
                result.Add(new CandlestickPattern(
                    "Bullish PinBar",
                    "Nến râu dưới dài, body nhỏ — từ chối bán, khả năng đảo lên",
                    PatternType.Reversal, Direction.Bullish));
            }
            // Bearish pinbar: long upper wick
            else if (upperWick >= 0.6m * range)
            {
                result.Add(new CandlestickPattern(
                    "Bearish PinBar",
                    "Nến râu trên dài, body nhỏ — từ chối mua, khả năng đảo xuống",
                    PatternType.Reversal, Direction.Bearish));
            }
        }
        return result;
    }

    /// <summary>
    /// Elliott Wave + RSI(6) divergence — simplified.
    /// Bearish divergence: price higher high, RSI lower high → sell signal.
    /// Bullish divergence: price lower low, RSI higher low → buy signal.
    /// </summary>
    public static ElliottSignal? DetectElliottWave(List<StockData> historyData, List<decimal> rsiHistory)
    {
        if (historyData.Count < 100 || rsiHistory.Count < 10) return null;

        var pivots = PatternPivots.GetTopBottomClean(historyData, 3);
        var tops = pivots.Where(p => p.IsTop).ToList();
        var bots = pivots.Where(p => p.IsBot).ToList();
        if (tops.Count < 2 || bots.Count < 2) return null;

        var cur = historyData[^1];
        var near = historyData[^2];

        // bearish divergence at top
        if (pivots.Count >= 2 && pivots[^2].IsTop)
        {
            var lastTop = tops[^1];
            var prevTop = tops[^2];
            var lastRsi = RsiAt(rsiHistory, historyData, lastTop.Date);
            var prevRsi = RsiAt(rsiHistory, historyData, prevTop.Date);
            if (lastRsi.HasValue && prevRsi.HasValue
                && lastTop.Value >= prevTop.Value && lastRsi < prevRsi)
            {
                return new ElliottSignal(5, "Bearish divergence", lastTop.Date, lastTop.Value,
                    "Price HH + RSI LH — sell signal");
            }
        }

        // bullish divergence at bottom
        if (pivots.Count >= 2 && pivots[^2].IsBot)
        {
            var lastBot = bots[^1];
            var prevBot = bots[^2];
            var lastRsi = RsiAt(rsiHistory, historyData, lastBot.Date);
            var prevRsi = RsiAt(rsiHistory, historyData, prevBot.Date);
            if (lastRsi.HasValue && prevRsi.HasValue
                && lastBot.Value <= prevBot.Value && lastRsi > prevRsi)
            {
                return new ElliottSignal(5, "Bullish divergence", lastBot.Date, lastBot.Value,
                    "Price LL + RSI HL — buy signal");
            }
        }

        return null;
    }

    private static decimal? RsiAt(List<decimal> rsiHistory, List<StockData> data, DateTime date)
    {
        var idx = data.FindIndex(d => d.Date == date);
        if (idx < 0 || idx >= rsiHistory.Count) return null;
        return rsiHistory[idx];
    }
}