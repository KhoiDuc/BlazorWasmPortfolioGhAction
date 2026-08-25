using System.Net.Http.Json;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Models.GitHub;

namespace BlazorWasmPortfolioGhAction.Services;

public interface IDevOpsService
{
    Task<bool> SendSlackMessageAsync(string channel, string message);
    Task<bool> SendDiscordMessageAsync(string message, string? title = null, string? description = null);
    Task<bool> SendTeamsMessageAsync(string title, string message);
    Task<GitHubSearchResult?> SearchGitHubReposAsync(string query, string? language = null, int sort = 0);
    Task<GitHubRestUser?> GetGitHubUserAsync(string username);
    Task<GitHubRestRepo?> GetGitHubRepoAsync(string owner, string repo);
    Task<List<GitHubRestRepo>?> GetGitHubTrendingAsync(string language, string timeRange);
    Task<GitHubRestGist?> CreateGistAsync(string description, Dictionary<string, string> files, bool isPublic);
}

public class DevOpsService : IDevOpsService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public DevOpsService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<bool> SendSlackMessageAsync(string channel, string message)
    {
        var webhookUrl = _config["DevOps:SlackWebhookUrl"];
        if (string.IsNullOrWhiteSpace(webhookUrl)) return false;

        var payload = new
        {
            channel = channel.StartsWith("#") ? channel : $"#{channel}",
            text = message,
            username = "Portfolio Bot"
        };

        try
        {
            var response = await _http.PostAsJsonAsync(webhookUrl, payload);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendDiscordMessageAsync(string message, string? title = null, string? description = null)
    {
        var webhookUrl = _config["DevOps:DiscordWebhookUrl"];
        if (string.IsNullOrWhiteSpace(webhookUrl)) return false;

        object payload = title != null
            ? new
            {
                embeds = new[]
                {
                    new
                    {
                        title = title,
                        description = description ?? message,
                        color = 0x5865F2,
                        footer = new { text = "Portfolio Bot" }
                    }
                }
            }
            : new { content = message };

        try
        {
            var response = await _http.PostAsJsonAsync(webhookUrl, payload);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendTeamsMessageAsync(string title, string message)
    {
        var webhookUrl = _config["DevOps:TeamsWebhookUrl"];
        if (string.IsNullOrWhiteSpace(webhookUrl)) return false;

        var payload = new
        {
            @type = "MessageCard",
            @context = "https://schema.org/extensions",
            summary = title,
            themeColor = "0078D4",
            sections = new[]
            {
                new
                {
                    activityTitle = title,
                    activitySubtitle = "Portfolio Bot",
                    text = message
                }
            }
        };

        try
        {
            var response = await _http.PostAsJsonAsync(webhookUrl, payload);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<GitHubSearchResult?> SearchGitHubReposAsync(string query, string? language = null, int sort = 0)
    {
        var token = _config["DevOps:GitHubToken"];
        var q = query;
        if (!string.IsNullOrWhiteSpace(language))
            q += $" language:{language}";

        var sortBy = sort switch
        {
            1 => "forks",
            2 => "updated",
            _ => "stars"
        };

        try
        {
            var url = $"https://api.github.com/search/repositories?q={Uri.EscapeDataString(q)}&sort={sortBy}&order=desc&per_page=30";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Add("Authorization", $"token {token}");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "BlazorPortfolio");

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GitHubSearchResult>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        catch
        {
            return null;
        }
    }

    public async Task<GitHubRestUser?> GetGitHubUserAsync(string username)
    {
        var token = _config["DevOps:GitHubToken"];

        try
        {
            var url = $"https://api.github.com/users/{username}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Add("Authorization", $"token {token}");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "BlazorPortfolio");

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GitHubRestUser>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        catch
        {
            return null;
        }
    }

    public async Task<GitHubRestRepo?> GetGitHubRepoAsync(string owner, string repo)
    {
        var token = _config["DevOps:GitHubToken"];

        try
        {
            var url = $"https://api.github.com/repos/{owner}/{repo}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Add("Authorization", $"token {token}");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "BlazorPortfolio");

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GitHubRestRepo>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<GitHubRestRepo>?> GetGitHubTrendingAsync(string language, string timeRange)
    {
        try
        {
            var date = GetTrendingDate(timeRange);
            var url = $"https://api.github.com/search/repositories?q=created:>{date}&sort=stars&order=desc&per_page=30";
            if (!string.IsNullOrWhiteSpace(language))
                url += $"&language={language}";

            var token = _config["DevOps:GitHubToken"];
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Add("Authorization", $"token {token}");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "BlazorPortfolio");

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GitHubSearchResult>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return result?.Items ?? new();
        }
        catch
        {
            return null;
        }
    }

    public async Task<GitHubRestGist?> CreateGistAsync(string description, Dictionary<string, string> files, bool isPublic)
    {
        var token = _config["DevOps:GitHubToken"];
        if (string.IsNullOrWhiteSpace(token)) return null;

        try
        {
            var filesObj = files.ToDictionary(
                kvp => kvp.Key,
                kvp => new { content = kvp.Value } as object
            );

            var payload = new
            {
                description,
                @public = isPublic,
                files = filesObj
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.github.com/gists")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"token {token}");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "BlazorPortfolio");

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GitHubRestGist>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        catch
        {
            return null;
        }
    }

    private static string GetTrendingDate(string timeRange) => timeRange switch
    {
        "week" => DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd"),
        "month" => DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd"),
        _ => DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")
    };
}
