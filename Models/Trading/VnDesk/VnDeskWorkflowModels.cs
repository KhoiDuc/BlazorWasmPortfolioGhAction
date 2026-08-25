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

public class PreTradeChecklist
{
    public string Symbol { get; set; } = "";
    public string Thesis { get; set; } = "";
    public string SourceData { get; set; } = "";
    public string Invalidation { get; set; } = "";
    public string MaxLoss { get; set; } = "";
    public string PositionSizeLogic { get; set; } = "";
    public string StopConditions { get; set; } = "";
    public string ScenarioChange { get; set; } = "";
    public string WithoutPressure { get; set; } = "";
    public bool Confirmed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int FilledCount => new[] { Thesis, SourceData, Invalidation, MaxLoss, PositionSizeLogic, StopConditions, ScenarioChange, WithoutPressure }
        .Count(x => !string.IsNullOrWhiteSpace(x));
    public bool Complete => FilledCount == 8;
}

public class TradePlan
{
    public string Symbol { get; set; } = "";
    public string EntryLogic { get; set; } = "";
    public string ExitCriteria { get; set; } = "";
    public string RiskParameters { get; set; } = "";
    public string ThesisSummary { get; set; } = "";
    public string InvalidationConditions { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class PlanStore
{
    public List<TradePlan> Plans { get; set; } = [];
}

public class JournalStore
{
    public List<SessionLog> Sessions { get; set; } = [];
    public List<TradeReview> Reviews { get; set; } = [];
}

public class SessionLog
{
    public DateTime At { get; set; } = DateTime.Now;
    public string Calendar { get; set; } = "";
    public string WatchlistFlags { get; set; } = "";
    public string Risks { get; set; } = "";
    public string SessionPlan { get; set; } = "";
    public string StopTime { get; set; } = "";
    public string Notes { get; set; } = "";
}

public class TradeReview
{
    public DateTime At { get; set; } = DateTime.Now;
    public string Symbol { get; set; } = "";
    public string WhatHappened { get; set; } = "";
    public string Learned { get; set; } = "";
    public string ProcessChange { get; set; } = "";
    public string? AiDraft { get; set; }
}

public class AlertLog
{
    public DateTime At { get; set; } = DateTime.Now;
    public string Symbol { get; set; } = "";
    public string Signal { get; set; } = "";
    public string ChecklistNote { get; set; } = "";
    public bool Confirmed { get; set; }
    public string Action { get; set; } = "";
}

public class NewsBrief
{
    public string Facts { get; set; } = "";
    public string Sources { get; set; } = "";
    public string Implications { get; set; } = "";
    public bool Verified { get; set; }
    public string Raw { get; set; } = "";
}
