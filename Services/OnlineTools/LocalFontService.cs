using System.Text.Json.Serialization;
using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Services.OnlineTools;

public sealed class LocalFontInfo
{
    [JsonPropertyName("postscriptName")]
    public string PostscriptName { get; set; } = "";

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = "";

    [JsonPropertyName("family")]
    public string Family { get; set; } = "";

    [JsonPropertyName("style")]
    public string Style { get; set; } = "";
}

public sealed class LocalFontService(IJSRuntime js)
{
    private readonly IJSRuntime _js = js;

    public ValueTask<bool> IsSupportedAsync() =>
        _js.InvokeAsync<bool>("localFontTools.isSupported");

    public async ValueTask<string> GetPermissionAsync()
    {
        try
        {
            return await _js.InvokeAsync<string>("localFontTools.getPermission");
        }
        catch
        {
            return "prompt";
        }
    }

    public ValueTask<List<LocalFontInfo>> QueryAsync() =>
        _js.InvokeAsync<List<LocalFontInfo>>("localFontTools.query");

    /// <summary>Fetch the font file bytes (base64) and detect the SFNT type. Returns (bytes, type).</summary>
    public async ValueTask<(byte[]? bytes, string type)> GetFontFileAsync(string postscriptName)
    {
        var result = await _js.InvokeAsync<FontFileResult?>("localFontTools.getFontFile", postscriptName);
        if (result is null || string.IsNullOrEmpty(result.Base64))
            return (null, "unknown");

        var bytes = Convert.FromBase64String(result.Base64);
        var type = DetectFontType(bytes);
        return (bytes, type);
    }

    private static string DetectFontType(byte[] bytes)
    {
        if (bytes.Length < 4) return "unknown";
        var sfnt = System.Text.Encoding.UTF8.GetString(bytes, 0, 4);
        return sfnt switch
        {
            "\x00\x01\x00\x00" or "true" or "typ1" => "truetype",
            "OTTO" => "cff",
            "wOF\x00" => "woff",
            _ => "unknown"
        };
    }

    private sealed class FontFileResult
    {
        [JsonPropertyName("base64")]
        public string Base64 { get; set; } = "";
    }
}