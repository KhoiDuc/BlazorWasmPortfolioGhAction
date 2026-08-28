namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public static class StockChatPrompts
{
    public record Template(string Id, string Label, string Icon, string Prompt);

    public const string SystemRole = """
        Role: Tro ly phan tich chung khoan Viet Nam. Tra loi bang tieng Viet.
        KHONG khuyen mua/ban cu the. Chi phan tich, giai thich, huong dan doc lap.
        Neu khong biet thi noi khong biet. Khong bia tin.
        Dung tu vung chung khoan VN: CP, KL, GTGD, san, lenh, ATO/ATC, PUT/CALL, ...
        """;

    public static readonly Template[] All =
    [
        new("explain-broker", "Giải thích khuyến nghị broker",
            "✦",
            """
            Day la nguyen van khuyen nghi cua broker:
            {INPUT}

            Giai thich: jargon, y nghia, muc gia, thoi diem mua (mua lien hay doi), rui ro, cau hoi nen hoi lai.
            """),
        new("analyze-stock", "Phân tích 1 mã cổ phiếu",
            "📊",
            """
            Phan tich ma CP: {INPUT}

            Trinh bay: nganh, vi tri trong nganh, diem manh/yeu, co ban (P/E, P/B, ROE neu biet),
            rui ro, co hoi. Khong khuyen mua/ban.
            """),
        new("explain-term", "Giải thích thuật ngữ CK",
            "📖",
            """
            Giai thich cu the cac thuat ngu chung khoan sau bang tieng Viet de hieu:
            {INPUT}

            Dinh dang: <thuat ngu> — <giai thich> (vi du neu co).
            """),
        new("strategy", "Chiến lược giao dịch",
            "🎯",
            """
            Cau hoi: {INPUT}

            Phan tich cac chien luoc phu hop: swing, scalp, dau tu gia tri, ...
            Giai thich diem manh/yeu moi chien luoc. Khong khuyen mua/ban cu the.
            """),
        new("risk-check", "Đánh giá rủi ro",
            "⚠",
            """
            Cau hoi/tinh huong: {INPUT}

            Liet ke rui ro: thi truong, co ban, thanh khoan, tam ly, ...
            Muc do rui ro (cao/trung binh/thap) + ly do.
            """),
        new("portfolio-review", "Rà soát danh mục",
            "📋",
            """
            Danh muc hien tai: {INPUT}

            Phan tich: phan bo nganh, trong tam, ty le nhan/lenh, rui ro tap trung,
            de xuat can bang lai (khong khuyen mua/ban cu the).
            """),
        new("free-question", "Hỏi tự do",
            "💬",
            """
            {INPUT}
            """),
    ];

    public static Template? Find(string id) =>
        All.FirstOrDefault(t => t.Id == id);

    public static string BuildPrompt(string templateId, string userInput)
    {
        var tmpl = Find(templateId) ?? All[0];
        var filled = tmpl.Prompt.Replace("{INPUT}", userInput.Trim());
        return $"{SystemRole}\n\n{filled}";
    }
}