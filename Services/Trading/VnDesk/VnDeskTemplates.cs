namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

/// <summary>Sample text and field hints for tab D/E — newbie-first with advanced fallback.</summary>
public static class VnDeskTemplates
{
    public const string NewbieDecideGuide = """
        Trả lời 3 bước ngắn để biết có nên mua mã này không. App không đặt lệnh — bạn tự đặt trên SSI, TCBS, DNSE...
        """;

    public const string NewbieJournalGuide = """
        Ghi lại hôm nay bạn làm gì với cổ phiếu — giúp nhìn lại cảm xúc và tránh lặp sai lầm.
        """;

    public const string DecideGuide = """
        Tab D giúp bạn quyết định CÓ vào lệnh hay không — trước khi đặt lệnh trên app broker (SSI, TCBS, DNSE…).
        Không tự đặt lệnh tại đây. Luồng gợi ý: Position size → Pre-trade checklist → Trading plan → (nếu có signal) Alert log.
        """;

    public const string PreTradeHint = "Trả lời 8 câu trước khi vào lệnh. Confirm = ghi journal (không gửi lệnh). Reject = bỏ qua setup.";

    public const string SizeHint = "Nhập vốn, % risk, giá và stop. Lot làm tròn 100 CP. R:R < 1.2 → no-trade.";

    public const string PlanHint = "Ghi rõ entry/exit/risk/thesis. Có thể bấm 'Điền mẫu' hoặc 'AI cấu trúc' sau khi nháp.";

    public const string AlertHint = "Khi screener/TA báo signal: Confirm = theo dõi & ghi log. Reject = bỏ qua. Không khớp lệnh tự động.";

    public const string Step1Hint = "Nếu dự đoán sai và chạm mức cắt lỗ, bạn mất tối đa bao nhiêu? Đây là số tiền bạn chấp nhận trước khi mua.";

    public const string Step2Hint = "Trả lời thật — không có câu đúng/sai. App chỉ giúp bạn nhìn lại trước khi bấm mua trên broker.";

    public const string Step3Hint = "Tóm tắt quyết định — có thể sửa trước khi lưu. Lưu xong sang tab E để ghi nhật ký sau phiên.";

    public static readonly string[] BuyReasons =
    [
        "Xu hướng đang tăng",
        "Giá hồi về vùng hỗ trợ",
        "Breakout / volume mạnh",
        "AI / screener gợi ý",
        "Khác (tự ghi)"
    ];

    public static readonly string[] DayPlans =
    [
        "Chỉ xem, không mua",
        "Sẵn sàng tối đa 1 lệnh",
        "Không trade hôm nay"
    ];

    public static readonly string[] JournalActions = ["Mua", "Bán", "Không vào"];

    public static readonly string[] JournalOutcomes = ["Đang giữ", "Lời", "Lỗ", "Hòa vốn"];

    public static readonly string[] JournalMoods = ["Bình tĩnh", "Sốt ruột", "Sợ", "Tham"];

    public static PreTradeSamples PreTrade(string symbol) => new(
        Thesis: $"Mua {symbol} vì xu hướng trung hạn còn tích cực, volume xác nhận.",
        SourceData: "VNDirect daily + TA tab B + sector heatmap tab A.",
        Invalidation: $"Đóng cửa dưới stop hoặc mất SMA20 kèm volume tăng mạnh.",
        MaxLoss: "Tối đa 1% NAV phiên này (~1 triệu trên 100M vốn).",
        PositionSizeLogic: "Risk 1% / stop distance → lot 100 CP (xem tab Position size).",
        StopConditions: "Cắt lỗ cứng tại stop; không gồng thêm nếu invalidate.",
        ScenarioChange: "Đổi plan nếu VN-Index giảm >1.5% hoặc tin vĩ mô bất lợi.",
        WithoutPressure: "Vẫn làm nếu setup đủ 8/8 câu và R:R ≥ 1.2 — không FOMO theo đám đông.");

    public static PlanSamples Plan(string symbol, decimal? price = null, decimal? stop = null) => new(
        EntryLogic: price is > 0
            ? $"Limit quanh {price:N2} khi pullback về hỗ trợ / xác nhận nến tăng + volume ≥ TB20."
            : "Chờ pullback về hỗ trợ gần nhất, xác nhận volume trước khi vào.",
        ExitCriteria: "Chốt 1/2 tại target 1; dời stop breakeven; phần còn lại theo trailing SMA20.",
        RiskParameters: stop is > 0
            ? $"Stop {stop:N2}; risk ≤1% vốn; size theo tab Position size."
            : "Stop dưới đáy swing gần nhất; risk ≤1% vốn.",
        ThesisSummary: $"{symbol}: trend + sector ủng hộ, RR hợp lý sau khi tính size.",
        Invalidation: "Đóng cửa tuần dưới stop hoặc thesis sector đảo chiều.");

    public static string BuildDecisionSummary(string symbol, SizeResult size, bool shouldBuy, string reason, int score)
    {
        if (size.NoTrade)
            return $"Mã {symbol}: Không nên mua — {size.Reason}.";

        var verdict = shouldBuy ? "Có thể mua" : "Chưa nên mua";
        var profit = size.Reward > 0 ? $"Nếu đúng (chạm chốt lời): lời khoảng {size.Reward:N0}đ." : "";
        return $"""
            Mã {symbol} — {verdict} ({score}/5 câu kỷ luật).
            Mua tối đa {size.Shares} CP (~{size.PositionValue:N0}đ).
            Nếu sai (chạm cắt lỗ {size.Stop:N2}): mất khoảng {size.RiskAmount:N0}đ.
            {profit}
            Lý do: {reason}.
            """;
    }

    public record PreTradeSamples(
        string Thesis, string SourceData, string Invalidation, string MaxLoss,
        string PositionSizeLogic, string StopConditions, string ScenarioChange, string WithoutPressure);

    public record PlanSamples(
        string EntryLogic, string ExitCriteria, string RiskParameters,
        string ThesisSummary, string Invalidation);
}

public static class VnNewbieScoring
{
    public static (int Score, bool ShouldBuy, string Verdict, List<string> Warnings) Evaluate(
        bool? q1ViewedChart,
        bool? q2HasStop,
        bool? q3LossOk,
        string? q4Motivation,
        bool? q5KeepStop,
        SizeResult? size)
    {
        var warnings = new List<string>();
        var score = 0;

        if (q1ViewedChart == true) score++;
        else if (q1ViewedChart == false) warnings.Add("Chưa xem chart — nên sang tab B Phân tích trước.");

        if (q2HasStop == true) score++;
        else if (q2HasStop == false) warnings.Add("Chưa chốt mức cắt lỗ — rất dễ gồng lỗ.");

        if (q3LossOk == true) score++;
        else if (q3LossOk == false) warnings.Add("Số tiền có thể mất làm bạn lo — nên giảm số CP hoặc bỏ qua.");

        if (q4Motivation == "planned") score++;
        else if (q4Motivation == "fomo") warnings.Add("Cảm giác FOMO / theo đám đông — nên chờ thêm.");

        if (q5KeepStop == true) score++;
        else if (q5KeepStop == false) warnings.Add("Không cam kết cắt lỗ — rủi ro cao.");

        if (size?.NoTrade == true)
            return (score, false, "Không nên mua", warnings);

        if (size is not null && size.RiskReward < 1.2m)
            warnings.Add("Lời dự kiến nhỏ hơn lỗ nhiều — setup chưa đẹp.");

        var shouldBuy = score >= 4 && size is not null && !size.NoTrade && size.RiskReward >= 1.2m;
        var verdict = shouldBuy ? "Có thể mua" : score >= 3 ? "Cân nhắc thêm" : "Chưa nên mua";
        return (score, shouldBuy, verdict, warnings);
    }
}
