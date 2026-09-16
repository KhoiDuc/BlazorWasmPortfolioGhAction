namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public static class StockAiPromptLibrary
{
    public record PromptTemplate(string Key, string Title, string Description, string Prompt);

    public static readonly PromptTemplate[] Templates =
    [
        new("general", "Trading_StockAiTpl_general_Title", "Trading_StockAiTpl_general_Desc", ""),
        new("explain-term", "Trading_StockAiTpl_explain-term_Title", "Trading_StockAiTpl_explain-term_Desc", "Giải thích thuật ngữ chứng khoán sau bằng tiếng Việt, ngắn gọn, ví dụ thực tế nếu có:\n\n{0}"),
        new("analyze-stock", "Trading_StockAiTpl_analyze-stock_Title", "Trading_StockAiTpl_analyze-stock_Desc", "Phân tích cổ phiếu {0} trên sàn Việt Nam. Trình bày:\n1) Ngành & mô hình kinh doanh\n2) Điểm cơ bản nổi bật (P/E, P/B, biên lợi nhuận, tăng trưởng)\n3) Điểm kỹ thuật xu hướng gần đây\n4) Rủi ro chính\nKHÔNG khuyên mua/bán. Chỉ phân tích khách quan."),
        new("portfolio-review", "Trading_StockAiTpl_portfolio-review_Title", "Trading_StockAiTpl_portfolio-review_Desc", "Rà soát danh mục cổ phiếu Việt Nam sau:\n\n{0}\n\nVới mỗi mã: nhận xét rủi ro/giá trị, có nên cắt lời/cut loss không. KHÔNG khuyên mua thêm. Chỉ gợi ý quản trị rủi ro dựa thông tin đã cho."),
        new("broker-note", "Trading_StockAiTpl_broker-note_Title", "Trading_StockAiTpl_broker-note_Desc", "Role: Trợ lý giải thích ngôn ngữ broker chứng khoán Việt Nam. KHÔNG khuyên mua/ban. Người dùng tự quyết định.\nTask: Giải thích note broker sau bằng tiếng Việt dễ hiểu.\n\nNote:\n{0}\n\nOutput:\n1) JARGON — giải thích từ/cụm từ kho\n2) Ý NGHĨA — broker đang nói gì, không suy diễn\n3) CÂU HỎI — 2-4 câu nên hỏi lại broker\n4) MÂU THUẪN — nếu có mâu thuẫn nội dung thì ghi rõ; không thì \"Không thấy mâu thuẫn\""),
        new("market-outlook", "Trading_StockAiTpl_market-outlook_Title", "Trading_StockAiTpl_market-outlook_Desc", "Đưa ra nhận định khách quan về thị trường chứng khoán Việt Nam (VN-Index) trong 1-2 tuần tới dựa bối cảnh hiện tại. KHÔNG khuyên mua/bán. Nêu rủi ro và cơ hội chung, không nói chắc chắn."),
        new("sector-scan", "Trading_StockAiTpl_sector-scan_Title", "Trading_StockAiTpl_sector-scan_Desc", "Quét ngành {0} trên thị trường VN. Liệt kê 3-5 mã đại diện, lý do đáng theo dõi (không khuyên mua). Nêu rủi ro ngành."),
    ];

    public static string Build(string templateKey, string userInput)
    {
        var tpl = Array.Find(Templates, t => t.Key == templateKey) ?? Templates[0];
        if (string.IsNullOrEmpty(tpl.Prompt)) return userInput.Trim();
        return tpl.Prompt.Contains("{0}")
            ? string.Format(tpl.Prompt, userInput.Trim())
            : $"{tpl.Prompt}\n\nCâu hỏi/ngữ cảnh thêm:\n{userInput.Trim()}";
    }
}