using System.Globalization;

namespace BlazorWasmPortfolioGhAction.Data;

public readonly record struct LocalizedText(string Vi, string En)
{
    public string Get(CultureInfo culture) =>
        culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase) ? En : Vi;

    public string Get(bool isEnglish) => isEnglish ? En : Vi;
}

public readonly record struct LocalizedParagraphs(string[] Vi, string[] En)
{
    public string[] Get(CultureInfo culture) =>
        culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase) ? En : Vi;

    public string[] Get(bool isEnglish) => isEnglish ? En : Vi;
}
