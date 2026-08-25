namespace BlazorWasmPortfolioGhAction.Extensions;

public static class TradingServiceExtensions
{
    public const string TradingApiSection = "TradingApi";
    public const string TradingApiClientName = "TradingApi";
    public const string TradingOsintClientName = "TradingOsint";

    public static IServiceCollection AddTradingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new BlazorWasmPortfolioGhAction.Services.Trading.TradingApiOptions();
        configuration.GetSection(TradingApiSection).Bind(options);

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            options.BaseUrl = "https://trading-api-dark-sunset-2092.fly.dev";

        if (string.IsNullOrWhiteSpace(options.OsintBaseUrl))
            options.OsintBaseUrl = "http://152.53.208.182:8080";

        services.AddSingleton(options);
        services.AddSingleton(sp =>
            new BlazorWasmPortfolioGhAction.Services.Trading.TradingEndpointResolver(
                sp.GetRequiredService<BlazorWasmPortfolioGhAction.Services.Trading.TradingApiOptions>()));

        services.AddHttpClient(TradingApiClientName, client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(8);
        });

        services.AddHttpClient(TradingOsintClientName, client =>
        {
            client.BaseAddress = new Uri(options.OsintBaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(8);
        });

        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.ITradingAuthService,
            BlazorWasmPortfolioGhAction.Services.Trading.TradingAuthService>();

        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.ITradingApiClient,
            BlazorWasmPortfolioGhAction.Services.Trading.TradingApiClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var http = factory.CreateClient(TradingApiClientName);
            var osint = factory.CreateClient(TradingOsintClientName);
            var opts = sp.GetRequiredService<BlazorWasmPortfolioGhAction.Services.Trading.TradingApiOptions>();
            var endpoints = sp.GetRequiredService<BlazorWasmPortfolioGhAction.Services.Trading.TradingEndpointResolver>();
            var auth = sp.GetRequiredService<BlazorWasmPortfolioGhAction.Services.Trading.ITradingAuthService>();
            return new BlazorWasmPortfolioGhAction.Services.Trading.TradingApiClient(http, osint, endpoints, auth);
        });

        return services;
    }
}
