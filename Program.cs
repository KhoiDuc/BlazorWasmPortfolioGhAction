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
        builder.Services.AddScoped<IMobileDetectionService, BlazorWebAssemblyMobileDetectionService>();
        //builder.Services.AddMsalAuthentication(options =>
        //{
        //    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
        //    options.ProviderOptions.DefaultAccessTokenScopes.Add("api://a90ff01b-640d-478f-8f16-05fe599a6574/Files.Read");
        //    options.ProviderOptions.LoginMode = "redirect";
        //});
        builder.Services.AddOidcAuthentication(options =>
        {
            options.ProviderOptions.Authority = "dev-a8pumil7eppq2o84.us.auth0.com"; // Replace with your Auth0 domain
            options.ProviderOptions.ClientId = "LuUc4a2iPHABVoGO9WyAkMZ7CJOQOpgf"; // Replace with your Auth0 Client ID
            options.ProviderOptions.ResponseType = "code"; // Use Authorization Code Flow
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
        builder.Services.AddBlazorGoogleMaps("YOUR_GOOGLE_MAPS_API_KEY");
        builder.Services.AddLocalization();

        // build the host
        var host = builder.Build();

        // resolve the dispatcher
        //var dispatcher = host.Services.GetRequiredService<IDispatcher>();

        // dispatch the LoadContentsFromRepoAction
        //dispatcher.Dispatch(new LoadContentsFromRepoAction());

        // Run the app
        await host.RunAsync();
    }
}
