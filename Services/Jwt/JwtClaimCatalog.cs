using System.Text.Json;

namespace BlazorWasmPortfolioGhAction.Services.Jwt;

public static class JwtClaimCatalog
{
    private static readonly Dictionary<string, string> Descriptions = new(StringComparer.Ordinal)
    {
        ["iss"] = "Issuer — who issued the token",
        ["sub"] = "Subject — the subject of the token (often user ID)",
        ["aud"] = "Audience — intended recipient(s)",
        ["exp"] = "Expiration time — token must not be accepted after this time",
        ["nbf"] = "Not before — token must not be accepted before this time",
        ["iat"] = "Issued at — when the token was issued",
        ["jti"] = "JWT ID — unique identifier for the token",
        ["name"] = "Full name of the subject",
        ["given_name"] = "Given name(s) of the subject",
        ["family_name"] = "Surname(s) of the subject",
        ["email"] = "Email address of the subject",
        ["admin"] = "Custom claim — admin flag"
    };

    public static string GetDescription(string claimName) =>
        Descriptions.TryGetValue(claimName, out var desc) ? desc : "Custom claim";

    public static string FormatValue(string claimName, JsonElement value)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return "null";

        if (claimName is "exp" or "nbf" or "iat" && value.ValueKind == JsonValueKind.Number)
        {
            if (value.TryGetInt64(out var unix))
            {
                var dt = DateTimeOffset.FromUnixTimeSeconds(unix);
                return $"{unix} ({dt:yyyy-MM-dd HH:mm:ss} UTC)";
            }
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? "",
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => value.GetRawText(),
            _ => value.GetRawText()
        };
    }

    public static IEnumerable<(string Name, string Value, string Description)> Breakdown(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            yield break;

        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
            yield break;

        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            yield return (prop.Name, FormatValue(prop.Name, prop.Value), GetDescription(prop.Name));
        }
    }
}
