using System.Globalization;
using BlazorWasmPortfolioGhAction.Models.Trading.Broker;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public static class BrokerPromptLibrary
{
    public static string ExplainNote(
        string symbol,
        string sector,
        string status,
        string? avgBuy,
        string? stop,
        string? target,
        string newNote,
        string priorNotes)
    {
        return $"""
            Role: Trợ lý giải thích ngôn ngữ broker chứng khoán Việt Nam. Không khuyến mua/bán. Người dùng tự quyết định.
            Task: Giải thích note broker về mã {symbol} bằng tiếng Việt dễ hiểu.

            Context:
            - Mã: {symbol}
            - Ngành: {sector}
            - Trạng thái đang ghi: {status}
            - Giá TB: {avgBuy}
            - Cắt lỗ: {stop}
            - Mục tiêu: {target}
            - Note cần giải thích:
            {newNote}
            - Note cũ cùng mã (để đối chiếu):
            {priorNotes}

            Output format (tiếng Việt, ngắn):
            1) JARGON — giải thích từ/cụm từ khó (tích lũy, breakout, PE, cảnh mua, ...)
            2) Ý NGHĨA — broker đang nói gì, không suy diễn tin không có trong note
            3) CÂU HỎI — 2-4 câu nên hỏi lại broker nếu còn mơ hồ
            4) MẬU THUẪN — nếu note mới mâu thuẫn note cũ thì ghi rõ; nếu không thì "Không thấy mâu thuẫn rõ"

            Verification: Không thêm tín hiệu mua/bán. Không dự đoán chắc chắn. Nếu note quá ngắn thì nói thiếu thông tin.
            """;
    }

    public static string ExplainRecommendation(string rawText)
    {
        return $"""
            Role: Trợ lý phân tích khuyến nghị broker chứng khoán Việt Nam. KHÔNG khuyến mua/bán. Chỉ phân tích ý broker.
            Task: Người dùng paste nguyên văn khuyến nghị của broker. Giải thích bằng tiếng Việt dễ hiểu.

            Khuyến nghị (raw):
            {rawText}

            Output format (tiếng Việt, rõ ràng, ngắn gọn):
            ## Mã CP
            Mã broker đề cập (nếu có). Nếu nhiều mã thì liệt kê.

            ## Jargon
            Giải thích các từ/cụm từ chuyên ngành: tích lũy, breakout, hấp thụ, cung cầu, MA, volume, ...
            Định dạng: <từ> — <giải thích ngắn 1-2 dòng>

            ## Ý nghĩa
            Broker muốn nói gì? Phân tích logic: vì sao mua, vì sao cắt lỗ ở mức đó, mục tiêu nào.
            KHÔNG suy diễn tin không có trong text. Nếu broker không nói rõ thì nói "Broker không đề cập".

            ## Mức giá
            Vùng mua, cắt lỗ, mục tiêu (nếu có). Trình bày bằng bullet, giá trị số rõ ràng.

            ## Thời điểm mua
            Dựa trên chữ nói của broker, nên:
            - MUA LIỀN — nếu broker nói "mua ngay", "mua nhượng giá hiện tại", "mua ở khả năng kỹ" hoặc tương tự.
            - ĐỢI — nếu broker nói "mua quanh", "mua vùng", "cảnh mua ở", "chờ lui về" hoặc có vùng giá mục tiêu.
            Nếu không rõ: "Broker không nói rõ thời điểm — cần hỏi lại."
            Chỉ trích dẫn từ gốc, KHÔNG thêm ý kiến của bản thân.

            ## Rủi ro
            Những rủi ro broker đề cập hoặc ngầm hiểu. Nếu không có thì "Không đề cập rủi ro rõ".

            ## Câu hỏi
            2-4 câu người dùng nên hỏi lại broker trước khi quyết định.

            Verification: KHÔNG thêm lời khuyến mua/bán của bản thân. KHÔNG dự đoán giá tương lai. Nếu text quá ngắn thì nói thiếu thông tin.
            """;
    }

    /// <summary>
    /// Builds a compact textual summary of the current recommendation list
    /// (active positions only) to inject as context for AI questions about the portfolio.
    /// </summary>
    public static string BuildPortfolioContext(BrokerPortfolio? portfolio)
    {
        if (portfolio?.Positions is null || portfolio.Positions.Count == 0)
            return "(Danh sách khuyến nghị hiện đang trống.)";

        var lines = new List<string>();
        var i = 0;
        foreach (var p in portfolio.Positions)
        {
            i++;
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(p.Symbol)) parts.Add($"Mã: {p.Symbol}");
            if (!string.IsNullOrWhiteSpace(p.Sector)) parts.Add($"Ngành: {p.Sector}");
            parts.Add($"Trạng thái: {BrokerStatusLabels.Vi(p.Status)}");
            if (p.AvgBuy is { } avg && avg > 0) parts.Add($"Giá TB: {avg.ToString("N2", CultureInfo.InvariantCulture)}");
            if (p.RemainingQuantity is { } rem && rem > 0) parts.Add($"KL còn: {rem.ToString("N0", CultureInfo.InvariantCulture)}");
            if (p.StopLoss is { } sl && sl > 0) parts.Add($"Cắt lỗ: {sl.ToString("N2", CultureInfo.InvariantCulture)}");
            if (p.TargetPrice is { } tp && tp > 0) parts.Add($"Mục tiêu: {tp.ToString("N2", CultureInfo.InvariantCulture)}");
            if (p.WeightPct is { } w && w > 0) parts.Add($"Trọng số: {w.ToString("N1", CultureInfo.InvariantCulture)}%");
            if (p.Tags is { } tags && tags.Count > 0) parts.Add($"Tag: {string.Join(", ", tags)}");
            lines.Add($"  {i}. {string.Join(" | ", parts)}");
        }
        return string.Join('\n', lines);
    }

    /// <summary>
    /// Prompt for answering free-form questions about the current recommendation list.
    /// Injects the full portfolio context so the user can ask things like
    /// "I have 3 bank stocks, should I merge into 1?".
    /// </summary>
    public static string ExplainWithPortfolioContext(string userQuestion, string portfolioContext)
    {
        return $"""
            Role: Trợ lý phân tích danh mục khuyến nghị broker chứng khoán Việt Nam. KHÔNG khuyến mua/bán. Chỉ phân tích và trả lời dựa trên danh sách hiện có.
            Task: Người dùng hỏi câu hỏi về danh mục khuyến nghị hiện tại. Trả lời bằng tiếng Việt, rõ ràng, dựa trên dưới đây.

            Danh sách khuyến nghị hiện tại:
            {portfolioContext}

            Câu hỏi của người dùng:
            {userQuestion}

            Hướng dẫn trả lời:
            - Trả lời dựa trên danh sách trên, KHÔNG suy diễn mã không có.
            - Nếu câu hỏi về góp/xén/giảm position: phân tích trùng lặp ngành, trọng số, rủi ro tập trung, mục tiêu/cắt lỗ trùng nhau.
            - Nếu câu hỏi về 1 mã cụ thể: dùng thông tin của mã đó trong danh sách.
            - Nếu câu hỏi về ngành/phân bổ: nhóm theo ngành, tính tổng trọng số, nhận xét tập trung.
            - Nếu thiếu thông tin để trả lời: nói rõ điều gì thiếu, KHÔNG báo lời khuyến.
            - Nếu danh sách trống: báo "Danh sách hiện đang trống, thêm khuyến nghị trước khi hỏi."

            Output format (tiếng Việt, ngắn gọn, dùng markdown):
            ## Trả lời
            Trả lời trực tiếp câu hỏi.

            ## Phân tích
            Phân tích chi tiết hơn: logic, so sánh, rủi ro.

            ## Đề xuất theo dõi
            1-3 đề xuất "nên theo dõi/giảm theo dõi/thêm thông tin" (KHÔNG phải lệnh mua/bán).

            ## Câu hỏi phụ
            1-2 câu hỏi người dùng nên tự hỏi lại.

            Verification: KHÔNG thêm lời khuyến mua/bán. KHÔNG dự đoán giá tương lai. Chỉ dùng dữ liệu trong danh sách.
            """;
    }
}