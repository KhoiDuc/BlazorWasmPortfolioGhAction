using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public static class MoneyFlowCalculator
{
    private const int MakerWindow = 5;
    private const int ScoreWindow = 10;
    private const int SLongWindow = 20;
    private const int DeltaLookback = 3;
    private const int HeatLookback = 30;
    private const int StrdWindow = 30;
    private const int StrdDeltaLookback = 5;
    private const int SmaPeriod = 20;
    private const int WaveSLongLookback = 10;
    private const double Scale = 10.0;

    public static MoneyFlowSnapshot Compute(string symbol, IReadOnlyList<StockData> history)
    {
        var snap = new MoneyFlowSnapshot { Symbol = symbol };
        if (history.Count < SLongWindow + WaveSLongLookback)
            return snap;

        var data = history.OrderBy(d => d.Date).ToList();
        var clv = data.Select(CalcClv).ToList();
        var vol = data.Select(d => (double)d.Volume).ToList();

        var makerRaw = RollingVwap(clv, vol, MakerWindow);
        var scoreRaw = RollingVwap(clv, vol, ScoreWindow);
        var sLongRaw = RollingVwap(clv, vol, SLongWindow);

        var series = new List<MoneyFlowDayPoint>();
        for (var i = 0; i < data.Count; i++)
        {
            if (double.IsNaN(makerRaw[i]) || double.IsNaN(scoreRaw[i]) || double.IsNaN(sLongRaw[i]))
                continue;

            series.Add(new MoneyFlowDayPoint
            {
                Date = data[i].Date,
                Maker = makerRaw[i] * Scale,
                Score = scoreRaw[i] * Scale,
                SLong = sLongRaw[i] * Scale
            });
        }

        if (series.Count == 0)
            return snap;

        ApplyStrd(series);

        var last = data[^1];
        var lastPt = series[^1];
        var idx3 = series.Count - 1 - DeltaLookback;
        var idx30 = series.Count - 1 - HeatLookback;

        snap.HasData = true;
        snap.LastClose = last.Close;
        snap.PercentChange = last.PercentChange;
        snap.Maker = lastPt.Maker;
        snap.Score = lastPt.Score;
        snap.SLong = lastPt.SLong;
        snap.DeltaMaker = idx3 >= 0 ? lastPt.Maker - series[idx3].Maker : 0;
        snap.DeltaScore = idx3 >= 0 ? lastPt.Score - series[idx3].Score : 0;
        snap.DeltaSLong = idx3 >= 0 ? lastPt.SLong - series[idx3].SLong : 0;
        snap.Heat30D = idx30 >= 0 ? lastPt.SLong - series[idx30].SLong : 0;
        snap.Series = series;

        var values = series.SelectMany(p => new[] { p.Maker, p.Score, p.SLong }).ToList();
        snap.ChartMin = values.Count > 0 ? values.Min() : 0;
        snap.ChartMax = values.Count > 0 ? values.Max() : 0;

        var closes = data.Select(d => (double)d.Close).ToList();
        var sma20 = closes.Count >= SmaPeriod ? closes.TakeLast(SmaPeriod).Average() : 0;
        var sLong10Idx = series.Count - 1 - WaveSLongLookback;
        var sLong10Ago = sLong10Idx >= 0 ? series[sLong10Idx].SLong : double.NaN;

        snap.WaveConditions =
        [
            new MediumWaveCondition
            {
                Key = "sLongLevel",
                Met = lastPt.SLong > 1.5,
                Detail = $"SLong {lastPt.SLong:F2} > 1.5"
            },
            new MediumWaveCondition
            {
                Key = "sLongRising",
                Met = !double.IsNaN(sLong10Ago) && lastPt.SLong > sLong10Ago,
                Detail = !double.IsNaN(sLong10Ago)
                    ? $"SLong {lastPt.SLong:F2} > {sLong10Ago:F2} (10 phiên trước)"
                    : "Thiếu lịch sử"
            },
            new MediumWaveCondition
            {
                Key = "scorePositive",
                Met = lastPt.Score > 0,
                Detail = $"Score {lastPt.Score:F2} > 0"
            },
            new MediumWaveCondition
            {
                Key = "makerFloor",
                Met = lastPt.Maker > -1,
                Detail = $"Maker {lastPt.Maker:F2} > -1"
            },
            new MediumWaveCondition
            {
                Key = "aboveSma20",
                Met = (double)last.Close > sma20,
                Detail = $"Close {(double)last.Close:N2} > SMA20 {sma20:N2}"
            }
        ];
        snap.MediumWaveSignal = snap.WaveConditions.All(c => c.Met);

        return snap;
    }

    public static void ApplyHeatPercentiles(IReadOnlyList<MoneyFlowSnapshot> snapshots)
    {
        if (snapshots.Count == 0) return;
        var heats = snapshots.Where(s => s.HasData).Select(s => s.Heat30D).OrderBy(h => h).ToList();
        if (heats.Count == 0) return;

        foreach (var s in snapshots.Where(s => s.HasData))
        {
            var rank = heats.Count(h => h <= s.Heat30D);
            s.Heat30Percentile = heats.Count > 1 ? (double)(rank - 1) / (heats.Count - 1) * 100 : 50;
        }
    }

    private static double CalcClv(StockData bar)
    {
        var range = (double)(bar.High - bar.Low);
        if (range <= 0) return 0;
        return ((2 * (double)bar.Close - (double)bar.High - (double)bar.Low) / range);
    }

    private static double[] RollingVwap(IReadOnlyList<double> clv, IReadOnlyList<double> vol, int window)
    {
        var result = new double[clv.Count];
        for (var i = 0; i < clv.Count; i++)
        {
            if (i + 1 < window)
            {
                result[i] = double.NaN;
                continue;
            }

            var start = i + 1 - window;
            var wSum = 0.0;
            var vSum = 0.0;
            for (var j = start; j <= i; j++)
            {
                wSum += clv[j] * vol[j];
                vSum += vol[j];
            }

            result[i] = vSum > 0 ? wSum / vSum : 0;
        }

        return result;
    }

    private static void ApplyStrd(List<MoneyFlowDayPoint> series)
    {
        var makers = series.Select(p => p.Maker).ToList();
        var scores = series.Select(p => p.Score).ToList();
        var slongs = series.Select(p => p.SLong).ToList();

        for (var i = 0; i < series.Count; i++)
        {
            series[i].MakerStrd = ZScoreAt(makers, i, StrdWindow);
            series[i].ScoreStrd = ZScoreAt(scores, i, StrdWindow);
            series[i].SLongStrd = ZScoreAt(slongs, i, StrdWindow);
            series[i].MakerDeltaStrd = DeltaZScoreAt(makers, i, StrdDeltaLookback, StrdWindow);
            series[i].ScoreDeltaStrd = DeltaZScoreAt(scores, i, StrdDeltaLookback, StrdWindow);
            series[i].SLongDeltaStrd = DeltaZScoreAt(slongs, i, StrdDeltaLookback, StrdWindow);
        }
    }

    private static double? ZScoreAt(IReadOnlyList<double> values, int index, int window)
    {
        if (index + 1 < window) return null;
        var slice = values.Skip(index + 1 - window).Take(window).ToList();
        var mean = slice.Average();
        var std = StdDev(slice, mean);
        if (std < 1e-9) return 0;
        return (values[index] - mean) / std;
    }

    private static double? DeltaZScoreAt(IReadOnlyList<double> values, int index, int deltaLookback, int window)
    {
        if (index < deltaLookback || index + 1 < window) return null;
        var deltas = new List<double>();
        for (var i = window - 1; i <= index; i++)
        {
            if (i >= deltaLookback)
                deltas.Add(values[i] - values[i - deltaLookback]);
        }

        if (deltas.Count == 0) return null;
        var current = values[index] - values[index - deltaLookback];
        var mean = deltas.Average();
        var std = StdDev(deltas, mean);
        if (std < 1e-9) return 0;
        return (current - mean) / std;
    }

    private static double StdDev(IReadOnlyList<double> values, double mean)
    {
        if (values.Count < 2) return 0;
        var sum = values.Sum(v => (v - mean) * (v - mean));
        return Math.Sqrt(sum / values.Count);
    }
}
