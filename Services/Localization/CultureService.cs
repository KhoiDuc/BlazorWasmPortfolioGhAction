using System.Globalization;
using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Services.Localization;

public sealed class CultureService : ICultureService
{
    public const string StorageKey = "app-culture";
    public const string DefaultCultureName = "vi";
    public const string DefaultUrlLang = "vn";

    private readonly IJSRuntime _js;
    private CultureInfo _current = new(DefaultCultureName);

    public CultureService(IJSRuntime js) => _js = js;

    public CultureInfo Current => _current;
    public string UrlLang => IsEnglish ? "en" : "vn";
    public bool IsEnglish =>
        _current.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase);

    public event Action? CultureChanged;

    public async Task InitializeAsync()
    {
        string? saved = null;
        try
        {
            saved = await _js.InvokeAsync<string?>("cultureManager.get");
        }
        catch
        {
            // JS not ready; keep default Vietnamese
        }

        Apply(Normalize(saved), persist: false, notify: false);
        await SyncDocumentLangAsync();
    }

    public async Task SetCultureAsync(string cultureOrUrlLang)
    {
        var cultureName = Normalize(cultureOrUrlLang);
        if (_current.TwoLetterISOLanguageName.Equals(cultureName, StringComparison.OrdinalIgnoreCase))
            return;

        Apply(cultureName, persist: true, notify: true);
        await SyncDocumentLangAsync();
    }

    public Task ToggleAsync() =>
        SetCultureAsync(IsEnglish ? DefaultCultureName : "en");

    private void Apply(string cultureName, bool persist, bool notify)
    {
        _current = new CultureInfo(cultureName);
        CultureInfo.DefaultThreadCurrentCulture = _current;
        CultureInfo.DefaultThreadCurrentUICulture = _current;

        if (persist)
        {
            _ = PersistAsync(cultureName);
        }

        if (notify)
            CultureChanged?.Invoke();
    }

    private async Task PersistAsync(string cultureName)
    {
        try
        {
            await _js.InvokeVoidAsync("cultureManager.set", cultureName);
        }
        catch
        {
            // ignore
        }
    }

    private async Task SyncDocumentLangAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("cultureManager.setDocumentLang", _current.TwoLetterISOLanguageName);
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    /// Accepts URL langs (vn/en) or culture names (vi/en/vi-VN/en-US).
    /// </summary>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DefaultCultureName;

        var v = value.Trim().ToLowerInvariant();
        return v switch
        {
            "en" or "en-us" or "en-gb" => "en",
            "vn" or "vi" or "vi-vn" => "vi",
            _ => DefaultCultureName
        };
    }

    public static bool IsSupportedUrlLang(string? lang) =>
        lang is "vn" or "en";
}
