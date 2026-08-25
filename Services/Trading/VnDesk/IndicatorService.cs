using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public sealed class IndicatorService
{
    public TechnicalIndicators? Calculate(string symbol, List<StockData> historyData)
    {
        historyData = historyData.OrderBy(d => d.Date).ToList();
        if (historyData.Count < 200)
            return null;

        var closes = historyData.Select(d => d.Close).ToArray();
        var highs = historyData.Select(d => d.High).ToArray();
        var lows = historyData.Select(d => d.Low).ToArray();
        var volumes = historyData.Select(d => d.Volume).ToArray();
        if (closes.Any(c => c <= 0) || volumes.Any(v => v < 0))
            return null;

        var rsiHistory = CalculateRsiHistory(closes);
        var (rsi, previousRsi) = CalculateRsiWithPrevious(closes, 14);
        var sma20 = Sma(closes, 20);
        var sma50 = Sma(closes, 50);
        var sma200 = Sma(closes, 200);
        var macd = CalculateMacd(closes, 12, 26, 9);
        var bb = CalculateBollinger(closes, 20, 2);
        var stoch = CalculateStochastic(closes, highs, lows, 14, 3);
        var atr = CalculateAtrPercent(historyData, 14);
        var obv = CalculateObv(closes, volumes);
        var ichimoku = CalculateIchimoku(closes, highs, lows, 9, 26, 52);
        var sr = CalculateSupportResistance(highs, lows);
        var divergence = DetectDivergence(closes, volumes, 30);
        var vol20 = AnalyzeVolume20(volumes);
        var vol50 = Sma(volumes, 50);
        var liquidity = AnalyzeLiquidity(volumes.Last(), vol20.Average, vol50);
        var trend = DetermineTrend(closes, sma20, sma50, sma200, volumes.Last(), vol20.Average, rsi, atr, bb.Upper, bb.Lower, bb.Middle);
        var patterns = CandlestickPatternDetector.DetectPatterns(historyData.TakeLast(50).ToList());
        var charts = DetectChartPatterns(historyData.TakeLast(50).ToList());
        var signal = GenerateTradingSignals(rsi, macd, stoch, trend, vol20, atr, sr, closes.Last());
        signal.Symbol = symbol;
        signal.SignalTime = DateTime.Now;

        return new TechnicalIndicators
        {
            Symbol = symbol,
            Date = historyData.Last().Date,
            RSI = rsi,
            PreviousRSI = previousRsi,
            SMA20 = sma20,
            SMA50 = sma50,
            SMA200 = sma200,
            MACD = macd.MacdLine,
            Signal = macd.SignalLine,
            Histogram = macd.Histogram,
            PreviousHistogram = macd.PreviousHistogram,
            BollingerUpper = bb.Upper,
            BollingerMiddle = bb.Middle,
            BollingerLower = bb.Lower,
            K_Stochastic = stoch.k,
            D_Stochastic = stoch.d,
            ATR = atr,
            OBV = obv,
            Ichimoku = ichimoku,
            VolumeAverage20 = vol20.Average,
            VolumeAverage50 = vol50,
            VolumeRatio = vol20.Ratio,
            Trend = trend,
            Patterns = patterns,
            ChartPatterns = charts,
            LiquidityAssessment = liquidity,
            Divergence = divergence,
            SupportResistance = sr,
            TradingSignal = signal,
            LatestClose = closes.Last(),
            LatestHigh = highs.Last(),
            LatestLow = lows.Last(),
            LatestHigh5 = highs.TakeLast(5).Max(),
            LatestLow5 = lows.TakeLast(5).Min(),
            LatestHigh20 = highs.TakeLast(20).Max(),
            LatestLow20 = lows.TakeLast(20).Min(),
            LatestVolume = volumes.Last(),
            PreviousClose = closes.Length >= 2 ? closes[^2] : 0,
            PreviousHigh = highs.Length >= 2 ? highs[^2] : 0,
            PreviousLow = lows.Length >= 2 ? lows[^2] : 0,
            RsiHistory = rsiHistory,
            Last3Rsi = rsiHistory.TakeLast(3).ToList()
        };
    }

    public void ApplyIntraday(TechnicalIndicators ind, List<IntradayData> ticks)
    {
        if (ticks.Count == 0)
        {
            ind.IntradayNote = "Khong co du lieu intraday (Python/TCBS).";
            return;
        }

        var sorted = ticks.OrderBy(d => d.Time).ToList();
        ind.IntradayData = sorted;
        ind.LatestIntradayDataClose = sorted.Last().Price;
        ind.LatestIntradayDataHigh = sorted.Max(d => d.Price);
        ind.LatestIntradayDataLow = sorted.Min(d => d.Price);

        var vn = DateTime.UtcNow.AddHours(7);
        var from = vn.AddMinutes(-30);
        ind.LatestIntradayDataVolume = sorted.Where(d => d.Time >= from).Sum(d => d.Volume);
        ind.VolumeRatioIntradayData = ind.VolumeAverage20 <= 0 ? 0 : Math.Min(10, ind.LatestIntradayDataVolume / ind.VolumeAverage20);

        if (sorted.Count >= 10 && sorted[^10].Price > 0)
            ind.IntradayMomentum = (sorted[^1].Price - sorted[^10].Price) / sorted[^10].Price * 100;

        var prices = sorted.Select(d => d.Price).ToArray();
        ind.SupportResistanceIntradayData = CalculateSupportResistance(prices, prices);
    }

    public TechnicalChecklist BuildTechnicalChecklist(TechnicalIndicators ind, string notes)
    {
        var close = ind.LatestClose;
        var nearestSup = ind.SupportResistance.SupportLevels.Where(s => s < close).DefaultIfEmpty(0).Max();
        var nearestRes = ind.SupportResistance.ResistanceLevels.Where(r => r > close).DefaultIfEmpty(0).Min();

        return new TechnicalChecklist
        {
            Symbol = ind.Symbol,
            Context = $"Trend: {ind.Trend}. Gia {close:N2} | SMA20 {ind.SMA20:N2} SMA50 {ind.SMA50:N2} SMA200 {ind.SMA200:N2}. KL x{ind.VolumeRatio:N2} TB20. {ind.LiquidityAssessment}",
            ConfirmInvalidate = $"Xac nhan: {ind.TradingSignal.Action} (quan sat). RSI {ind.RSI:N1}, MACD hist {ind.Histogram:N3}, {ind.Divergence}. Invalidate: mat {nearestSup:N2} hoac RSI dao chieu.",
            Risk = $"ATR% {ind.ATR:N2}. Stop goi y {ind.TradingSignal.StopLoss:N2}. TP {ind.TradingSignal.TakeProfit:N2}. Khang cu {nearestRes:N2}.",
            Verify = string.IsNullOrWhiteSpace(notes)
                ? "Doi chieu lai nen + KL goc VNDirect truoc khi hanh dong."
                : notes,
            Observation = string.Join("; ", ind.Patterns.Take(5).Select(p => p.Name))
        };
    }

    private static decimal Sma(decimal[] values, int period)
    {
        if (values.Length < period) return 0;
        decimal sum = 0;
        for (int i = values.Length - period; i < values.Length; i++)
            sum += values[i];
        return sum / period;
    }

    private static decimal[] SmaSeries(decimal[] values, int period)
    {
        if (values.Length < period) return [];
        var series = new decimal[values.Length - period + 1];
        decimal sum = 0;
        for (int i = 0; i < period; i++) sum += values[i];
        series[0] = sum / period;
        for (int i = period; i < values.Length; i++)
        {
            sum = sum - values[i - period] + values[i];
            series[i - period + 1] = sum / period;
        }
        return series;
    }

    private static decimal[] Ema(decimal[] values, int period)
    {
        var ema = new decimal[values.Length];
        if (values.Length < period) return ema;
        decimal k = 2m / (period + 1);
        decimal sum = 0;
        for (int i = 0; i < period; i++) sum += values[i];
        ema[period - 1] = sum / period;
        for (int i = period; i < values.Length; i++)
            ema[i] = (values[i] - ema[i - 1]) * k + ema[i - 1];
        return ema;
    }

    private static List<decimal> CalculateRsiHistory(decimal[] closes, int period = 14)
    {
        var list = new List<decimal>();
        if (closes.Length <= period) return list;
        for (int i = period; i < closes.Length; i++)
        {
            var slice = closes.Skip(i - period).Take(period + 1).ToArray();
            decimal gains = 0, losses = 0;
            for (int j = 1; j < slice.Length; j++)
            {
                var ch = slice[j] - slice[j - 1];
                if (ch > 0) gains += ch; else losses -= ch;
            }
            if (losses == 0) list.Add(100);
            else
            {
                var rs = (gains / period) / (losses / period);
                list.Add(Math.Round(100 - 100 / (1 + rs), 2));
            }
        }
        return list;
    }

    private static (decimal current, decimal previous) CalculateRsiWithPrevious(decimal[] closes, int period)
    {
        if (closes.Length < period + 2) return (50, 50);
        var changes = new decimal[closes.Length];
        for (int i = 1; i < closes.Length; i++)
            changes[i] = closes[i] - closes[i - 1];
        var gains = changes.Select(c => Math.Max(0, c)).ToArray();
        var losses = changes.Select(c => Math.Max(0, -c)).ToArray();
        decimal avgGain = gains.Skip(1).Take(period).Sum() / period;
        decimal avgLoss = losses.Skip(1).Take(period).Sum() / period;
        var avgGains = new decimal[closes.Length];
        var avgLosses = new decimal[closes.Length];
        avgGains[period] = avgGain;
        avgLosses[period] = avgLoss;
        for (int i = period + 1; i < closes.Length; i++)
        {
            avgGains[i] = (avgGains[i - 1] * (period - 1) + gains[i]) / period;
            avgLosses[i] = (avgLosses[i - 1] * (period - 1) + losses[i]) / period;
        }
        decimal Rsi(decimal g, decimal l) => l == 0 ? 100 : 100 - 100 / (1 + g / l);
        return (Rsi(avgGains[^1], avgLosses[^1]), Rsi(avgGains[^2], avgLosses[^2]));
    }

    private static decimal[] RsiValues(decimal[] closes, int period = 14)
    {
        if (closes.Length < period + 1) return [];
        var rsi = new decimal[closes.Length];
        var gains = new decimal[closes.Length];
        var losses = new decimal[closes.Length];
        for (int i = 1; i < closes.Length; i++)
        {
            var diff = closes[i] - closes[i - 1];
            gains[i] = diff > 0 ? diff : 0;
            losses[i] = diff < 0 ? -diff : 0;
        }
        decimal avgGain = gains.Skip(1).Take(period).Average();
        decimal avgLoss = losses.Skip(1).Take(period).Average();
        int start = period;
        rsi[start] = avgLoss == 0 ? 100 : 100 - 100 / (1 + avgGain / avgLoss);
        for (int i = start + 1; i < closes.Length; i++)
        {
            avgGain = (avgGain * (period - 1) + gains[i]) / period;
            avgLoss = (avgLoss * (period - 1) + losses[i]) / period;
            rsi[i] = avgLoss == 0 ? 100 : 100 - 100 / (1 + avgGain / avgLoss);
        }
        return rsi.Skip(start).ToArray();
    }

    private static MACDResult CalculateMacd(decimal[] closes, int fast, int slow, int signalPeriod)
    {
        if (closes.Length < Math.Max(fast, slow) + signalPeriod - 1)
            return new(0, 0, 0, 0);
        var emaFast = Ema(closes, fast);
        var emaSlow = Ema(closes, slow);
        var macdLine = new decimal[closes.Length];
        int first = Math.Max(fast, slow) - 1;
        for (int i = first; i < closes.Length; i++)
            macdLine[i] = emaFast[i] - emaSlow[i];
        var valid = macdLine.Skip(first).ToArray();
        if (valid.Length < signalPeriod)
            return new(macdLine[^1], macdLine[^1], 0, 0);
        var signalLine = Ema(valid, signalPeriod);
        decimal macd = macdLine[^1];
        decimal signal = signalLine[^1];
        decimal hist = macd - signal;
        decimal prev = signalLine.Length >= 2 ? valid[^2] - signalLine[^2] : 0;
        return new(macd, signal, hist, prev);
    }

    private static BollingerBandsResult CalculateBollinger(decimal[] closes, int period, decimal k)
    {
        if (closes.Length < period) return new(0, 0, 0);
        var middle = Sma(closes, period);
        decimal variance = 0;
        for (int i = closes.Length - period; i < closes.Length; i++)
            variance += (closes[i] - middle) * (closes[i] - middle);
        var std = (decimal)Math.Sqrt((double)(variance / period));
        return new(middle + k * std, middle, middle - k * std);
    }

    private static StochasticResult CalculateStochastic(decimal[] closes, decimal[] highs, decimal[] lows, int kPeriod, int dPeriod)
    {
        if (closes.Length < kPeriod + dPeriod - 1) return new(0, 0);
        var kValues = new decimal[closes.Length];
        for (int i = kPeriod - 1; i < closes.Length; i++)
        {
            var hh = highs.Skip(i - kPeriod + 1).Take(kPeriod).Max();
            var ll = lows.Skip(i - kPeriod + 1).Take(kPeriod).Min();
            kValues[i] = hh == ll ? 50 : (closes[i] - ll) / (hh - ll) * 100;
        }
        var validK = kValues.Skip(kPeriod - 1).ToArray();
        if (validK.Length < dPeriod) return new(kValues[^1], kValues[^1]);
        var dSeries = SmaSeries(validK, dPeriod);
        return new(kValues[^1], dSeries[^1]);
    }

    private static decimal CalculateAtrPercent(List<StockData> data, int period)
    {
        if (data.Count < period + 1) return 0;
        var tr = new List<decimal>();
        for (int i = 1; i < data.Count; i++)
        {
            var hl = Math.Max(0, data[i].High - data[i].Low);
            var hc = Math.Abs(data[i].High - data[i - 1].Close);
            var lc = Math.Abs(data[i].Low - data[i - 1].Close);
            tr.Add(Math.Max(hl, Math.Max(hc, lc)));
        }
        if (tr.Count < period) return 0;
        decimal atr = tr.Take(period).Average();
        for (int i = period; i < tr.Count; i++)
            atr = ((period - 1) * atr + tr[i]) / period;
        var close = data.Last().Close;
        return close == 0 ? 0 : atr / close * 100;
    }

    private static decimal CalculateObv(decimal[] closes, decimal[] volumes)
    {
        decimal obv = 0;
        for (int i = 1; i < closes.Length; i++)
        {
            if (closes[i] > closes[i - 1]) obv += volumes[i];
            else if (closes[i] < closes[i - 1]) obv -= volumes[i];
        }
        return obv;
    }

    private static IchimokuResult CalculateIchimoku(decimal[] closes, decimal[] highs, decimal[] lows, int tenkan, int kijun, int senkouB)
    {
        decimal Mid(int p) => (highs.TakeLast(p).Max() + lows.TakeLast(p).Min()) / 2;
        var t = Mid(tenkan);
        var k = Mid(kijun);
        return new IchimokuResult
        {
            TenkanSen = t,
            KijunSen = k,
            SenkouSpanA = (t + k) / 2,
            SenkouSpanB = Mid(senkouB),
            ChikouSpan = closes.Length >= 26 ? closes[closes.Length - 26] : closes.Last()
        };
    }

    private static SupportResistanceResult CalculateSupportResistance(decimal[] highs, decimal[] lows, int window = 11)
    {
        var supports = new List<decimal>();
        var resistances = new List<decimal>();
        int half = window / 2;
        for (int i = half; i < highs.Length - half; i++)
        {
            var wh = highs.Skip(i - half).Take(window).Max();
            var wl = lows.Skip(i - half).Take(window).Min();
            if (highs[i] == wh && !resistances.Any(r => r != 0 && Math.Abs(r - highs[i]) / r < 0.005m))
                resistances.Add(highs[i]);
            if (lows[i] == wl && !supports.Any(s => s != 0 && Math.Abs(s - lows[i]) / s < 0.005m))
                supports.Add(lows[i]);
        }
        return new SupportResistanceResult
        {
            SupportLevels = FilterClose(supports, 0.01m).OrderBy(x => x).ToList(),
            ResistanceLevels = FilterClose(resistances, 0.01m).OrderBy(x => x).ToList()
        };
    }

    private static List<decimal> FilterClose(List<decimal> levels, decimal threshold)
    {
        var filtered = new List<decimal>();
        foreach (var lv in levels.OrderBy(x => x))
        {
            if (filtered.Count == 0 || Math.Abs(lv - filtered.Last()) / filtered.Last() > threshold)
                filtered.Add(lv);
        }
        return filtered;
    }

    private static VolumeAnalysis AnalyzeVolume20(decimal[] volumes)
    {
        if (volumes.Length < 20) return new(0, 1);
        decimal avg = volumes.TakeLast(20).Average();
        return new(avg, avg == 0 ? 1 : volumes[^1] / avg);
    }

    private static string AnalyzeLiquidity(decimal latest, decimal avg20, decimal avg50)
    {
        if (avg20 <= 0 && avg50 <= 0) return "Khong du du lieu khoi luong.";
        if (avg20 > 0 && avg20 < 50_000) return "Thanh khoan rat thap (TB20 < 50k).";
        var shortR = avg20 > 0 ? latest / avg20 : 1;
        var longR = avg20 > 0 && avg50 > 0 ? avg20 / avg50 : 1;
        if (longR < 0.7m && shortR < 0.7m) return "Thanh khoan giam dan va thap phien gan nhat.";
        if (longR < 0.7m) return "Thanh khoan giam dan (TB20 < 70% TB50).";
        if (shortR < 0.7m) return "Thanh khoan phien gan nhat thap hon 70% TB20.";
        return "Thanh khoan binh thuong.";
    }

    private static string DetermineTrend(decimal[] closes, decimal sma20, decimal sma50, decimal sma200,
        decimal latestVolume, decimal volumeAverage, decimal rsi, decimal atr,
        decimal bbUpper, decimal bbLower, decimal bbMiddle)
    {
        if (closes.Length < 20 || volumeAverage <= 0) return "Khong du du lieu";
        var latest = closes.Last();
        var prev20 = closes.TakeLast(21).Take(20).Average();
        var prev50 = closes.Length >= 51 ? closes.TakeLast(51).Take(50).Average() : 0;
        var prev200 = closes.Length >= 201 ? closes.TakeLast(201).Take(200).Average() : 0;
        bool volOk = latestVolume > volumeAverage * 1.3m;
        bool rsiUp = rsi > 55, rsiDn = rsi < 45;
        bool sideways = atr / latest < 0.02m || (bbMiddle != 0 && (bbUpper - bbLower) / bbMiddle < 0.04m);
        bool sma20Up = sma20 > prev20;

        if (closes.Length >= 200 && prev200 > 0)
        {
            if (latest > sma200 && sma50 > sma200)
            {
                if (sma20 > sma50 && sma20Up && sma50 > prev50 && volOk && rsiUp && !sideways) return "Tang manh dai han";
                if (sma20 > sma50 || (volOk && rsiUp && !sideways)) return "Tang dai han";
            }
            if (latest < sma200 && sma50 < sma200)
            {
                if (sma20 < sma50 && !sma20Up && sma50 < prev50 && volOk && rsiDn && !sideways) return "Giam manh dai han";
                if (sma20 < sma50 || (volOk && rsiDn && !sideways)) return "Giam dai han";
            }
        }
        if (closes.Length >= 50 && prev50 > 0)
        {
            if (latest > sma50)
            {
                if (sma20 > sma50 && sma20Up && sma50 > prev50 && volOk && rsiUp && !sideways) return "Tang manh trung han";
                if ((sma20 > sma50 && sma20Up) || (volOk && rsiUp && !sideways)) return "Tang trung han";
            }
            if (latest < sma50)
            {
                if (sma20 < sma50 && !sma20Up && sma50 < prev50 && volOk && rsiDn && !sideways) return "Giam manh trung han";
                if ((sma20 < sma50 && !sma20Up) || (volOk && rsiDn && !sideways)) return "Giam trung han";
            }
        }
        if (latest > sma20)
        {
            if (sma20Up && volOk && rsiUp && !sideways) return "Tang manh ngan han";
            if (sma20Up || volOk) return "Tang ngan han";
        }
        if (latest < sma20)
        {
            if (!sma20Up && volOk && rsiDn && !sideways) return "Giam manh ngan han";
            if (!sma20Up || volOk) return "Giam ngan han";
        }
        if (sma20 != 0 && Math.Abs(latest - sma20) / sma20 < 0.01m) return "Di ngang chat";
        return "Di ngang";
    }

    private static TradingSignal GenerateTradingSignals(decimal rsi, MACDResult macd, StochasticResult stoch,
        string trend, VolumeAnalysis volume, decimal atr, SupportResistanceResult sr, decimal latest)
    {
        var signal = new TradingSignal { Action = "Hold", Recommendation = RecommendationAction.Hold };
        if (latest <= 0 || atr <= 0) return signal;

        bool bullX = macd.MacdLine > macd.SignalLine && macd.Histogram > 0;
        bool bearX = macd.MacdLine < macd.SignalLine && macd.Histogram < 0;
        bool buy = rsi < 45 && bullX && stoch.k < 35 && stoch.k > stoch.d && volume.Ratio > 1.1m && (trend.Contains("Tang") || trend.Contains("ngang"));
        bool sell = rsi > 55 && bearX && stoch.k > 65 && stoch.k < stoch.d && volume.Ratio > 1.1m && (trend.Contains("Giam") || trend.Contains("ngang"));
        var supports = sr.SupportLevels.OrderByDescending(s => s).ToArray();
        var resistances = sr.ResistanceLevels.OrderBy(r => r).ToArray();
        var atrAbs = atr / 100 * latest;

        if (buy)
        {
            signal.Action = "Buy";
            signal.Recommendation = RecommendationAction.Buy;
            signal.EntryPrice = latest;
            var res = resistances.FirstOrDefault(r => r > latest);
            signal.TakeProfit = res > 0 ? Math.Min(res, latest * 1.07m) : latest + 1.5m * atrAbs;
            var sup = supports.FirstOrDefault(s => s < latest);
            signal.StopLoss = sup > 0 ? Math.Max(sup, latest * 0.95m) : latest - 1.5m * atrAbs;
            int score = 0;
            if (rsi < 40) score++;
            if (volume.Ratio > 1.3m) score++;
            if (macd.MacdLine - macd.SignalLine > 0.2m) score++;
            if (stoch.k < 25) score++;
            if (trend.Contains("manh")) score++;
            if (score >= 3) { signal.Action = "StrongBuy"; signal.Recommendation = RecommendationAction.StrongBuy; }
            signal.Rationale = "Quan sat: RSI thap + MACD cat len + volume. Khong phai lenh.";
        }
        if (sell)
        {
            signal.Action = "Sell";
            signal.Recommendation = RecommendationAction.Sell;
            signal.EntryPrice = latest;
            var sup = supports.FirstOrDefault(s => s < latest);
            signal.TakeProfit = sup > 0 ? Math.Max(sup, latest * 0.93m) : latest - 1.5m * atrAbs;
            var res = resistances.FirstOrDefault(r => r > latest);
            signal.StopLoss = res > 0 ? Math.Min(res, latest * 1.05m) : latest + 1.5m * atrAbs;
            int score = 0;
            if (rsi > 60) score++;
            if (volume.Ratio > 1.3m) score++;
            if (macd.SignalLine - macd.MacdLine > 0.2m) score++;
            if (stoch.k > 75) score++;
            if (trend.Contains("manh")) score++;
            if (score >= 3) { signal.Action = "StrongSell"; signal.Recommendation = RecommendationAction.StrongSell; }
            signal.Rationale = "Quan sat: RSI cao + MACD cat xuong + volume. Khong phai lenh.";
        }
        return signal;
    }

    private static decimal[] MacdLineValues(decimal[] closes, int shortP = 12, int longP = 26)
    {
        if (closes.Length < longP) return [];
        var ema12 = Ema(closes, shortP);
        var ema26 = Ema(closes, longP);
        var macd = new decimal[closes.Length];
        for (int i = 0; i < closes.Length; i++)
            macd[i] = ema12[i] - ema26[i];
        return macd.Skip(longP - 1).ToArray();
    }

    private static string DetectDivergence(decimal[] closes, decimal[] volumes, int lookback)
    {
        if (closes.Length < lookback || lookback < 5) return "Khong du du lieu";
        var rsi = RsiValues(closes);
        var macd = MacdLineValues(closes);
        var recentC = closes.TakeLast(lookback).ToArray();
        var recentR = rsi.TakeLast(lookback).ToArray();
        var recentM = macd.TakeLast(lookback).ToArray();
        if (recentR.Length < lookback || recentM.Length < lookback) return "Khong du du lieu RSI/MACD";
        var highs = new List<(int i, decimal p, decimal r, decimal m)>();
        var lows = new List<(int i, decimal p, decimal r, decimal m)>();
        for (int i = 1; i < recentC.Length - 1; i++)
        {
            if (recentC[i] > recentC[i - 1] && recentC[i] > recentC[i + 1])
                highs.Add((i, recentC[i], recentR[i], recentM[i]));
            if (recentC[i] < recentC[i - 1] && recentC[i] < recentC[i + 1])
                lows.Add((i, recentC[i], recentR[i], recentM[i]));
        }
        if (highs.Count >= 2)
        {
            var last = highs[^1]; var prev = highs[^2];
            if (last.p > prev.p && (last.r < prev.r || last.m < prev.m))
                return "Bearish Divergence";
        }
        if (lows.Count >= 2)
        {
            var last = lows[^1]; var prev = lows[^2];
            if (last.p < prev.p && (last.r > prev.r || last.m > prev.m))
                return "Bullish Divergence";
        }
        return "Khong co phan ky";
    }

    private static List<string> DetectChartPatterns(List<StockData> data, decimal tolerance = 0.03m)
    {
        var patterns = new List<string>();
        if (data.Count < 20) return ["Khong du du lieu"];
        var closes = data.Select(d => d.Close).ToArray();
        var highs = data.Select(d => d.High).ToArray();
        var lows = data.Select(d => d.Low).ToArray();
        var vols = data.Select(d => d.Volume).ToArray();
        var avgVol = vols.TakeLast(20).Average();
        if (avgVol < 100_000) return ["Thanh khoan thap, loai tin hieu"];

        var recentLows = lows.TakeLast(20).ToArray();
        var lowIdx = recentLows.Select((v, i) => new { v, i }).OrderBy(x => x.v).Take(2).OrderBy(x => x.i).ToArray();
        if (lowIdx.Length == 2 && Math.Abs(lowIdx[0].v - lowIdx[1].v) / lowIdx[0].v <= tolerance && lowIdx[1].i - lowIdx[0].i >= 3)
            patterns.Add("Double Bottom (quan sat)");

        var recentHighs = highs.TakeLast(20).ToArray();
        var highIdx = recentHighs.Select((v, i) => new { v, i }).OrderByDescending(x => x.v).Take(2).OrderBy(x => x.i).ToArray();
        if (highIdx.Length == 2 && Math.Abs(highIdx[0].v - highIdx[1].v) / highIdx[0].v <= tolerance && highIdx[1].i - highIdx[0].i >= 3)
            patterns.Add("Double Top (quan sat)");

        return patterns.Count > 0 ? patterns : ["Khong phat hien mau hinh"];
    }
}

public class TechnicalChecklist
{
    public string Symbol { get; set; } = "";
    public string Context { get; set; } = "";
    public string ConfirmInvalidate { get; set; } = "";
    public string Risk { get; set; } = "";
    public string Verify { get; set; } = "";
    public string Observation { get; set; } = "";
}
