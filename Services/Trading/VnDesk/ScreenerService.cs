using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public sealed class VnScreenerService
{
    private readonly VnDeskDataService _data;
    private Dictionary<string, List<string>>? _lists;

    public VnScreenerService(VnDeskDataService data) => _data = data;

    public async Task<Dictionary<string, List<string>>> GetListsAsync()
    {
        _lists ??= await _data.GetListsAsync();
        return _lists;
    }

    public List<PotentialStock> Scan(List<StockData> history)
    {
        var a = FindTrend(history);
        var b = FindRecovery(history);
        return a.Concat(b)
            .GroupBy(x => x.Symbol)
            .Select(g => g.OrderByDescending(x => x.PotentialScore).First())
            .OrderByDescending(x => x.PotentialScore)
            .ToList();
    }

    private static List<PotentialStock> FindTrend(List<StockData> historicalData, int minDays = 50, int maShort = 20, int maLong = 50)
    {
        var result = new List<PotentialStock>();
        foreach (var group in historicalData.GroupBy(d => d.Symbol))
        {
            var data = group.OrderBy(d => d.Date).ToList();
            if (data.Count < minDays) continue;
            var closes = data.Select(d => (double)d.Close).ToArray();
            var volumes = data.Select(d => (double)d.Volume).ToArray();
            var (up, score) = CheckTrend(closes, maShort, maLong);
            if (!up) continue;
            var (priceScore, priceReason) = EvaluatePrice(closes);
            var (volScore, volReason) = EvaluateVolume(volumes);
            var total = priceScore + volScore + score;
            if (total > 50)
            {
                result.Add(new PotentialStock
                {
                    Symbol = group.Key,
                    LastPrice = (decimal)closes.Last(),
                    PriceChange = data.Last().PercentChange,
                    Volume = (decimal)volumes.Last(),
                    PotentialScore = total,
                    Reason = $"Price: {priceReason} | Volume: {volReason} | Trend: {score}"
                });
            }
        }
        return result;
    }

    private static List<PotentialStock> FindRecovery(List<StockData> historicalData,
        double maxDrop = 0.20, double minRecovery = 0.03, int recoveryDays = 5)
    {
        return historicalData
            .GroupBy(d => d.Symbol)
            .Where(g => g.Count() >= 50)
            .Select(g => AnalyzeRecovery(g.OrderBy(d => d.Date).ToList(), maxDrop, minRecovery, recoveryDays))
            .Where(x => x is not null)
            .Cast<PotentialStock>()
            .ToList();
    }

    private static PotentialStock? AnalyzeRecovery(List<StockData> data, double maxDrop, double minRecovery, int recoveryDays)
    {
        var closes = data.Select(d => (double)d.Close).ToArray();
        var volumes = data.Select(d => (double)d.Volume).ToArray();
        var last60 = closes.Skip(Math.Max(0, closes.Length - 60)).ToArray();
        var peak = last60.Max();
        var peakIdx = Array.LastIndexOf(last60, peak);
        var search = last60.Skip(peakIdx).Take(30).ToArray();
        if (search.Length == 0) return null;
        var bottom = search.Min();
        var bottomIdx = Array.LastIndexOf(search, bottom) + peakIdx;
        var current = closes.Last();
        var drop = (peak - bottom) / peak;
        var rec = bottom == 0 ? 0 : (current - bottom) / bottom;
        var volSurge = volumes.TakeLast(20).Average() == 0 ? 1 : volumes.Last() / volumes.TakeLast(20).Average();
        if (drop < 0.08 || drop > maxDrop || rec < minRecovery) return null;
        var absBottom = closes.Length - last60.Length + bottomIdx;
        if (closes.Length - absBottom > 15) return null;
        var recoveryPrices = closes.Skip(absBottom).Take(recoveryDays).ToArray();
        var (slope, r2, _) = AdvancedSlope(recoveryPrices);
        if (slope <= 0.3 || r2 <= 0.6) return null;
        var score = drop * 200 + rec * 300 + Math.Min(25, (volSurge - 1) * 10);
        return new PotentialStock
        {
            Symbol = data.Last().Symbol,
            LastPrice = (decimal)current,
            PriceChange = data.Count >= 2 ? (decimal)((current - (double)data[^2].Close) / (double)data[^2].Close * 100) : 0,
            Volume = (decimal)volumes.Last(),
            PotentialScore = score,
            Reason = $"Giam {drop:P1} ({peak:N0}->{bottom:N0}) | Hoi {rec:P1} | Vol x{volSurge:N1}"
        };
    }

    private static (bool up, int score) CheckTrend(double[] closes, int shortP, int longP)
    {
        var maS = closes.TakeLast(shortP).Average();
        var maL = closes.TakeLast(longP).Average();
        var pos = maS > maL ? 30 : 0;
        var recent = closes.Skip(closes.Length - shortP).ToArray();
        var (slope, r2, _) = AdvancedSlope(recent);
        var baseScore = (int)(slope * 1000);
        var conf = r2 > 0.8 ? 1.5 : r2 > 0.6 ? 1.2 : r2 > 0.4 ? 1.0 : 0.8;
        return (maS > maL, pos + (int)(baseScore * conf));
    }

    private static (int score, string reason) EvaluatePrice(double[] closes)
    {
        var low = closes.TakeLast(20).Min();
        var cur = closes.Last();
        var dist = (cur - low) / low;
        if (dist <= 0.05) return (30, $"Gan ho tro (cach day {dist * 100:F1}%)");
        if (dist <= 0.1) return (20, $"Tiem can ho tro (cach day {dist * 100:F1}%)");
        return (0, $"Vung trung binh (cach day {dist * 100:F1}%)");
    }

    private static (int score, string reason) EvaluateVolume(double[] volumes)
    {
        var last = volumes.Last();
        var avg = volumes.TakeLast(20).Average();
        var ratio = avg == 0 ? 1 : last / avg;
        if (last > avg * 2) return (30, $"Volume dot bien x{ratio:F1}");
        if (last > avg * 1.5) return (20, $"Volume cao x{ratio:F1}");
        return (0, $"Volume TB x{ratio:F1}");
    }

    public static (double slope, double rSquared, double intercept) AdvancedSlope(double[] data, bool weighted = true)
    {
        if (data.Length < 2) return (0, 0, 0);
        int n = data.Length;
        var x = Enumerable.Range(0, n).Select(i => (double)i).ToArray();
        var w = weighted ? Enumerable.Range(1, n).Select(i => (double)i / n).ToArray() : Enumerable.Repeat(1.0, n).ToArray();
        double sumW = 0, sumWX = 0, sumWY = 0, sumWXY = 0, sumWX2 = 0;
        for (int i = 0; i < n; i++)
        {
            sumW += w[i];
            sumWX += w[i] * x[i];
            sumWY += w[i] * data[i];
            sumWXY += w[i] * x[i] * data[i];
            sumWX2 += w[i] * x[i] * x[i];
        }
        var den = sumW * sumWX2 - sumWX * sumWX;
        if (Math.Abs(den) < 1e-10) return (0, 0, 0);
        var slope = (sumW * sumWXY - sumWX * sumWY) / den;
        var intercept = (sumWY - slope * sumWX) / sumW;
        var yMean = sumWY / sumW;
        double ssTot = 0, ssRes = 0;
        for (int i = 0; i < n; i++)
        {
            var pred = slope * x[i] + intercept;
            ssTot += w[i] * Math.Pow(data[i] - yMean, 2);
            ssRes += w[i] * Math.Pow(data[i] - pred, 2);
        }
        return (slope, ssTot == 0 ? 0 : 1 - ssRes / ssTot, intercept);
    }
}
