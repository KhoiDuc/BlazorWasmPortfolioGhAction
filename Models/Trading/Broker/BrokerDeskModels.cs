using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Models.Trading.Broker;

public enum BrokerPositionStatus
{
    ChuaQuyet,
    ChoMua,
    NamGiu,
    CatLo,
    ChotLoi,
    BoTheoDoi
}

public enum BrokerNoteKind
{
    Broker,
    Self
}

public class BrokerPortfolio
{
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public List<BrokerPosition> Positions { get; set; } = [];
    public List<BrokerPosition> ClosedPositions { get; set; } = [];
}

public class BrokerPosition
{
    public string Symbol { get; set; } = "";
    public string Sector { get; set; } = "";
    public BrokerPositionStatus Status { get; set; } = BrokerPositionStatus.ChuaQuyet;
    public decimal? StopLoss { get; set; }
    public BrokerLevelInputMode? StopLossMode { get; set; }
    public decimal? StopLossInput { get; set; }
    public decimal? TargetPrice { get; set; }
    public BrokerLevelInputMode? TargetPriceMode { get; set; }
    public decimal? TargetPriceInput { get; set; }
    public decimal? WeightPct { get; set; }
    public List<BrokerLot> Buys { get; set; } = [];
    public List<BrokerSell> Sells { get; set; } = [];
    public List<BrokerNote> Notes { get; set; } = [];
    public List<string> Tags { get; set; } = [];

    [JsonIgnore]
    public decimal? TotalQuantity
    {
        get
        {
            var qty = Buys.Where(b => b.Quantity is > 0).Sum(b => b.Quantity!.Value);
            return qty > 0 ? qty : null;
        }
    }

    [JsonIgnore]
    public decimal? SoldQuantity
    {
        get
        {
            var qty = Sells.Where(s => s.Quantity is > 0).Sum(s => s.Quantity!.Value);
            return qty > 0 ? qty : null;
        }
    }

    [JsonIgnore]
    public decimal? RemainingQuantity
    {
        get
        {
            var rem = (TotalQuantity ?? 0m) - (SoldQuantity ?? 0m);
            return rem > 0 ? rem : 0m;
        }
    }

    [JsonIgnore]
    public decimal? CostBasis
    {
        get
        {
            var withQty = Buys.Where(b => b.Price > 0 && b.Quantity is > 0).ToList();
            if (withQty.Count == 0) return null;
            return withQty.Sum(b => BrokerMoney.PositionValueVnd(b.Price, b.Quantity!.Value));
        }
    }

    [JsonIgnore]
    public bool HasPartialQtyLots
    {
        get
        {
            var lots = Buys.Where(b => b.Price > 0).ToList();
            if (lots.Count == 0) return false;
            var withQty = lots.Count(b => b.Quantity is > 0);
            return withQty > 0 && withQty < lots.Count;
        }
    }

    [JsonIgnore]
    public decimal? AvgBuy
    {
        get
        {
            var lots = Buys.Where(b => b.Price > 0).ToList();
            if (lots.Count == 0) return null;
            var withQty = lots.Where(b => b.Quantity is > 0).ToList();
            if (withQty.Count > 0)
            {
                var qty = withQty.Sum(b => b.Quantity!.Value);
                return qty <= 0 ? null : withQty.Sum(b => b.Price * b.Quantity!.Value) / qty;
            }
            return lots.Average(b => b.Price);
        }
    }

    /// <summary>Realized P&L (VND) using FIFO matching of sells against buy lots.</summary>
    [JsonIgnore]
    public decimal? RealizedPnl
    {
        get
        {
            if (Sells.Count == 0) return null;
            var lots = Buys.Where(b => b.Price > 0 && b.Quantity is > 0)
                          .OrderBy(b => b.BoughtAt)
                          .ThenBy(b => b.Id)
                          .Select(b => (Price: b.Price, Qty: b.Quantity!.Value))
                          .ToList();
            if (lots.Count == 0) return null;

            var lotIdx = 0;
            var lotRemaining = lots[0].Qty;
            decimal realized = 0m;

            foreach (var sell in Sells.Where(s => s.Quantity is > 0 && s.Price > 0)
                                       .OrderBy(s => s.SoldAt)
                                       .ThenBy(s => s.Id))
            {
                var toMatch = sell.Quantity!.Value;
                while (toMatch > 0 && lotIdx < lots.Count)
                {
                    var take = Math.Min(toMatch, lotRemaining);
                    realized += BrokerMoney.PositionValueVnd(sell.Price - lots[lotIdx].Price, take);
                    lotRemaining -= take;
                    toMatch -= take;
                    if (lotRemaining <= 0)
                    {
                        lotIdx++;
                        if (lotIdx < lots.Count) lotRemaining = lots[lotIdx].Qty;
                    }
                }
                // Subtract trading fee + tax for this sell
                realized -= sell.Fee ?? 0m;
                realized -= sell.Tax ?? 0m;
            }

            return realized;
        }
    }

    [JsonIgnore]
    public decimal? RealizedPnlPct
    {
        get
        {
            var pnl = RealizedPnl;
            if (pnl is null) return null;
            var avg = AvgBuy;
            var sold = SoldQuantity;
            if (avg is null or 0 || sold is null or 0) return null;
            var costSold = BrokerMoney.PositionValueVnd(avg.Value, sold.Value);
            if (costSold == 0) return null;
            return pnl.Value / costSold * 100m;
        }
    }

    [JsonIgnore]
    public bool IsClosed => Sells.Count > 0 && (RemainingQuantity is null or 0);

    [JsonIgnore]
    public DateTime? ClosedAt =>
        IsClosed ? Sells.Where(s => s.SoldAt != default).MaxBy(s => s.SoldAt)?.SoldAt : null;

    public decimal? PnlPct(decimal? current)
    {
        var avg = AvgBuy;
        if (avg is null or 0 || current is null or 0) return null;
        return (current.Value - avg.Value) / avg.Value * 100m;
    }

    /// <summary>Unrealized P&L on remaining (unsold) shares.</summary>
    public decimal? PnlAmount(decimal? current)
    {
        var qty = RemainingQuantity;
        if (qty is null or 0) return null;
        var avg = AvgBuy;
        if (avg is null or 0 || current is null or 0) return null;
        return BrokerMoney.PositionValueVnd(current.Value, qty.Value) - BrokerMoney.PositionValueVnd(avg.Value, qty.Value);
    }

    public decimal? MarketValueVnd(decimal? current)
    {
        var qty = RemainingQuantity;
        if (qty is null or 0 || current is null or 0) return null;
        return BrokerMoney.PositionValueVnd(current.Value, qty.Value);
    }

    public BrokerNote? LatestNote =>
        Notes.OrderByDescending(n => n.At).FirstOrDefault();
}

public class BrokerLot
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public DateTime BoughtAt { get; set; } = DateTime.Today;
    public decimal Price { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? StopLoss { get; set; }
    public BrokerLevelInputMode? StopLossMode { get; set; }
    public decimal? StopLossInput { get; set; }
    public decimal? TargetPrice { get; set; }
    public BrokerLevelInputMode? TargetPriceMode { get; set; }
    public decimal? TargetPriceInput { get; set; }
    public string? Note { get; set; }
    public List<string> Tags { get; set; } = [];
}

public class BrokerSell
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public DateTime SoldAt { get; set; } = DateTime.Today;
    public decimal Price { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Fee { get; set; }
    public decimal? Tax { get; set; }
    public string? Note { get; set; }
}

public class BrokerNote
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public DateTime At { get; set; } = DateTime.Now;
    public BrokerNoteKind Kind { get; set; } = BrokerNoteKind.Broker;
    public string Text { get; set; } = "";
    public string? AiExplain { get; set; }
}

public static class BrokerStatusLabels
{
    public static string Vi(BrokerPositionStatus status) => status switch
    {
        BrokerPositionStatus.ChuaQuyet => "Chưa quyết",
        BrokerPositionStatus.ChoMua => "Chờ mua",
        BrokerPositionStatus.NamGiu => "Nắm giữ",
        BrokerPositionStatus.CatLo => "Cắt lỗ",
        BrokerPositionStatus.ChotLoi => "Chốt lời",
        BrokerPositionStatus.BoTheoDoi => "Bỏ theo dõi",
        _ => status.ToString()
    };

    public static string Vi(BrokerNoteKind kind) => kind == BrokerNoteKind.Self ? "Của tôi" : "Broker";
}

public static class BrokerMoney
{
    /// <summary>Giá CP VN trong app là nghìn đồng (VD: 28.12 = 28.120đ/CP).</summary>
    public const decimal PriceUnitVnd = 1000m;

    public static decimal PositionValueVnd(decimal priceInThousands, decimal quantity) =>
        priceInThousands * quantity * PriceUnitVnd;
}

public static class BrokerFormat
{
    public static string Quantity(decimal? qty) =>
        qty is null or 0 ? "—" : qty.Value.ToString("0.##");

    public static string VndPlain(decimal amountVnd) => FormatVnd(amountVnd);

    public static string Vnd(decimal? amountVnd)
    {
        if (amountVnd is null) return "—";
        return FormatVndSigned(amountVnd.Value);
    }

    public static string Pct(decimal? pct) =>
        pct is null ? "—" : $"{pct.Value:N2}%";

    private static string FormatVndSigned(decimal amountVnd)
    {
        var sign = amountVnd > 0 ? "+" : amountVnd < 0 ? "−" : "";
        return $"{sign}{FormatVnd(Math.Abs(amountVnd))}";
    }

    private static string FormatVnd(decimal amountVnd)
    {
        if (amountVnd >= 1_000_000_000m)
            return $"{amountVnd / 1_000_000_000m:N2} tỷ đ";
        if (amountVnd >= 1_000_000m)
            return $"{amountVnd / 1_000_000m:N2} triệu đ";
        if (amountVnd >= 1_000m)
            return $"{amountVnd / 1_000m:N1} nghìn đ";
        return $"{amountVnd:N0} đ";
    }
}

public sealed class BrokerPortfolioStats
{
    public int SymbolCount { get; init; }
    public int TrackedCount { get; init; }
    public int MissingQtyCount { get; init; }
    public decimal TotalCost { get; init; }
    public decimal TotalMarketValue { get; init; }
    public decimal TotalPnl { get; init; }
    public decimal? TotalPnlPct => TotalCost > 0 ? TotalPnl / TotalCost * 100m : null;
    public decimal TotalQuantity { get; init; }
    public int WinningCount { get; init; }
    public int LosingCount { get; init; }
    public int FlatCount { get; init; }
    public bool HasMoneyStats => TrackedCount > 0;

    public decimal TotalRealized { get; init; }
    public int ClosedCount { get; init; }
    public bool HasRealized => ClosedCount > 0;
}

public static class BrokerPortfolioStatsCalculator
{
    public static BrokerPortfolioStats Compute(
        BrokerPortfolio portfolio,
        IReadOnlyDictionary<string, decimal> quotes)
    {
        decimal totalCost = 0, totalMkt = 0, totalPnl = 0, totalQty = 0;
        var winning = 0;
        var losing = 0;
        var flat = 0;
        var tracked = 0;
        var missingQty = 0;
        var positions = portfolio.Positions ?? [];

        foreach (var p in positions)
        {
            if (p.IsClosed) continue;

            var cost = p.CostBasis;
            var qty = p.TotalQuantity;
            if (qty is null or 0 || cost is null or 0)
            {
                missingQty++;
                continue;
            }

            if (!quotes.TryGetValue(p.Symbol, out var current) || current <= 0)
            {
                missingQty++;
                continue;
            }

            var pnl = p.PnlAmount(current) ?? 0m;
            var mkt = p.MarketValueVnd(current) ?? 0m;
            tracked++;
            totalCost += cost.Value;
            totalMkt += mkt;
            totalPnl += pnl;
            totalQty += qty.Value;

            if (pnl > 0) winning++;
            else if (pnl < 0) losing++;
            else flat++;
        }

        return new BrokerPortfolioStats
        {
            SymbolCount = positions.Count,
            TrackedCount = tracked,
            MissingQtyCount = missingQty,
            TotalCost = totalCost,
            TotalMarketValue = totalMkt,
            TotalPnl = totalPnl,
            TotalQuantity = totalQty,
            WinningCount = winning,
            LosingCount = losing,
            FlatCount = flat,
            TotalRealized = ComputeRealized(portfolio),
            ClosedCount = (portfolio.ClosedPositions?.Count ?? 0)
        };
    }

    public static decimal ComputeRealized(BrokerPortfolio portfolio)
    {
        var closed = portfolio.ClosedPositions ?? [];
        var openWithSells = (portfolio.Positions ?? []).Where(p => p.Sells.Count > 0);
        return closed.Sum(p => p.RealizedPnl ?? 0m)
             + openWithSells.Sum(p => p.RealizedPnl ?? 0m);
    }

    public static BrokerPerformanceReport ComputePerformance(BrokerPortfolio portfolio)
    {
        var closed = portfolio.ClosedPositions ?? [];
        var openWithSells = (portfolio.Positions ?? []).Where(p => p.Sells.Count > 0);

        var allSells = closed.SelectMany(p => p.Sells, (p, s) => (Position: p, Sell: s))
            .Concat(openWithSells.SelectMany(p => p.Sells, (p, s) => (Position: p, Sell: s)))
            .Where(x => x.Sell.Quantity is > 0 && x.Sell.Price > 0)
            .ToList();

        if (allSells.Count == 0)
            return new BrokerPerformanceReport();

        var byMonth = allSells
            .GroupBy(x => new { x.Sell.SoldAt.Year, x.Sell.SoldAt.Month })
            .OrderByDescending(g => g.Key.Year * 100 + g.Key.Month)
            .Select(g =>
            {
                var trades = g.Select(x =>
                {
                    var pnl = x.Position.RealizedPnl ?? 0m;
                    return (Symbol: x.Position.Symbol, Pnl: pnl);
                }).ToList();
                var wins = trades.Where(t => t.Pnl > 0).ToList();
                var losses = trades.Where(t => t.Pnl < 0).ToList();
                return new BrokerPerformanceMonth
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Realized = trades.Sum(t => t.Pnl),
                    TradeCount = trades.Count,
                    WinCount = wins.Count,
                    LossCount = losses.Count,
                    AvgWin = wins.Count > 0 ? wins.Average(t => t.Pnl) : 0m,
                    AvgLoss = losses.Count > 0 ? losses.Average(t => t.Pnl) : 0m,
                    BestSymbol = trades.MaxBy(t => t.Pnl).Symbol,
                    WorstSymbol = trades.MinBy(t => t.Pnl).Symbol
                };
            })
            .ToList();

        var totalRealized = byMonth.Sum(m => m.Realized);
        var totalTrades = byMonth.Sum(m => m.TradeCount);
        var totalWins = byMonth.Sum(m => m.WinCount);
        var totalLosses = byMonth.Sum(m => m.LossCount);

        return new BrokerPerformanceReport
        {
            Months = byMonth,
            TotalRealized = totalRealized,
            TotalTrades = totalTrades,
            TotalWins = totalWins,
            TotalLosses = totalLosses,
            WinRate = totalTrades > 0 ? (decimal)totalWins / totalTrades * 100m : 0m
        };
    }
}

public sealed class BrokerPerformanceReport
{
    public List<BrokerPerformanceMonth> Months { get; init; } = [];
    public decimal TotalRealized { get; init; }
    public int TotalTrades { get; init; }
    public int TotalWins { get; init; }
    public int TotalLosses { get; init; }
    public decimal WinRate { get; init; }
    public bool HasData => TotalTrades > 0;
}

public sealed class BrokerPerformanceMonth
{
    public int Year { get; init; }
    public int Month { get; init; }
    public string Period => $"{Year}-{Month:D2}";
    public decimal Realized { get; init; }
    public int TradeCount { get; init; }
    public int WinCount { get; init; }
    public int LossCount { get; init; }
    public decimal AvgWin { get; init; }
    public decimal AvgLoss { get; init; }
    public string BestSymbol { get; init; } = "";
    public string WorstSymbol { get; init; } = "";
}
