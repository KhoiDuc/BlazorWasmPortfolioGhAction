using Bogus;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using OneOf.Types;
using System.Net.Http.Headers;
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
        public string Bio { get; set; }
        public string Email { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GitHubRepository
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Stars { get; set; }
        public int Forks { get; set; }
        public string Url { get; set; }
    }

    public class GitHubRepositoryConnection
    {
        public List<GitHubRepository> Repositories { get; set; } = new List<GitHubRepository>();
        public string EndCursor { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class GitHubRateLimit
    {
        public int Remaining { get; set; }
        public DateTime ResetAt { get; set; }
    }
    public sealed class Result<T>
    {
        private Result()
        {
        }

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
    public static class GraphQLClientExtensions
    {
        public static void AddHttpAuthorization(this IGraphQLClient client, string accessToken)
        {
            if (client is GraphQLHttpClient httpClient)
            {
                httpClient.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            }
        }
    }
    public interface IGitHubGraphQLQueryService
    {
        Task<Result<List<GitHubUser>>> SearchUsersAsync(string searchText, string accessToken);
        Task<Result<GitHubUserDetail>> GetUserAsync(string login, string accessToken);
        Task<Result<GitHubRepositoryConnection>> GetUserRepositoriesAsync(string login, string accessToken, int first = 20, string? after = null);
        Task<Result<GitHubRateLimit>> GetRateLimitAsync(string accessToken);
    }
    public interface ISearchUsersService
    {
        Task<Result<List<UserViewModel>>> SearchUsersAsync(string searchText, string accessToken);
    }
    public class GitHubGraphQLQueryServiceMock : IGitHubGraphQLQueryService
    {
        public Task<Result<List<GitHubUser>>> SearchUsersAsync(string searchText, string accessToken)
        {
            var faker = new Faker<GitHubUser>()
                .RuleFor(u => u.Login, f => f.Person.UserName)
                .RuleFor(u => u.Name, f => f.Name.FullName());

            return Task.FromResult(Result<List<GitHubUser>>.Success(faker.Generate(50)));
        }

        public Task<Result<GitHubUserDetail>> GetUserAsync(string login, string accessToken)
        {
            var faker = new Faker<GitHubUserDetail>()
                .RuleFor(u => u.Login, f => f.Person.UserName)
                .RuleFor(u => u.Name, f => f.Name.FullName())
                .RuleFor(u => u.Bio, f => f.Lorem.Sentence())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.FollowersCount, f => f.Random.Int(0, 1000))
                .RuleFor(u => u.FollowingCount, f => f.Random.Int(0, 500))
                .RuleFor(u => u.CreatedAt, f => f.Date.Past());

            return Task.FromResult(Result<GitHubUserDetail>.Success(faker.Generate()));
        }

        public Task<Result<GitHubRepositoryConnection>> GetUserRepositoriesAsync(
            string login, string accessToken, int first = 20, string? after = null)
        {
            var repoFaker = new Faker<GitHubRepository>()
                .RuleFor(r => r.Name, f => f.Company.CatchPhrase())
                .RuleFor(r => r.Description, f => f.Lorem.Sentence())
                .RuleFor(r => r.Stars, f => f.Random.Int(0, 5000))
                .RuleFor(r => r.Forks, f => f.Random.Int(0, 1000))
                .RuleFor(r => r.Url, f => f.Internet.Url());

            return Task.FromResult(Result<GitHubRepositoryConnection>.Success(
                new GitHubRepositoryConnection
                {
                    Repositories = repoFaker.Generate(50),
                    HasNextPage = false,
                    EndCursor = null
                }));
        }

        public Task<Result<GitHubRateLimit>> GetRateLimitAsync(string accessToken)
        {
            return Task.FromResult(Result<GitHubRateLimit>.Success(
                new GitHubRateLimit
                {
                    Remaining = 5000,
                    ResetAt = DateTime.UtcNow.AddHours(1)
                }));
        }
    }
    public class GitHubService : IGitHubGraphQLQueryService
    {
        private readonly IGraphQLClient _graphQlClient;

        public GitHubService(IGraphQLClient graphQlClient)
        {
            _graphQlClient = graphQlClient;
        }

        public async Task<Result<List<GitHubUser>>> SearchUsersAsync(string searchText, string accessToken)
        {
            try
            {
                _graphQlClient.AddHttpAuthorization(accessToken);
                var request = new GraphQLRequest
                {
                    Query = """
                query ($searchText: String!) {
                  search(query: $searchText, type: USER, first: 20) {
                    edges {
                      node {
                        ... on User {
                          login
                          name
                        }
                      }
                    }
                  }
                }
                """,
                    Variables = new { searchText }
                };

                var response = await _graphQlClient.SendQueryAsync<SearchResponse>(request);
                var users = response.Data.Search?.Edges?.Select(e => e.Node).ToList() ?? new List<GitHubUser>();
                return Result<List<GitHubUser>>.Success(users);
            }
            catch (GraphQLHttpRequestException ex)
            {
                return Result<List<GitHubUser>>.Failure("Failed to search users: " + ex.Message);
            }
        }

        public async Task<Result<GitHubUserDetail>> GetUserAsync(string login, string accessToken)
        {
            try
            {
                _graphQlClient.AddHttpAuthorization(accessToken);
                var request = new GraphQLRequest
                {
                    Query = """
                query ($login: String!) {
                  user(login: $login) {
                    login
                    name
                    bio
                    email
                    followers {
                      totalCount
                    }
                    following {
                      totalCount
                    }
                    createdAt
                  }
                }
                """,
                    Variables = new { login }
                };

                var response = await _graphQlClient.SendQueryAsync<UserResponse>(request);
                var userDetail = MapUserDetail(response.Data.User);
                return Result<GitHubUserDetail>.Success(userDetail);
            }
            catch (GraphQLHttpRequestException ex)
            {
                return Result<GitHubUserDetail>.Failure("Failed to fetch user details: " + ex.Message);
            }
        }

        public async Task<Result<GitHubRepositoryConnection>> GetUserRepositoriesAsync(
            string login, string accessToken, int first = 20, string? after = null)
        {
            try
            {
                _graphQlClient.AddHttpAuthorization(accessToken);
                var request = new GraphQLRequest
                {
                    Query = """
                query ($login: String!, $first: Int, $after: String) {
                  user(login: $login) {
                    repositories(first: $first, after: $after) {
                      edges {
                        node {
                          name
                          description
                          stargazerCount
                          forkCount
                          url
                        }
                        cursor
                      }
                      pageInfo {
                        endCursor
                        hasNextPage
                      }
                    }
                  }
                }
                """,
                    Variables = new { login, first, after }
                };

                var response = await _graphQlClient.SendQueryAsync<RepositoryResponse>(request);
                var repositories = response.Data.User.Repositories.Edges.Select(e => e.Node).ToList();
                var pageInfo = response.Data.User.Repositories.PageInfo;

                return Result<GitHubRepositoryConnection>.Success(new GitHubRepositoryConnection
                {
                    Repositories = repositories,
                    EndCursor = pageInfo.EndCursor,
                    HasNextPage = pageInfo.HasNextPage
                });
            }
            catch (GraphQLHttpRequestException ex)
            {
                return Result<GitHubRepositoryConnection>.Failure("Failed to fetch repositories: " + ex.Message);
            }
        }

        public async Task<Result<GitHubRateLimit>> GetRateLimitAsync(string accessToken)
        {
            try
            {
                _graphQlClient.AddHttpAuthorization(accessToken);
                var request = new GraphQLRequest
                {
                    Query = """
                {
                  rateLimit {
                    remaining
                    resetAt
                  }
                }
                """
                };

                var response = await _graphQlClient.SendQueryAsync<RateLimitResponse>(request);
                return Result<GitHubRateLimit>.Success(response.Data.RateLimit);
            }
            catch (GraphQLHttpRequestException ex)
            {
                return Result<GitHubRateLimit>.Failure("Failed to fetch rate limit: " + ex.Message);
            }
        }

        private GitHubUserDetail MapUserDetail(UserResponse.UserData user)
        {
            return new GitHubUserDetail
            {
                Login = user.Login,
                Name = user.Name,
                Bio = user.Bio,
                Email = user.Email,
                FollowersCount = user.Followers.TotalCount,
                FollowingCount = user.Following.TotalCount,
                CreatedAt = user.CreatedAt
            };
        }
    }

    // Supporting response classes
    public class SearchResponse
    {
        public SearchData Search { get; set; }

        public class SearchData
        {
            public List<Edge> Edges { get; set; }
        }

        public class Edge
        {
            public GitHubUser Node { get; set; }
        }
    }

    public class UserResponse
    {
        public UserData User { get; set; }

        public class UserData
        {
            public string Login { get; set; }
            public string Name { get; set; }
            public string Bio { get; set; }
            public string Email { get; set; }
            public FollowerData Followers { get; set; }
            public FollowerData Following { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class FollowerData
        {
            public int TotalCount { get; set; }
        }
    }

    public class RepositoryResponse
    {
        public UserData User { get; set; }

        public class UserData
        {
            public RepositoryConnection Repositories { get; set; }
        }

        public class RepositoryConnection
        {
            public List<Edge> Edges { get; set; }
            public PageInfo PageInfo { get; set; }
        }

        public class Edge
        {
            public GitHubRepository Node { get; set; }
            public string Cursor { get; set; }
        }

        public class PageInfo
        {
            public string EndCursor { get; set; }
            public bool HasNextPage { get; set; }
        }
    }

    public class RateLimitResponse
    {
        public GitHubRateLimit RateLimit { get; set; }
    }
    public class StateContainer
    {
        public List<GitHubUser> SearchedUsers { get; } = new();
    }
    public class SearchUsersService : ISearchUsersService
    {
        private readonly IGitHubGraphQLQueryService _gitHubGraphQlQueryService;
        private readonly StateContainer _stateContainer;

        public SearchUsersService(IGitHubGraphQLQueryService gitHubGraphQlQueryService, StateContainer stateContainer)
        {
            _gitHubGraphQlQueryService = gitHubGraphQlQueryService;
            _stateContainer = stateContainer;
        }

        public async Task<Result<List<UserViewModel>>> SearchUsersAsync(string searchText, string accessToken)
        {
            var result = await _gitHubGraphQlQueryService.SearchUsersAsync(searchText, accessToken);
            if (!result.IsSuccess)
            {
                return Result<List<UserViewModel>>.Failure(result.Error!);
            }

            _stateContainer.SearchedUsers.Clear();
            _stateContainer.SearchedUsers.AddRange(result.Value!);

            var viewModels = result.Value!.Select(u => u.MapToViewModel()).ToList();

            return Result<List<UserViewModel>>.Success(viewModels);
        }
    }
}
