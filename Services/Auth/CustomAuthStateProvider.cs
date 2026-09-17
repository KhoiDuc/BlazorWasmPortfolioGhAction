using System.Security.Claims;
using Blazored.LocalStorage;
using BlazorWasmPortfolioGhAction.Services.Trading.Broker;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorWasmPortfolioGhAction.Services.Auth;

/// <summary>
/// Local auth for wiki admin and Broker desk JWT sessions.
/// </summary>
public sealed class CustomAuthStateProvider : AuthenticationStateProvider
{
    private const string AdminStorageKey = "portfolio-admin-session";
    private readonly ILocalStorageService _localStorage;

    public CustomAuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var claims = new List<Claim>();

        var isAdmin = await _localStorage.GetItemAsync<bool?>(AdminStorageKey) == true;
        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Name, "admin"));
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var brokerToken = await _localStorage.GetItemAsync<string?>(BrokerAuthService.TokenKey);
        if (!string.IsNullOrWhiteSpace(brokerToken))
        {
            var brokerUsername = await _localStorage.GetItemAsync<string?>(BrokerAuthService.UsernameKey);
            claims.Add(new Claim(ClaimTypes.Name, string.IsNullOrWhiteSpace(brokerUsername) ? "broker" : brokerUsername));
            claims.Add(new Claim(ClaimTypes.Role, "Broker"));
        }

        if (claims.Count == 0)
            return Anonymous();

        var identity = new ClaimsIdentity(claims, authenticationType: "PortfolioAuth");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        if (!string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase)
            || password != "admin")
        {
            return false;
        }

        await _localStorage.SetItemAsync(AdminStorageKey, true);
        NotifyAuthStateChanged();
        return true;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(AdminStorageKey);
        NotifyAuthStateChanged();
    }

    public void NotifyAuthStateChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private static AuthenticationState Anonymous() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));
}
