using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorWasmPortfolioGhAction.Services.Trading;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public interface IBrokerGeminiClient
{
    bool HasDirectKey { get; }
    string? UserKey { get; set; }
    bool HasUserKey { get; }
    string? UserModel { get; set; }
    Task<string?> ExplainAsync(string prompt, CancellationToken ct = default);
}

public sealed class BrokerGeminiClient : IBrokerGeminiClient
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private readonly BrokerOptions _options;
    private readonly ITradingApiClient _api;

    public BrokerGeminiClient(HttpClient http, BrokerOptions options, ITradingApiClient api)
    {
        _http = http;
        _options = options;
        _api = api;
    }

    public bool HasDirectKey => !string.IsNullOrWhiteSpace(_options.ApiKey);
    public string? UserKey { get; set; }
    public bool HasUserKey => !string.IsNullOrWhiteSpace(UserKey);
    public string? UserModel { get; set; }

    public async Task<string?> ExplainAsync(string prompt, CancellationToken ct = default)
    {
        if (HasUserKey)
        {
            try
            {
                var direct = await CallGeminiAsync(prompt, UserKey!.Trim(), UserModel, ct);
                if (!string.IsNullOrWhiteSpace(direct))
                    return direct;
            }
            catch (Exception ex)
            {
                return $"[Lỗi Gemini] {ex.Message}";
            }
        }

        if (HasDirectKey)
        {
            try
            {
                var direct = await CallGeminiAsync(prompt, _options.ApiKey.Trim(), _options.Model, ct);
                if (!string.IsNullOrWhiteSpace(direct))
                    return direct;
            }
            catch (Exception ex)
            {
                return $"[Lỗi Gemini] {ex.Message}";
            }
        }

        return await _api.ChatAsync(prompt, context: "broker-desk", ct);
    }

    private async Task<string?> CallGeminiAsync(string prompt, string apiKey, string? model, CancellationToken ct)
    {
        var m = string.IsNullOrWhiteSpace(model) ? GeminiModels.DefaultId : model.Trim();
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{m}:generateContent?key={apiKey}";
        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.2, maxOutputTokens = 1200 }
        };

        using var resp = await _http.PostAsJsonAsync(url, body, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var errBody = await resp.Content.ReadAsStringAsync(ct);
            var shortErr = string.IsNullOrWhiteSpace(errBody)
                ? resp.StatusCode.ToString()
                : errBody.Length > 300 ? errBody[..300] + "…" : errBody;
            throw new HttpRequestException($"HTTP {(int)resp.StatusCode} {resp.StatusCode} — {shortErr}");
        }

        var parsed = await resp.Content.ReadFromJsonAsync<GeminiResponse>(JsonOpts, ct);
        return parsed?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text?.Trim();
    }

    private sealed class GeminiResponse
    {
        public Candidate[]? Candidates { get; set; }
    }

    private sealed class Candidate
    {
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        public Part[]? Parts { get; set; }
    }

    private sealed class Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
