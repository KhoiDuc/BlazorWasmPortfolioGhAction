using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmPortfolioGhAction.Services;

public interface IHtmlSanitizerService
{
    string Sanitize(string? html);
    MarkupString ToSafeMarkup(string? html);
}

public partial class HtmlSanitizerService : IHtmlSanitizerService
{
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "strong", "b", "em", "i", "u", "s", "sub", "sup",
        "h1", "h2", "h3", "h4", "h5", "h6",
        "ul", "ol", "li", "blockquote", "pre", "code",
        "a", "img", "table", "thead", "tbody", "tr", "th", "td",
        "div", "span", "hr", "small"
    };

    public string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var result = ScriptTagRegex().Replace(html, string.Empty);
        result = StyleTagRegex().Replace(result, string.Empty);
        result = EventHandlerRegex().Replace(result, string.Empty);
        result = JavascriptUrlRegex().Replace(result, string.Empty);
        result = IframeTagRegex().Replace(result, string.Empty);
        result = ObjectTagRegex().Replace(result, string.Empty);
        result = EmbedTagRegex().Replace(result, string.Empty);

        return result;
    }

    public MarkupString ToSafeMarkup(string? html) => new(Sanitize(html));

    [GeneratedRegex(@"<script\b[^>]*>[\s\S]*?</script>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptTagRegex();

    [GeneratedRegex(@"<style\b[^>]*>[\s\S]*?</style>", RegexOptions.IgnoreCase)]
    private static partial Regex StyleTagRegex();

    [GeneratedRegex(@"\s(on\w+)\s*=\s*(""[^""]*""|'[^']*'|[^\s>]+)", RegexOptions.IgnoreCase)]
    private static partial Regex EventHandlerRegex();

    [GeneratedRegex(@"(href|src)\s*=\s*[""']\s*javascript:[^""']*[""']", RegexOptions.IgnoreCase)]
    private static partial Regex JavascriptUrlRegex();

    [GeneratedRegex(@"<iframe\b[^>]*>[\s\S]*?</iframe>", RegexOptions.IgnoreCase)]
    private static partial Regex IframeTagRegex();

    [GeneratedRegex(@"<object\b[^>]*>[\s\S]*?</object>", RegexOptions.IgnoreCase)]
    private static partial Regex ObjectTagRegex();

    [GeneratedRegex(@"<embed\b[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex EmbedTagRegex();
}
