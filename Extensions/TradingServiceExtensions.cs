using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Extensions;

public static class TradingServiceExtensions
{
    public const string TradingApiSection = "TradingApi";
    public const string TradingApiClientName = "TradingApi";
    public const string VnMarketClientName = "VnMarket";
    public const string VnCafeFClientName = "VnCafeF";

    public static IServiceCollection AddTradingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new BlazorWasmPortfolioGhAction.Services.Trading.TradingApiOptions();
        configuration.GetSection(TradingApiSection).Bind(options);
        services.AddSingleton(options);

        services.AddSingleton(sp =>
            new BlazorWasmPortfolioGhAction.Services.Trading.TradingEndpointResolver(
                sp.GetRequiredService<BlazorWasmPortfolioGhAction.Services.Trading.TradingApiOptions>()));

        // Proxy HttpClient — no BaseAddress (absolute URLs only for external services).
        services.AddHttpClient(TradingApiClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
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
        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.VnDesk.BacktestService>();

        var brokerOptions = new BlazorWasmPortfolioGhAction.Services.Trading.Broker.BrokerOptions();
        configuration.GetSection("Gemini").Bind(brokerOptions);
        services.AddSingleton(brokerOptions);
        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.Broker.IBrokerDeskStore,
            BlazorWasmPortfolioGhAction.Services.Trading.Broker.BrokerDeskStore>();
        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.Broker.IBrokerGeminiClient,
            BlazorWasmPortfolioGhAction.Services.Trading.Broker.BrokerGeminiClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var http = factory.CreateClient(nameof(BlazorWasmPortfolioGhAction.Services.Trading.Broker.BrokerGeminiClient));
            var opts = sp.GetRequiredService<BlazorWasmPortfolioGhAction.Services.Trading.Broker.BrokerOptions>();
            return new BlazorWasmPortfolioGhAction.Services.Trading.Broker.BrokerGeminiClient(http, opts);
        });
        services.AddHttpClient(nameof(BlazorWasmPortfolioGhAction.Services.Trading.Broker.BrokerGeminiClient), client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.ITradingApiClient,
            BlazorWasmPortfolioGhAction.Services.Trading.TradingApiClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var http = factory.CreateClient(TradingApiClientName);
            var endpoints = sp.GetRequiredService<BlazorWasmPortfolioGhAction.Services.Trading.TradingEndpointResolver>();
            return new BlazorWasmPortfolioGhAction.Services.Trading.TradingApiClient(http, endpoints);
        });

        services.AddScoped<BlazorWasmPortfolioGhAction.Services.Trading.IPriceAlertService,
            BlazorWasmPortfolioGhAction.Services.Trading.PriceAlertService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var http = factory.CreateClient(nameof(BlazorWasmPortfolioGhAction.Services.Trading.PriceAlertService));
            return new BlazorWasmPortfolioGhAction.Services.Trading.PriceAlertService(
                sp.GetRequiredService<IJSRuntime>(),
                http,
                sp.GetRequiredService<IConfiguration>());
        });
        services.AddHttpClient(nameof(BlazorWasmPortfolioGhAction.Services.Trading.PriceAlertService));

        return services;
    }
}