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
    Task<GeminiExplainResult> ExplainAsync(string prompt, CancellationToken ct = default);
}

public sealed class BrokerGeminiClient : IBrokerGeminiClient
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private readonly BrokerOptions _options;

    public BrokerGeminiClient(HttpClient http, BrokerOptions options)
    {
        _http = http;
        _options = options;
    }

    public bool HasDirectKey => !string.IsNullOrWhiteSpace(_options.ApiKey);
    public string? UserKey { get; set; }
    public bool HasUserKey => !string.IsNullOrWhiteSpace(UserKey);
    public string? UserModel { get; set; }

    public async Task<GeminiExplainResult> ExplainAsync(string prompt, CancellationToken ct = default)
    {
        if (HasUserKey)
        {
            var userResult = await TryDirectAsync(prompt, UserKey!.Trim(), UserModel, ct);
            if (userResult is not null)
                return userResult;
        }

        if (HasDirectKey)
        {
            var configResult = await TryDirectAsync(prompt, _options.ApiKey.Trim(), _options.Model, ct);
            if (configResult is not null)
                return configResult;
        }

        // ponytail: no internal proxy fallback anymore — require a key.
        return new GeminiExplainResult(null,
            "Không có Gemini API key. Nhập key (nút khóa trên Broker desk) hoặc cấu hình Gemini:ApiKey.");
    }

    private async Task<GeminiExplainResult?> TryDirectAsync(
        string prompt,
        string apiKey,
        string? model,
        CancellationToken ct)
    {
        try
        {
            var direct = await CallGeminiAsync(prompt, apiKey, model, ct);
            if (!string.IsNullOrWhiteSpace(direct))
                return new GeminiExplainResult(direct, null);
        }
        catch (Exception ex)
        {
            return new GeminiExplainResult(null, ex.Message);
        }

        return null;
    }

    private async Task<string?> CallGeminiAsync(string prompt, string apiKey, string? model, CancellationToken ct)
    {
        var m = string.IsNullOrWhiteSpace(model) ? GeminiModels.DefaultId : model.Trim();
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{m}:generateContent?key={apiKey}";
        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.2, maxOutputTokens = 2048 }
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
