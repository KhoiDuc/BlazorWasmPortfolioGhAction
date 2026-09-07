using System.Text.Json;
using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;
using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public interface IVnDeskStore
{
    Task<WatchlistState> GetWatchlistAsync();
    Task SaveWatchlistAsync(WatchlistState state);
    Task<Dictionary<string, int>> GetSectorRankHistoryAsync();
    Task SaveSectorRankHistoryAsync(Dictionary<string, int> ranks);
}

public sealed class VnDeskStore : IVnDeskStore
{
    private const string WatchlistKey = "vndesk.watchlist";
    private const string SectorRankKey = "vndesk.sectorrank";
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly IJSRuntime _js;

    public VnDeskStore(IJSRuntime js) => _js = js;

    public async Task<WatchlistState> GetWatchlistAsync()
    {
        var json = await GetItemAsync(WatchlistKey);
        if (string.IsNullOrWhiteSpace(json)) return new WatchlistState();
        return JsonSerializer.Deserialize<WatchlistState>(json, JsonOpts) ?? new WatchlistState();
    }

    public Task SaveWatchlistAsync(WatchlistState state) =>
        SetItemAsync(WatchlistKey, JsonSerializer.Serialize(state, JsonOpts));

    public async Task<Dictionary<string, int>> GetSectorRankHistoryAsync()
    {
        var json = await GetItemAsync(SectorRankKey);
        if (string.IsNullOrWhiteSpace(json)) return [];
        return JsonSerializer.Deserialize<Dictionary<string, int>>(json, JsonOpts) ?? [];
    }

    public Task SaveSectorRankHistoryAsync(Dictionary<string, int> ranks) =>
        SetItemAsync(SectorRankKey, JsonSerializer.Serialize(ranks, JsonOpts));

    private async Task<string?> GetItemAsync(string key) =>
        await _js.InvokeAsync<string?>("tradingAuth.getItem", key);

    private async Task SetItemAsync(string key, string value) =>
        await _js.InvokeVoidAsync("tradingAuth.setItem", key, value);
}