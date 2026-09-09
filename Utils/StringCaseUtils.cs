using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace BlazorWasmPortfolioGhAction.Utils;

/// <summary>
/// String case/casing conversion utilities. Consolidates:
/// - <c>Common.cs</c> (FirstCharToUpper, Invert string/char)
/// - <c>Components/StringConverter/StringConverter.razor</c> statics (Sentence/Camel/Pascal/Alternating/SplitWords)
/// - <c>Components/OnlineTools/JsonToTypeScript.razor</c> + <c>JsonToJava.razor</c> ToCamelCase/ToPascalCase.
/// </summary>
public static class StringCaseUtils
{
    /// <summary>Capitalize first char. Throws on null/empty.</summary>
    public static string FirstCharToUpper(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };

    /// <summary>Invert case of every letter in the string.</summary>
    public static string Invert(this string s)
    {
        var chars = s.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
            chars[i] = chars[i].Invert();
        return new string(chars);
    }

    /// <summary>Invert case of a single letter.</summary>
    public static char Invert(this char c) =>
        !char.IsLetter(c) ? c : char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c);

    /// <summary>Sentence case: capitalize first letter after . ! ? separators.</summary>
    public static string ToSentenceCase(string text)
    {
        var parts = text.Split([". ", "! ", "? "], StringSplitOptions.None);
        return string.Join(". ", parts.Select(s =>
            string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..].ToLowerInvariant()));
    }

    /// <summary>camelCase. Splits on whitespace, '_', '-', and case transitions.</summary>
    public static string ToCamelCase(string text)
    {
        var words = SplitWords(text);
        if (words.Count == 0) return text;
        return words[0].ToLowerInvariant() + string.Concat(words.Skip(1).Select(Pascalize));
    }

    /// <summary>PascalCase. Splits on whitespace, '_', '-', and case transitions.</summary>
    public static string ToPascalCase(string text) =>
        string.Concat(SplitWords(text).Select(Pascalize));

    /// <summary>Capitalize first char of one word, lower rest.</summary>
    public static string Pascalize(string w) =>
        string.IsNullOrEmpty(w) ? w : char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant();

    /// <summary>aLtErNaTiNg cAsE — letters only, toggling starting with lowercase.</summary>
    public static string ToAlternating(string text)
    {
        var sb = new StringBuilder(text.Length);
        var upper = true;
        foreach (var c in text)
        {
            if (char.IsLetter(c))
            {
                sb.Append(upper ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
                upper = !upper;
            }
            else sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Split text into words. Recognizes whitespace, '_', '-', '.', and case transitions
    /// (e.g. "helloWorld" → ["hello","World"]). Digits attach to preceding letter run.
    /// </summary>
    public static List<string> SplitWords(string text)
    {
        var words = new List<string>();
        var current = new StringBuilder();
        foreach (var c in text)
        {
            if (char.IsLetterOrDigit(c)) current.Append(c);
            else if (current.Length > 0)
            {
                words.Add(current.ToString());
                current.Clear();
            }
        }
        if (current.Length > 0) words.Add(current.ToString());
        return words;
    }

    [Conditional("DEBUG")]
    public static void SelfCheck()
    {
        Debug.Assert(ToCamelCase("hello world") == "helloWorld");
        Debug.Assert(ToPascalCase("hello world") == "HelloWorld");
        Debug.Assert(FirstCharToUpper("hi") == "Hi");
        Debug.Assert(Invert("AbC") == "aBc");
    }
}