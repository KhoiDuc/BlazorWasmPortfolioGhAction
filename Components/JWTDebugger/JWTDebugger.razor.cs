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
        LoadExampleForAlgorithm();
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

        LoadExampleForAlgorithm();
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
        var newAlg = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(newAlg) || !JwtAlgorithm.IsSupported(newAlg))
            return;

        _algorithm = newAlg;
        if (!_initialized || _syncing)
            return;

        LoadExampleForAlgorithm();
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

        var verifyAlgorithm = decode.Algorithm ?? _algorithm;
        var verify = Codec.Verify(
            _encoded,
            verifyAlgorithm,
            _hmacSecret,
            _secretIsBase64Url,
            _publicKeyPem);

        _canVerify = verify.CanVerify;
        _isSignatureVerified = verify.IsVerified;
        _verifyMessage = verify.Message;
    }

    private void LoadExample() => LoadExampleForAlgorithm();

    private void LoadExampleForAlgorithm()
    {
        ApplyExample(Codec.CreateExample(_algorithm));
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

    private void ClearEncoded()
    {
        _encoded = string.Empty;
        _structureMessage = null;
        _verifyMessage = null;
        _isValidStructure = false;
        _isSignatureVerified = false;
        _canVerify = false;
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
