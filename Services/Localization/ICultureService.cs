using System.Globalization;

namespace BlazorWasmPortfolioGhAction.Services.Localization;

public interface ICultureService
{
    CultureInfo Current { get; }
    string UrlLang { get; }
    bool IsEnglish { get; }
    event Action? CultureChanged;
    Task InitializeAsync();
    Task SetCultureAsync(string cultureOrUrlLang);
    Task ToggleAsync();
}
