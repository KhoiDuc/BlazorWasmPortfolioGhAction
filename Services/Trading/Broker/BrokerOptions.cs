namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public sealed class BrokerOptions
{
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "gemini-2.0-flash";
}
