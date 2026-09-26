using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Shared.Model;
using Microsoft.Extensions.Logging;

namespace BlazorWasmPortfolioGhAction.Services;

public interface IWikiContentService
{
    Task<IReadOnlyList<string>> GetManifestFilesAsync(CancellationToken cancellationToken = default);
    Task<List<ContentHolder>> LoadFileContentsAsync(string fileNameWithoutSuffix, CancellationToken cancellationToken = default);
    Task<bool> UpdateGitHubContentAsync(
        List<ContentHolder> contentHolders,
        string commitMessage,
        string page,
        string section,
        Dictionary<string, string> shaDictionary,
        CancellationToken cancellationToken = default);
    Task<string?> GetFileShaAsync(string fileKey, CancellationToken cancellationToken = default);
    Task<bool> DeleteGitHubFileAsync(string commitMessage, string fileKey, string sha, CancellationToken cancellationToken = default);
}

public class WikiContentService : IWikiContentService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<WikiContentService> _logger;

    public WikiContentService(HttpClient http, IConfiguration config, ILogger<WikiContentService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> GetManifestFilesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var manifest = await _http.GetFromJsonAsync<WikiManifest>("wiki/manifest.json", cancellationToken);
            return manifest?.Files ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load wiki manifest");
            return [];
        }
    }

    public async Task<List<ContentHolder>> LoadFileContentsAsync(string fileNameWithoutSuffix, CancellationToken cancellationToken = default)
    {
        try
        {
            var contents = await _http.GetFromJsonAsync<List<ContentHolder>>(
                $"wiki/{fileNameWithoutSuffix}.json", cancellationToken);
            return contents ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load wiki file {File}", fileNameWithoutSuffix);
            return [];
        }
    }

    public async Task<bool> UpdateGitHubContentAsync(
        List<ContentHolder> contentHolders,
        string commitMessage,
        string page,
        string section,
        Dictionary<string, string> shaDictionary,
        CancellationToken cancellationToken = default)
    {
        var token = _config["DevOps:GitHubToken"];
        var owner = _config["Wiki:GitHubOwner"];
        var repo = _config["Wiki:GitHubRepo"];
        var branch = _config["Wiki:GitHubBranch"] ?? "master";
        var contentDir = _config["Wiki:ContentDirectory"] ?? "wwwroot/wiki";

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
            return false;

        var fileKey = $"{page}{section}";
        var fileName = $"{fileKey}.json";
        var path = $"{contentDir}/{fileName}";
        var json = JsonSerializer.Serialize(contentHolders, new JsonSerializerOptions { WriteIndented = true });
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        shaDictionary.TryGetValue(fileKey, out var sha);

        var payload = new Dictionary<string, object>
        {
            ["message"] = commitMessage,
            ["content"] = base64,
            ["branch"] = branch
        };

        if (!string.IsNullOrEmpty(sha))
            payload["sha"] = sha;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Put,
                $"https://api.github.com/repos/{owner}/{repo}/contents/{path}")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("Authorization", $"token {token}");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "BlazorPortfolio");

            var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Wiki update failed with HTTP {Status} for {Path}", (int)response.StatusCode, path);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update wiki file {Path}", path);
            return false;
        }
    }

    public async Task<bool> DeleteGitHubFileAsync(string commitMessage, string fileKey, string sha, CancellationToken cancellationToken = default)
    {
        var token = _config["DevOps:GitHubToken"];
        var owner = _config["Wiki:GitHubOwner"];
        var repo = _config["Wiki:GitHubRepo"];
        var branch = _config["Wiki:GitHubBranch"] ?? "master";
        var contentDir = _config["Wiki:ContentDirectory"] ?? "wwwroot/wiki";

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo) || string.IsNullOrWhiteSpace(sha))
            return false;

        var path = $"{contentDir}/{fileKey}.json";
        var payload = new { message = commitMessage, sha, branch };

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete,
                $"https://api.github.com/repos/{owner}/{repo}/contents/{path}")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("Authorization", $"token {token}");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "BlazorPortfolio");

            var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Wiki delete failed with HTTP {Status} for {Path}", (int)response.StatusCode, path);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete wiki file {Path}", path);
            return false;
        }
    }

    public async Task<string?> GetFileShaAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var token = _config["DevOps:GitHubToken"];
        var owner = _config["Wiki:GitHubOwner"];
        var repo = _config["Wiki:GitHubRepo"];
        var contentDir = _config["Wiki:ContentDirectory"] ?? "wwwroot/wiki";

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
            return null;

        var path = $"{contentDir}/{fileKey}.json";

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.github.com/repos/{owner}/{repo}/contents/{path}");
            request.Headers.Add("Authorization", $"token {token}");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "BlazorPortfolio");

            var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("sha", out var shaEl) ? shaEl.GetString() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read wiki SHA for {Path}", path);
            return null;
        }
    }

    private sealed class WikiManifest
    {
        public List<string> Files { get; set; } = [];
    }
}
