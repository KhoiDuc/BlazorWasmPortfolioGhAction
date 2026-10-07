namespace BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

public class WatchlistState
{
    public List<string> Criteria { get; set; } =
    [
        "Trading_Criteria_Liquidity",
        "Trading_Criteria_Trend",
        "Trading_Criteria_Rsi",
        "Trading_Criteria_NearSupport",
        "Trading_Criteria_Volume"
    ];
    public List<WatchlistScore> LastScores { get; set; } = [];
    public DateTime UpdatedAt { get; set; }
}

public class WatchlistScore
{
    public string Symbol { get; set; } = "";
    public int Liquidity { get; set; }
    public int Trend { get; set; }
    public int Rsi { get; set; }
    public int NearSupport { get; set; }
    public int Volume { get; set; }
    public int Total => Liquidity + Trend + Rsi + NearSupport + Volume;
    public bool Pass => Total >= 12;
    public string Note { get; set; } = "";
}

public enum RsiZone
{
    Neutral,
    OverboughtStrong,
    OverboughtMild,
    OversoldMild,
    OversoldStrong
}

public class RsiScanRow
{
    public string Symbol { get; set; } = "";
    public decimal Rsi { get; set; }
    public decimal PreviousRsi { get; set; }
    public RsiZone Zone { get; set; }
    public decimal LastPrice { get; set; }
    public decimal PriceChange { get; set; }
    public TrendDirection Trend { get; set; }
    public string TrendLabel { get; set; } = "";
    public string Signal { get; set; } = "";
    public decimal VolumeRatio { get; set; }
}

public class NewsBrief
{
    public string Facts { get; set; } = "";
    public string Sources { get; set; } = "";
    public string Implications { get; set; } = "";
    public bool Verified { get; set; }
    public string Raw { get; set; } = "";
}