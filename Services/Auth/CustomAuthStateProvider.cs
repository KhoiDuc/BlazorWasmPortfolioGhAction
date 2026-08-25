using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorWasmPortfolioGhAction.Services.Auth;

/// <summary>
/// Simple local admin auth for wiki editing (learning/demo only — not production-grade).
/// </summary>
public sealed class CustomAuthStateProvider : AuthenticationStateProvider
{
    private const string StorageKey = "portfolio-admin-session";
    private readonly ILocalStorageService _localStorage;

    public CustomAuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var isAuthenticated = await _localStorage.GetItemAsync<bool?>(StorageKey) == true;
        if (!isAuthenticated)
            return Anonymous();

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "Admin")
        }, authenticationType: "PortfolioAdmin");

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        if (!string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase)
            || password != "admin")
        {
            return false;
        }

        await _localStorage.SetItemAsync(StorageKey, true);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return true;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(StorageKey);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static AuthenticationState Anonymous() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));
}
