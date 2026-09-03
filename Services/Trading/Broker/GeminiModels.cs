namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public static class GeminiModels
{
    public const string DefaultId = "gemini-2.5-flash";

    public record Model(string Id, string Label);

    public static readonly Model[] All =
    [
        new(DefaultId, "Gemini 2.5 Flash"),
        new("gemini-2.5-flash-lite", "Gemini 2.5 Flash Lite"),
        new("gemini-2.5-pro", "Gemini 2.5 Pro"),
        new("gemini-flash-latest", "Gemini Flash Latest"),
        new("gemini-pro-latest", "Gemini Pro Latest"),
    ];
}
