using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public sealed class WatchlistScorer
{
    public WatchlistScore Score(TechnicalIndicators ind)
    {
        var close = ind.LatestClose;
        var low20 = ind.LatestLow20;
        var dist = low20 <= 0 ? 1 : (close - low20) / low20;

        var s = new WatchlistScore { Symbol = ind.Symbol };
        s.Liquidity = ind.LatestVolume >= ind.VolumeAverage20 * 0.8m && ind.VolumeAverage20 >= 50_000 ? 4 : ind.VolumeAverage20 >= 50_000 ? 2 : 0;
        s.Trend = close > ind.SMA20 || ind.SMA20 > ind.SMA50 ? 4 : close > ind.SMA50 ? 2 : 0;
        s.Rsi = ind.RSI >= 40 && ind.RSI <= 65 ? 4 : ind.RSI >= 35 && ind.RSI <= 70 ? 2 : 0;
        s.NearSupport = dist <= 0.05m ? 4 : dist <= 0.08m ? 2 : 0;
        s.Volume = ind.VolumeRatio >= 1.2m ? 4 : ind.VolumeRatio >= 1.0m ? 2 : 0;
        s.Note = s.Pass ? "Dat" : "Loai";
        return s;
    }
}
