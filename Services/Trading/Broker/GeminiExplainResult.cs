namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public sealed record GeminiExplainResult(string? Text, string? Error)
{
    public bool IsSuccess => string.IsNullOrEmpty(Error) && !string.IsNullOrWhiteSpace(Text);
}
