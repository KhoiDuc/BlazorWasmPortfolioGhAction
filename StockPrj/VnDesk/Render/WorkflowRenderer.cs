using Spectre.Console;
using VnDesk.Models;

namespace VnDesk.Render;

public static class WorkflowRenderer
{
    public static void Checklist(PreTradeChecklist c)
    {
        Ui.Header($"Pre-trade {c.Symbol}");
        var ready = c.Complete ? $"[{Ui.Up}]Đủ 8/8[/]" : $"[{Ui.Hold}]Thiếu ({c.FilledCount}/8) — chưa đủ để vào lệnh[/]";
        Ui.Panel("Trạng thái", ready);

        void Row(string title, string body, string color = "grey")
        {
            Ui.Panel(title, string.IsNullOrWhiteSpace(body) ? $"[{Ui.Hold}](trống)[/]" : Ui.E(body), color);
        }

        Row("1 Thesis", c.Thesis);
        Row("2 Nguồn dữ liệu", c.SourceData);
        Row("3 Invalidate", c.Invalidation);
        AnsiConsole.MarkupLine($"[{Ui.Down}]--- Risk ---[/]");
        Row("4 Max loss", c.MaxLoss, Ui.Down);
        Row("5 Position size", c.PositionSizeLogic, Ui.Down);
        Row("6 Stop", c.StopConditions, Ui.Down);
        Row("7 Scenario đổi plan", c.ScenarioChange);
        Row("8 Vẫn làm nếu không áp lực?", c.WithoutPressure);
    }

    public static void Plan(TradePlan p)
    {
        Ui.Header($"Trading plan {p.Symbol}");
        Ui.Panel("ENTRY LOGIC", Ui.E(p.EntryLogic));
        Ui.Panel("EXIT CRITERIA", Ui.E(p.ExitCriteria));
        AnsiConsole.MarkupLine($"[{Ui.Down}]RISK[/]");
        Ui.Panel("RISK PARAMETERS", Ui.E(p.RiskParameters));
        Ui.Panel("THESIS SUMMARY", Ui.E(p.ThesisSummary));
        Ui.Panel("INVALIDATION", Ui.E(p.InvalidationConditions));
    }

    public static void Session(SessionLog s)
    {
        Ui.Header($"Session {s.At:dd/MM/yyyy HH:mm}");
        Ui.Panel("Calendar", Ui.E(s.Calendar));
        Ui.Panel("Watchlist flags", Ui.E(s.WatchlistFlags));
        Ui.Panel("Risks", Ui.E(s.Risks));
        Ui.Panel("Plan phiên", Ui.E(s.SessionPlan));
        Ui.Panel("Giờ dừng", Ui.E(s.StopTime));
        if (!string.IsNullOrWhiteSpace(s.Notes))
            Ui.Panel("Notes", Ui.E(s.Notes));
    }

    public static void Review(TradeReview r)
    {
        Ui.Header($"Review {r.Symbol}");
        var t = new Table().Border(TableBorder.Rounded).BorderColor(Color.DarkOrange);
        t.AddColumn("Mục");
        t.AddColumn("Nội dung");
        t.AddRow("Facts", Ui.E(r.WhatHappened));
        t.AddRow("Judgment", Ui.E(r.Learned));
        t.AddRow("Next process", Ui.E(r.ProcessChange));
        AnsiConsole.Write(t);
    }

    public static void News(NewsBrief n)
    {
        Ui.Header("News brief");
        Ui.Panel("Facts", Ui.E(n.Facts));
        Ui.Panel("Sources", Ui.E(n.Sources));
        Ui.Panel("Implications", Ui.E(n.Implications));
        AnsiConsole.MarkupLine(n.Verified
            ? $"[{Ui.Up}]Đã verify nguồn[/]"
            : $"[{Ui.Hold}]chưa verify nguồn[/]");
    }

    public static void AlertSteps(string signal)
    {
        Ui.Header("Alert workflow");
        AnsiConsole.MarkupLine($"1. Signal: {Ui.E(signal)}");
        AnsiConsole.MarkupLine("2. Alert — hiện panel này");
        AnsiConsole.MarkupLine("3. Checklist — xem D1 nếu cần");
        AnsiConsole.MarkupLine("4. Trader [[C]]onfirm / [[R]]eject");
        AnsiConsole.MarkupLine("5. Action — [yellow]không tự khớp lệnh[/]");
        AnsiConsole.MarkupLine("6. Logged vào journal");
    }
}
