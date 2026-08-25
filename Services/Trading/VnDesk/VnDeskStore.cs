using System.Text.Json;
using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;
using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public interface IVnDeskStore
{
    Task<WatchlistState> GetWatchlistAsync();
    Task SaveWatchlistAsync(WatchlistState state);
    Task<PlanStore> GetPlansAsync();
    Task SavePlansAsync(PlanStore store);
    Task<JournalStore> GetJournalAsync();
    Task SaveJournalAsync(JournalStore store);
}

public sealed class VnDeskStore : IVnDeskStore
{
    private const string WatchlistKey = "vndesk.watchlist";
    private const string PlansKey = "vndesk.plans";
    private const string JournalKey = "vndesk.journal";
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

    public async Task<PlanStore> GetPlansAsync()
    {
        var json = await GetItemAsync(PlansKey);
        if (string.IsNullOrWhiteSpace(json)) return new PlanStore();
        return JsonSerializer.Deserialize<PlanStore>(json, JsonOpts) ?? new PlanStore();
    }

    public Task SavePlansAsync(PlanStore store) =>
        SetItemAsync(PlansKey, JsonSerializer.Serialize(store, JsonOpts));

    public async Task<JournalStore> GetJournalAsync()
    {
        var json = await GetItemAsync(JournalKey);
        if (string.IsNullOrWhiteSpace(json)) return new JournalStore();
        return JsonSerializer.Deserialize<JournalStore>(json, JsonOpts) ?? new JournalStore();
    }

    public Task SaveJournalAsync(JournalStore store) =>
        SetItemAsync(JournalKey, JsonSerializer.Serialize(store, JsonOpts));

    private async Task<string?> GetItemAsync(string key) =>
        await _js.InvokeAsync<string?>("tradingAuth.getItem", key);

    private async Task SetItemAsync(string key, string value) =>
        await _js.InvokeVoidAsync("tradingAuth.setItem", key, value);
}

public sealed class ChecklistService
{
    public PreTradeChecklist Last { get; private set; } = new();
    public void Set(PreTradeChecklist c) => Last = c;
}
