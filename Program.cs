using Microsoft.AspNetCore.Components;
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
using System.Globalization;
using BlazorWasmPortfolioGhAction.Shared.Model;
using ManuHub.Blazor.Wasm.BrowserStorage;
using GoogleMapsComponents;
using Fluxor.Blazor.Web.ReduxDevTools;
using BlazorWasmPortfolioGhAction.Extensions;
using BlazorWasmPortfolioGhAction.Services;
using BlazorWasmPortfolioGhAction.Services.Auth;
using BlazorWasmPortfolioGhAction.Services.Jwt;
using BlazorWasmPortfolioGhAction.Services.Localization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Blazored.LocalStorage;
// using Microsoft.Authentication.WebAssembly.Msal; // MSAL — disabled (learning/demo only)

public static partial class Program
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
        builder.Services.AddScoped<JwtCodec>();
        builder.Services.AddScoped<IMobileDetectionService, BlazorWebAssemblyMobileDetectionService>();

        // MSAL / Azure AD — disabled (was for learning only)
        // builder.Services.AddMsalAuthentication(options =>
        // {
        //     builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
        //     options.ProviderOptions.LoginMode = "redirect";
        //     var baseAddress = builder.HostEnvironment.BaseAddress.TrimEnd('/') + "/";
        //     options.ProviderOptions.Authentication.RedirectUri ??= $"{baseAddress}authentication/login-callback";
        //     options.ProviderOptions.Authentication.PostLogoutRedirectUri ??= baseAddress;
        // });

        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<CustomAuthStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
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
        builder.Services.AddBlazoredLocalStorage();
        var googleMapsKey = builder.Configuration["GoogleMaps:ApiKey"] ?? "YOUR_GOOGLE_MAPS_API_KEY";
        builder.Services.AddBlazorGoogleMaps(googleMapsKey);
        builder.Services.AddLocalization();
        builder.Services.AddScoped<ICultureService, CultureService>();
        builder.Services.AddTradingServices(builder.Configuration);

        // build the host
        var host = builder.Build();

        // Culture must be set before first render. Do NOT resolve scoped services
        // (ICultureService / IJSRuntime) from the root provider — that crashes WASM boot.
        await SetStartupCultureAsync(host.Services);

        // Run the app
        await host.RunAsync();
    }

    private static async Task SetStartupCultureAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var js = scope.ServiceProvider.GetRequiredService<IJSRuntime>();
        var navigation = scope.ServiceProvider.GetRequiredService<NavigationManager>();

        // Prefer /en|/vn from the URL so deep links load the matching satellite assembly.
        CulturePath.Split(navigation.ToBaseRelativePath(navigation.Uri), out var urlLang, out _);

        string? saved = null;
        if (urlLang is not null)
        {
            saved = CultureService.Normalize(urlLang);
        }
        else
        {
            try
            {
                saved = await js.InvokeAsync<string?>("cultureManager.get");
            }
            catch
            {
                // JS bridge may not be ready; keep Vietnamese default
            }
        }

        var cultureName = CultureService.Normalize(saved);
        CultureInfo culture;
        try
        {
            culture = CultureInfo.GetCultureInfo(cultureName);
        }
        catch (CultureNotFoundException)
        {
            culture = CultureInfo.GetCultureInfo(CultureService.DefaultCultureName);
        }

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        try
        {
            await js.InvokeVoidAsync("cultureManager.set", cultureName);
            await js.InvokeVoidAsync("cultureManager.setDocumentLang", culture.TwoLetterISOLanguageName);
        }
        catch
        {
            // ignore
        }
    }
}
