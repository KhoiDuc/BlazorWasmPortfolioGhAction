using Microsoft.IdentityModel.Tokens;

namespace BlazorWasmPortfolioGhAction.Services.Jwt;

public enum JwtKeyKind
{
    None,
    Hmac,
    Rsa,
    Ecdsa
}

public static class JwtAlgorithm
{
    public static readonly string[] All =
    [
        "none",
        "HS256", "HS384", "HS512",
        "RS256", "RS384", "RS512",
        "ES256", "ES384", "ES512",
        "PS256", "PS384", "PS512"
    ];

    public static JwtKeyKind GetKeyKind(string algorithm) => algorithm switch
    {
        "none" => JwtKeyKind.None,
        "HS256" or "HS384" or "HS512" => JwtKeyKind.Hmac,
        "RS256" or "RS384" or "RS512" or "PS256" or "PS384" or "PS512" => JwtKeyKind.Rsa,
        "ES256" or "ES384" or "ES512" => JwtKeyKind.Ecdsa,
        _ => JwtKeyKind.Hmac
    };

    public static string? ToSecurityAlgorithm(string algorithm) => algorithm switch
    {
        "HS256" => SecurityAlgorithms.HmacSha256,
        "HS384" => SecurityAlgorithms.HmacSha384,
        "HS512" => SecurityAlgorithms.HmacSha512,
        "RS256" => SecurityAlgorithms.RsaSha256,
        "RS384" => SecurityAlgorithms.RsaSha384,
        "RS512" => SecurityAlgorithms.RsaSha512,
        "ES256" => SecurityAlgorithms.EcdsaSha256,
        "ES384" => SecurityAlgorithms.EcdsaSha384,
        "ES512" => SecurityAlgorithms.EcdsaSha512,
        "PS256" => SecurityAlgorithms.RsaSsaPssSha256,
        "PS384" => SecurityAlgorithms.RsaSsaPssSha384,
        "PS512" => SecurityAlgorithms.RsaSsaPssSha512,
        "none" => SecurityAlgorithms.None,
        _ => null
    };

    public static bool IsSupported(string? algorithm) =>
        !string.IsNullOrWhiteSpace(algorithm) && All.Contains(algorithm, StringComparer.Ordinal);
}
