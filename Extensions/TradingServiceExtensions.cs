namespace BlazorWasmPortfolioGhAction.Extensions;

public static class TradingServiceExtensions
{
    public const string TradingApiSection = "TradingApi";
    public const string TradingApiClientName = "TradingApi";

    public static IServiceCollection AddTradingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration[$"{TradingApiSection}:BaseUrl"]
            ?? "https://trading-api-dark-sunset-2092.fly.dev";

        services.AddHttpClient(TradingApiClientName, client =>
        {
            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.ITradingAuthService,
            BlazorWasmPortfolioGhAction.Services.Trading.TradingAuthService>();

        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.ITradingApiClient,
            BlazorWasmPortfolioGhAction.Services.Trading.TradingApiClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var http = factory.CreateClient(TradingApiClientName);
            var auth = sp.GetRequiredService<BlazorWasmPortfolioGhAction.Services.Trading.ITradingAuthService>();
            return new BlazorWasmPortfolioGhAction.Services.Trading.TradingApiClient(http, auth);
        });

        return services;
    }
}
