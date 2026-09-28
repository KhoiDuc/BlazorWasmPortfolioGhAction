using Spectre.Console;
using VnDesk.Models;

namespace VnDesk.Render;

public static class MarketRenderer
{
    public static void Indices(IReadOnlyList<MarketIndex> indices)
    {
        Ui.Header("Chỉ số thị trường");
        if (indices.Count == 0)
        {
            Ui.Warn("Chưa có dữ liệu. Bấm r để reload.");
            return;
        }

        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.DarkOrange);
        table.AddColumn("Chỉ số");
        table.AddColumn(new TableColumn("Giá trị").RightAligned());
        table.AddColumn(new TableColumn("Thay đổi").RightAligned());
        table.AddColumn(new TableColumn("%").RightAligned());
        table.AddColumn(new TableColumn("KL").RightAligned());

        foreach (var i in indices.OrderBy(x => x.Name))
        {
            table.AddRow(
                Ui.E(i.Name),
                $"{i.ParsedIndex:N2}",
                Ui.Signed((decimal)i.ParsedChange),
                Ui.Pct((decimal)i.ParsedPercent),
                $"{i.ParsedVolume:N0}");
        }
        AnsiConsole.Write(table);

        var up = indices.Count(x => x.ParsedChange >= 0);
        var down = indices.Count(x => x.ParsedChange < 0);
        Ui.Panel("Breadth", $"Tăng: [{Ui.Up}]{up}[/]  Giảm: [{Ui.Down}]{down}[/]  Tổng: {indices.Count}");
    }

    public static void Movers(IReadOnlyList<StockData> stocks)
    {
        if (stocks.Count == 0)
        {
            Ui.Warn("Chưa có bảng giá. Reload (r) để tải VNDirect.");
            return;
        }

        RenderTop("Top tăng", stocks.Where(s => s.PercentChange > 0).OrderByDescending(s => s.PercentChange).Take(10).ToList());
        RenderTop("Top giảm", stocks.Where(s => s.PercentChange < 0).OrderBy(s => s.PercentChange).Take(10).ToList());
        RenderTop("Top khối lượng", stocks.OrderByDescending(s => s.Volume).Take(10).ToList());

        var up = stocks.Count(s => s.PercentChange > 0);
        var down = stocks.Count(s => s.PercentChange < 0);
        Ui.Panel("Thị trường", $"Mã: {stocks.Count}  Tăng [{Ui.Up}]{up}[/]  Giảm [{Ui.Down}]{down}[/]");
    }

    private static void RenderTop(string title, List<StockData> rows)
    {
        Ui.SubHeader(title);
        var t = new Table().Border(TableBorder.Simple).BorderColor(Color.Grey);
        t.AddColumn("Mã");
        t.AddColumn(new TableColumn("Giá").RightAligned());
        t.AddColumn(new TableColumn("%").RightAligned());
        t.AddColumn(new TableColumn("KL").RightAligned());
        foreach (var s in rows)
            t.AddRow(Ui.E(s.Symbol), $"{s.Close:N2}", Ui.Pct(s.PercentChange), $"{s.Volume:N0}");
        AnsiConsole.Write(t);
    }

    public static void Sectors(Dictionary<string, SectorAnalysis> map, string? focus = null)
    {
        Ui.Header("Heatmap ngành");
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.DarkOrange);
        table.AddColumn("Ngành");
        table.AddColumn(new TableColumn("Mã").RightAligned());
        table.AddColumn(new TableColumn("Lên").RightAligned());
        table.AddColumn(new TableColumn("Xuống").RightAligned());
        table.AddColumn(new TableColumn("% TB").RightAligned());
        table.AddColumn("Top+");
        table.AddColumn("Top-");
        table.AddColumn(new TableColumn("KL tr").RightAligned());

        foreach (var kv in map.OrderByDescending(s => s.Value.AverageChange))
        {
            var a = kv.Value;
            table.AddRow(
                Ui.E(kv.Key),
                a.StockCount.ToString(),
                $"[{Ui.Up}]{a.UpCount}[/]",
                $"[{Ui.Down}]{a.DownCount}[/]",
                Ui.Pct(a.AverageChange),
                $"{Ui.E(a.TopGainer?.Symbol)} {Ui.Pct(a.TopGainer?.PercentChange ?? 0)}",
                $"{Ui.E(a.TopLoser?.Symbol)} {Ui.Pct(a.TopLoser?.PercentChange ?? 0)}",
                $"{a.TotalVolume / 1_000_000m:N2}");
        }
        AnsiConsole.Write(table);

        if (!string.IsNullOrEmpty(focus) && map.TryGetValue(focus, out var one))
        {
            Ui.Panel(focus,
                $"Mã {one.StockCount} | Tăng {one.UpCount} Giảm {one.DownCount} | %TB {one.AverageChange:+0.00;-0.00}%\n" +
                $"Top+ {Ui.E(one.TopGainer?.Symbol)} {one.TopGainer?.PercentChange:+0.00;-0.00}% | Top- {Ui.E(one.TopLoser?.Symbol)}");
        }
    }
}
