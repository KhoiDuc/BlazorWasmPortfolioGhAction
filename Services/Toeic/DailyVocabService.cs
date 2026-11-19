using System.Text.Json;
using Blazored.LocalStorage;
using BlazorWasmPortfolioGhAction.Pages;
using BlazorWasmPortfolioGhAction.Services.Trading.Broker;
using Microsoft.Extensions.Logging;

namespace BlazorWasmPortfolioGhAction.Services.Toeic;

public interface IDailyVocabService
{
    Task<DailyVocabResult> GetTodayAsync(CancellationToken ct = default);
    Task<List<DailyHistoryEntry>> GetHistoryAsync(CancellationToken ct = default);
    int GetStreak(List<DailyHistoryEntry> history);
}

public sealed class DailyVocabService : IDailyVocabService
{
    private readonly IBrokerGeminiClient _gemini;
    private readonly IToeicHistoryClient _historyClient;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<DailyVocabService> _logger;

    private const string TodayCacheKey = "toeic_today_cache";
    private const int WordCount = 10;

    private static readonly string[] TopicPool =
    {
        "cat-business", "cat-office", "cat-finance", "cat-marketing",
        "cat-travel", "cat-dining", "cat-shopping", "cat-tech"
    };

    private static readonly Dictionary<string, (string En, string Vi)> TopicNames = new()
    {
        ["cat-business"] = ("Business Meetings & Negotiations", "Họp & Đàm phán kinh doanh"),
        ["cat-office"] = ("Office & Corporate Communication", "Văn phòng & Giao tiếp công sở"),
        ["cat-finance"] = ("Finance & Banking", "Tài chính & Ngân hàng"),
        ["cat-marketing"] = ("Marketing & Advertising", "Tiếp thị & Quảng cáo"),
        ["cat-travel"] = ("Travel & Transportation", "Du lịch & Giao thông"),
        ["cat-dining"] = ("Dining & Restaurants", "Ăn uống & Nhà hàng"),
        ["cat-shopping"] = ("Shopping & Retail", "Mua sắm & Bán lẻ"),
        ["cat-tech"] = ("Technology & IT", "Công nghệ & IT"),
    };

    public DailyVocabService(
        IBrokerGeminiClient gemini,
        IToeicHistoryClient historyClient,
        ILocalStorageService localStorage,
        ILogger<DailyVocabService> logger)
    {
        _gemini = gemini;
        _historyClient = historyClient;
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task<DailyVocabResult> GetTodayAsync(CancellationToken ct = default)
    {
        var todayKey = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var cached = await _localStorage.GetItemAsync<DailyVocabResult>(TodayCacheKey);
        if (cached is { DateKey: var key } && key == todayKey && cached.Words.Count > 0)
        {
            cached.FromCache = true;
            return cached;
        }

        var existing = await _historyClient.GetByDateAsync(todayKey, ct);
        if (existing is { Words.Count: > 0 })
        {
            var (topicEn, _) = TopicNames.GetValueOrDefault(existing.TopicId, ("Unknown", "Không rõ"));
            var words = await GenerateWordsFromHistory(existing.Words, topicEn, new HashSet<string>(), ct);
            if (words.Count > 0)
            {
                var result = new DailyVocabResult
                {
                    DateKey = todayKey,
                    TopicId = existing.TopicId,
                    Words = words,
                    FromCache = true,
                };
                await _localStorage.SetItemAsync(TodayCacheKey, result);
                return result;
            }
        }

        var history = await _historyClient.GetAllAsync(ct);
        var allUsed = history.SelectMany(h => h.Words).Select(w => w.ToLowerInvariant()).ToHashSet();

        var dayNumber = (int)(DateTime.UtcNow - new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalDays;
        var topicId = TopicPool[Math.Abs((int)dayNumber) % TopicPool.Length];
        var (topicEnName, _) = TopicNames.GetValueOrDefault(topicId, ("Business", "Kinh doanh"));

        var generated = await GenerateWordsFromGemini(topicEnName, allUsed, ct);
        if (generated.Count == 0)
        {
            return new DailyVocabResult
            {
                DateKey = todayKey,
                TopicId = topicId,
                Words = new List<ToeicWordItem>(),
                Error = "Không thể tạo từ vựng. Vui lòng kiểm tra kết nối/API key.",
            };
        }

        var finalResult = new DailyVocabResult
        {
            DateKey = todayKey,
            TopicId = topicId,
            Words = generated,
            FromCache = false,
        };

        await _localStorage.SetItemAsync(TodayCacheKey, finalResult);

        _ = _historyClient.SaveAsync(new DailyHistoryEntry
        {
            DateKey = todayKey,
            TopicId = topicId,
            Words = generated.Select(w => w.En).ToList(),
        }, ct);

        return finalResult;
    }

    public async Task<List<DailyHistoryEntry>> GetHistoryAsync(CancellationToken ct = default)
    {
        return await _historyClient.GetAllAsync(ct);
    }

    public int GetStreak(List<DailyHistoryEntry> history)
    {
        if (history.Count == 0) return 0;
        var dates = history.Select(h => h.DateKey).OrderByDescending(d => d).ToHashSet();
        var streak = 0;
        var cursor = DateTime.UtcNow.ToString("yyyy-MM-dd");
        while (dates.Contains(cursor))
        {
            streak++;
            cursor = DateTime.UtcNow.AddDays(-streak).ToString("yyyy-MM-dd");
        }
        return streak;
    }

    private async Task<List<ToeicWordItem>> GenerateWordsFromHistory(List<string> words, string topicEnName, HashSet<string> exclude, CancellationToken ct)
    {
        var prompt = BuildPrompt(topicEnName, exclude, words);
        return await CallGeminiAndParse(prompt, exclude, ct);
    }

    private async Task<List<ToeicWordItem>> GenerateWordsFromGemini(string topicEnName, HashSet<string> exclude, CancellationToken ct)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var prompt = BuildPrompt(topicEnName, exclude);
            var words = await CallGeminiAndParse(prompt, exclude, ct);
            if (words.Count >= WordCount) return words.Take(WordCount).ToList();
            exclude.UnionWith(words.Select(w => w.En.ToLowerInvariant()));
        }
        return new List<ToeicWordItem>();
    }

    private static string BuildPrompt(string topicEnName, HashSet<string> exclude, List<string>? knownWords = null)
    {
        var excludeList = exclude.Count > 0
            ? string.Join(", ", exclude.Take(200))
            : "(none — first day)";

        var knownSection = knownWords is { Count: > 0 }
            ? $"\nThese words were already assigned today (provide full details for them): {string.Join(", ", knownWords)}"
            : "";

        return $$"""
You are a TOEIC vocabulary curator. Topic: {{topicEnName}} (TOEIC business context).

EXCLUDE (already used on previous days, do NOT repeat): {{excludeList}}{{knownSection}}

Return EXACTLY {{WordCount}} TOEIC-relevant words as a JSON array. Each element:
{"en":"word","ipa":"/ipa/","pos":"n","vi":"nghĩa tiếng việt","collocation":"common phrase","example":"sentence using the word"}

Rules:
- Mix 7 new words + 3 review words (from last 7 days if any, otherwise new).
- All words must be TOEIC-level business English.
- Do NOT wrap JSON in markdown fences. Output raw JSON array only.
""";
    }

    private async Task<List<ToeicWordItem>> CallGeminiAndParse(string prompt, HashSet<string> exclude, CancellationToken ct)
    {
        var result = await _gemini.ExplainAsync(prompt, ct);
        if (!string.IsNullOrEmpty(result.Error) || string.IsNullOrEmpty(result.Text))
        {
            _logger.LogWarning("Gemini returned error: {Error}", result.Error);
            return new List<ToeicWordItem>();
        }

        var json = ExtractJson(result.Text);
        if (string.IsNullOrEmpty(json)) return new List<ToeicWordItem>();

        try
        {
            var dtos = JsonSerializer.Deserialize<List<GeminiWordDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
            if (dtos == null) return new List<ToeicWordItem>();

            return dtos
                .Where(d => !string.IsNullOrWhiteSpace(d.En) && !exclude.Contains(d.En.ToLowerInvariant()))
                .Select(d => new ToeicWordItem
                {
                    En = d.En.Trim(),
                    Ipa = d.Ipa?.Trim() ?? "",
                    Pos = d.Pos?.Trim() ?? "",
                    Vi = d.Vi?.Trim() ?? "",
                    Collocation = d.Collocation?.Trim() ?? "",
                    Example = d.Example?.Trim() ?? "",
                })
                .ToList();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini JSON: {Json}", json[..Math.Min(200, json.Length)]);
            return new List<ToeicWordItem>();
        }
    }

    private static string ExtractJson(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline >= 0) trimmed = trimmed[(firstNewline + 1)..];
            var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence >= 0) trimmed = trimmed[..lastFence];
        }

        var start = trimmed.IndexOf('[');
        var end = trimmed.LastIndexOf(']');
        if (start >= 0 && end > start)
            return trimmed[start..(end + 1)];

        return trimmed;
    }
}
