using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Shared.Model;

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
    Task<bool> DeleteGitHubFileAsync(string commitMessage, string section, string sha, CancellationToken cancellationToken = default);
}

public class WikiContentService : IWikiContentService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public WikiContentService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<IReadOnlyList<string>> GetManifestFilesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var manifest = await _http.GetFromJsonAsync<WikiManifest>("wiki/manifest.json", cancellationToken);
            return manifest?.Files ?? [];
        }
        catch
        {
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
        catch
        {
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

        shaDictionary.TryGetValue(section, out var sha);

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
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteGitHubFileAsync(string commitMessage, string section, string sha, CancellationToken cancellationToken = default)
    {
        var token = _config["DevOps:GitHubToken"];
        var owner = _config["Wiki:GitHubOwner"];
        var repo = _config["Wiki:GitHubRepo"];
        var branch = _config["Wiki:GitHubBranch"] ?? "master";
        var contentDir = _config["Wiki:ContentDirectory"] ?? "wwwroot/wiki";

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo) || string.IsNullOrWhiteSpace(sha))
            return false;

        var path = $"{contentDir}/{section}.json";
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
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private sealed class WikiManifest
    {
        public List<string> Files { get; set; } = [];
    }
}
