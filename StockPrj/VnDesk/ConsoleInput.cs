using Spectre.Console;
using TextCopy;

namespace VnDesk;

public static class ConsoleInput
{
    public static bool IsQuit(string? s) =>
        s is not null && (s.Equals("q", StringComparison.OrdinalIgnoreCase)
                       || s.Equals("exit", StringComparison.OrdinalIgnoreCase)
                       || s.Equals("quit", StringComparison.OrdinalIgnoreCase));

    public static bool IsBack(string? s) =>
        s is not null && (s.Equals("back", StringComparison.OrdinalIgnoreCase)
                       || s.Equals("exit", StringComparison.OrdinalIgnoreCase)
                       || s.Equals("q", StringComparison.OrdinalIgnoreCase)
                       || s.Equals("b", StringComparison.OrdinalIgnoreCase));

    public static string ReadLine(string prompt)
    {
        AnsiConsole.Markup($"[grey]{Markup.Escape(prompt)}[/] ");
        return Console.ReadLine()?.Trim() ?? "";
    }

    public static string ReadKeyChoice(string prompt)
    {
        var line = ReadLine(prompt);
        return line.Trim();
    }

    /// <summary>Multiline until a blank line. 'clip' on first line pulls clipboard.</summary>
    public static string ReadMultiline(string prompt)
    {
        AnsiConsole.MarkupLine($"[grey]{Markup.Escape(prompt)}[/]");
        AnsiConsole.MarkupLine("[grey]Dan van ban. Dong trong = het. 'clip' = clipboard. back/q = huy.[/]");

        var first = Console.ReadLine();
        if (first is null)
            return "";

        var trimmed = first.Trim();
        if (IsBack(trimmed))
            return "";

        if (trimmed.Equals("clip", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return ClipboardService.GetText() ?? "";
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Clipboard: {Markup.Escape(ex.Message)}[/]");
                return "";
            }
        }

        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(first))
            lines.Add(first);

        while (true)
        {
            var line = Console.ReadLine();
            if (line is null || line.Length == 0)
                break;
            lines.Add(line);
        }

        return string.Join(Environment.NewLine, lines).Trim();
    }

    public static string ReadSymbol(string prompt = "Ma CK")
    {
        var s = ReadLine($"{prompt} (vd HPG):");
        return s.ToUpperInvariant();
    }

    public static List<string> ParseSymbols(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return [];

        return raw
            .Split([',', ' ', ';', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.Trim().ToUpperInvariant())
            .Where(x => x.Length >= 2 && x.Length <= 12)
            .Distinct()
            .ToList();
    }

    public static bool? ConfirmCr(string prompt = "[C]onfirm / [R]eject")
    {
        var s = ReadLine(prompt + ":").ToLowerInvariant();
        if (s is "c" or "confirm" or "y" or "yes")
            return true;
        if (s is "r" or "reject" or "n" or "no")
            return false;
        return null;
    }

    public static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Enter de tiep tuc...[/]");
        Console.ReadLine();
    }
}
