using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

internal static class BrokerJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
