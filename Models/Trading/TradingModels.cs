using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Models.Trading;

public record WatchlistResponse<T>(T[] Data, DateTime? LatestUpdated);

public record CryptoSignal(
    string Crypto,
    string? IsAth,
    string SignalType,
    string SignalLabel,
    double HighestPrice,
    double MarketCap,
    double ScoreDiff);

public record FuturesSignal(
    string Symbol,
    string SignalType,
    string SignalLabel,
    double HighestPrice,
    double MarketCap);

public record StockSignal(
    string Symbol,
    string SignalType,
    string SignalLabel,
    long Volume,
    double HighestPrice,
    double LowestPrice,
    double ScoreDiff);

public record WorldStock(string Symbol, string Country);

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
