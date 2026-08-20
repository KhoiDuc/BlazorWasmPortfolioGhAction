using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Store.Services
{
    public class GitHubEdge<T>
    {
        [JsonPropertyName("node")]
        public T? Node { get; set; }
    }

    public class GitHubSearch<T>
    {
        [JsonPropertyName("edges")]
        public IEnumerable<GitHubEdge<T>>? Edges { get; set; }
    }

    public class GitHubSearchData<T>
    {
        [JsonPropertyName("search")]
        public GitHubSearch<T>? Search { get; set; }
    }

    public class GitHubUser
    {
        [JsonPropertyName("login")]
        public string? Login { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class GitHubUserDetail : GitHubUser
    {
        public string Bio { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GitHubRepository
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Stars { get; set; }
        public int Forks { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    public class GitHubRepositoryConnection
    {
        public List<GitHubRepository> Repositories { get; set; } = new List<GitHubRepository>();
        public string EndCursor { get; set; } = string.Empty;
        public bool HasNextPage { get; set; }
    }

    public class GitHubRateLimit
    {
        public int Remaining { get; set; }
        public DateTime ResetAt { get; set; }
    }

    public sealed class Result<T>
    {
        private Result() { }

        public T? Value { get; set; }
        public string? Error { get; set; }

        public bool IsSuccess => Value is not null;

        public static Result<T> Success(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new Result<T> { Value = value };
        }

        public static Result<T> Failure(string error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            return new Result<T> { Error = error };
        }
    }

    public record UserViewModel(string Login, string Name);

    public static class SectionNames
    {
        public const string GitHubGraphQLBaseUrl = "GitHubGraphQLBaseUrl";
        public const string MockApi = "MockApi";
    }

    public static class GitHubUserExtensions
    {
        public static UserViewModel MapToViewModel(this GitHubUser user) => new(Login: user.Login ?? "", Name: user.Name ?? "");
    }
}
