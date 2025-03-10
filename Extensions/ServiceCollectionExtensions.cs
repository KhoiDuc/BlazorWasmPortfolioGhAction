using BlazorWasmPortfolioGhAction.Store.Services;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

namespace BlazorWasmPortfolioGhAction.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGraphQLClient(this IServiceCollection services)
        {
            services.AddScoped<IGraphQLClient>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var baseUrl = configuration[SectionNames.GitHubGraphQLBaseUrl];
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    throw new Exception("Missing github base url configuration");
                }

                return new GraphQLHttpClient(baseUrl, new SystemTextJsonSerializer());
            });

            return services;
        }

        public static IServiceCollection AddGitHubGraphQLQueryService(this IServiceCollection services)
        {
            services.AddScoped<IGitHubGraphQLQueryService>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                _ = bool.TryParse(configuration[SectionNames.MockApi], out var mockApi);

                return mockApi ? new GitHubGraphQLQueryServiceMock() : new GitHubService(sp.GetRequiredService<IGraphQLClient>());
            });

            return services;
        }
    }
}
