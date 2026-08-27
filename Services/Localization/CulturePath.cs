namespace BlazorWasmPortfolioGhAction.Services.Localization;

/// <summary>
/// URL helpers for site-wide /{vn|en}/... language prefixes.
/// </summary>
public static class CulturePath
{
    public static bool IsLangSegment(string? segment) =>
        segment is not null
        && (segment.Equals("vn", StringComparison.OrdinalIgnoreCase)
            || segment.Equals("en", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Splits "en/Utility/base64" → lang=en, remainder=Utility/base64.
    /// Splits "vn" → lang=vn, remainder="".
    /// Splits "Utility" → lang=null, remainder=Utility.
    /// </summary>
    public static void Split(string? relativePath, out string? lang, out string remainder)
    {
        lang = null;
        remainder = (relativePath ?? "").Split('?', 2)[0].Trim('/');

        if (string.IsNullOrEmpty(remainder))
            return;

        var slash = remainder.IndexOf('/');
        var first = slash < 0 ? remainder : remainder[..slash];
        if (!IsLangSegment(first))
            return;

        lang = first.ToLowerInvariant();
        remainder = slash < 0 ? "" : remainder[(slash + 1)..];
    }

    public static string Combine(string lang, string remainder)
    {
        lang = lang.ToLowerInvariant();
        remainder = (remainder ?? "").Trim('/');
        return string.IsNullOrEmpty(remainder) ? lang : $"{lang}/{remainder}";
    }

    /// <summary>
    /// Swap vn↔en in the current relative path (adds vn if missing).
    /// </summary>
    public static string Toggle(string? relativePath, string newLang)
    {
        Split(relativePath, out _, out var remainder);
        return Combine(newLang, remainder);
    }

    /// <summary>
    /// Ensure path has a lang prefix (default <paramref name="fallbackLang"/>).
    /// </summary>
    public static string EnsurePrefixed(string? relativePath, string fallbackLang)
    {
        Split(relativePath, out var lang, out var remainder);
        return Combine(lang ?? fallbackLang, remainder);
    }
}
