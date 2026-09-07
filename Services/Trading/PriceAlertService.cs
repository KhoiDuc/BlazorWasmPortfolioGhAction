using System.Net.Http.Json;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Models.Trading;
using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Services.Trading;

/// <summary>
/// Client-side price alert service: localStorage persistence + Discord webhook delivery.
/// ponytail: no real-time price feed — relies on caller passing current price via CheckAlertsAsync.
/// Upgrade: wire to live ticker (Binance WebSocket / VnMarket poll) for auto-trigger.
/// </summary>
public interface IPriceAlertService
{
    Task<List<PriceAlert>> LoadAsync();
    Task SaveAsync(List<PriceAlert> alerts);
    Task<PriceAlert> AddAsync(string symbol, string assetType, double threshold, AlertOperator op);
    Task RemoveAsync(Guid id);
    Task DismissAsync(Guid id);
    Task<int> CheckAlertsAsync(string symbol, double currentPrice);
    bool IsDiscordConfigured { get; }
}

public class PriceAlertService : IPriceAlertService
{
    private readonly IJSRuntime _js;
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private List<PriceAlert> _cache = new();
    private bool _loaded;

    private const string StorageKey = "priceAlerts";
    private static readonly JsonSerializerOptions JsonOpts = new()
    { PropertyNameCaseInsensitive = true, WriteIndented = false };

    public PriceAlertService(IJSRuntime js, HttpClient http, IConfiguration config)
    {
        _js = js;
        _http = http;
        _config = config;
    }

    public bool IsDiscordConfigured =>
        !string.IsNullOrWhiteSpace(_config["DevOps:DiscordWebhookUrl"]);

    public async Task<List<PriceAlert>> LoadAsync()
    {
        if (_loaded) return _cache;
        try
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
                _cache = JsonSerializer.Deserialize<List<PriceAlert>>(json, JsonOpts) ?? new();
        }
        catch { _cache = new(); }
        _loaded = true;
        return _cache;
    }

    public async Task SaveAsync(List<PriceAlert> alerts)
    {
        _cache = alerts;
        var json = JsonSerializer.Serialize(alerts, JsonOpts);
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    public async Task<PriceAlert> AddAsync(string symbol, string assetType, double threshold, AlertOperator op)
    {
        var alerts = await LoadAsync();
        // ponytail: dedup exact match only. Upgrade: allow multiple alerts same symbol different thresholds.
        if (alerts.Any(a => a.Symbol == symbol && a.AssetType == assetType
            && a.Threshold == threshold && a.Operator == op && a.Status == AlertStatus.Active))
            return alerts.First(a => a.Symbol == symbol && a.AssetType == assetType
                && a.Threshold == threshold && a.Operator == op && a.Status == AlertStatus.Active);

        var alert = new PriceAlert
        {
            Symbol = symbol,
            AssetType = assetType,
            Threshold = threshold,
            Operator = op,
            Status = AlertStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        alerts.Add(alert);
        await SaveAsync(alerts);
        return alert;
    }

    public async Task RemoveAsync(Guid id)
    {
        var alerts = await LoadAsync();
        alerts.RemoveAll(a => a.Id == id);
        await SaveAsync(alerts);
    }

    public async Task DismissAsync(Guid id)
    {
        var alerts = await LoadAsync();
        var a = alerts.FirstOrDefault(x => x.Id == id);
        if (a != null) { a.Status = AlertStatus.Dismissed; await SaveAsync(alerts); }
    }

    /// <summary>
    /// Check active alerts for a symbol against current price. Fire Discord for each newly triggered.
    /// Returns count of newly triggered alerts.
    /// </summary>
    public async Task<int> CheckAlertsAsync(string symbol, double currentPrice)
    {
        var alerts = await LoadAsync();
        var active = alerts.Where(a => a.Symbol == symbol && a.Status == AlertStatus.Active).ToList();
        if (active.Count == 0) return 0;

        var triggered = new List<PriceAlert>();
        foreach (var a in active)
        {
            var hit = a.Operator == AlertOperator.Above
                ? currentPrice >= a.Threshold
                : currentPrice <= a.Threshold;
            if (!hit) continue;
            a.Status = AlertStatus.Triggered;
            a.TriggeredPrice = currentPrice;
            a.TriggeredAt = DateTime.UtcNow;
            triggered.Add(a);
        }

        if (triggered.Count > 0)
        {
            await SaveAsync(alerts);
            foreach (var a in triggered)
                _ = SendDiscordAsync(a);
        }
        return triggered.Count;
    }

    private async Task SendDiscordAsync(PriceAlert alert)
    {
        var webhookUrl = _config["DevOps:DiscordWebhookUrl"];
        if (string.IsNullOrWhiteSpace(webhookUrl)) return;

        var dir = alert.Operator == AlertOperator.Above ? "↑ crossed above" : "↓ crossed below";
        var title = $"Price Alert: {alert.Symbol}";
        var desc = $"{alert.Symbol} {dir} **{alert.Threshold:N2}**\n" +
                   $"Current: **{alert.TriggeredPrice:N2}** ({alert.AssetType})\n" +
                   $"Time: {alert.TriggeredAt:yyyy-MM-dd HH:mm} UTC";

        var payload = new
        {
            embeds = new[]
            {
                new
                {
                    title = title,
                    description = desc,
                    color = alert.Operator == AlertOperator.Above ? 0x00FF00 : 0xFF0000,
                    footer = new { text = "Blazor Portfolio Price Alert" },
                    timestamp = alert.TriggeredAt?.ToString("O")
                }
            }
        };

        try { await _http.PostAsJsonAsync(webhookUrl, payload); }
        catch { /* swallow — alert already recorded locally */ }
    }
}