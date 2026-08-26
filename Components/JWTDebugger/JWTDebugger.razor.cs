using BlazorWasmPortfolioGhAction.Services;
using BlazorWasmPortfolioGhAction.Services.Jwt;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmPortfolioGhAction.Components.JWTDebugger;

public partial class JWTDebugger
{
    [Inject] private JwtCodec Codec { get; set; } = default!;
    [Inject] private IClipboardService Clipboard { get; set; } = default!;

    public enum JwtMode { Decoder, Encoder }
    public enum JsonView { Json, Claims }

    private JwtMode _mode = JwtMode.Decoder;
    private JsonView _headerView = JsonView.Json;
    private JsonView _payloadView = JsonView.Json;

    private string _encoded = string.Empty;
    private string _headerJson = string.Empty;
    private string _payloadJson = string.Empty;
    private string _algorithm = "HS256";
    private string _hmacSecret = string.Empty;
    private bool _secretIsBase64Url;
    private string _publicKeyPem = string.Empty;
    private string _privateKeyPem = string.Empty;

    private bool _isValidStructure;
    private bool _isSignatureVerified;
    private bool _canVerify;
    private string? _structureMessage;
    private string? _verifyMessage;
    private string? _headerError;
    private string? _payloadError;
    private string? _encodeError;

    private bool _syncing;
    private bool _initialized;

    private bool IsDecoder => _mode == JwtMode.Decoder;
    private bool IsEncoder => _mode == JwtMode.Encoder;

    protected override void OnInitialized()
    {
        LoadDecoderExample();
        _initialized = true;
    }

    private bool IsHeaderValid =>
        !string.IsNullOrWhiteSpace(_headerJson)
        && _headerError is null
        && Codec.IsValidJson(_headerJson);

    private bool IsPayloadValid =>
        !string.IsNullOrWhiteSpace(_payloadJson)
        && _payloadError is null
        && Codec.IsValidJson(_payloadJson);

    private JwtKeyKind KeyKind => JwtAlgorithm.GetKeyKind(_algorithm);

    private void SetMode(JwtMode mode)
    {
        if (_mode == mode)
            return;

        _mode = mode;
        _headerView = JsonView.Json;
        _payloadView = JsonView.Json;
        _headerError = null;
        _payloadError = null;
        _encodeError = null;
        _structureMessage = null;
        _verifyMessage = null;

        if (mode == JwtMode.Decoder)
            LoadDecoderExample();
        else
            LoadEncoderExample();
    }

    private void SetHeaderView(JsonView view) => _headerView = view;
    private void SetPayloadView(JsonView view) => _payloadView = view;

    private void OnEncodedInput(ChangeEventArgs e)
    {
        _encoded = e.Value?.ToString() ?? string.Empty;
        if (!_initialized || _syncing)
            return;

        SyncFromEncoded();
    }

    private void OnHeaderInput(ChangeEventArgs e)
    {
        _headerJson = e.Value?.ToString() ?? string.Empty;
        if (!_initialized || _syncing)
            return;

        if (IsEncoder || HasSigningMaterial())
            SyncFromJson();
    }

    private void OnPayloadInput(ChangeEventArgs e)
    {
        _payloadJson = e.Value?.ToString() ?? string.Empty;
        if (!_initialized || _syncing)
            return;

        if (IsEncoder || HasSigningMaterial())
            SyncFromJson();
    }

    private void OnAlgorithmChanged(ChangeEventArgs e)
    {
        _algorithm = e.Value?.ToString() ?? "HS256";
        if (!_initialized || _syncing)
            return;

        if (IsEncoder)
        {
            ApplyAlgorithmToHeaderJson();
            EnsureEncoderKeysForAlgorithm();
            SyncFromJson();
        }
        else
        {
            VerifyCurrent();
        }
    }

    private void ApplyAlgorithmToHeaderJson()
    {
        if (!Codec.IsValidJson(_headerJson))
            return;

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(_headerJson);
            var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var prop in doc.RootElement.EnumerateObject())
                dict[prop.Name] = prop.Value.ValueKind == System.Text.Json.JsonValueKind.String
                    ? prop.Value.GetString()
                    : System.Text.Json.JsonSerializer.Deserialize<object>(prop.Value.GetRawText());

            dict["typ"] = "JWT";
            dict["alg"] = _algorithm;
            _headerJson = System.Text.Json.JsonSerializer.Serialize(
                dict,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            // keep existing header text if update fails
        }
    }

    private void EnsureEncoderKeysForAlgorithm()
    {
        if (KeyKind is JwtKeyKind.Rsa or JwtKeyKind.Ecdsa && string.IsNullOrWhiteSpace(_privateKeyPem))
        {
            var example = Codec.CreateRs256Example();
            _publicKeyPem = example.PublicKeyPem ?? string.Empty;
            _privateKeyPem = example.PrivateKeyPem ?? string.Empty;
        }
        else if (KeyKind == JwtKeyKind.Hmac && string.IsNullOrWhiteSpace(_hmacSecret))
        {
            _hmacSecret = "a-string-secret-at-least-256-bits-long";
        }
    }

    private void OnHmacSecretInput(ChangeEventArgs e)
    {
        _hmacSecret = e.Value?.ToString() ?? string.Empty;
        if (!_initialized || _syncing)
            return;

        if (IsEncoder || !string.IsNullOrWhiteSpace(_hmacSecret))
            SyncFromJson();
        else
            VerifyCurrent();
    }

    private void OnPublicKeyInput(ChangeEventArgs e)
    {
        _publicKeyPem = e.Value?.ToString() ?? string.Empty;
        if (!_initialized || _syncing)
            return;
        VerifyCurrent();
    }

    private void OnPrivateKeyInput(ChangeEventArgs e)
    {
        _privateKeyPem = e.Value?.ToString() ?? string.Empty;
        if (!_initialized || _syncing)
            return;

        if (IsEncoder)
            SyncFromJson();
        else
            VerifyCurrent();
    }

    private void OnSecretEncodingChanged()
    {
        if (!_initialized || _syncing)
            return;

        if (IsEncoder || !string.IsNullOrWhiteSpace(_hmacSecret))
            SyncFromJson();
        else
            VerifyCurrent();
    }

    private bool HasSigningMaterial() =>
        KeyKind switch
        {
            JwtKeyKind.None => true,
            JwtKeyKind.Hmac => !string.IsNullOrWhiteSpace(_hmacSecret),
            JwtKeyKind.Rsa or JwtKeyKind.Ecdsa => !string.IsNullOrWhiteSpace(_privateKeyPem),
            _ => false
        };

    private void SyncFromEncoded()
    {
        _syncing = true;
        try
        {
            var result = Codec.Decode(_encoded);
            _isValidStructure = result.IsValidStructure;
            _structureMessage = result.ErrorMessage;

            if (!result.IsValidStructure)
            {
                _headerError = null;
                _payloadError = null;
                _encodeError = null;
                VerifyCurrent();
                return;
            }

            _headerJson = result.HeaderJson ?? string.Empty;
            _payloadJson = result.PayloadJson ?? string.Empty;
            _headerError = null;
            _payloadError = null;
            _encodeError = null;

            if (!string.IsNullOrEmpty(result.Algorithm) && JwtAlgorithm.IsSupported(result.Algorithm))
                _algorithm = result.Algorithm!;

            VerifyCurrent();
        }
        finally
        {
            _syncing = false;
        }
    }

    private void SyncFromJson()
    {
        _headerError = Codec.IsValidJson(_headerJson) ? null : "Invalid header JSON";
        _payloadError = Codec.IsValidJson(_payloadJson) ? null : "Invalid payload JSON";

        if (_headerError is not null || _payloadError is not null)
        {
            _encodeError = "Fix JSON errors before encoding.";
            VerifyCurrent();
            return;
        }

        _syncing = true;
        try
        {
            var result = Codec.Encode(
                _headerJson,
                _payloadJson,
                _algorithm,
                _hmacSecret,
                _secretIsBase64Url,
                _privateKeyPem);

            _encodeError = result.Success ? null : result.ErrorMessage;

            if (result.Success && !string.IsNullOrEmpty(result.Token))
            {
                _encoded = result.Token;
                _isValidStructure = true;
                _structureMessage = null;
            }

            VerifyCurrent();
        }
        finally
        {
            _syncing = false;
        }
    }

    private void VerifyCurrent()
    {
        if (string.IsNullOrWhiteSpace(_encoded))
        {
            _isValidStructure = false;
            _canVerify = false;
            _isSignatureVerified = false;
            _verifyMessage = null;
            if (IsEncoder)
                _structureMessage = null;
            return;
        }

        var decode = Codec.Decode(_encoded);
        _isValidStructure = decode.IsValidStructure;
        _structureMessage = decode.ErrorMessage;

        if (!decode.IsValidStructure)
        {
            _canVerify = false;
            _isSignatureVerified = false;
            _verifyMessage = null;
            return;
        }

        var verify = Codec.Verify(
            _encoded,
            _algorithm,
            _hmacSecret,
            _secretIsBase64Url,
            _publicKeyPem);

        _canVerify = verify.CanVerify;
        _isSignatureVerified = verify.IsVerified;
        _verifyMessage = verify.Message;
    }

    private void LoadExample()
    {
        if (_mode == JwtMode.Decoder)
            LoadDecoderExample();
        else
            LoadEncoderExample();
    }

    private void LoadDecoderExample()
    {
        ApplyExample(Codec.CreateHs256Example());
    }

    private void LoadEncoderExample()
    {
        _syncing = true;
        try
        {
            if (KeyKind is JwtKeyKind.Rsa or JwtKeyKind.Ecdsa)
            {
                var rsaExample = Codec.CreateRs256Example();
                _algorithm = rsaExample.Algorithm;
                _headerJson = rsaExample.HeaderJson;
                _payloadJson = rsaExample.PayloadJson;
                _hmacSecret = string.Empty;
                _publicKeyPem = rsaExample.PublicKeyPem ?? string.Empty;
                _privateKeyPem = rsaExample.PrivateKeyPem ?? string.Empty;
            }
            else
            {
                var example = Codec.CreateHs256Example();
                _algorithm = example.Algorithm;
                _headerJson = example.HeaderJson;
                _payloadJson = example.PayloadJson;
                _hmacSecret = example.HmacSecret ?? string.Empty;
                _publicKeyPem = string.Empty;
                _privateKeyPem = string.Empty;
            }

            _secretIsBase64Url = false;
            _encoded = string.Empty;
            _headerError = null;
            _payloadError = null;
            _encodeError = null;
            _structureMessage = null;
            _verifyMessage = null;

            SyncFromJson();
        }
        finally
        {
            _syncing = false;
        }
    }

    private void ApplyExample(JwtExample example)
    {
        _syncing = true;
        try
        {
            _encoded = example.Token;
            _headerJson = example.HeaderJson;
            _payloadJson = example.PayloadJson;
            _algorithm = example.Algorithm;
            _hmacSecret = example.HmacSecret ?? string.Empty;
            _publicKeyPem = example.PublicKeyPem ?? string.Empty;
            _privateKeyPem = example.PrivateKeyPem ?? string.Empty;
            _secretIsBase64Url = false;
            _headerError = null;
            _payloadError = null;
            _encodeError = null;
            VerifyCurrent();
        }
        finally
        {
            _syncing = false;
        }
    }

    private void ClearAll()
    {
        _encoded = string.Empty;
        _headerJson = string.Empty;
        _payloadJson = string.Empty;
        _hmacSecret = string.Empty;
        _publicKeyPem = string.Empty;
        _privateKeyPem = string.Empty;
        _headerError = null;
        _payloadError = null;
        _encodeError = null;
        _structureMessage = null;
        _verifyMessage = null;
        _isValidStructure = false;
        _isSignatureVerified = false;
        _canVerify = false;
    }

    private Task CopyEncoded() => Clipboard.CopyTextAsync(_encoded);
    private Task CopyHeader() => Clipboard.CopyTextAsync(_headerJson);
    private Task CopyPayload() => Clipboard.CopyTextAsync(_payloadJson);
}
