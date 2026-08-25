using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Fluxor;
using BlazorWasmPortfolioGhAction;
using BlazorWasmPortfolioGhAction.Store.Services;
using BlazorComponentBus;
using BlazorWasmPortfolioGhAction.Contexts;
using BlazorWasmPortfolioGhAction.Data;
using Microsoft.EntityFrameworkCore;
using BlazorWasmPortfolioGhAction.Pages;
using System.Diagnostics.CodeAnalysis;
using BlazorWasmPortfolioGhAction.Shared.Model;
using ManuHub.Blazor.Wasm.BrowserStorage;
using GoogleMapsComponents;
using Fluxor.Blazor.Web.ReduxDevTools;
using BlazorWasmPortfolioGhAction.Extensions;
using BlazorWasmPortfolioGhAction.Services;

public static class Program
{
    /// <summary>
    /// FIXME: This is required for EF Core 6.0 as it is not compatible with trimming.
    ///
    /// For more information:
    ///   [.NET 6] Migrate API - Could not find method 'AddYears' on type 'System.DateOnly'
    ///   https://github.com/dotnet/efcore/issues/26860
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private static Type _keepDateOnly = typeof(DateOnly);

    public static async Task Main(string[] args)
    {
#if DEBUG
        // Allow some time for debugger to attach to Blazor framework debugging proxy
        await Task.Delay(TimeSpan.FromSeconds(2));
#endif

        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        builder.Services.AddScoped<ComponentBus>();
        builder.Services.AddScoped<RandomFactsService>();
        builder.Services.AddScoped<QRCodeService>();
        builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
        builder.Services.AddGraphQLClient();

        builder.Services.AddSingleton<StateContainer>();
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddScoped<IDevOpsService, DevOpsService>();
        builder.Services.AddScoped<IWikiContentService, WikiContentService>();
        builder.Services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
        builder.Services.AddScoped<IScriptLoaderService, ScriptLoaderService>();
        builder.Services.AddScoped<IClipboardService, ClipboardService>();
        builder.Services.AddScoped<IMobileDetectionService, BlazorWebAssemblyMobileDetectionService>();
        builder.Services.AddOidcAuthentication(options =>
        {
            builder.Configuration.Bind("Auth0", options.ProviderOptions);
        });
        builder.Services.AddSingleton<ITimeZoneQueryProviderService, TimeZoneQueryProviderService>();

        builder.Services.AddFluxor(opt => {
            opt.ScanAssemblies(typeof(Program).Assembly);
            opt.UseRouting();
            #if DEBUG
               opt.UseReduxDevTools();
            #endif
        });
        builder.Services.AddScoped<TemperatureStore>();
        builder.Services.AddSingleton<ApiKeyModel>();
        builder.Services.AddScoped<ISearchUsersService, SearchUsersService>();
        builder.Services.AddGitHubGraphQLQueryService();
        builder.Services.AddDbContextFactory<ClientSideDbContext>(options =>
              options
                .UseSqlite($"Filename={Sqlite.SqliteDbFilename}")
                .EnableSensitiveDataLogging());

        builder.Services.AddWasmBrowserStorage();
        var googleMapsKey = builder.Configuration["GoogleMaps:ApiKey"] ?? "YOUR_GOOGLE_MAPS_API_KEY";
        builder.Services.AddBlazorGoogleMaps(googleMapsKey);
        builder.Services.AddLocalization();

        // build the host
        var host = builder.Build();

        // Run the app
        await host.RunAsync();
    }
}
