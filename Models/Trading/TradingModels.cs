using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Models.Trading;

public record WatchlistResponse<T>(
    [property: JsonPropertyName("data")] T[] Data,
    [property: JsonPropertyName("latest_updated")] DateTime? LatestUpdated);

public record CryptoSignal(
    [property: JsonPropertyName("crypto")] string Crypto,
    [property: JsonPropertyName("is_ath")] string? IsAth,
    [property: JsonPropertyName("signal_type")] string SignalType,
    [property: JsonPropertyName("signal_label")] string SignalLabel,
    [property: JsonPropertyName("highest_price")] double HighestPrice,
    [property: JsonPropertyName("market_cap")] double MarketCap,
    [property: JsonPropertyName("score_diff")] double ScoreDiff);

public record FuturesSignal(
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("signal_type")] string SignalType,
    [property: JsonPropertyName("signal_label")] string SignalLabel,
    [property: JsonPropertyName("highest_price")] double HighestPrice,
    [property: JsonPropertyName("market_cap")] double MarketCap);

public record StockSignal(
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("signal_type")] string SignalType,
    [property: JsonPropertyName("signal_label")] string SignalLabel,
    [property: JsonPropertyName("volume")] long Volume,
    [property: JsonPropertyName("highest_price")] double HighestPrice,
    [property: JsonPropertyName("lowest_price")] double LowestPrice,
    [property: JsonPropertyName("score_diff")] double ScoreDiff);

public record WorldStock(
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("country")] string Country);

public record ForexPair(
    string Pair,
    string Action,
    double ScoreDiff,
    string? Note,
    DateTime? UpdatedAt);

public record TriggeredAlert(
    int Id,
    string AssetType,
    string Symbol,
    double Price,
    string Message,
    bool IsRead,
    DateTime CreatedAt);

public record PriceAlert(
    string Symbol,
    string AssetType,
    double AlertPrice,
    string Operator,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateAlertRequest(
    string Symbol,
    string AssetType,
    double AlertPrice,
    string Operator);

public record JournalEntry(
    int Id,
    string UserId,
    string AssetType,
    string Symbol,
    double Quantity,
    double Price,
    string Currency,
    DateTime EntryDate,
    string? Notes,
    double? CurrentPrice,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CommunityPost(
    int Id,
    string UserId,
    string UserName,
    string UserCode,
    string Content,
    string? Image,
    int Likes,
    DateTime CreatedAt);

public record CommunityComment(
    int Id,
    int PostId,
    string UserId,
    string UserName,
    string Content,
    DateTime CreatedAt);

public record RealEstatePrice(
    long Id,
    string Region,
    string Location,
    string PriceText,
    long PriceNumeric,
    string PropertyType,
    string? Url,
    DateTime FetchedAt,
    double Area);

public record NewsGroup(
    int Id,
    string Name,
    string? Description,
    string? Conclusion,
    string? UserId);

public record NewsItem(
    int Id,
    int GroupId,
    string Title,
    string? Content,
    string Status,
    DateTime? EventDate,
    DateTime CreatedAt);

public record SystemSetting(string Key, string Value);

public record DnseLoginResponse(
    [property: JsonPropertyName("token")] string? Token,
    [property: JsonPropertyName("refreshToken")] string? RefreshToken);

public record DnseUserInfo
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("custodyCode")] public string? CustodyCode { get; set; }
    [JsonPropertyName("id")] public int Id { get; set; }
}

public record ChatRequest(string Message, string? Context);
public record ChatResponse(string Reply);

public record RunScriptRequest(string ScriptType);

public record ScriptStatusResponse(
    [property: JsonPropertyName("running")] bool Running,
    [property: JsonPropertyName("last_heartbeat")] string? LastHeartbeat);

public class OsintWorldState
{
    public string? Summary { get; set; }
    public Dictionary<string, object>? Regions { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class OsintThesis
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? AssetClass { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public record TelegramNewsItem(string Channel, string Text, DateTime? Date);

public class OsintSignal
{
    public int Id { get; set; }
    public string? Symbol { get; set; }
    public string? AssetClass { get; set; }
    public string? SignalType { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Source { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public record FxRateRow(
    string Currency,
    string? Name,
    double Rate,
    double? ChangePct);

public record CalendarEvent(
    string? Title,
    string? Country,
    string? Currency,
    string? Impact,
    string? Actual,
    string? Forecast,
    string? Previous,
    DateTime? DateTime);

public record DnseOrderRequest(
    string Symbol,
    int Quantity,
    double Price,
    string Side,
    string? AccountId = null,
    string OrderType = "LO");
