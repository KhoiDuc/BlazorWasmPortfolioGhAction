using System.Net.Http.Json;
using System.Text.Json;

namespace VnDesk.Clients;

public sealed class OllamaClient
{
    private readonly HttpClient _http;
    private readonly AppConfig _cfg;

    public OllamaClient(AppConfig cfg)
    {
        _cfg = cfg;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(90) };
        _http.BaseAddress = new Uri(cfg.OllamaUrl.TrimEnd('/') + "/");
    }

    public async Task<(bool ok, string text)> GenerateAsync(string prompt)
    {
        try
        {
            var body = new { model = _cfg.OllamaModel, prompt, stream = false };
            using var resp = await _http.PostAsJsonAsync("api/generate", body);
            if (!resp.IsSuccessStatusCode)
                return (false, $"Ollama HTTP {(int)resp.StatusCode}");

            await using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var text = doc.RootElement.TryGetProperty("response", out var r) ? r.GetString() ?? "" : "";
            return (true, text.Trim());
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
