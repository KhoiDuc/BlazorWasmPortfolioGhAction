using System.Text;
using Spectre.Console;
using VnDesk.Render;

namespace VnDesk;

public static class Program
{
    public static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        var cfg = AppConfig.Load();
        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "data"));
        try
        {
            await new App(cfg).RunAsync();
        }
        catch (Exception ex)
        {
            Ui.Error(ex.Message);
        }
        AnsiConsole.MarkupLine("[grey]Bye.[/]");
    }
}
