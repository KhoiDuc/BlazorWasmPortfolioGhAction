namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public static class GeminiModels
{
    public const string DefaultId = "gemini-2.5-flash";

    public record Model(string Id, string Label);

    public static readonly Model[] All =
    [
        new("gemini-3.7-flash", "Gemini 3.7 Flash"),
        new("gemini-3.6-flash", "Gemini 3.6 Flash"),
        new("gemini-3.5-flash", "Gemini 3.5 Flash"),
        new("gemini-3.5-flash-lite", "Gemini 3.5 Flash Lite"),
        new("gemini-3.1-flash-lite", "Gemini 3.1 Flash Lite"),
        new("gemini-3.1-pro-preview", "Gemini 3.1 Pro Preview"),
        new("gemini-3-flash-preview", "Gemini 3 Flash Preview"),
        new("gemini-flash-latest", "Gemini Flash Latest"),
        new("gemini-flash-lite-latest", "Gemini Flash Lite Latest"),
        new("gemini-pro-latest", "Gemini Pro Latest"),
        new("gemini-2.5-flash", "Gemini 2.5 Flash"),
        new("gemini-2.5-flash-lite", "Gemini 2.5 Flash Lite"),
        new("gemini-2.5-pro", "Gemini 2.5 Pro"),
    ];
}