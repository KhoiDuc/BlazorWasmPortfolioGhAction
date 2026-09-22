using System.Text.Json;
using Spectre.Console;

namespace VnDesk.Render;

public static class Ui
{
    public const string Accent = "darkorange";
    public const string Up = "green";
    public const string Down = "red";
    public const string Hold = "yellow";

    public static string E(string? s) => Markup.Escape(s ?? "");

    public static void Header(string title)
    {
        AnsiConsole.Write(new Rule($"[{Accent} bold]{E(title)}[/]").RuleStyle(Accent));
    }

    public static void SubHeader(string title)
    {
        AnsiConsole.Write(new Rule($"[{Accent}]{E(title)}[/]").RuleStyle("grey"));
    }

    public static void Warn(string msg) => AnsiConsole.MarkupLine($"[{Hold}]{E(msg)}[/]");
    public static void Error(string msg) => AnsiConsole.MarkupLine($"[red]{E(msg)}[/]");
    public static void Ok(string msg) => AnsiConsole.MarkupLine($"[green]{E(msg)}[/]");
    public static void Info(string msg) => AnsiConsole.MarkupLine($"[grey]{E(msg)}[/]");

    public static string Pct(decimal value)
    {
        var color = value > 0 ? Up : value < 0 ? Down : Hold;
        return $"[{color}]{value:+0.00;-0.00;0.00}%[/]";
    }

    public static string Signed(decimal value)
    {
        var color = value > 0 ? Up : value < 0 ? Down : Hold;
        return $"[{color}]{value:+0.00;-0.00;0.00}[/]";
    }

    public static void Panel(string title, string body, string? border = null)
    {
        var p = new Panel(new Markup(body))
            .Header($"[{Accent}]{E(title)}[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.DarkOrange);
        AnsiConsole.Write(p);
    }

    public static void JsonPanel(string title, object obj)
    {
        var json = JsonSerializer.Serialize(obj, JsonStore.Pretty);
        var escaped = E(json);
        var p = new Panel(new Markup($"[grey]{escaped}[/]"))
            .Header($"[{Accent}]{E(title)}[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey);
        AnsiConsole.Write(p);
    }

    public static void MenuLines(params (string key, string label)[] items)
    {
        foreach (var (key, label) in items)
        {
            AnsiConsole.Write("  [");
            AnsiConsole.Markup($"[{Accent} bold]{E(key)}[/]");
            AnsiConsole.WriteLine($"] {label}");
        }
    }
}
