namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public static class PromptLibrary
{
    public static string Wrap(string role, string task, string context, string outputFormat, string verification) =>
        $"""
        Context: {context}
        Role: {role}
        Task: {task}
        Output format: {outputFormat}
        Verification: {verification}
        Dùng ngôn ngữ trung lập. Không dự đoán chắc chắn. Không khuyến mua/bán. Người dùng tự quyết định.
        """;

    public static string News(string raw) => Wrap(
        "Trợ lý nghiên cứu thị trường VN, không ra tín hiệu giao dịch",
        "Tách văn bản tin thành Facts / Sources / Implications",
        raw,
        "Ba mục: FACTS, SOURCES, IMPLICATIONS. Ngắn gọn.",
        "Gắn 'chưa verify nguồn'. Không thêm tin không có trong input.");

    public static string Thesis(string symbol, string numbers, string notes) => Wrap(
        "Người soạn thesis, không thay thế phán đoán",
        $"Viết nhập thesis cho {symbol}: assumptions, evidence, invalidation",
        $"Số liệu app:\n{numbers}\nGhi chú:\n{notes}",
        "ASSUMPTIONS / EVIDENCE / INVALIDATION",
        "Mỗi claim phải gắn số liệu nguồn. Không thêm tín hiệu mua/bán.");

    public static string NeutralChecklist(string symbol, string numbers, string notes) => Wrap(
        "Người mô tả setup kỹ thuật trung lập",
        $"Mô tả observation vs interpretation vs invalidation cho {symbol}",
        $"{numbers}\n{notes}",
        "CONTEXT / CONFIRM-INVALIDATE / RISK / VERIFY",
        "Không hype, không chắc chắn, không ra lệnh.");
}