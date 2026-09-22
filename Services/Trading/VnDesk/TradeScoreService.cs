using BlazorWasmPortfolioGhAction.Models.Trading.Broker;
using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public sealed class TradeScoreService
{
    public TradeScoreResult Score(
        TechnicalIndicators ta,
        FundamentalSnapshot? fundamentals,
        MarketContext? context)
    {
        var result = new TradeScoreResult
        {
            Symbol = ta.Symbol,
            AnalysisDate = ta.Date,
            Price = ta.LatestClose,
            Fundamentals = fundamentals,
            Context = context,
            FundamentalsPartial = fundamentals?.IsPartial ?? true
        };

        ScoreTechnical(ta, result.Technical);
        ScoreFundamental(fundamentals, result.Fundamental);
        ScoreMarket(context, result.Market);
        ScoreRisk(ta, result.Risk);

        result.TechnicalScore = result.Technical.Score;
        result.FundamentalScore = result.Fundamental.Score;
        result.MarketScore = result.Market.Score;
        result.RiskScore = result.Risk.Score;
        result.TotalScore = result.TechnicalScore + result.FundamentalScore + result.MarketScore + result.RiskScore;
        result.Recommendation = MapRecommendation(result.TotalScore);
        result.Plan = BuildPlan(ta);

        return result;
    }

    public void ApplyTcbsBias(TradeScoreResult result, decimal? buySellRatio, decimal? foreignNet)
    {
        var notes = result.Market.Notes;
        var bonus = 0;
        if (buySellRatio is > 1.1m) bonus += 1;
        if (foreignNet is > 0) bonus += 1;
        if (buySellRatio is < 0.9m) bonus -= 1;
        if (bonus == 0 && buySellRatio is null && foreignNet is null) return;
        notes.Add($"TCBS cung cầu {buySellRatio?.ToString("N2") ?? "—"}, NN ròng {foreignNet?.ToString("N0") ?? "—"}: {bonus:+#;-#;0}");
        result.Market.Score = Math.Clamp(result.Market.Score + bonus, 0, result.Market.MaxScore);
        result.MarketScore = result.Market.Score;
        result.TotalScore = result.TechnicalScore + result.FundamentalScore + result.MarketScore + result.RiskScore;
        result.Recommendation = MapRecommendation(result.TotalScore);
    }

    private static void ScoreTechnical(TechnicalIndicators ta, TradeScoreBreakdown bd)
    {
        var notes = bd.Notes;
        int score = 0;

        var trendPts = ta.Trend switch
        {
            TrendDirection.StrongUpLong or TrendDirection.StrongUpMid or TrendDirection.StrongUpShort => 10,
            TrendDirection.UpLong or TrendDirection.UpMid or TrendDirection.UpShort => 8,
            TrendDirection.Sideways => 5,
            TrendDirection.DownLong or TrendDirection.DownMid or TrendDirection.DownShort => 2,
            TrendDirection.StrongDownLong or TrendDirection.StrongDownMid or TrendDirection.StrongDownShort => 0,
            _ => 3
        };
        score += trendPts;
        notes.Add($"Trend: {trendPts}/10");

        int maPts = 0;
        if (ta.LatestClose > ta.SMA20) maPts += 3;
        if (ta.SMA20 > ta.SMA50) maPts += 3;
        if (ta.SMA50 > ta.SMA200) maPts += 2;
        score += maPts;
        notes.Add($"MA stack: {maPts}/8");

        int rsiPts = ta.RSI switch
        {
            >= 45 and <= 70 => 6,
            >= 40 and < 45 or > 70 and <= 75 => 4,
            >= 35 and < 40 or > 75 and <= 80 => 2,
            _ => 1
        };
        score += rsiPts;
        notes.Add($"RSI {ta.RSI:F1}: {rsiPts}/6");

        int macdPts = ta.Histogram > 0 ? 5 : 0;
        score += macdPts;
        notes.Add($"MACD hist: {macdPts}/5");

        int volPts = ta.VolumeRatio switch
        {
            >= 2.0m => 6,
            >= 1.5m => 5,
            >= 1.2m => 4,
            >= 1.0m => 2,
            _ => 0
        };
        score += volPts;
        notes.Add($"Vol xTB {ta.VolumeRatio:F1}: {volPts}/6");

        bd.Score = Math.Min(score, bd.MaxScore);
    }

    private static void ScoreFundamental(FundamentalSnapshot? f, TradeScoreBreakdown bd)
    {
        if (f is null)
        {
            bd.Notes.Add("Không có dữ liệu cơ bản");
            return;
        }

        int score = 0;

        if (f.Pe is decimal pe)
        {
            var pts = pe switch { < 10 => 10, < 15 => 8, < 20 => 6, < 25 => 4, _ => 2 };
            score += pts;
            bd.Notes.Add($"P/E {pe:F1}: {pts}/10");
        }

        if (f.Pb is decimal pb)
        {
            var pts = pb switch { < 1.5m => 7, < 2.5m => 5, < 3.5m => 3, _ => 1 };
            score += pts;
            bd.Notes.Add($"P/B {pb:F2}: {pts}/7");
        }

        if (f.Roe is decimal roe)
        {
            var roePct = roe <= 1 ? roe * 100 : roe;
            var pts = roePct switch { >= 20 => 10, >= 15 => 8, >= 10 => 6, >= 5 => 3, _ => 1 };
            score += pts;
            bd.Notes.Add($"ROE {roePct:F1}%: {pts}/10");
        }

        if (f.ProfitGrowthYoY is decimal growth)
        {
            var gPct = growth <= 1 && growth >= -1 ? growth * 100 : growth;
            var pts = gPct switch { >= 20 => 8, >= 10 => 6, >= 0 => 4, _ => 1 };
            score += pts;
            bd.Notes.Add($"LNST YoY {gPct:F1}%: {pts}/8");
        }

        bd.Score = Math.Min(score, bd.MaxScore);
    }

    private static void ScoreMarket(MarketContext? ctx, TradeScoreBreakdown bd)
    {
        if (ctx is null)
        {
            bd.Notes.Add("Không có dữ liệu thị trường");
            return;
        }

        int trendPts = ctx.VnIndexTrend switch
        {
            TrendDirection.StrongUpLong or TrendDirection.StrongUpMid or TrendDirection.StrongUpShort => 10,
            TrendDirection.UpLong or TrendDirection.UpMid or TrendDirection.UpShort => 8,
            TrendDirection.Sideways => 5,
            TrendDirection.DownLong or TrendDirection.DownMid or TrendDirection.DownShort => 2,
            _ => 3
        };
        bd.Notes.Add($"VN-Index trend: {trendPts}/10");

        var rs = ctx.SectorRelativeStrength;
        int rsPts = rs switch
        {
            >= 5 => 10,
            >= 3 => 8,
            >= 1 => 6,
            >= 0 => 5,
            >= -2 => 3,
            _ => 1
        };
        var sectorLabel = ctx.SectorName ?? "—";
        bd.Notes.Add($"Ngành {sectorLabel} RS {rs:+#.#;-#.#;0}%: {rsPts}/10");

        bd.Score = Math.Min(trendPts + rsPts, bd.MaxScore);
    }

    private static void ScoreRisk(TechnicalIndicators ta, TradeScoreBreakdown bd)
    {
        int score = 0;

        int atrPts = ta.ATR switch
        {
            < 2 => 4,
            < 4 => 3,
            < 6 => 2,
            _ => 1
        };
        score += atrPts;
        bd.Notes.Add($"ATR% {ta.ATR:F1}: {atrPts}/4");

        int liqPts = ta.VolumeAverage20 switch
        {
            >= 500_000 => 3,
            >= 100_000 => 2,
            >= 50_000 => 1,
            _ => 0
        };
        score += liqPts;
        bd.Notes.Add($"Thanh khoản TB20: {liqPts}/3");

        var sl = ta.TradingSignal.StopLoss ?? ta.LatestLow20;
        var stopPct = ta.LatestClose > 0 ? (ta.LatestClose - sl) / ta.LatestClose * 100m : 0m;
        int stopPts = stopPct switch
        {
            <= 5 and > 0 => 3,
            <= 8 => 2,
            <= 12 => 1,
            _ => 0
        };
        score += stopPts;
        bd.Notes.Add($"Khoảng cách SL {stopPct:F1}%: {stopPts}/3");

        bd.Score = Math.Min(score, bd.MaxScore);
    }

    private static TradePlan BuildPlan(TechnicalIndicators ta)
    {
        var signal = ta.TradingSignal;
        var supports = ta.SupportResistance.SupportLevels.OrderByDescending(s => s).ToArray();
        var resistances = ta.SupportResistance.ResistanceLevels.OrderBy(r => r).ToArray();

        var support = supports.FirstOrDefault(s => s < ta.LatestClose);
        if (support <= 0) support = ta.LatestLow20;

        var resistance = resistances.FirstOrDefault(r => r > ta.LatestClose);
        if (resistance <= 0) resistance = ta.LatestHigh20;

        var stopLoss = signal.StopLoss ?? Math.Max(support, ta.LatestClose * 0.93m);
        var target = signal.TakeProfit ?? resistance;

        var buyLow = Math.Max(support, ta.LatestClose * 0.97m);
        var buyHigh = ta.LatestClose;

        var risk = ta.LatestClose - stopLoss;
        var rr = risk > 0 ? Math.Round((target - ta.LatestClose) / risk, 2) : 0m;
        var stopPct = ta.LatestClose > 0 ? Math.Round((ta.LatestClose - stopLoss) / ta.LatestClose * 100m, 1) : 0m;
        var targetPct = ta.LatestClose > 0 ? Math.Round((target - ta.LatestClose) / ta.LatestClose * 100m, 1) : 0m;

        return new TradePlan
        {
            BuyZoneLow = buyLow,
            BuyZoneHigh = buyHigh,
            StopLoss = stopLoss,
            Target = target,
            RiskRewardRatio = rr > 0 ? rr : signal.RiskRewardRatio1,
            StopLossPct = stopPct,
            TargetPct = targetPct
        };
    }

    private static TradeScoreRecommendation MapRecommendation(int total) => total switch
    {
        >= 75 => TradeScoreRecommendation.StrongBuy,
        >= 60 => TradeScoreRecommendation.Buy,
        >= 45 => TradeScoreRecommendation.Watch,
        _ => TradeScoreRecommendation.Avoid
    };

    public static decimal PriceToVnd(decimal priceInThousands) =>
        priceInThousands * BrokerMoney.PriceUnitVnd;
}
