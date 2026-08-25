namespace BlazorWasmPortfolioGhAction.Extensions;

public static class TradingServiceExtensions
{
    public const string TradingApiSection = "TradingApi";
    public const string TradingApiClientName = "TradingApi";
    public const string TradingOsintClientName = "TradingOsint";
    public const string VnMarketClientName = "VnMarket";
    public const string VnCafeFClientName = "VnCafeF";

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

        services.AddHttpClient(VnMarketClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.vndirect.com.vn/");
        });

        services.AddHttpClient(VnCafeFClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        });

        var vnOptions = new BlazorWasmPortfolioGhAction.Services.Trading.VnDesk.VnDeskOptions();
        configuration.GetSection("VnDesk").Bind(vnOptions);
        services.AddSingleton(vnOptions);

        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.VnDesk.VnDeskDataService>();

        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.VnDesk.IVnMarketClient,
            BlazorWasmPortfolioGhAction.Services.Trading.VnDesk.VnMarketClient>();
        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.VnDesk.IVnDeskStore,
            BlazorWasmPortfolioGhAction.Services.Trading.VnDesk.VnDeskStore>();
        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.VnDesk.IndicatorService>();
        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.VnDesk.VnScreenerService>();
        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.VnDesk.VnSectorService>();
        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.VnDesk.WatchlistScorer>();
        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.VnDesk.PositionService>();
        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.VnDesk.ChecklistService>();

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
