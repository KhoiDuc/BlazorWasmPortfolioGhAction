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
        // Prefer culture already applied at startup (Program.SetStartupCultureAsync)
        var startup = CultureInfo.DefaultThreadCurrentUICulture?.TwoLetterISOLanguageName;
        if (!string.IsNullOrEmpty(startup)
            && (startup.Equals("vi", StringComparison.OrdinalIgnoreCase)
                || startup.Equals("en", StringComparison.OrdinalIgnoreCase)))
        {
            Apply(Normalize(startup), persist: false, notify: false);
        }

        string? saved = null;
        try
        {
            saved = await _js.InvokeAsync<string?>("cultureManager.get");
        }
        catch
        {
            // JS not ready; keep current
        }

        if (!string.IsNullOrEmpty(saved))
            Apply(Normalize(saved), persist: false, notify: false);

        await SyncDocumentLangAsync();
    }

    public async Task SetCultureAsync(string cultureOrUrlLang)
    {
        var cultureName = Normalize(cultureOrUrlLang);
        if (_current.TwoLetterISOLanguageName.Equals(cultureName, StringComparison.OrdinalIgnoreCase))
            return;

        Apply(cultureName, persist: false, notify: true);
        // Must finish writing localStorage BEFORE any force-reload, otherwise boot
        // still sees the old culture and never loads the other satellite assembly.
        await PersistAsync(cultureName);
        await SyncDocumentLangAsync();
    }

    public Task ToggleAsync() =>
        SetCultureAsync(IsEnglish ? DefaultCultureName : "en");

    private void Apply(string cultureName, bool persist, bool notify)
    {
        CultureInfo culture;
        try
        {
            culture = CultureInfo.GetCultureInfo(cultureName);
        }
        catch (CultureNotFoundException)
        {
            culture = CultureInfo.GetCultureInfo(DefaultCultureName);
        }

        _current = culture;
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
