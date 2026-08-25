using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Models.Trading;

namespace BlazorWasmPortfolioGhAction.Services.Trading;

public interface ITradingApiClient
{
    string BaseUrl { get; }
    Task<WatchlistResponse<CryptoSignal>?> GetPotentialCoinsAsync(string? signalType = null, CancellationToken ct = default);
    Task<WatchlistResponse<FuturesSignal>?> GetPotentialFuturesAsync(string? signalType = null, CancellationToken ct = default);
    Task<WatchlistResponse<StockSignal>?> GetPotentialSymbolsAsync(string? signalType = null, CancellationToken ct = default);
    Task<WatchlistResponse<WorldStock>?> GetPotentialWorldSymbolsAsync(CancellationToken ct = default);
    Task<WatchlistResponse<ForexPair>?> GetPotentialForexPairsAsync(CancellationToken ct = default);
    Task<RealEstatePrice[]?> GetRealEstateAsync(string? region = null, string? type = null, string? location = null, CancellationToken ct = default);
    Task<TriggeredAlert[]?> GetTriggeredAlertsAsync(int limit = 50, CancellationToken ct = default);
    Task MarkAlertsReadAsync(CancellationToken ct = default);
    Task<ScriptStatusResponse?> GetScriptStatusAsync(CancellationToken ct = default);
    Task RunSshScriptAsync(string scriptType, CancellationToken ct = default);
    Task RestartScriptAsync(CancellationToken ct = default);
    Task<PriceAlert[]?> GetPriceAlertsAsync(CancellationToken ct = default);
    Task CreatePriceAlertAsync(CreateAlertRequest request, CancellationToken ct = default);
    Task DeletePriceAlertAsync(string symbol, string assetType, CancellationToken ct = default);
    Task<JournalEntry[]?> GetJournalAsync(string userId, CancellationToken ct = default);
    Task CreateJournalEntryAsync(string userId, object entry, CancellationToken ct = default);
    Task DeleteJournalEntryAsync(string userId, int id, CancellationToken ct = default);
    Task<CommunityPost[]?> GetCommunityPostsAsync(CancellationToken ct = default);
    Task CreateCommunityPostAsync(object post, CancellationToken ct = default);
    Task<CommunityComment[]?> GetCommunityCommentsAsync(int postId, CancellationToken ct = default);
    Task CreateCommunityCommentAsync(object comment, CancellationToken ct = default);
    Task<NewsGroup[]?> GetNewsGroupsAsync(string? userId = null, CancellationToken ct = default);
    Task<NewsItem[]?> GetNewsItemsAsync(int groupId, CancellationToken ct = default);
    Task<string?> GenerateStrategyPromptAsync(CancellationToken ct = default);
    Task<OsintWorldState?> GetWorldStateAsync(CancellationToken ct = default);
    Task<OsintThesis[]?> GetThesesAsync(string? userId = null, bool refresh = false, CancellationToken ct = default);
    Task TriggerThesisUpdateAsync(CancellationToken ct = default);
    Task<TelegramNewsItem[]?> GetTelegramNewsAsync(CancellationToken ct = default);
    Task<SystemSetting[]?> GetSystemSettingsAsync(CancellationToken ct = default);
    Task UpdateSystemSettingAsync(string key, string value, CancellationToken ct = default);
    Task<string?> ChatAsync(string message, string? context = null, CancellationToken ct = default);
    Task<JsonElement?> GetCalendarAsync(CancellationToken ct = default);
    Task<JsonElement?> GetFxRatesAsync(CancellationToken ct = default);
    Task<OsintSignal[]?> GetOsintSignalsAsync(CancellationToken ct = default);
    Task LikeCommunityPostAsync(int postId, CancellationToken ct = default);
    Task UpdateNewsItemAsync(int id, object body, CancellationToken ct = default);
    Task DeleteNewsItemAsync(int id, CancellationToken ct = default);
    Task UpdateNewsGroupAsync(int id, object body, CancellationToken ct = default);
    Task DeleteNewsGroupAsync(int id, CancellationToken ct = default);
    Task<JsonElement?> GetDnseDealsAsync(string? accountId = null, CancellationToken ct = default);
    Task<JsonElement?> PlaceDnseOrderAsync(object order, CancellationToken ct = default);
    Task<JsonElement?> ConfirmDnseOtpAsync(string otp, string? sessionId = null, CancellationToken ct = default);
    Task<JsonElement?> GetTcbsStockInfoAsync(string symbol, CancellationToken ct = default);
    Task<JsonElement?> GetProxyJsonAsync(string path, CancellationToken ct = default);
    Task ToggleScriptScanAsync(bool enabled, CancellationToken ct = default);
    Task<HttpClient> CreateAuthorizedClientAsync();
    string ProxyUrl(string path);
}

public class TradingApiClient : ITradingApiClient
{
    private readonly HttpClient _http;
    private readonly ITradingAuthService _auth;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public TradingApiClient(HttpClient http, ITradingAuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    public string BaseUrl => _http.BaseAddress?.ToString().TrimEnd('/') ?? "";

    public string ProxyUrl(string path) =>
        string.IsNullOrEmpty(path) ? BaseUrl : $"{BaseUrl}/{path.TrimStart('/')}";

    public async Task<HttpClient> CreateAuthorizedClientAsync()
    {
        // Token applied per-request via helper; HttpClient is shared.
        await _auth.GetTokenAsync();
        return _http;
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        await _auth.AuthorizeAsync(request);
        return await _http.SendAsync(request, ct);
    }

    private async Task<T?> GetAsync<T>(string path, CancellationToken ct)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, path.TrimStart('/'));
            var resp = await SendAuthorizedAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return default;
            return await resp.Content.ReadFromJsonAsync<T>(JsonOpts, ct);
        }
        catch
        {
            return default;
        }
    }

    public Task<WatchlistResponse<CryptoSignal>?> GetPotentialCoinsAsync(string? signalType = null, CancellationToken ct = default) =>
        GetAsync<WatchlistResponse<CryptoSignal>>($"getPotentialCoins{Query("signal_type", signalType)}", ct);

    public Task<WatchlistResponse<FuturesSignal>?> GetPotentialFuturesAsync(string? signalType = null, CancellationToken ct = default) =>
        GetAsync<WatchlistResponse<FuturesSignal>>($"getPotentialFuturesCoins{Query("signal_type", signalType)}", ct);

    public Task<WatchlistResponse<StockSignal>?> GetPotentialSymbolsAsync(string? signalType = null, CancellationToken ct = default) =>
        GetAsync<WatchlistResponse<StockSignal>>($"getPotentialSymbols{Query("signal_type", signalType)}", ct);

    public Task<WatchlistResponse<WorldStock>?> GetPotentialWorldSymbolsAsync(CancellationToken ct = default) =>
        GetAsync<WatchlistResponse<WorldStock>>("getPotentialWorldSymbols", ct);

    public Task<WatchlistResponse<ForexPair>?> GetPotentialForexPairsAsync(CancellationToken ct = default) =>
        GetAsync<WatchlistResponse<ForexPair>>("getPotentialForexPairs", ct);

    public async Task<RealEstatePrice[]?> GetRealEstateAsync(string? region = null, string? type = null, string? location = null, CancellationToken ct = default)
    {
        var q = new List<string>();
        if (!string.IsNullOrEmpty(region)) q.Add($"region={Uri.EscapeDataString(region)}");
        if (!string.IsNullOrEmpty(type)) q.Add($"type={Uri.EscapeDataString(type)}");
        if (!string.IsNullOrEmpty(location)) q.Add($"location={Uri.EscapeDataString(location)}");
        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        return await GetAsync<RealEstatePrice[]>($"getRealEstate{qs}", ct);
    }

    public Task<TriggeredAlert[]?> GetTriggeredAlertsAsync(int limit = 50, CancellationToken ct = default) =>
        GetAsync<TriggeredAlert[]>($"triggeredAlerts?limit={limit}", ct);

    public Task MarkAlertsReadAsync(CancellationToken ct = default) =>
        PostAsync("triggeredAlerts/read", new { }, ct);

    public Task<ScriptStatusResponse?> GetScriptStatusAsync(CancellationToken ct = default) =>
        GetAsync<ScriptStatusResponse>("scriptStatus", ct);

    public Task RunSshScriptAsync(string scriptType, CancellationToken ct = default) =>
        PostAsync("runSSHScript", new RunScriptRequest(scriptType), ct);

    public Task RestartScriptAsync(CancellationToken ct = default) =>
        PostAsync("restartScript", new { }, ct);

    public Task<PriceAlert[]?> GetPriceAlertsAsync(CancellationToken ct = default) =>
        GetAsync<PriceAlert[]>("priceAlerts", ct);

    public Task CreatePriceAlertAsync(CreateAlertRequest request, CancellationToken ct = default) =>
        PostAsync("priceAlerts", request, ct);

    public Task DeletePriceAlertAsync(string symbol, string assetType, CancellationToken ct = default) =>
        DeleteAsync($"priceAlerts/?symbol={Uri.EscapeDataString(symbol)}&asset_type={Uri.EscapeDataString(assetType)}", ct);

    public Task<JournalEntry[]?> GetJournalAsync(string userId, CancellationToken ct = default) =>
        GetAsync<JournalEntry[]>($"journal?user_id={Uri.EscapeDataString(userId)}", ct);

    public Task CreateJournalEntryAsync(string userId, object entry, CancellationToken ct = default) =>
        PostAsync($"journal?user_id={Uri.EscapeDataString(userId)}", entry, ct);

    public Task DeleteJournalEntryAsync(string userId, int id, CancellationToken ct = default) =>
        SendAsync(HttpMethod.Delete, $"journal?user_id={Uri.EscapeDataString(userId)}&id={id}", null, ct);

    public Task<CommunityPost[]?> GetCommunityPostsAsync(CancellationToken ct = default) =>
        GetAsync<CommunityPost[]>("community/posts", ct);

    public Task CreateCommunityPostAsync(object post, CancellationToken ct = default) =>
        PostAsync("community/posts", post, ct);

    public Task<CommunityComment[]?> GetCommunityCommentsAsync(int postId, CancellationToken ct = default) =>
        GetAsync<CommunityComment[]>($"community/comments?post_id={postId}", ct);

    public Task CreateCommunityCommentAsync(object comment, CancellationToken ct = default) =>
        PostAsync("community/comments", comment, ct);

    public Task<NewsGroup[]?> GetNewsGroupsAsync(string? userId = null, CancellationToken ct = default) =>
        GetAsync<NewsGroup[]>($"api/news-groups{Query("user_id", userId)}", ct);

    public Task<NewsItem[]?> GetNewsItemsAsync(int groupId, CancellationToken ct = default) =>
        GetAsync<NewsItem[]>($"api/news-items?group_id={groupId}", ct);

    public async Task<string?> GenerateStrategyPromptAsync(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/news-groups/generate-prompt", ct);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadAsStringAsync(ct) : null;
    }

    public Task<OsintWorldState?> GetWorldStateAsync(CancellationToken ct = default) =>
        GetAsync<OsintWorldState>("api/osint/world-state", ct);

    public Task<OsintThesis[]?> GetThesesAsync(string? userId = null, bool refresh = false, CancellationToken ct = default)
    {
        var q = new List<string>();
        if (!string.IsNullOrEmpty(userId)) q.Add($"user_id={Uri.EscapeDataString(userId)}");
        if (refresh) q.Add("refresh=true");
        var qs = q.Count > 0 ? "?" + string.Join("&", q) : "";
        return GetAsync<OsintThesis[]>($"api/osint/theses{qs}", ct);
    }

    public Task TriggerThesisUpdateAsync(CancellationToken ct = default) =>
        PostAsync("api/osint/theses/trigger", new { }, ct);

    public Task<TelegramNewsItem[]?> GetTelegramNewsAsync(CancellationToken ct = default) =>
        GetAsync<TelegramNewsItem[]>("api/news/telegram", ct);

    public Task<SystemSetting[]?> GetSystemSettingsAsync(CancellationToken ct = default) =>
        GetAsync<SystemSetting[]>("api/settings", ct);

    public Task UpdateSystemSettingAsync(string key, string value, CancellationToken ct = default) =>
        PostAsync("api/settings/update", new { key, value }, ct);

    public async Task<string?> ChatAsync(string message, string? context = null, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/chat", new ChatRequest(message, context), ct);
        if (!resp.IsSuccessStatusCode) return null;
        var data = await resp.Content.ReadFromJsonAsync<ChatResponse>(JsonOpts, ct);
        return data?.Reply;
    }

    public async Task<JsonElement?> GetCalendarAsync(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("ff_calendar_thisweek.json", ct);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOpts, ct);
    }

    public async Task<JsonElement?> GetFxRatesAsync(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/rates", ct);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOpts, ct);
    }

    public Task<OsintSignal[]?> GetOsintSignalsAsync(CancellationToken ct = default) =>
        GetAsync<OsintSignal[]>("api/osint/signals", ct);

    public Task LikeCommunityPostAsync(int postId, CancellationToken ct = default) =>
        PostAsync($"community/posts/{postId}/like", new { }, ct);

    public Task UpdateNewsItemAsync(int id, object body, CancellationToken ct = default) =>
        SendAsync(HttpMethod.Put, $"api/news-items/{id}", body, ct);

    public Task DeleteNewsItemAsync(int id, CancellationToken ct = default) =>
        DeleteAsync($"api/news-items/{id}", ct);

    public Task UpdateNewsGroupAsync(int id, object body, CancellationToken ct = default) =>
        SendAsync(HttpMethod.Put, $"api/news-groups/{id}", body, ct);

    public Task DeleteNewsGroupAsync(int id, CancellationToken ct = default) =>
        DeleteAsync($"api/news-groups/{id}", ct);

    public Task<JsonElement?> GetDnseDealsAsync(string? accountId = null, CancellationToken ct = default)
    {
        var path = string.IsNullOrWhiteSpace(accountId)
            ? "dnse-order-service/deals"
            : $"dnse-order-service/accounts/{Uri.EscapeDataString(accountId)}/deals";
        return GetJsonAsync(path, ct);
    }

    public Task<JsonElement?> PlaceDnseOrderAsync(object order, CancellationToken ct = default) =>
        PostJsonAsync("dnse-order-service/orders", order, ct);

    public Task<JsonElement?> ConfirmDnseOtpAsync(string otp, string? sessionId = null, CancellationToken ct = default) =>
        PostJsonAsync("dnse-order-service/orders/confirm", new { otp, session_id = sessionId }, ct);

    public Task<JsonElement?> GetTcbsStockInfoAsync(string symbol, CancellationToken ct = default) =>
        GetJsonAsync($"tcanalysis/{Uri.EscapeDataString(symbol)}", ct);

    public Task<JsonElement?> GetProxyJsonAsync(string path, CancellationToken ct = default) =>
        GetJsonAsync(path, ct);

    public Task ToggleScriptScanAsync(bool enabled, CancellationToken ct = default) =>
        UpdateSystemSettingAsync("script_scan_enabled", enabled ? "true" : "false", ct);

    private async Task<JsonElement?> GetJsonAsync(string path, CancellationToken ct)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, path.TrimStart('/'));
            var resp = await SendAuthorizedAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOpts, ct);
        }
        catch
        {
            return null;
        }
    }

    private async Task<JsonElement?> PostJsonAsync(string path, object body, CancellationToken ct)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Post, path.TrimStart('/'))
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            var resp = await SendAuthorizedAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOpts, ct);
        }
        catch
        {
            return null;
        }
    }

    private async Task PostAsync(string path, object body, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, path.TrimStart('/'))
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        await SendAuthorizedAsync(req, ct);
    }

    private async Task DeleteAsync(string path, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, path.TrimStart('/'));
        await SendAuthorizedAsync(req, ct);
    }

    private Task SendAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        var req = new HttpRequestMessage(method, path.TrimStart('/'));
        if (body != null)
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        return SendAuthorizedAsync(req, ct);
    }

    private static string Query(string key, string? value) =>
        string.IsNullOrWhiteSpace(value) ? "" : $"?{key}={Uri.EscapeDataString(value)}";
}
