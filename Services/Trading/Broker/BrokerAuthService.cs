using Blazored.LocalStorage;
using BlazorWasmPortfolioGhAction.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public interface IBrokerAuthService
{
    Task<(bool Ok, string? Error)> LoginAsync(string username, string password, CancellationToken ct = default);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<string?> GetUsernameAsync();
}

public sealed class BrokerAuthService : IBrokerAuthService
{
    internal const string TokenKey = "broker.auth.token";
    internal const string UsernameKey = "broker.auth.username";
    internal const string ExpiresAtKey = "broker.auth.expiresAt";

    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public BrokerAuthService(
        HttpClient http,
        IConfiguration config,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _config = config;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    private string? BaseUrl => _config["BrokerApi:BaseUrl"]?.Trim().TrimEnd('/');

    public async Task<(bool Ok, string? Error)> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            return (false, "BrokerApi:BaseUrl is not configured.");

        try
        {
            using var resp = await _http.PostAsJsonAsync(
                $"{BaseUrl}/api/auth/login",
                new { username = username.Trim(), password },
                cancellationToken: ct);

            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return (false, null);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                return (false, $"HTTP {(int)resp.StatusCode}: {body}");
            }

            var result = await resp.Content.ReadFromJsonAsync<BrokerLoginResponse>(cancellationToken: ct);
            if (string.IsNullOrWhiteSpace(result?.Token))
                return (false, "Invalid login response.");

            await _localStorage.SetItemAsync(TokenKey, result.Token);
            await _localStorage.SetItemAsync(UsernameKey, result.Username ?? username.Trim());
            if (!string.IsNullOrWhiteSpace(result.ExpiresAt))
                await _localStorage.SetItemAsync(ExpiresAtKey, result.ExpiresAt);

            if (_authStateProvider is CustomAuthStateProvider custom)
                custom.NotifyAuthStateChanged();

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        await _localStorage.RemoveItemAsync(UsernameKey);
        await _localStorage.RemoveItemAsync(ExpiresAtKey);

        if (_authStateProvider is CustomAuthStateProvider custom)
            custom.NotifyAuthStateChanged();
    }

    public async Task<string?> GetTokenAsync() =>
        await _localStorage.GetItemAsync<string?>(TokenKey);

    public async Task<string?> GetUsernameAsync() =>
        await _localStorage.GetItemAsync<string?>(UsernameKey);

    private sealed class BrokerLoginResponse
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
        public string? ExpiresAt { get; set; }
    }
}
