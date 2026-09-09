namespace BlazorWasmPortfolioGhAction.Utils;

/// <summary>
/// Misc text helpers. Consolidates:
/// - BrokerWatchlistTable.razor <c>Truncate</c>
/// - VnAiPanel.razor <c>Slice</c>
/// - LineOperations.razor <c>ParseDelimiter</c>
/// - Portfolio.razor <c>FormatPhone</c>
/// - LuhnChecker.razor <c>LuhnCheck</c>
/// - FactorialCalculator.razor <c>Factorial</c>
/// </summary>
public static class TextUtils
{
    /// <summary>Truncate text to max chars, appending ellipsis if shortened.</summary>
    public static string Truncate(string text, int max) =>
        text.Length <= max ? text : text[..max] + "…";

    /// <summary>Slice text between two markers (case-insensitive). <paramref name="until"/> null → to end.</summary>
    public static string Slice(string text, string from, string? until)
    {
        var i = text.IndexOf(from, StringComparison.OrdinalIgnoreCase);
        if (i < 0) return "";
        i += from.Length;
        var j = until is null ? text.Length : text.IndexOf(until, i, StringComparison.OrdinalIgnoreCase);
        if (j < 0) j = text.Length;
        return text[i..j].Trim().TrimStart(':', '-', '\n', '\r', ' ');
    }

    /// <summary>Parse a delimiter string, expanding escape sequences \n \r \t \\.</summary>
    public static string ParseDelimiter(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return input
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\t", "\t")
            .Replace("\\\\", "\\");
    }

    /// <summary>Format Vietnamese phone: strip non-digits, prefix (+84).</summary>
    public static string FormatPhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("84") && digits.Length >= 11)
            return $"(+84) {digits[2..]}";
        if (digits.StartsWith("0") && digits.Length >= 10)
            return $"(+84) {digits[1..]}";
        return phone;
    }

    /// <summary>Luhn checksum validation (credit cards, IMEI, etc.).</summary>
    public static bool LuhnCheck(string digits)
    {
        if (string.IsNullOrEmpty(digits)) return false;
        return digits.All(char.IsDigit) && digits.Reverse()
            .Select(c => c - 48)
            .Select((thisNum, i) => i % 2 == 0
                ? thisNum
                : ((thisNum *= 2) > 9 ? thisNum - 9 : thisNum)
            ).Sum() % 10 == 0;
    }

    /// <summary>Factorial as double (overflow-tolerant up to 170!).</summary>
    public static double Factorial(int n)
    {
        double result = 1;
        for (var i = 2; i <= n; i++)
            result *= i;
        return result;
    }
}