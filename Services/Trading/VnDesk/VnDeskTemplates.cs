namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

/// <summary>Sample text and field hints for tab D workflow forms.</summary>
public static class VnDeskTemplates
{
    public const string DecideGuide = """
        Tab D giúp bạn quyết định CÓ vào lệnh hay không — trước khi đặt lệnh trên app broker (SSI, TCBS, DNSE…).
        Không tự đặt lệnh tại đây. Luồng gợi ý: Position size → Pre-trade checklist → Trading plan → (nếu có signal) Alert log.
        """;

    public const string PreTradeHint = "Trả lời 8 câu trước khi vào lệnh. Confirm = ghi journal (không gửi lệnh). Reject = bỏ qua setup.";

    public const string SizeHint = "Nhập vốn, % risk, giá và stop. Lot làm tròn 100 CP. R:R < 1.2 → no-trade.";

    public const string PlanHint = "Ghi rõ entry/exit/risk/thesis. Có thể bấm 'Điền mẫu' hoặc 'AI cấu trúc' sau khi nháp.";

    public const string AlertHint = "Khi screener/TA báo signal: Confirm = theo dõi & ghi log. Reject = bỏ qua. Không khớp lệnh tự động.";

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

    public record PreTradeSamples(
        string Thesis, string SourceData, string Invalidation, string MaxLoss,
        string PositionSizeLogic, string StopConditions, string ScenarioChange, string WithoutPressure);

    public record PlanSamples(
        string EntryLogic, string ExitCriteria, string RiskParameters,
        string ThesisSummary, string Invalidation);
}
