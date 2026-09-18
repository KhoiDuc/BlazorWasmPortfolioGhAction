namespace BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

public class FundamentalSnapshot
{
    public string Symbol { get; set; } = "";
    public decimal? Pe { get; set; }
    public decimal? Pb { get; set; }
    public decimal? Roe { get; set; }
    public decimal? Roa { get; set; }
    public decimal? ProfitGrowthYoY { get; set; }
    public decimal? PretaxGrowthYoY { get; set; }
    public decimal? Eps { get; set; }
    public decimal? Bvps { get; set; }
    public decimal? MarketCap { get; set; }
    public decimal? DividendYield { get; set; }
    public DateTime? ReportDate { get; set; }
    public bool IsPartial { get; set; }
}

public class MarketContext
{
    public string? SectorName { get; set; }
    public TrendDirection VnIndexTrend { get; set; } = TrendDirection.InsufficientData;
    public decimal VnIndexChange20d { get; set; }
    public decimal SectorRelativeStrength { get; set; }
    public decimal? VnIndexSupport { get; set; }
    public decimal? VnIndexResistance { get; set; }
}

public enum TradeScoreRecommendation
{
    Avoid,
    Watch,
    Buy,
    StrongBuy
}

public class TradeScoreBreakdown
{
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public List<string> Notes { get; set; } = [];
}

public class TradePlan
{
    public decimal BuyZoneLow { get; set; }
    public decimal BuyZoneHigh { get; set; }
    public decimal StopLoss { get; set; }
    public decimal Target { get; set; }
    public decimal RiskRewardRatio { get; set; }
    public decimal StopLossPct { get; set; }
    public decimal TargetPct { get; set; }
}

public class TradeScoreResult
{
    public string Symbol { get; set; } = "";
    public DateTime AnalysisDate { get; set; }
    public decimal Price { get; set; }
    public int TotalScore { get; set; }
    public int TechnicalScore { get; set; }
    public int FundamentalScore { get; set; }
    public int MarketScore { get; set; }
    public int RiskScore { get; set; }
    public TradeScoreRecommendation Recommendation { get; set; }
    public bool FundamentalsPartial { get; set; }
    public TradeScoreBreakdown Technical { get; set; } = new() { MaxScore = 35 };
    public TradeScoreBreakdown Fundamental { get; set; } = new() { MaxScore = 35 };
    public TradeScoreBreakdown Market { get; set; } = new() { MaxScore = 20 };
    public TradeScoreBreakdown Risk { get; set; } = new() { MaxScore = 10 };
    public TradePlan Plan { get; set; } = new();
    public FundamentalSnapshot? Fundamentals { get; set; }
    public MarketContext? Context { get; set; }
}

public class VnDirectRatioData
{
    public string ratioCode { get; set; } = "";
    public decimal value { get; set; }
    public DateTime reportDate { get; set; }
}

public class VnDirectRatioResponse
{
    public List<VnDirectRatioData> data { get; set; } = [];
}

public class VnMarketPriceData
{
    public string code { get; set; } = "";
    public DateTime date { get; set; }
    public decimal open { get; set; }
    public decimal high { get; set; }
    public decimal low { get; set; }
    public decimal close { get; set; }
    public decimal change { get; set; }
    public decimal pctChange { get; set; }
    public decimal nmVolume { get; set; }
    public decimal accumulatedVol { get; set; }
}

public class VnMarketPriceResponse
{
    public List<VnMarketPriceData> data { get; set; } = [];
}
