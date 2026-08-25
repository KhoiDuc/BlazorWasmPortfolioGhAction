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
    public decimal? TargetPrice { get; set; }
    public decimal? WeightPct { get; set; }
    public List<BrokerLot> Buys { get; set; } = [];
    public List<BrokerNote> Notes { get; set; } = [];

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
    public decimal? TargetPrice { get; set; }
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
