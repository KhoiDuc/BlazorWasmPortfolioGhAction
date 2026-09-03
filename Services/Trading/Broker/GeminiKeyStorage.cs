namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public static class GeminiKeyStorage
{
    public const string KeyStorage = "trading.gemini.key";
    public const string ModelStorage = "trading.gemini.model";

    public static readonly string[] LegacyKeyStorages = ["stockai.gemini.key", "broker.gemini.key"];
    public static readonly string[] LegacyModelStorages = ["stockai.gemini.model", "broker.gemini.model"];
}
