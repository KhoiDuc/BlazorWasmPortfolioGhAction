using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorWasmPortfolioGhAction.Services.Trading;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public interface IBrokerGeminiClient
{
    bool HasDirectKey { get; }
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

    public async Task<string?> ExplainAsync(string prompt, CancellationToken ct = default)
    {
        if (HasDirectKey)
        {
            var direct = await CallGeminiAsync(prompt, ct);
            if (!string.IsNullOrWhiteSpace(direct))
                return direct;
        }

        return await _api.ChatAsync(prompt, context: "broker-desk", ct);
    }

    private async Task<string?> CallGeminiAsync(string prompt, CancellationToken ct)
    {
        var model = string.IsNullOrWhiteSpace(_options.Model) ? "gemini-2.0-flash" : _options.Model.Trim();
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_options.ApiKey.Trim()}";
        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.2, maxOutputTokens = 1200 }
        };

        using var resp = await _http.PostAsJsonAsync(url, body, ct);
        if (!resp.IsSuccessStatusCode)
            return null;

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
