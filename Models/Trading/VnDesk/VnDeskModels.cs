namespace BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

public class CandlestickPattern
{
    public string Name { get; }
    public string Description { get; }
    public PatternType Type { get; }
    public Direction Direction { get; }
    public decimal Reliability { get; }

    public CandlestickPattern(string name, string description, PatternType type, Direction direction = Direction.Neutral)
    {
        Name = name;
        Description = description;
        Type = type;
        Direction = direction;
    }

    public override string ToString() => Description;
}

public enum PatternType { Reversal, Continuation, Indecision, Doji, Support, Resistance }
public enum Direction { Bullish, Bearish, Neutral }

public class TrendInfo
{
    public bool IsUptrend { get; set; }
    public bool IsDowntrend { get; set; }
    public bool IsShortTermUptrend { get; set; }
    public bool IsShortTermDowntrend { get; set; }
}

public class StockData
{
    public string Symbol { get; set; } = "";
    public DateTime Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public decimal Change { get; set; }
    public decimal PercentChange { get; set; }

    public decimal BodySize => Math.Abs(Open - Close);
    public decimal Range => High - Low;
    public bool IsBullish => Close > Open;
    public bool IsBearish => Close < Open;
    public decimal BodyMidPoint => (Open + Close) / 2;
    public decimal LowerWickSize => IsBullish ? Open - Low : Close - Low;
    public decimal UpperWickSize => IsBullish ? High - Close : High - Open;
}

public class TradingSignal
{
    public string? Symbol { get; set; }
    public DateTime SignalTime { get; set; }
    public decimal? EntryPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit1 { get; set; }
    public decimal? TakeProfit2 { get; set; }
    public RecommendationAction Recommendation { get; set; }
    public string? Rationale { get; set; }
    public decimal RiskRewardRatio1 { get; set; }
    public decimal RiskRewardRatio2 { get; set; }
    public string? Action { get; set; }
    public decimal? TakeProfit { get; set; }
}

public enum RecommendationAction
{
    None, StrongBuy, Buy, Hold, Sell, Avoid, StrongSell
}

public class SectorAnalysis
{
    public string? Name { get; set; }
    public int StockCount { get; set; }
    public int UpCount { get; set; }
    public int DownCount { get; set; }
    public int UnchangedCount { get; set; }
    public decimal AverageChange { get; set; }
    public decimal TotalVolume { get; set; }
    public StockData? TopGainer { get; set; }
    public StockData? TopLoser { get; set; }
    public StockData? TopVolume { get; set; }
}

public record VolumeAnalysis(decimal Average, decimal Ratio);
public record MACDResult(decimal MacdLine, decimal SignalLine, decimal Histogram, decimal PreviousHistogram);
public record BollingerBandsResult(decimal Upper, decimal Middle, decimal Lower);
public record StochasticResult(decimal k, decimal d);

public class IchimokuResult
{
    public decimal TenkanSen { get; set; }
    public decimal KijunSen { get; set; }
    public decimal SenkouSpanA { get; set; }
    public decimal SenkouSpanB { get; set; }
    public decimal ChikouSpan { get; set; }
}

public class SupportResistanceResult
{
    public List<decimal> SupportLevels { get; set; } = [];
    public List<decimal> ResistanceLevels { get; set; } = [];
}

public class IntradayData
{
    public DateTime Time { get; set; }
    public decimal Price { get; set; }
    public decimal Volume { get; set; }
    public decimal AccumulatedVal { get; set; }
    public decimal AccumulatedVol { get; set; }
    public string MatchType { get; set; } = "";
}

public class TechnicalIndicators
{
    public string Symbol { get; set; } = "";
    public DateTime Date { get; set; }
    public decimal RSI { get; set; }
    public decimal SMA20 { get; set; }
    public decimal SMA50 { get; set; }
    public decimal SMA200 { get; set; }
    public decimal MACD { get; set; }
    public decimal Signal { get; set; }
    public decimal Histogram { get; set; }
    public decimal BollingerUpper { get; set; }
    public decimal BollingerMiddle { get; set; }
    public decimal BollingerLower { get; set; }
    public decimal K_Stochastic { get; set; }
    public decimal D_Stochastic { get; set; }
    public decimal ATR { get; set; }
    public decimal OBV { get; set; }
    public IchimokuResult Ichimoku { get; set; } = new();
    public decimal VolumeAverage20 { get; set; }
    public decimal VolumeAverage50 { get; set; }
    public decimal VolumeRatio { get; set; }
    public decimal VolumeRatioIntradayData { get; set; }
    public string Trend { get; set; } = "";
    public List<CandlestickPattern> Patterns { get; set; } = [];
    public List<string> ChartPatterns { get; set; } = [];
    public string LiquidityAssessment { get; set; } = "";
    public string? Divergence { get; set; }
    public SupportResistanceResult SupportResistance { get; set; } = new();
    public SupportResistanceResult SupportResistanceIntradayData { get; set; } = new();
    public TradingSignal TradingSignal { get; set; } = new();
    public string Summary { get; set; } = "";
    public decimal LatestClose { get; set; }
    public decimal LatestHigh { get; set; }
    public decimal LatestLow { get; set; }
    public decimal LatestHigh5 { get; set; }
    public decimal LatestLow5 { get; set; }
    public decimal LatestHigh20 { get; set; }
    public decimal LatestLow20 { get; set; }
    public decimal LatestVolume { get; set; }
    public decimal LatestIntradayDataClose { get; set; }
    public decimal LatestIntradayDataHigh { get; set; }
    public decimal LatestIntradayDataLow { get; set; }
    public decimal LatestIntradayDataVolume { get; set; }
    public List<IntradayData> IntradayData { get; set; } = [];
    public decimal PreviousHigh { get; set; }
    public decimal PreviousLow { get; set; }
    public decimal PreviousClose { get; set; }
    public decimal PreviousRSI { get; set; }
    public decimal PreviousHistogram { get; set; }
    public List<decimal> RsiHistory { get; set; } = [];
    public List<decimal> Last3Rsi { get; set; } = [];
    public decimal IntradayMomentum { get; set; }
    public string? IntradayNote { get; set; }
}

public class MarketIndex
{
    public string Change { get; set; } = "";
    public string Index { get; set; } = "";
    public string Name { get; set; } = "";
    public string Percent { get; set; } = "";
    public string Volume { get; set; } = "";
    public string Value { get; set; } = "";

    public double ParsedIndex => Parse(Index);
    public double ParsedChange => Parse(Change);
    public double ParsedPercent => Parse(Percent);
    public long ParsedVolume => ParseLong(Volume);
    public double ParsedValue => Parse(Value);

    private static double Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return 0;
        return double.TryParse(input.Replace(",", ""), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : 0;
    }

    private static long ParseLong(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return 0;
        return long.TryParse(input.Replace(",", ""), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var r) ? r : 0;
    }
}

public class MarketSnapshot
{
    public List<MarketIndex> Indices { get; set; } = [];
    public List<StockData> Stocks { get; set; } = [];
    public DateTime LoadedAt { get; set; }
}

public class PotentialStock
{
    public string Symbol { get; set; } = "";
    public decimal LastPrice { get; set; }
    public decimal PriceChange { get; set; }
    public decimal Volume { get; set; }
    public double PotentialScore { get; set; }
    public string Reason { get; set; } = "";
}

public class PositionResult
{
    public string Symbol { get; set; } = "";
    public string Broker { get; set; } = "";
    public decimal FeeRatePct { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public int Shares { get; set; }
    public decimal BuyValue { get; set; }
    public decimal BuyFee { get; set; }
    public decimal TotalCost { get; set; }
    public decimal SellValue { get; set; }
    public decimal SellFee { get; set; }
    public decimal SellTax { get; set; }
    public decimal NetSellValue { get; set; }
    public decimal ProfitLoss { get; set; }
    public decimal ProfitLossPercent { get; set; }
    public string Note { get; set; } = "";
}

public class SizeResult
{
    public decimal Capital { get; set; }
    public decimal RiskPct { get; set; }
    public decimal Price { get; set; }
    public decimal Stop { get; set; }
    public decimal RiskAmount { get; set; }
    public decimal StopDistance { get; set; }
    public int Shares { get; set; }
    public decimal PositionValue { get; set; }
    public decimal Reward { get; set; }
    public decimal RiskReward { get; set; }
    public bool NoTrade { get; set; }
    public string Reason { get; set; } = "";
}

public class VnDirectResponse
{
    public List<VnDirectStockData> data { get; set; } = [];
}

public class VnDirectStockData
{
    public string code { get; set; } = "";
    public DateTime date { get; set; }
    public decimal open { get; set; }
    public decimal high { get; set; }
    public decimal low { get; set; }
    public decimal close { get; set; }
    public decimal nmVolume { get; set; }
    public decimal change { get; set; }
    public decimal pctChange { get; set; }
}
