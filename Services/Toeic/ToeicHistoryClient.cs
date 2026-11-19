using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;

namespace BlazorWasmPortfolioGhAction.Services.Toeic;

public interface IToeicHistoryClient
{
    Task<List<DailyHistoryEntry>> GetAllAsync(CancellationToken ct = default);
    Task<DailyHistoryEntry?> GetByDateAsync(string dateKey, CancellationToken ct = default);
    Task<(bool Ok, string? Error)> SaveAsync(DailyHistoryEntry entry, CancellationToken ct = default);
}

public sealed class ToeicHistoryClient : IToeicHistoryClient
{
    private readonly HttpClient _http;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
    private readonly Services.Trading.Broker.IBrokerAuthService _auth;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<ToeicHistoryClient> _logger;

    private const string CacheKey = "toeic_daily_history_cache";

    public ToeicHistoryClient(
        HttpClient http,
        Microsoft.Extensions.Configuration.IConfiguration config,
        Services.Trading.Broker.IBrokerAuthService auth,
        ILocalStorageService localStorage,
        ILogger<ToeicHistoryClient> logger)
    {
        _http = http;
        _config = config;
        _auth = auth;
        _localStorage = localStorage;
        _logger = logger;
    }

    private string? BaseUrl => _config["BrokerApi:BaseUrl"]?.Trim().TrimEnd('/');

    public async Task<List<DailyHistoryEntry>> GetAllAsync(CancellationToken ct = default)
    {
        var baseUrl = BaseUrl;
        if (string.IsNullOrEmpty(baseUrl)) return new List<DailyHistoryEntry>();

        var token = await _auth.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            var cached = await _localStorage.GetItemAsync<List<DailyHistoryEntry>>(CacheKey);
            return cached ?? new List<DailyHistoryEntry>();
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/toeic/history");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var resp = await _http.SendAsync(request, ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("GET /api/toeic/history returned {Status}", resp.StatusCode);
                var cached = await _localStorage.GetItemAsync<List<DailyHistoryEntry>>(CacheKey);
                return cached ?? new List<DailyHistoryEntry>();
            }
            var payload = await resp.Content.ReadFromJsonAsync<HistoryListResponse>(cancellationToken: ct);
            var entries = payload?.Entries ?? new List<DailyHistoryEntry>();
            await _localStorage.SetItemAsync(CacheKey, entries);
            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET /api/toeic/history failed");
            var cached = await _localStorage.GetItemAsync<List<DailyHistoryEntry>>(CacheKey);
            return cached ?? new List<DailyHistoryEntry>();
        }
    }

    public async Task<DailyHistoryEntry?> GetByDateAsync(string dateKey, CancellationToken ct = default)
    {
        var baseUrl = BaseUrl;
        if (string.IsNullOrEmpty(baseUrl)) return null;

        var token = await _auth.GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/toeic/history?dateKey={dateKey}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var resp = await _http.SendAsync(request, ct);
            if (!resp.IsSuccessStatusCode) return null;
            var payload = await resp.Content.ReadFromJsonAsync<HistorySingleResponse>(cancellationToken: ct);
            return payload?.Entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET /api/toeic/history?dateKey={DateKey} failed", dateKey);
            return null;
        }
    }

    public async Task<(bool Ok, string? Error)> SaveAsync(DailyHistoryEntry entry, CancellationToken ct = default)
    {
        try
        {
            var cached = await _localStorage.GetItemAsync<List<DailyHistoryEntry>>(CacheKey) ?? new List<DailyHistoryEntry>();
            var idx = cached.FindIndex(e => e.DateKey == entry.DateKey);
            if (idx >= 0) cached[idx] = entry;
            else cached.Add(entry);
            await _localStorage.SetItemAsync(CacheKey, cached);
        }
        catch { }

        var baseUrl = BaseUrl;
        if (string.IsNullOrEmpty(baseUrl)) return (true, null);

        var token = await _auth.GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return (true, "offline");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/toeic/history");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(new
            {
                dateKey = entry.DateKey,
                topicId = entry.TopicId,
                words = entry.Words,
            });
            using var resp = await _http.SendAsync(request, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                return (false, $"HTTP {(int)resp.StatusCode}: {body}");
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST /api/toeic/history failed");
            return (false, ex.Message);
        }
    }

    private sealed class HistoryListResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("entries")]
        public List<DailyHistoryEntry>? Entries { get; set; }
    }

    private sealed class HistorySingleResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("entry")]
        public DailyHistoryEntry? Entry { get; set; }
    }
}
