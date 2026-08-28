using System.Text.Json.Serialization;
using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Services.OnlineTools;

public sealed class FontViewGlyph
{
    [JsonPropertyName("unicode")]
    public int Unicode { get; set; }

    [JsonPropertyName("hex")]
    public string Hex { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

public sealed class FontViewFont
{
    [JsonPropertyName("family")]
    public string Family { get; set; } = "";

    [JsonPropertyName("subfamily")]
    public string Subfamily { get; set; } = "";

    [JsonPropertyName("numGlyphs")]
    public int NumGlyphs { get; set; }

    [JsonPropertyName("glyphs")]
    public List<FontViewGlyph> Glyphs { get; set; } = new();
}

public sealed class FontViewService(IJSRuntime js)
{
    private readonly IJSRuntime _js = js;

    public async Task<FontViewFont?> LoadFontAsync(byte[] bytes)
    {
        var base64 = Convert.ToBase64String(bytes);
        return await _js.InvokeAsync<FontViewFont?>("fontViewTools.loadFont", base64);
    }

    public async Task<string?> RegisterFontFaceAsync(byte[] bytes, string family)
    {
        var base64 = Convert.ToBase64String(bytes);
        return await _js.InvokeAsync<string?>("fontViewTools.registerFontFace", base64, family);
    }
}