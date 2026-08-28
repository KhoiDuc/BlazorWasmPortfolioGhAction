namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public static class GeminiModels
{
    public const string DefaultId = "gemini-2.0-flash";

    public record Model(string Id, string Label);

    public static readonly Model[] All =
    [
        // Dòng 2.0 mới nhất & tối ưu nhất (Khuyên dùng mặc định)
        new("gemini-2.0-flash", "Gemini 2.0 Flash (Nhanh & Đa năng)"),
        new("gemini-2.0-flash-lite", "Gemini 2.0 Flash Lite (Tiết kiệm chi phí)"),
        new("gemini-2.0-pro-exp-02-05", "Gemini 2.0 Pro Experimental (Suy luận phức tạp / Code)"),
        new("gemini-2.0-flash-thinking-exp-01-21", "Gemini 2.0 Flash Thinking (Có suy luận theo chuỗi)"),

        // Dòng 1.5 ổn định (Context window lớn tới 2M tokens)
        new("gemini-1.5-flash", "Gemini 1.5 Flash"),
        new("gemini-1.5-pro", "Gemini 1.5 Pro"),
        new("gemini-1.5-flash-8b", "Gemini 1.5 Flash 8B (Siêu nhẹ)")
    ];
}