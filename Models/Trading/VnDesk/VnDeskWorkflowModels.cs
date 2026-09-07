namespace BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

public class WatchlistState
{
    public List<string> Criteria { get; set; } =
    [
        "Thanh khoan: KL >= TB20 * 0.8",
        "Trend: gia > SMA20 hoac SMA20 > SMA50",
        "RSI vung 40-65 (khong qua mua cuc doan)",
        "Gan ho tro: cach day 20 phien <= 8%",
        "Volume: KL >= TB20 * 1.2"
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

public class NewsBrief
{
    public string Facts { get; set; } = "";
    public string Sources { get; set; } = "";
    public string Implications { get; set; } = "";
    public bool Verified { get; set; }
    public string Raw { get; set; } = "";
}