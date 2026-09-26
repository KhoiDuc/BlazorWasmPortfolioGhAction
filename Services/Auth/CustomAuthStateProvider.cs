using System.Net.Http.Json;
using System.Security.Claims;
using Blazored.LocalStorage;
using BlazorWasmPortfolioGhAction.Services.Trading.Broker;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace BlazorWasmPortfolioGhAction.Services.Auth;

/// <summary>
/// Wiki admin and the broker desk share the broker-api JWT. There is no local password.
/// </summary>
public sealed class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    public CustomAuthStateProvider(
        ILocalStorageService localStorage,
        HttpClient http,
        IConfiguration config,
        ILogger<CustomAuthStateProvider> logger)
    {
        _localStorage = localStorage;
        _http = http;
        _config = config;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var brokerToken = await _localStorage.GetItemAsync<string?>(BrokerAuthService.TokenKey);
        if (string.IsNullOrWhiteSpace(brokerToken))
            return Anonymous();

        var brokerUsername = await _localStorage.GetItemAsync<string?>(BrokerAuthService.UsernameKey);
        var name = string.IsNullOrWhiteSpace(brokerUsername) ? "broker" : brokerUsername;
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Role, "Broker"),
            new(ClaimTypes.Role, "Admin"),
        };
        var identity = new ClaimsIdentity(claims, authenticationType: "PortfolioAuth");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task<(bool Ok, string? Error)> LoginAsync(string username, string password)
    {
        var baseUrl = _config["BrokerApi:BaseUrl"]?.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            return (false, "BrokerApi:BaseUrl is not configured.");

        try
        {
            using var resp = await _http.PostAsJsonAsync($"{baseUrl}/api/auth/login", new { username = username.Trim(), password });
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (false, null);
            if (resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                return (false, "Too many login attempts.");
            if (!resp.IsSuccessStatusCode)
                return (false, $"HTTP {(int)resp.StatusCode}");

            var result = await resp.Content.ReadFromJsonAsync<BrokerLoginResponse>();
            if (string.IsNullOrWhiteSpace(result?.Token))
                return (false, "Invalid login response.");

            await _localStorage.SetItemAsync(BrokerAuthService.TokenKey, result.Token);
            await _localStorage.SetItemAsync(BrokerAuthService.UsernameKey, result.Username ?? username.Trim());
            if (!string.IsNullOrWhiteSpace(result.ExpiresAt))
                await _localStorage.SetItemAsync(BrokerAuthService.ExpiresAtKey, result.ExpiresAt);
            await _localStorage.RemoveItemAsync("portfolio-admin-session");
            NotifyAuthStateChanged();
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Portfolio login failed for {Username}", username.Trim());
            return (false, ex.Message);
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(BrokerAuthService.TokenKey);
        await _localStorage.RemoveItemAsync(BrokerAuthService.UsernameKey);
        await _localStorage.RemoveItemAsync(BrokerAuthService.ExpiresAtKey);
        await _localStorage.RemoveItemAsync("portfolio-admin-session");
        NotifyAuthStateChanged();
    }

    public void NotifyAuthStateChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private static AuthenticationState Anonymous() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private sealed class BrokerLoginResponse
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
        public string? ExpiresAt { get; set; }
    }
}
