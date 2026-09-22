using Spectre.Console;
using VnDesk.Models;
using VnDesk.Services;

namespace VnDesk.Render;

public static class AnalysisRenderer
{
    public static void Technical(TechnicalIndicators ind)
    {
        Ui.Header($"Phan tich {ind.Symbol}  {ind.Date:dd/MM/yyyy}");
        var trendColor = ind.Trend.Contains("Tang") ? Ui.Up : ind.Trend.Contains("Giam") ? Ui.Down : Ui.Hold;

        Ui.Panel("Gia",
            $"Close [{Ui.Accent}]{ind.LatestClose:N2}[/]  H {ind.LatestHigh:N2}  L {ind.LatestLow:N2}\n" +
            $"H5 {ind.LatestHigh5:N2} L5 {ind.LatestLow5:N2} | H20 {ind.LatestHigh20:N2} L20 {ind.LatestLow20:N2}\n" +
            $"Xu huong: [{trendColor}]{Ui.E(ind.Trend)}[/]");

        var t = new Table().Border(TableBorder.Rounded).BorderColor(Color.DarkOrange);
        t.AddColumn("Nhom");
        t.AddColumn("Chi bao");
        t.AddColumn("Gia tri");
        t.AddRow("Trend", "SMA20 / 50 / 200", $"{ind.SMA20:N2} / {ind.SMA50:N2} / {ind.SMA200:N2}");
        t.AddRow("Trend", "Ichimoku Tenkan/Kijun", $"{ind.Ichimoku.TenkanSen:N2} / {ind.Ichimoku.KijunSen:N2}");
        t.AddRow("Momentum", "RSI(14)", ColorRsi(ind.RSI));
        t.AddRow("Momentum", "MACD / Signal / Hist", $"{ind.MACD:N3} / {ind.Signal:N3} / {ind.Histogram:N3}");
        t.AddRow("Momentum", "Stoch K/D", $"{ind.K_Stochastic:N1} / {ind.D_Stochastic:N1}");
        t.AddRow("Volatility", "ATR%", $"{ind.ATR:N2}");
        t.AddRow("Volatility", "BB U/M/L", $"{ind.BollingerUpper:N2} / {ind.BollingerMiddle:N2} / {ind.BollingerLower:N2}");
        t.AddRow("Volume", "KL / TB20 / x", $"{ind.LatestVolume:N0} / {ind.VolumeAverage20:N0} / {ind.VolumeRatio:N2}");
        t.AddRow("Volume", "OBV / thanh khoan", $"{ind.OBV:N0} | {Ui.E(ind.LiquidityAssessment)}");
        AnsiConsole.Write(t);

        var sr = ind.SupportResistance;
        Ui.Panel("Ho tro / Khang cu",
            $"[{Ui.Up}]Ho tro:[/] {Ui.E(string.Join(" | ", sr.SupportLevels.TakeLast(5).Select(x => x.ToString("N2"))))}\n" +
            $"[{Ui.Down}]Khang cu:[/] {Ui.E(string.Join(" | ", sr.ResistanceLevels.TakeLast(5).Select(x => x.ToString("N2"))))}\n" +
            $"Phan ky: {Ui.E(ind.Divergence)}");

        var patterns = ind.Patterns.Count == 0
            ? "Khong co nen noi bat"
            : string.Join("\n", ind.Patterns.Take(8).Select(p => $"• {Ui.E(p.Name)} — {Ui.E(p.Description)}"));
        Ui.Panel("Nen / mau hinh (quan sat)", patterns + "\n" + Ui.E(string.Join(", ", ind.ChartPatterns)));

        var sig = ind.TradingSignal;
        var sc = sig.Action is "Buy" or "StrongBuy" ? Ui.Up : sig.Action is "Sell" or "StrongSell" ? Ui.Down : Ui.Hold;
        Ui.Panel("Khuyen nghi — QUAN SAT, khong phai lenh",
            $"[{sc} bold]{Ui.E(sig.Action)}[/]  Entry {sig.EntryPrice:N2}  SL {sig.StopLoss:N2}  TP {sig.TakeProfit:N2}\n" +
            Ui.E(sig.Rationale ?? "Khong co tin hieu ro."));

        if (ind.IntradayData.Count > 0)
        {
            Ui.Panel("Intraday",
                $"Last {ind.LatestIntradayDataClose:N2}  H {ind.LatestIntradayDataHigh:N2} L {ind.LatestIntradayDataLow:N2}\n" +
                $"KL 30p {ind.LatestIntradayDataVolume:N0}  xTB20 {ind.VolumeRatioIntradayData:N2}  Mom {ind.IntradayMomentum:N2}%");
        }
        else if (!string.IsNullOrEmpty(ind.IntradayNote))
        {
            Ui.Warn(ind.IntradayNote);
        }
    }

    public static void Checklist(TechnicalChecklist c)
    {
        Ui.Header($"Checklist ky thuat {c.Symbol}");
        var t = new Table().Border(TableBorder.Rounded).BorderColor(Color.DarkOrange);
        t.AddColumn("Muc");
        t.AddColumn("Noi dung");
        t.AddRow("[grey]Context[/]", Ui.E(c.Context));
        t.AddRow("[grey]Confirm / Invalidate[/]", Ui.E(c.ConfirmInvalidate));
        t.AddRow($"[{Ui.Down}]Risk[/]", Ui.E(c.Risk));
        t.AddRow("[grey]Verify[/]", Ui.E(c.Verify));
        if (!string.IsNullOrEmpty(c.Observation))
            t.AddRow("Observation", Ui.E(c.Observation));
        AnsiConsole.Write(t);
    }

    public static void Position(PositionResult p)
    {
        Ui.Header($"P&L {p.Symbol}");
        var color = p.ProfitLoss >= 0 ? Ui.Up : Ui.Down;
        Ui.Panel("Ket qua",
            $"[{color} bold]{p.ProfitLoss:N0} VND ({p.ProfitLossPercent:N2}%)[/]\n" +
            Ui.E(p.Note));

        var t = new Table().Border(TableBorder.Simple);
        t.AddColumn("Hang");
        t.AddColumn(new TableColumn("VND").RightAligned());
        t.AddRow("Gia mua", $"{p.EntryPrice:N0}");
        t.AddRow("Gia hien tai", $"{p.CurrentPrice:N0}");
        t.AddRow("KL", $"{p.Shares:N0}");
        t.AddRow($"Phi {Ui.E(p.Broker)} {p.FeeRatePct:N2}% mua", $"{p.BuyFee:N0}");
        t.AddRow("Tong chi phi", $"{p.TotalCost:N0}");
        t.AddRow("Gia tri ban", $"{p.SellValue:N0}");
        t.AddRow("Phi ban", $"{p.SellFee:N0}");
        t.AddRow("Thue 0.1%", $"{p.SellTax:N0}");
        t.AddRow("Rong sau ban", $"{p.NetSellValue:N0}");
        AnsiConsole.Write(t);
    }

    public static void Size(SizeResult s)
    {
        Ui.Header("Position size");
        if (s.NoTrade)
            Ui.Panel("No-trade", $"[{Ui.Hold}]{Ui.E(s.Reason)}[/]");

        var t = new Table().Border(TableBorder.Rounded).BorderColor(Color.DarkOrange);
        t.AddColumn("Muc");
        t.AddColumn("Gia tri");
        t.AddRow("Von", $"{s.Capital:N0}");
        t.AddRow("% risk", $"{s.RiskPct:N2}%");
        t.AddRow("Tien risk", $"{s.RiskAmount:N0}");
        t.AddRow("Gia / Stop", $"{s.Price:N2} / {s.Stop:N2}");
        t.AddRow("Khoang stop", $"{s.StopDistance:N2}");
        t.AddRow("So CP (lot 100)", $"{s.Shares:N0}");
        t.AddRow("Gia tri vi the", $"{s.PositionValue:N0}");
        if (s.RiskReward > 0)
            t.AddRow("R:R", $"{s.RiskReward:N2}");
        AnsiConsole.Write(t);
    }

    private static string ColorRsi(decimal rsi)
    {
        if (rsi < 40) return $"[{Ui.Up}]{rsi:N1} qua ban[/]";
        if (rsi > 60) return $"[{Ui.Down}]{rsi:N1} qua mua[/]";
        return $"[{Ui.Hold}]{rsi:N1} trung tinh[/]";
    }
}
