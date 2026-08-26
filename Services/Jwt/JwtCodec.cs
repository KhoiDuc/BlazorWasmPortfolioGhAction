using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace BlazorWasmPortfolioGhAction.Services.Jwt;

public class JwtCodec
{
    private static readonly JsonSerializerOptions PrettyJson = new() { WriteIndented = true };
    private static readonly JsonSerializerOptions CompactJson = new() { WriteIndented = false };

    public JwtDecodeResult Decode(string? token)
    {
        token = token?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(token))
        {
            return new JwtDecodeResult(
                false, null, null, null, null, null, null, null);
        }

        var parts = token.Split('.');
        if (parts.Length < 2)
        {
            return new JwtDecodeResult(
                false, null, null, null, null, null, null,
                "Invalid JWT: must contain at least header and payload segments.");
        }

        try
        {
            var headerJson = PrettyPrintJson(DecodeSegmentToUtf8(parts[0]));
            var payloadJson = PrettyPrintJson(DecodeSegmentToUtf8(parts[1]));
            var signature = parts.Length > 2 ? parts[2] : string.Empty;
            var algorithm = ExtractAlgorithm(headerJson);

            return new JwtDecodeResult(
                true, headerJson, payloadJson, parts[0], parts[1], signature, algorithm, null);
        }
        catch (Exception ex)
        {
            return new JwtDecodeResult(
                false, null, null, null, null, null, null, ex.Message);
        }
    }

    public JwtVerifyResult Verify(
        string? token,
        string algorithm,
        string? hmacSecret,
        bool secretIsBase64Url,
        string? publicKeyPem)
    {
        token = token?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(token))
            return new JwtVerifyResult(false, false, "No token to verify.");

        var parts = token.Split('.');
        if (parts.Length < 2)
            return new JwtVerifyResult(false, false, "Invalid JWT structure.");

        if (string.Equals(algorithm, "none", StringComparison.OrdinalIgnoreCase))
        {
            var valid = parts.Length == 2 || string.IsNullOrEmpty(parts[2]);
            return new JwtVerifyResult(valid, true, valid ? null : "none algorithm requires an empty signature.");
        }

        var key = CreateVerificationKey(algorithm, hmacSecret, secretIsBase64Url, publicKeyPem);
        if (key is null)
            return new JwtVerifyResult(false, false, "Enter a verification key to check the signature.");

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                RequireSignedTokens = true,
                RequireExpirationTime = false,
                ValidAlgorithms = [algorithm]
            };

            handler.ValidateToken(token, parameters, out _);
            return new JwtVerifyResult(true, true, null);
        }
        catch (Exception ex)
        {
            return new JwtVerifyResult(false, true, ex.Message);
        }
    }

    public JwtEncodeResult Encode(
        string? headerJson,
        string? payloadJson,
        string algorithm,
        string? hmacSecret,
        bool secretIsBase64Url,
        string? privateKeyPem)
    {
        if (string.IsNullOrWhiteSpace(headerJson) || string.IsNullOrWhiteSpace(payloadJson))
            return new JwtEncodeResult(false, null, "Header and payload JSON are required.");

        try
        {
            var headerElement = ParseAndValidateJson(headerJson);
            var payloadElement = ParseAndValidateJson(payloadJson);

            var headerDict = JsonElementToDictionary(headerElement);
            headerDict["typ"] = "JWT";
            headerDict["alg"] = algorithm;

            var compactHeader = JsonSerializer.Serialize(headerDict, CompactJson);
            var compactPayload = JsonSerializer.Serialize(JsonElementToDictionary(payloadElement), CompactJson);

            var headerSegment = Base64UrlEncode(Encoding.UTF8.GetBytes(compactHeader));
            var payloadSegment = Base64UrlEncode(Encoding.UTF8.GetBytes(compactPayload));
            var signingInput = $"{headerSegment}.{payloadSegment}";

            if (string.Equals(algorithm, "none", StringComparison.OrdinalIgnoreCase))
                return new JwtEncodeResult(true, $"{signingInput}.", null);

            var credentials = CreateSigningCredentials(algorithm, hmacSecret, secretIsBase64Url, privateKeyPem);
            if (credentials is null)
                return new JwtEncodeResult(false, null, "Enter a signing key to create a signed token.");

            var signatureBytes = Sign(signingInput, credentials);
            var signatureSegment = Base64UrlEncode(signatureBytes);

            return new JwtEncodeResult(true, $"{signingInput}.{signatureSegment}", null);
        }
        catch (JsonException ex)
        {
            return new JwtEncodeResult(false, null, $"Invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new JwtEncodeResult(false, null, ex.Message);
        }
    }

    public bool IsValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            ParseAndValidateJson(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public JwtExample CreateExample(string algorithm)
    {
        if (!JwtAlgorithm.IsSupported(algorithm))
            algorithm = "HS256";

        const string payloadJson = """
            {
              "sub": "1234567890",
              "name": "John Doe",
              "admin": true,
              "iat": 1516239022
            }
            """;

        var headerJson = $$"""
            {
              "alg": "{{algorithm}}",
              "typ": "JWT"
            }
            """;

        string? secret = null;
        string? publicKey = null;
        string? privateKey = null;

        switch (JwtAlgorithm.GetKeyKind(algorithm))
        {
            case JwtKeyKind.Hmac:
                secret = JwtExampleKeys.GetHmacSecret(algorithm);
                break;
            case JwtKeyKind.Rsa:
                publicKey = JwtExampleKeys.RsaPublicKeyPem;
                privateKey = JwtExampleKeys.RsaPrivateKeyPem;
                break;
            case JwtKeyKind.Ecdsa:
                (publicKey, privateKey) = JwtExampleKeys.GetEcKeys(algorithm);
                break;
        }

        var token = JwtPrebuiltExamples.Tokens.TryGetValue(algorithm, out var prebuilt)
            ? prebuilt
            : string.Empty;

        return new JwtExample(
            token,
            algorithm,
            PrettyPrintJson(headerJson),
            PrettyPrintJson(payloadJson),
            secret,
            publicKey,
            privateKey);
    }

    public JwtExample CreateHs256Example() => CreateExample("HS256");

    public JwtExample CreateRs256Example() => CreateExample("RS256");

    public static string Base64UrlEncode(byte[] input) =>
        Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static byte[] Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }

        return Convert.FromBase64String(padded);
    }

    public static string HighlightToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return string.Empty;

        var parts = token.Split('.');
        if (parts.Length == 1)
            return $"<span class=\"jwt-part jwt-part-header\">{Escape(parts[0])}</span>";

        var header = Escape(parts[0]);
        var payload = Escape(parts[1]);
        var signature = parts.Length > 2 ? Escape(parts[2]) : string.Empty;

        if (parts.Length == 2)
        {
            return $"<span class=\"jwt-part jwt-part-header\">{header}</span>" +
                   $"<span class=\"jwt-part-dot\">.</span>" +
                   $"<span class=\"jwt-part jwt-part-payload\">{payload}</span>";
        }

        return $"<span class=\"jwt-part jwt-part-header\">{header}</span>" +
               $"<span class=\"jwt-part-dot\">.</span>" +
               $"<span class=\"jwt-part jwt-part-payload\">{payload}</span>" +
               $"<span class=\"jwt-part-dot\">.</span>" +
               $"<span class=\"jwt-part jwt-part-signature\">{signature}</span>";
    }

    public static string HighlightJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return string.Empty;

        var sb = new StringBuilder();
        var i = 0;
        while (i < json.Length)
        {
            var c = json[i];
            if (char.IsWhiteSpace(c))
            {
                sb.Append(c);
                i++;
                continue;
            }

            if (c == '"')
            {
                var start = i;
                i++;
                while (i < json.Length)
                {
                    if (json[i] == '\\')
                    {
                        i += 2;
                        continue;
                    }

                    if (json[i] == '"')
                    {
                        i++;
                        break;
                    }

                    i++;
                }

                var segment = json[start..i];
                var isKey = i < json.Length && SkipWhitespace(json, ref i) == ':';
                i = start + segment.Length;
                sb.Append(isKey
                    ? $"<span class=\"jwt-json-key\">{Escape(segment)}</span>"
                    : $"<span class=\"jwt-json-string\">{Escape(segment)}</span>");
                continue;
            }

            if (c is '{' or '}' or '[' or ']' or ':' or ',')
            {
                sb.Append(Escape(c.ToString()));
                i++;
                continue;
            }

            if (c == '-' || char.IsDigit(c))
            {
                var start = i;
                i++;
                while (i < json.Length && (char.IsDigit(json[i]) || json[i] is '.' or 'e' or 'E' or '+' or '-'))
                    i++;
                sb.Append($"<span class=\"jwt-json-number\">{Escape(json[start..i])}</span>");
                continue;
            }

            if (json.AsSpan(i).StartsWith("true") || json.AsSpan(i).StartsWith("false") || json.AsSpan(i).StartsWith("null"))
            {
                var len = json.AsSpan(i).StartsWith("null") ? 4 : json.AsSpan(i).StartsWith("true") ? 4 : 5;
                var word = json.Substring(i, len);
                sb.Append($"<span class=\"jwt-json-boolean\">{Escape(word)}</span>");
                i += len;
                continue;
            }

            sb.Append(Escape(c.ToString()));
            i++;
        }

        return sb.ToString();
    }

    private static char SkipWhitespace(string json, ref int i)
    {
        while (i < json.Length && char.IsWhiteSpace(json[i]))
            i++;
        return i < json.Length ? json[i] : '\0';
    }

    private static string Escape(string text) =>
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    private static string DecodeSegmentToUtf8(string segment)
    {
        var bytes = Base64UrlDecode(segment);
        return Encoding.UTF8.GetString(bytes);
    }

    private static string PrettyPrintJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return string.Empty;

        using var doc = JsonDocument.Parse(json.Trim());
        return JsonSerializer.Serialize(doc.RootElement, PrettyJson);
    }

    private static JsonElement ParseAndValidateJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("JSON must be an object.");
        return doc.RootElement.Clone();
    }

    private static Dictionary<string, object?> JsonElementToDictionary(JsonElement element)
    {
        var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var prop in element.EnumerateObject())
            dict[prop.Name] = JsonElementToObject(prop.Value);
        return dict;
    }

    private static object? JsonElementToObject(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => JsonElementToDictionary(element),
        JsonValueKind.Array => element.EnumerateArray().Select(JsonElementToObject).ToList(),
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number when element.TryGetInt64(out var l) => l,
        JsonValueKind.Number => element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null
    };

    private static string? ExtractAlgorithm(string headerJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(headerJson);
            if (doc.RootElement.TryGetProperty("alg", out var alg) && alg.ValueKind == JsonValueKind.String)
                return alg.GetString();
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static SecurityKey? CreateVerificationKey(
        string algorithm,
        string? hmacSecret,
        bool secretIsBase64Url,
        string? publicKeyPem)
    {
        return JwtAlgorithm.GetKeyKind(algorithm) switch
        {
            JwtKeyKind.Hmac => CreateSymmetricKey(hmacSecret, secretIsBase64Url),
            JwtKeyKind.Rsa or JwtKeyKind.Ecdsa => CreateAsymmetricPublicKey(algorithm, publicKeyPem),
            _ => null
        };
    }

    private static SigningCredentials? CreateSigningCredentials(
        string algorithm,
        string? hmacSecret,
        bool secretIsBase64Url,
        string? privateKeyPem)
    {
        var securityAlg = JwtAlgorithm.ToSecurityAlgorithm(algorithm);
        if (securityAlg is null)
            return null;

        SecurityKey? key = JwtAlgorithm.GetKeyKind(algorithm) switch
        {
            JwtKeyKind.Hmac => CreateSymmetricKey(hmacSecret, secretIsBase64Url),
            JwtKeyKind.Rsa => CreateRsaPrivateKey(privateKeyPem),
            JwtKeyKind.Ecdsa => CreateEcPrivateKey(privateKeyPem),
            _ => null
        };

        return key is null ? null : new SigningCredentials(key, securityAlg);
    }

    private static SymmetricSecurityKey? CreateSymmetricKey(string? secret, bool secretIsBase64Url)
    {
        if (string.IsNullOrEmpty(secret))
            return null;

        var keyBytes = secretIsBase64Url ? Base64UrlDecode(secret) : Encoding.UTF8.GetBytes(secret);
        return new SymmetricSecurityKey(keyBytes);
    }

    private static SecurityKey? CreateAsymmetricPublicKey(string algorithm, string? pem)
    {
        if (string.IsNullOrWhiteSpace(pem))
            return null;

        if (JwtAlgorithm.GetKeyKind(algorithm) == JwtKeyKind.Rsa)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(pem);
            return new RsaSecurityKey(rsa);
        }

        var ec = ECDsa.Create();
        ec.ImportFromPem(pem);
        return new ECDsaSecurityKey(ec);
    }

    private static SecurityKey? CreateRsaPrivateKey(string? pem)
    {
        if (string.IsNullOrWhiteSpace(pem))
            return null;

        var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        return new RsaSecurityKey(rsa);
    }

    private static SecurityKey? CreateEcPrivateKey(string? pem)
    {
        if (string.IsNullOrWhiteSpace(pem))
            return null;

        var ec = ECDsa.Create();
        ec.ImportFromPem(pem);
        return new ECDsaSecurityKey(ec);
    }

    private static byte[] Sign(string signingInput, SigningCredentials credentials)
    {
        var inputBytes = Encoding.UTF8.GetBytes(signingInput);
        var cryptoFactory = credentials.CryptoProviderFactory ?? CryptoProviderFactory.Default;
        var provider = cryptoFactory.CreateForSigning(credentials.Key, credentials.Algorithm);
        try
        {
            return provider.Sign(inputBytes);
        }
        finally
        {
            if (provider is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
