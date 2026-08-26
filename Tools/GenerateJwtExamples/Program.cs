using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

const string HmacSecret256 = "a-string-secret-at-least-256-bits-long";
const string HmacSecret384 = "a-string-secret-at-least-384-bits-long-for-hmac-sha384-example-key!!";
const string HmacSecret512 = "a-string-secret-at-least-512-bits-long-for-hmac-sha512-example-key-value!!!!!!";
const string PayloadJson = """{"sub":"1234567890","name":"John Doe","admin":true,"iat":1516239022}""";

const string RsaPrivateKeyPem = """
-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEApLr0Yl0uZLUsswBAe2sQIojIhQ63khSPo3JuriLIduZbTLOS
ujz5W6KPDU2vfmfmksY3d8ayxNKxOIhhadQcMYhv5FxweBPwOVXyFaR9jBjjiWsq
V5svHCX1Tv0zTGc31YHXKGOnNiyYbZ6ilTJrVdGfc7+mLRDt9ETK8RlU/6Fke0Mq
LAzC0jqNC5eWVr5l6L6RZf7PZgIe+FoP3uHSGABgE1CAMyW6b36ma5AmAPa/nLnv
RnSPY7vavn0WUw1egjmy4FbchUeHIc/KHqKPM5d3Kb8Fft4VPsaf5s+vapw2Rx99
nOKEcUaaa/yw6M2kRCGkpexjB3N4JDiXOK+1cQIDAQABAoIBABOxkiPsVe6ORmDE
B/F5VD18stRNdR7OvHli5d6FpAeq9IFP+puvTHRrE8UYY4GuND+Z2OnF5HtpYOyM
SXEYfaJiWFYpwvxoDdEc+EskFipYk2NLX42HrPMlipU96hMZR/CLjEh02Xm9jR+V
1oQVugVRgUmaQOikXH5Wdxip2aKR3MaDp2Te7FGQflWYVAaVE7j7L860wKkj1CIC
GRhoB+lWbCQi3g8Y7/CXKZegjQ6/rioIWpYtKKx7qs+W4FH/RmmFRu+UD3mj3WEf
j5+bPcoc8TGTVuPFzB7bSOFZoWad4GRrVLgsCtyx+4YKRSLTEdJWAEdEzwgB4Gxk
Pzfjmz0CgYEAxJ1l4GmD1h3cjUNDrUhpk4WbOi3lJt6KvyNy1YmVkapbeL7ZrPhm
Lf/MsKv2f6XIVyxDp3sqdgMkNMHcWPepRWBLghgZQC/kgj1rawRAJWQY1jFT0cmH
S3u+1uHtBpGqOTY34cfXqMfqrOks7UXPbo+o8p4WLqJ64w7cE0qzOHMCgYEA1nww
JvHvMA/s82VvVg8ctfWFI/1uqfAKGpnanMNfQetUaYRlgGb9Iz3llCiLSJrfC8C5
kYWSWJSQvp1UDpakWrc6Re1LAVaeBwyM3NwDrcK6WL6A0SCme7cXWLsXTitW+7d8
PN2Ln/E2zZZ87hwTZN9xhRAAmwd2GfWc9gW+9YsCgYEAwOLBXWVmhxqIkrVYoZkW
qjk6zdrUoktUxqtaLw9pApykXFKvbjfK1nnLL8+kZhoX7x1nydjVES2HyZIeP9HH
6F3N9fT6YLQbc4IZ7cUfwQ7wJsIBvx/8cRsXX+wJApbI7pO4QvIxKk3mdnfTMXt5
QrgBQPPCQ0H3WOXtFagFJu0CgYAitD1mT8cP08PXLhZxqkhO+MbuJ9d8D6bS3woM
S2BiXh1uZUm3GPLkfONksZq3yLdGQ4zDCwW+52r0LKipI568PuFi5x43vTgck99G
7SJ7PwK/0TMuo0XMxrc0WDFRKJoOaIXlo0MwQetIII1eSZSpwa2whgJTE0X8tU0M
UamwOQKBgQCARuBguOYIXqJdXeij3ghHE5bbDy8KpWSwU6vyyiLrHHEvAzX6yygu
xIrKXII9phE2zh2ULcnt2V8AiUOJc956oCoNYmh5dY5EgTEi8m3vPoU5T0gX1v50
2aoBqv0lzZKKpeScskWrG70pqo3OmDQ2EOgkZLn18QT7wmyASJuKLA==
-----END RSA PRIVATE KEY-----
""";

const string Es256PrivateKeyPem = """
-----BEGIN EC PRIVATE KEY-----
MHcCAQEEINHq0XMjkrQkukDn7vhF3hdBE2SY5gizICByD0INh+AqoAoGCCqGSM49
AwEHoUQDQgAEu7U4pkTIVNbKwbXYZNUkoDNxl1WI02vqikFH4enJrLHuGYgBY3uY
LydC/iMVNb9JMjnFL+9WHxDnd1/KciP6Uw==
-----END EC PRIVATE KEY-----
""";

const string Es384PrivateKeyPem = """
-----BEGIN EC PRIVATE KEY-----
MIGkAgEBBDBB/6Xn/tUNX7Fh9dsBdGMgSfGcXyeFJLCEzRCs0XKm9fzNnfcInuat
L7e43d+VH2agBwYFK4EEACKhZANiAASiYUU+vqLwXdXXoL6txnvEO52NsATFTfCA
29mneLlG2eckw/z5lNGLwvdovGE3ZDHRORkpl21XrgQX2CzleEe62l0gVJc2nVYs
FNhfAzSXk+0szYPMP6RlqKfI6YSXKWk=
-----END EC PRIVATE KEY-----
""";

const string Es512PrivateKeyPem = """
-----BEGIN EC PRIVATE KEY-----
MIHcAgEBBEIAKhbBKISlktNOIJjF3GbLPMOgArJk8CdtrAErMZfncB1dnIpee4ld
ufSr9Q6tyba8gbg4i9QzTdmhGSApIFUT4GigBwYFK4EEACOhgYkDgYYABAAWB3p5
jeJvT/4su+Vxv3zry4XugVT2qoxLxKyiaehHCti8hPv8UbyizNqdy3B8Xn/uLpGo
LwFWsPmtTfZpogYdJgCES9UD/0IMi/z9QGGFPSNSa/zmir758v2Zh6qRKg0N220f
Rhi38Mly2MJC8lr78xvTOJPzF/hrihUf3oWKLxeSAg==
-----END EC PRIVATE KEY-----
""";

var algorithms = new[]
{
    "none", "HS256", "HS384", "HS512",
    "RS256", "RS384", "RS512",
    "ES256", "ES384", "ES512",
    "PS256", "PS384", "PS512"
};

Console.WriteLine("namespace BlazorWasmPortfolioGhAction.Services.Jwt;");
Console.WriteLine();
Console.WriteLine("public static class JwtPrebuiltExamples");
Console.WriteLine("{");
Console.WriteLine("    public static readonly IReadOnlyDictionary<string, string> Tokens = new Dictionary<string, string>(StringComparer.Ordinal)");
Console.WriteLine("    {");

foreach (var alg in algorithms)
{
    var token = CreateToken(alg);
    Console.WriteLine($"        [\"{alg}\"] = \"{token}\",");
}

Console.WriteLine("    };");
Console.WriteLine("}");

static string CreateToken(string algorithm)
{
    var header = JsonSerializer.Serialize(new Dictionary<string, string> { ["alg"] = algorithm, ["typ"] = "JWT" });
    var headerSeg = B64Url(Encoding.UTF8.GetBytes(header));
    var payloadSeg = B64Url(Encoding.UTF8.GetBytes(PayloadJson));
    var signingInput = $"{headerSeg}.{payloadSeg}";

    if (algorithm == "none")
        return $"{signingInput}.";

    var creds = CreateCredentials(algorithm);
    var sig = Sign(signingInput, creds);
    return $"{signingInput}.{B64Url(sig)}";
}

static SigningCredentials CreateCredentials(string algorithm)
{
    var secAlg = algorithm switch
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
        _ => throw new ArgumentOutOfRangeException(nameof(algorithm))
    };

    SecurityKey key = algorithm switch
    {
        "HS256" => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(HmacSecret256)),
        "HS384" => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(HmacSecret384)),
        "HS512" => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(HmacSecret512)),
        var a when a.StartsWith("ES") => new ECDsaSecurityKey(ImportEc(a)),
        _ => new RsaSecurityKey(ImportRsa())
    };

    return new SigningCredentials(key, secAlg);
}

static RSA ImportRsa()
{
    var rsa = RSA.Create();
    rsa.ImportFromPem(RsaPrivateKeyPem);
    return rsa;
}

static ECDsa ImportEc(string algorithm)
{
    var ec = ECDsa.Create();
    ec.ImportFromPem(algorithm switch
    {
        "ES384" => Es384PrivateKeyPem,
        "ES512" => Es512PrivateKeyPem,
        _ => Es256PrivateKeyPem
    });
    return ec;
}

static byte[] Sign(string signingInput, SigningCredentials credentials)
{
    var inputBytes = Encoding.UTF8.GetBytes(signingInput);
    var factory = credentials.CryptoProviderFactory ?? CryptoProviderFactory.Default;
    var provider = factory.CreateForSigning(credentials.Key, credentials.Algorithm);
    try { return provider.Sign(inputBytes); }
    finally { if (provider is IDisposable d) d.Dispose(); }
}

static string B64Url(byte[] input) =>
    Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
