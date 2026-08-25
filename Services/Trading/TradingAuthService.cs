using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Models.Trading;
using BlazorWasmPortfolioGhAction.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Services.Trading;

public interface ITradingAuthService
{
    event Action? AuthStateChanged;
    Task<bool> IsLoggedInAsync();
    Task<DnseUserInfo?> GetUserInfoAsync();
    Task<string?> GetTokenAsync();
    Task<string?> GetUserIdAsync();
    Task<DnseLoginResponse?> LoginAsync(string email, string password);
    Task DemoLoginAsync();
    Task LogoutAsync();
    Task<DnseUserInfo?> RefreshUserInfoAsync();
    Task<HttpRequestMessage> AuthorizeAsync(HttpRequestMessage request);
}

public class TradingAuthService : ITradingAuthService
{
    private const string TokenKey = "ts_token";
    private const string RefreshKey = "ts_refreshToken";
    private const string UserKey = "ts_userInfo";

    private readonly IJSRuntime _js;
    private readonly HttpClient _http;

    public event Action? AuthStateChanged;

    public TradingAuthService(IJSRuntime js, IHttpClientFactory factory)
    {
        _js = js;
        _http = factory.CreateClient(TradingServiceExtensions.TradingApiClientName);
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var user = await GetUserInfoAsync();
        return user?.CustodyCode is not null and not "";
    }

    public async Task<DnseUserInfo?> GetUserInfoAsync()
    {
        var json = await _js.InvokeAsync<string?>("tradingAuth.getItem", UserKey);
        if (string.IsNullOrEmpty(json)) return null;
        try { return JsonSerializer.Deserialize<DnseUserInfo>(json); }
        catch { return null; }
    }

    public async Task<string?> GetTokenAsync() =>
        await _js.InvokeAsync<string?>("tradingAuth.getItem", TokenKey);

    public async Task<string?> GetUserIdAsync()
    {
        var user = await GetUserInfoAsync();
        return user?.CustodyCode ?? user?.Id.ToString();
    }

    public async Task<DnseLoginResponse?> LoginAsync(string email, string password)
    {
        var resp = await _http.PostAsJsonAsync("dnse-auth-service/login", new { username = email, password });
        if (!resp.IsSuccessStatusCode) return null;

        var data = await resp.Content.ReadFromJsonAsync<DnseLoginResponse>();
        if (data?.Token is null) return null;

        await _js.InvokeVoidAsync("tradingAuth.setItem", TokenKey, data.Token);
        if (data.RefreshToken != null)
            await _js.InvokeVoidAsync("tradingAuth.setItem", RefreshKey, data.RefreshToken);

        await RefreshUserInfoAsync();
        AuthStateChanged?.Invoke();
        return data;
    }

    public async Task DemoLoginAsync()
    {
        await _js.InvokeVoidAsync("tradingAuth.setItem", TokenKey, "demo-token");
        var demo = new DnseUserInfo { Name = "Demo User", CustodyCode = "DEMO001", Id = 0 };
        await _js.InvokeVoidAsync("tradingAuth.setItem", UserKey, JsonSerializer.Serialize(demo));
        AuthStateChanged?.Invoke();
    }

    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("tradingAuth.removeItem", TokenKey);
        await _js.InvokeVoidAsync("tradingAuth.removeItem", RefreshKey);
        await _js.InvokeVoidAsync("tradingAuth.removeItem", UserKey);
        AuthStateChanged?.Invoke();
    }

    public async Task<DnseUserInfo?> RefreshUserInfoAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "dnse-user-service/api/me");
            await AuthorizeAsync(req);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return null;
            var user = await resp.Content.ReadFromJsonAsync<DnseUserInfo>();
            if (user != null)
                await _js.InvokeVoidAsync("tradingAuth.setItem", UserKey, JsonSerializer.Serialize(user));
            return user;
        }
        catch
        {
            return await GetUserInfoAsync();
        }
    }

    public async Task<HttpRequestMessage> AuthorizeAsync(HttpRequestMessage request)
    {
        var token = await GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
