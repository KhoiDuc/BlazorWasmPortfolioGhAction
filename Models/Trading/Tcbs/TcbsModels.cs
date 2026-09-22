namespace BlazorWasmPortfolioGhAction.Models.Trading.Tcbs;

public sealed class TcbsStatus
{
    public bool Connected { get; set; }
    public bool NeedsReauth { get; set; }
    public bool ReadOnly { get; set; } = true;
    public string CustodyCode { get; set; } = "";
    public string AccountNo { get; set; } = "";
}

public sealed class TcbsCall
{
    public bool Ok { get; init; }
    public bool NeedsReauth { get; init; }
    public int Status { get; init; }
    public string? Error { get; init; }
    public string Json { get; init; } = "";
}

public sealed class TcbsSubAccount
{
    public string AccountNo { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public bool IsDefault { get; set; }
    public bool IsDerivative { get; set; }
}

public sealed class TcbsAsset
{
    public string Symbol { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal AvgPrice { get; set; }
    public decimal MarketValue { get; set; }
}

public sealed class TcbsCash
{
    public string AccountNo { get; set; } = "";
    public decimal CashBalance { get; set; }
    public decimal BodBalance { get; set; }
}

public sealed class TcbsStatementRow
{
    public DateTime Date { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Description { get; set; } = "";
}

public sealed class TcbsOrder
{
    public string OrderId { get; set; } = "";
    public string Symbol { get; set; } = "";
    public string ExecType { get; set; } = "";
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public decimal ExecQuantity { get; set; }
    public string Status { get; set; } = "";
    public string PriceType { get; set; } = "";
    public DateTime? Time { get; set; }
}

public sealed class TcbsMatch
{
    public string OrderId { get; set; } = "";
    public string TradeId { get; set; } = "";
    public string Symbol { get; set; } = "";
    public string Side { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime? Time { get; set; }
}

public sealed class TcbsQuote
{
    public string Symbol { get; set; } = "";
    public decimal MatchPrice { get; set; }
    public decimal CeilPrice { get; set; }
    public decimal FloorPrice { get; set; }
    public decimal RefPrice { get; set; }
    public decimal ChangePercent { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal BuyForeign { get; set; }
    public decimal SellForeign { get; set; }
    public string Room { get; set; } = "";
}

public sealed class TcbsBuyingPower
{
    public decimal PurchasingPower { get; set; }
    public decimal MaxQuantity { get; set; }
}

public sealed class TcbsOrderPreview
{
    public string ConfirmToken { get; set; } = "";
    public bool ReadOnly { get; set; }
    public decimal Notional { get; set; }
    public decimal Fee { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Symbol { get; set; } = "";
    public string ExecType { get; set; } = "";
    public int Quantity { get; set; }
    public decimal PriceVnd { get; set; }
    public string Exchange { get; set; } = "";
}

public sealed class TcbsSupplyPoint
{
    public string Time { get; set; } = "";
    public decimal Buy { get; set; }
    public decimal Sell { get; set; }
    public decimal Ratio { get; set; }
}

public sealed class TcbsPutThrough
{
    public string Symbol { get; set; } = "";
    public string Side { get; set; } = "";
    public decimal Price { get; set; }
    public decimal Volume { get; set; }
    public string Time { get; set; } = "";
    public string Kind { get; set; } = "";
}

public sealed class TcbsSyncRow
{
    public string Symbol { get; set; } = "";
    public string Action { get; set; } = "";
    public string Detail { get; set; } = "";
    public bool Selected { get; set; } = true;
}

public sealed class TcbsWsTicket
{
    public string Ticket { get; set; } = "";
    public string Stream { get; set; } = "";
    public string Symbol { get; set; } = "";
    public string RelayUrl { get; set; } = "";
    public int ExpiresInSec { get; set; }
}
