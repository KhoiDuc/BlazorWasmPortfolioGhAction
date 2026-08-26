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
    public List<BrokerNote> Notes { get; set; } = [];

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
    public decimal? AvgBuy
    {
        get
        {
            var lots = Buys.Where(b => b.Price > 0).ToList();
            if (lots.Count == 0) return null;
            var withQty = lots.Where(b => b.Quantity is > 0).ToList();
            if (withQty.Count == lots.Count)
            {
                var qty = withQty.Sum(b => b.Quantity!.Value);
                return qty <= 0 ? null : withQty.Sum(b => b.Price * b.Quantity!.Value) / qty;
            }
            return lots.Average(b => b.Price);
        }
    }

    public decimal? PnlPct(decimal? current)
    {
        var avg = AvgBuy;
        if (avg is null or 0 || current is null or 0) return null;
        return (current.Value - avg.Value) / avg.Value * 100m;
    }

    public decimal? PnlAmount(decimal? current)
    {
        var qty = TotalQuantity;
        var cost = CostBasis;
        if (qty is null or 0 || cost is null || current is null or 0) return null;
        return BrokerMoney.PositionValueVnd(current.Value, qty.Value) - cost.Value;
    }

    public decimal? MarketValueVnd(decimal? current)
    {
        var qty = TotalQuantity;
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
            FlatCount = flat
        };
    }
}
