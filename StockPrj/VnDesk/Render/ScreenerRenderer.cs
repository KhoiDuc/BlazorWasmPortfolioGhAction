using Spectre.Console;
using VnDesk.Models;

namespace VnDesk.Render;

public static class ScreenerRenderer
{
    public static void Results(IReadOnlyList<PotentialStock> rows)
    {
        Ui.Header("Screener tiem nang");
        if (rows.Count == 0)
        {
            Ui.Warn("Khong co ma dat nguong.");
            return;
        }

        var t = new Table().Border(TableBorder.Rounded).BorderColor(Color.DarkOrange);
        t.AddColumn("Ma");
        t.AddColumn(new TableColumn("Gia").RightAligned());
        t.AddColumn(new TableColumn("%").RightAligned());
        t.AddColumn(new TableColumn("KL").RightAligned());
        t.AddColumn(new TableColumn("Diem").RightAligned());
        t.AddColumn("Ly do");

        foreach (var s in rows.Take(30))
        {
            t.AddRow(
                Ui.E(s.Symbol),
                $"{s.LastPrice:N2}",
                Ui.Pct(s.PriceChange),
                $"{s.Volume:N0}",
                $"{s.PotentialScore:N1}",
                Ui.E(s.Reason.Length > 60 ? s.Reason[..57] + "..." : s.Reason));
        }
        AnsiConsole.Write(t);
        Ui.Info($"{rows.Count} ma. [d] drill-down 1 ma. json de dump.");
    }

    public static void Watchlist(IReadOnlyList<WatchlistScore> scores, IReadOnlyList<string> criteria)
    {
        Ui.Header("Watchlist cham diem");
        Ui.Panel("Tieu chi", string.Join("\n", criteria.Select((c, i) => $"{i + 1}. {Ui.E(c)}")));

        var t = new Table().Border(TableBorder.Rounded).BorderColor(Color.DarkOrange);
        t.AddColumn("Ma");
        t.AddColumn("Liq");
        t.AddColumn("Trend");
        t.AddColumn("RSI");
        t.AddColumn("Support");
        t.AddColumn("Vol");
        t.AddColumn("Tong");
        t.AddColumn("KQ");

        foreach (var s in scores.OrderByDescending(x => x.Total).Take(10))
        {
            var tag = s.Pass ? $"[{Ui.Up}]Dat[/]" : $"[{Ui.Down}]Loai[/]";
            t.AddRow(Ui.E(s.Symbol), s.Liquidity.ToString(), s.Trend.ToString(), s.Rsi.ToString(),
                s.NearSupport.ToString(), s.Volume.ToString(), s.Total.ToString(), tag);
        }
        AnsiConsole.Write(t);
    }
}
