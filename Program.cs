using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Fluxor;
using BlazorWasmPortfolioGhAction.Store.Actions;
using BlazorWasmPortfolioGhAction;
using BlazorWasmPortfolioGhAction.Store.Services;
using BlazorComponentBus;
using BlazorWasmPortfolioGhAction.Contexts;
using BlazorWasmPortfolioGhAction.Data;
using Microsoft.EntityFrameworkCore;
using BlazorWasmPortfolioGhAction.Pages;
using SQLitePCL;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System;
using BlazorWasmPortfolioGhAction.Shared.Model;
using ManuHub.Blazor.Wasm.BrowserStorage;

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
        builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddScoped<IMobileDetectionService, BlazorWebAssemblyMobileDetectionService>();
        builder.Services.AddMsalAuthentication(options =>
        {
            builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
            options.ProviderOptions.DefaultAccessTokenScopes.Add("api://a90ff01b-640d-478f-8f16-05fe599a6574/Files.Read");
            options.ProviderOptions.LoginMode = "redirect";
        });
        builder.Services.AddSingleton<ITimeZoneQueryProviderService, TimeZoneQueryProviderService>();

        builder.Services.AddFluxor(o => o.ScanAssemblies(typeof(Program).Assembly));
        builder.Services.AddScoped<TemperatureStore>();
        builder.Services.AddSingleton<ApiKeyModel>();

        builder.Services.AddDbContextFactory<ClientSideDbContext>(options =>
              options
                .UseSqlite($"Filename={Sqlite.SqliteDbFilename}")
                .EnableSensitiveDataLogging());

        builder.Services.AddWasmBrowserStorage();

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
