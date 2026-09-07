using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

/// <summary>
/// Backtest engine — runs indicator signals over historical data, computes win-rate / avg TP / hold-days / PnL.
/// Ported scaffolding from Survey/StockLib PartternService (PrintBuy/PrintBuyLast/RankChungKhoan).
/// Uses IVnMarketClient (VnDirect/CafeF) — pure client-side.
/// </summary>
public sealed class BacktestService
{
    private readonly IVnMarketClient _market;
    private readonly IndicatorService _indicators;

    public BacktestService(IVnMarketClient market, IndicatorService indicators)
    {
        _market = market;
        _indicators = indicators;
    }

    /// <summary>Run backtest for a single symbol + indicator.</summary>
    public async Task<BacktestResult> BacktestAsync(BacktestRequest req, CancellationToken ct = default)
    {
        var trades = new List<BacktestTrade>();

        foreach (var sym in req.Symbols)
        {
            var hist = await _market.GetHistoricalAsync(sym, 300, ct: ct);
            if (hist.Count < 200) continue;

            var ordered = hist.OrderBy(d => d.Date).Where(d => d.Date >= req.From && d.Date <= req.To).ToList();
            if (ordered.Count < 50) continue;

            var tradesForSym = RunIndicator(req.Indicator, sym, ordered, req.TakeProfitPct, req.StopLossPct);
            trades.AddRange(tradesForSym);
        }

        return Aggregate(req.Indicator, trades);
    }

    /// <summary>Rank symbols by win-rate for a given indicator.</summary>
    public async Task<List<(string Symbol, double WinRate)>> RankByIndicatorAsync(string indicator, IReadOnlyList<string> symbols, CancellationToken ct = default)
    {
        var results = new List<(string Symbol, double WinRate)>();
        foreach (var sym in symbols)
        {
            var hist = await _market.GetHistoricalAsync(sym, 300, ct: ct);
            if (hist.Count < 200) continue;
            var ordered = hist.OrderBy(d => d.Date).ToList();
            var trades = RunIndicator(indicator, sym, ordered, 10, 7);
            var wr = trades.Count > 0 ? (double)trades.Count(t => t.IsWin) / trades.Count * 100 : 0;
            results.Add((sym, wr));
        }
        return results.OrderByDescending(r => r.WinRate).ToList();
    }

    private List<BacktestTrade> RunIndicator(string indicator, string symbol, List<StockData> data, decimal tpPct, decimal slPct)
    {
        return indicator switch
        {
            "SuperTrend" => RunSuperTrend(symbol, data, tpPct, slPct),
            "T3" => RunT3(symbol, data, tpPct, slPct),
            "VCP" => RunVCP(symbol, data, tpPct, slPct),
            "PinBar" => RunPinBar(symbol, data, tpPct, slPct),
            "OrderBlock" => RunOrderBlock(symbol, data, tpPct, slPct),
            "Elliott" => RunElliott(symbol, data, tpPct, slPct),
            _ => []
        };
    }

    private List<BacktestTrade> RunSuperTrend(string symbol, List<StockData> data, decimal tpPct, decimal slPct)
    {
        var trades = new List<BacktestTrade>();
        var st = _indicators.CalculateSuperTrend(data);
        bool? inPosition = null;
        BacktestTrade? openTrade = null;

        for (var i = 10; i < data.Count; i++)
        {
            if (inPosition != true && st.IsUptrend[i] && !st.IsUptrend[i - 1])
            {
                openTrade = new BacktestTrade(symbol, data[i].Date, data[i].Close, null, null,
                    data[i].Close * (1 + tpPct / 100m), data[i].Close * (1 - slPct / 100m), 0, 0, 0, false);
                inPosition = true;
            }
            else if (inPosition == true && openTrade is not null)
            {
                if (data[i].Close >= openTrade.TakeProfit || data[i].Close <= openTrade.StopLoss)
                {
                    var exit = data[i].Close;
                    var pnl = exit - openTrade.EntryPrice;
                    var pnlPct = (double)(pnl / openTrade.EntryPrice * 100);
                    trades.Add(openTrade with
                    {
                        Exit = data[i].Date,
                        ExitPrice = exit,
                        HoldDays = (data[i].Date - openTrade.Entry).Days,
                        Pnl = pnl,
                        PnlPct = pnlPct,
                        IsWin = pnl > 0
                    });
                    inPosition = null;
                    openTrade = null;
                }
            }
        }
        return trades;
    }

    private List<BacktestTrade> RunT3(string symbol, List<StockData> data, decimal tpPct, decimal slPct)
    {
        var trades = new List<BacktestTrade>();
        var t3 = _indicators.CalculateT3(data);
        bool? inPosition = null;
        BacktestTrade? openTrade = null;

        for (var i = 10; i < data.Count; i++)
        {
            // buy when close crosses above T3, sell when crosses below
            if (inPosition != true && data[i].Close > t3[i] && data[i - 1].Close <= t3[i - 1])
            {
                openTrade = new BacktestTrade(symbol, data[i].Date, data[i].Close, null, null,
                    data[i].Close * (1 + tpPct / 100m), data[i].Close * (1 - slPct / 100m), 0, 0, 0, false);
                inPosition = true;
            }
            else if (inPosition == true && openTrade is not null)
            {
                if (data[i].Close >= openTrade.TakeProfit || data[i].Close <= openTrade.StopLoss || data[i].Close < t3[i])
                {
                    var exit = data[i].Close;
                    var pnl = exit - openTrade.EntryPrice;
                    var pnlPct = (double)(pnl / openTrade.EntryPrice * 100);
                    trades.Add(openTrade with
                    {
                        Exit = data[i].Date,
                        ExitPrice = exit,
                        HoldDays = (data[i].Date - openTrade.Entry).Days,
                        Pnl = pnl,
                        PnlPct = pnlPct,
                        IsWin = pnl > 0
                    });
                    inPosition = null;
                    openTrade = null;
                }
            }
        }
        return trades;
    }

    private List<BacktestTrade> RunVCP(string symbol, List<StockData> data, decimal tpPct, decimal slPct)
    {
        var trades = new List<BacktestTrade>();
        for (var i = 0; i < data.Count; i++)
        {
            var slice = data.Take(i + 1).ToList();
            var vcp = ChartPatterns.DetectVCP(slice);
            if (vcp is null) continue;
            var entry = data[i].Close;
            var tp = entry * (1 + tpPct / 100m);
            var sl = entry * (1 - slPct / 100m);
            // forward-simulate exit
            for (var j = i + 1; j < data.Count; j++)
            {
                if (data[j].Close >= tp || data[j].Close <= sl)
                {
                    var pnl = data[j].Close - entry;
                    trades.Add(new BacktestTrade(symbol, data[i].Date, entry, data[j].Date, data[j].Close,
                        tp, sl, (data[j].Date - data[i].Date).Days, pnl, (double)(pnl / entry * 100), pnl > 0));
                    break;
                }
            }
        }
        return trades;
    }

    private List<BacktestTrade> RunPinBar(string symbol, List<StockData> data, decimal tpPct, decimal slPct)
    {
        var trades = new List<BacktestTrade>();
        var pinbars = CandlestickPatternDetector.DetectPinBar(data);
        // detect bullish pinbar entries
        foreach (var pb in pinbars.Where(p => p.Direction == Direction.Bullish))
        {
            // find the bar
            var idx = data.FindLastIndex(d => d.Date <= DateTime.Now);
            if (idx < 0) continue;
            // ponytail: PinBar detector returns patterns without date — simplify: use last bar entries
        }
        // Simplified: scan for bullish pinbar bars directly
        for (var i = 20; i < data.Count - 1; i++)
        {
            var d = data[i];
            var range = d.Range();
            if (range <= 0) continue;
            var body = d.BodySize();
            var lowerWick = d.LowerWickSize;
            if (body <= range / 3m && lowerWick >= 0.6m * range)
            {
                var entry = data[i + 1].Open;
                var tp = entry * (1 + tpPct / 100m);
                var sl = entry * (1 - slPct / 100m);
                for (var j = i + 1; j < data.Count; j++)
                {
                    if (data[j].Close >= tp || data[j].Close <= sl)
                    {
                        var pnl = data[j].Close - entry;
                        trades.Add(new BacktestTrade(symbol, data[i + 1].Date, entry, data[j].Date, data[j].Close,
                            tp, sl, (data[j].Date - data[i + 1].Date).Days, pnl, (double)(pnl / entry * 100), pnl > 0));
                        break;
                    }
                }
            }
        }
        return trades;
    }

    private List<BacktestTrade> RunOrderBlock(string symbol, List<StockData> data, decimal tpPct, decimal slPct)
    {
        var trades = new List<BacktestTrade>();
        // scan BotPinbar / BotInsideBar order blocks as long entries
        for (var i = 20; i < data.Count - 1; i++)
        {
            var slice = data.Take(i + 1).ToList();
            var ob = OrderBlockDetector.Detect(slice);
            if (ob is null || !ob.Mode.StartsWith("Bot")) continue;
            var entry = data[i + 1].Open;
            var tp = entry * (1 + tpPct / 100m);
            var sl = entry * (1 - slPct / 100m);
            for (var j = i + 1; j < data.Count; j++)
            {
                if (data[j].Close >= tp || data[j].Close <= sl)
                {
                    var pnl = data[j].Close - entry;
                    trades.Add(new BacktestTrade(symbol, data[i + 1].Date, entry, data[j].Date, data[j].Close,
                        tp, sl, (data[j].Date - data[i + 1].Date).Days, pnl, (double)(pnl / entry * 100), pnl > 0));
                    break;
                }
            }
        }
        return trades;
    }

    private List<BacktestTrade> RunElliott(string symbol, List<StockData> data, decimal tpPct, decimal slPct)
    {
        var trades = new List<BacktestTrade>();
        var rsi = _indicators.GetRsiHistory(data.Select(d => d.Close).ToArray());
        for (var i = 100; i < data.Count - 1; i++)
        {
            var slice = data.Take(i + 1).ToList();
            var rsiSlice = rsi.Take(i + 1).ToList();
            var sig = CandlestickPatternDetector.DetectElliottWave(slice, rsiSlice);
            if (sig is null || !sig.Direction.Contains("Bullish")) continue;
            var entry = data[i + 1].Open;
            var tp = entry * (1 + tpPct / 100m);
            var sl = entry * (1 - slPct / 100m);
            for (var j = i + 1; j < data.Count; j++)
            {
                if (data[j].Close >= tp || data[j].Close <= sl)
                {
                    var pnl = data[j].Close - entry;
                    trades.Add(new BacktestTrade(symbol, data[i + 1].Date, entry, data[j].Date, data[j].Close,
                        tp, sl, (data[j].Date - data[i + 1].Date).Days, pnl, (double)(pnl / entry * 100), pnl > 0));
                    break;
                }
            }
        }
        return trades;
    }

    private static BacktestResult Aggregate(string indicator, List<BacktestTrade> trades)
    {
        var wins = trades.Count(t => t.IsWin);
        var losses = trades.Count - wins;
        var winRate = trades.Count > 0 ? (double)wins / trades.Count * 100 : 0;
        var avgTp = trades.Count > 0 ? trades.Average(t => t.PnlPct) : 0;
        var avgHold = trades.Count > 0 ? trades.Average(t => t.HoldDays) : 0;
        var totalPnl = trades.Sum(t => t.Pnl);
        return new BacktestResult(indicator, trades.Count, wins, losses, winRate, avgTp, avgHold, totalPnl, trades);
    }
}