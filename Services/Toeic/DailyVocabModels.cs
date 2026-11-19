using System.Text.Json.Serialization;
using BlazorWasmPortfolioGhAction.Pages;

namespace BlazorWasmPortfolioGhAction.Services.Toeic;

public sealed class DailyHistoryEntry
{
    [JsonPropertyName("dateKey")] public string DateKey { get; set; } = "";
    [JsonPropertyName("topicId")] public string TopicId { get; set; } = "";
    [JsonPropertyName("words")] public List<string> Words { get; set; } = new();
}

public sealed class DailyVocabResult
{
    public string DateKey { get; set; } = "";
    public string TopicId { get; set; } = "";
    public List<ToeicWordItem> Words { get; set; } = new();
    public bool FromCache { get; set; }
    public string? Error { get; set; }
}

public sealed class GeminiWordDto
{
    [JsonPropertyName("en")] public string En { get; set; } = "";
    [JsonPropertyName("ipa")] public string Ipa { get; set; } = "";
    [JsonPropertyName("pos")] public string Pos { get; set; } = "";
    [JsonPropertyName("vi")] public string Vi { get; set; } = "";
    [JsonPropertyName("collocation")] public string Collocation { get; set; } = "";
    [JsonPropertyName("example")] public string Example { get; set; } = "";
    [JsonPropertyName("synonym")] public string Synonym { get; set; } = "";
}
