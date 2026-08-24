using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Services;

public interface IDevOpsService
{
    Task<bool> SendSlackMessageAsync(string channel, string message);
    Task<bool> SendDiscordMessageAsync(string message, string? title = null, string? description = null);
    Task<bool> SendTeamsMessageAsync(string title, string message);
    Task<GitHubSearchResult?> SearchGitHubReposAsync(string query, string? language = null, int sort = 0);
    Task<GitHubUser?> GetGitHubUserAsync(string username);
    Task<GitHubRepo?> GetGitHubRepoAsync(string owner, string repo);
    Task<List<GitHubRepo>?> GetGitHubTrendingAsync(string language, string timeRange);
    Task<GitHubGist?> CreateGistAsync(string description, Dictionary<string, string> files, bool isPublic);
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

    public async Task<GitHubUser?> GetGitHubUserAsync(string username)
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
            return JsonSerializer.Deserialize<GitHubUser>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        catch
        {
            return null;
        }
    }

    public async Task<GitHubRepo?> GetGitHubRepoAsync(string owner, string repo)
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
            return JsonSerializer.Deserialize<GitHubRepo>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<GitHubRepo>?> GetGitHubTrendingAsync(string language, string timeRange)
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

    public async Task<GitHubGist?> CreateGistAsync(string description, Dictionary<string, string> files, bool isPublic)
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
            return JsonSerializer.Deserialize<GitHubGist>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
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

// DTOs
public class GitHubSearchResult
{
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    [JsonPropertyName("items")]
    public List<GitHubRepo> Items { get; set; } = new();
}

public class GitHubRepo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("stargazers_count")]
    public int Stars { get; set; }

    [JsonPropertyName("forks_count")]
    public int Forks { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("topics")]
    public List<string> Topics { get; set; } = new();

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("owner")]
    public GitHubUser? Owner { get; set; }
}

public class GitHubUser
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("bio")]
    public string? Bio { get; set; }

    [JsonPropertyName("company")]
    public string? Company { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("public_repos")]
    public int PublicRepos { get; set; }

    [JsonPropertyName("followers")]
    public int Followers { get; set; }

    [JsonPropertyName("following")]
    public int Following { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

public class GitHubGist
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("public")]
    public bool Public { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
