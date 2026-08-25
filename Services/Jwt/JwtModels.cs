namespace BlazorWasmPortfolioGhAction.Services.Jwt;

public sealed record JwtDecodeResult(
    bool IsValidStructure,
    string? HeaderJson,
    string? PayloadJson,
    string? HeaderSegment,
    string? PayloadSegment,
    string? SignatureSegment,
    string? Algorithm,
    string? ErrorMessage);

public sealed record JwtVerifyResult(
    bool IsVerified,
    bool CanVerify,
    string? Message);

public sealed record JwtEncodeResult(
    bool Success,
    string? Token,
    string? ErrorMessage);

public sealed record JwtExample(
    string Token,
    string Algorithm,
    string HeaderJson,
    string PayloadJson,
    string? HmacSecret,
    string? PublicKeyPem,
    string? PrivateKeyPem);
