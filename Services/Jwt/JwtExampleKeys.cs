namespace BlazorWasmPortfolioGhAction.Services.Jwt;

public static class JwtExampleKeys
{
    public const string HmacSecret = "a-string-secret-at-least-256-bits-long";

    public const string HmacSecret384 = "a-string-secret-at-least-384-bits-long-for-hmac-sha384-example-key!!";

    public const string HmacSecret512 = "a-string-secret-at-least-512-bits-long-for-hmac-sha512-example-key-value!!!!!!";

    public static string GetHmacSecret(string algorithm) => algorithm switch
    {
        "HS384" => HmacSecret384,
        "HS512" => HmacSecret512,
        _ => HmacSecret
    };

    public static bool MatchesExampleKeys(
        string algorithm,
        string? hmacSecret,
        bool secretIsBase64Url,
        string? publicKeyPem)
    {
        return JwtAlgorithm.GetKeyKind(algorithm) switch
        {
            JwtKeyKind.None => true,
            JwtKeyKind.Hmac => !secretIsBase64Url
                && string.Equals(hmacSecret, GetHmacSecret(algorithm), StringComparison.Ordinal),
            JwtKeyKind.Rsa => PemEquals(publicKeyPem, RsaPublicKeyPem),
            JwtKeyKind.Ecdsa => PemEquals(publicKeyPem, GetEcKeys(algorithm).Public),
            _ => false
        };
    }

    private static bool PemEquals(string? left, string? right) =>
        NormalizePem(left) == NormalizePem(right);

    private static string NormalizePem(string? pem) =>
        string.IsNullOrWhiteSpace(pem)
            ? string.Empty
            : string.Concat(pem.Where(c => !char.IsWhiteSpace(c)));

    public const string RsaPrivateKeyPem =
        """
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

    public const string RsaPublicKeyPem =
        """
        -----BEGIN RSA PUBLIC KEY-----
        MIIBCgKCAQEApLr0Yl0uZLUsswBAe2sQIojIhQ63khSPo3JuriLIduZbTLOSujz5
        W6KPDU2vfmfmksY3d8ayxNKxOIhhadQcMYhv5FxweBPwOVXyFaR9jBjjiWsqV5sv
        HCX1Tv0zTGc31YHXKGOnNiyYbZ6ilTJrVdGfc7+mLRDt9ETK8RlU/6Fke0MqLAzC
        0jqNC5eWVr5l6L6RZf7PZgIe+FoP3uHSGABgE1CAMyW6b36ma5AmAPa/nLnvRnSP
        Y7vavn0WUw1egjmy4FbchUeHIc/KHqKPM5d3Kb8Fft4VPsaf5s+vapw2Rx99nOKE
        cUaaa/yw6M2kRCGkpexjB3N4JDiXOK+1cQIDAQAB
        -----END RSA PUBLIC KEY-----
        """;

    public static (string Public, string Private) GetEcKeys(string algorithm) => algorithm switch
    {
        "ES384" => (Es384PublicKeyPem, Es384PrivateKeyPem),
        "ES512" => (Es512PublicKeyPem, Es512PrivateKeyPem),
        _ => (Es256PublicKeyPem, Es256PrivateKeyPem)
    };

    public const string Es256PublicKeyPem =
        """
        -----BEGIN PUBLIC KEY-----
        MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEu7U4pkTIVNbKwbXYZNUkoDNxl1WI
        02vqikFH4enJrLHuGYgBY3uYLydC/iMVNb9JMjnFL+9WHxDnd1/KciP6Uw==
        -----END PUBLIC KEY-----
        """;

    public const string Es256PrivateKeyPem =
        """
        -----BEGIN EC PRIVATE KEY-----
        MHcCAQEEINHq0XMjkrQkukDn7vhF3hdBE2SY5gizICByD0INh+AqoAoGCCqGSM49
        AwEHoUQDQgAEu7U4pkTIVNbKwbXYZNUkoDNxl1WI02vqikFH4enJrLHuGYgBY3uY
        LydC/iMVNb9JMjnFL+9WHxDnd1/KciP6Uw==
        -----END EC PRIVATE KEY-----
        """;

    public const string Es384PublicKeyPem =
        """
        -----BEGIN PUBLIC KEY-----
        MHYwEAYHKoZIzj0CAQYFK4EEACIDYgAEomFFPr6i8F3V16C+rcZ7xDudjbAExU3w
        gNvZp3i5RtnnJMP8+ZTRi8L3aLxhN2Qx0TkZKZdtV64EF9gs5XhHutpdIFSXNp1W
        LBTYXwM0l5PtLM2DzD+kZainyOmElylp
        -----END PUBLIC KEY-----
        """;

    public const string Es384PrivateKeyPem =
        """
        -----BEGIN EC PRIVATE KEY-----
        MIGkAgEBBDBB/6Xn/tUNX7Fh9dsBdGMgSfGcXyeFJLCEzRCs0XKm9fzNnfcInuat
        L7e43d+VH2agBwYFK4EEACKhZANiAASiYUU+vqLwXdXXoL6txnvEO52NsATFTfCA
        29mneLlG2eckw/z5lNGLwvdovGE3ZDHRORkpl21XrgQX2CzleEe62l0gVJc2nVYs
        FNhfAzSXk+0szYPMP6RlqKfI6YSXKWk=
        -----END EC PRIVATE KEY-----
        """;

    public const string Es512PublicKeyPem =
        """
        -----BEGIN PUBLIC KEY-----
        MIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQAFgd6eY3ib0/+LLvlcb9868uF7oFU
        9qqMS8SsomnoRwrYvIT7/FG8oszanctwfF5/7i6RqC8BVrD5rU32aaIGHSYAhEvV
        A/9CDIv8/UBhhT0jUmv85oq++fL9mYeqkSoNDdttH0YYt/DJctjCQvJa+/Mb0ziT
        8xf4a4oVH96Fii8XkgI=
        -----END PUBLIC KEY-----
        """;

    public const string Es512PrivateKeyPem =
        """
        -----BEGIN EC PRIVATE KEY-----
        MIHcAgEBBEIAKhbBKISlktNOIJjF3GbLPMOgArJk8CdtrAErMZfncB1dnIpee4ld
        ufSr9Q6tyba8gbg4i9QzTdmhGSApIFUT4GigBwYFK4EEACOhgYkDgYYABAAWB3p5
        jeJvT/4su+Vxv3zry4XugVT2qoxLxKyiaehHCti8hPv8UbyizNqdy3B8Xn/uLpGo
        LwFWsPmtTfZpogYdJgCES9UD/0IMi/z9QGGFPSNSa/zmir758v2Zh6qRKg0N220f
        Rhi38Mly2MJC8lr78xvTOJPzF/hrihUf3oWKLxeSAg==
        -----END EC PRIVATE KEY-----
        """;
}
