using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BlazorWasmPortfolioGhAction.Resources;
using Microsoft.Extensions.Localization;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public interface IBrokerGeminiClient
{
    bool HasDirectKey { get; }
    string? UserKey { get; set; }
    bool HasUserKey { get; }
    string? UserModel { get; set; }
    Task<GeminiExplainResult> ExplainAsync(string prompt, CancellationToken ct = default);
}

public sealed class BrokerGeminiClient : IBrokerGeminiClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly IBrokerAuthService _auth;
    private readonly IStringLocalizer<SharedResources> _L;

    public BrokerGeminiClient(HttpClient http, IConfiguration config, IBrokerAuthService auth, IStringLocalizer<SharedResources> L)
    {
        _http = http;
        _config = config;
        _auth = auth;
        _L = L;
    }

    public bool HasDirectKey => false;
    public string? UserKey { get; set; }
    public bool HasUserKey => false;
    public string? UserModel { get; set; }

    public async Task<GeminiExplainResult> ExplainAsync(string prompt, CancellationToken ct = default)
    {
        var baseUrl = _config["BrokerApi:BaseUrl"]?.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            return new GeminiExplainResult(null, "BrokerApi:BaseUrl is not configured.");

        var token = await _auth.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return new GeminiExplainResult(null, _L["Trading_Gemini_MissingKey"].Value);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/ai/chat");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(new { prompt, model = string.IsNullOrWhiteSpace(UserModel) ? null : UserModel });
            using var resp = await _http.SendAsync(request, ct);
            var payload = await resp.Content.ReadFromJsonAsync<AiChatResponse>(cancellationToken: ct);
            if (!resp.IsSuccessStatusCode)
                return new GeminiExplainResult(null, payload?.Error ?? $"HTTP {(int)resp.StatusCode}");
            return new GeminiExplainResult(payload?.Text, string.IsNullOrWhiteSpace(payload?.Text) ? "Empty Gemini response." : null);
        }
        catch (Exception ex)
        {
            return new GeminiExplainResult(null, ex.Message);
        }
    }

    private sealed class AiChatResponse
    {
        public string? Text { get; set; }
        public string? Error { get; set; }
        [JsonPropertyName("model")]
        public string? Model { get; set; }
    }
}
