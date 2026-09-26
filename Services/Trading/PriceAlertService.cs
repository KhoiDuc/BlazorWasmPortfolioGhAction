using System.Text.Json;
using BlazorWasmPortfolioGhAction.Models.Trading;
using BlazorWasmPortfolioGhAction.Services.Trading.Broker;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Services.Trading;

/// <summary>
/// Price alerts stored on broker-api. Stock alerts are checked by the server cron.
/// Gold, silver, and oil are checked in the browser, then fired through the API.
/// </summary>
public interface IPriceAlertService
{
    Task<List<PriceAlert>> LoadAsync();
    Task<PriceAlert> AddAsync(string symbol, string assetType, double threshold, AlertOperator op);
    Task RemoveAsync(string id);
    Task<int> CheckAlertsAsync(string symbol, double currentPrice);
    void RememberPrice(string symbol, double price);
    bool TryGetPrice(string symbol, out double price);
    bool IsDiscordConfigured { get; }
}

public class PriceAlertService : IPriceAlertService
{
    private readonly IJSRuntime _js;
    private readonly IBrokerApiClient _api;
    private readonly ILogger<PriceAlertService> _logger;
    private List<PriceAlert> _cache = [];
    private bool _loaded;
    private readonly Dictionary<string, double> _prices = new(StringComparer.OrdinalIgnoreCase);

    private const string StorageKey = "priceAlerts";
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public PriceAlertService(IJSRuntime js, IBrokerApiClient api, ILogger<PriceAlertService> logger)
    {
        _js = js;
        _api = api;
        _logger = logger;
    }

    public bool IsDiscordConfigured => true;

    public void RememberPrice(string symbol, double price)
    {
        if (!string.IsNullOrWhiteSpace(symbol) && price > 0)
            _prices[symbol.Trim().ToUpperInvariant()] = price;
    }

    public bool TryGetPrice(string symbol, out double price) =>
        _prices.TryGetValue(symbol.Trim().ToUpperInvariant(), out price);

    public async Task<List<PriceAlert>> LoadAsync()
    {
        if (_loaded) return _cache;
        await MigrateLocalAsync();
        var call = await _api.GetTextAsync("/api/alerts");
        if (!call.Ok || string.IsNullOrWhiteSpace(call.Body))
        {
            _cache = [];
            return _cache;
        }
        var rows = JsonSerializer.Deserialize<List<ServerAlert>>(call.Body, JsonOpts) ?? [];
        _cache = rows.Select(Map).ToList();
        _loaded = true;
        return _cache;
    }

    public async Task<PriceAlert> AddAsync(string symbol, string assetType, double threshold, AlertOperator op)
    {
        var body = JsonSerializer.Serialize(new
        {
            symbol,
            assetType,
            direction = op == AlertOperator.Above ? "above" : "below",
            price = threshold,
            channel = "discord",
        }, JsonOpts);
        var call = await _api.SendTextAsync(HttpMethod.Post, "/api/alerts", body, "application/json");
        if (!call.Ok) throw new InvalidOperationException(call.Error ?? "Không lưu được cảnh báo.");
        _loaded = false;
        var alerts = await LoadAsync();
        return alerts.FirstOrDefault(a => a.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase)
            && Math.Abs(a.Threshold - threshold) < 0.0001
            && a.Status == AlertStatus.Active) ?? Map(JsonSerializer.Deserialize<ServerAlert>(call.Body!, JsonOpts)!);
    }

    public async Task RemoveAsync(string id)
    {
        await _api.SendTextAsync(HttpMethod.Delete, $"/api/alerts/{id}", null, null);
        _cache.RemoveAll(a => a.Id == id);
    }

    public async Task<int> CheckAlertsAsync(string symbol, double currentPrice)
    {
        RememberPrice(symbol, currentPrice);
        var alerts = await LoadAsync();
        var active = alerts.Where(a => a.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase)
            && a.Status == AlertStatus.Active
            && !a.AssetType.Equals("stock", StringComparison.OrdinalIgnoreCase)).ToList();
        var fired = 0;
        foreach (var alert in active)
        {
            var hit = alert.Operator == AlertOperator.Above
                ? currentPrice >= alert.Threshold
                : currentPrice <= alert.Threshold;
            if (!hit) continue;
            var body = JsonSerializer.Serialize(new { price = currentPrice }, JsonOpts);
            var call = await _api.SendTextAsync(HttpMethod.Post, $"/api/alerts/{alert.Id}/fire", body, "application/json");
            if (!call.Ok) continue;
            alert.Status = AlertStatus.Triggered;
            alert.TriggeredPrice = currentPrice;
            alert.TriggeredAt = DateTime.UtcNow;
            fired++;
        }
        return fired;
    }

    private async Task MigrateLocalAsync()
    {
        string? json;
        try { json = await _js.InvokeAsync<string>("localStorage.getItem", StorageKey); }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Local price-alert storage was not readable");
            return;
        }
        if (string.IsNullOrWhiteSpace(json)) return;
        var local = JsonSerializer.Deserialize<List<PriceAlert>>(json, JsonOpts) ?? [];
        foreach (var alert in local.Where(a => a.Status == AlertStatus.Active && a.Threshold > 0))
        {
            var body = JsonSerializer.Serialize(new
            {
                symbol = alert.Symbol,
                assetType = string.IsNullOrWhiteSpace(alert.AssetType) ? "stock" : alert.AssetType,
                direction = alert.Operator == AlertOperator.Above ? "above" : "below",
                price = alert.Threshold,
                channel = "discord",
            }, JsonOpts);
            await _api.SendTextAsync(HttpMethod.Post, "/api/alerts", body, "application/json");
        }
        try { await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Migrated price alerts but could not clear local storage");
        }
    }

    private static PriceAlert Map(ServerAlert row) => new()
    {
        Id = row.Id,
        Symbol = row.Symbol,
        AssetType = row.AssetType ?? "stock",
        Threshold = row.Price,
        Operator = row.Direction == "below" ? AlertOperator.Below : AlertOperator.Above,
        Status = row.Active ? AlertStatus.Active : AlertStatus.Triggered,
        TriggeredPrice = row.TriggeredPrice,
        TriggeredAt = row.TriggeredAt,
    };

    private sealed class ServerAlert
    {
        public string Id { get; set; } = "";
        public string Symbol { get; set; } = "";
        public string Direction { get; set; } = "above";
        public double Price { get; set; }
        public string? AssetType { get; set; }
        public bool Active { get; set; }
        public double? TriggeredPrice { get; set; }
        public DateTime? TriggeredAt { get; set; }
    }
}
