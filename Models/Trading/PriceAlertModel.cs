using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Models.Trading;

public enum AlertOperator { Above, Below }

public enum AlertStatus { Active, Triggered, Dismissed }

public class PriceAlert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Symbol { get; set; } = "";
    public string AssetType { get; set; } = "";
    public double Threshold { get; set; }
    public AlertOperator Operator { get; set; } = AlertOperator.Above;
    public AlertStatus Status { get; set; } = AlertStatus.Active;
    public double? TriggeredPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? TriggeredAt { get; set; }

    [JsonIgnore] public bool IsTriggered => Status == AlertStatus.Triggered;
}