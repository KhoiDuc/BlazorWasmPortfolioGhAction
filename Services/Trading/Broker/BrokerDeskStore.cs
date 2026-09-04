using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorWasmPortfolioGhAction.Models.Trading.Broker;
using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public interface IBrokerDeskStore
{
    Task<BrokerPortfolio> LoadAsync(CancellationToken ct = default);
    Task<BrokerPortfolio> LoadFromFileAsync(CancellationToken ct = default);
    Task SaveDraftAsync(BrokerPortfolio portfolio, CancellationToken ct = default);
    Task ClearDraftAsync(CancellationToken ct = default);
    Task DownloadJsonAsync(BrokerPortfolio portfolio);
    Task DownloadCsvAsync(BrokerPortfolio portfolio);
    BrokerPortfolio ParseJson(string json);
}

public sealed class BrokerDeskStore : IBrokerDeskStore
{
    public const string PortfolioPath = "trading/broker/portfolio.json";
    private const string DraftKey = "broker.desk.draft";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    public BrokerDeskStore(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<BrokerPortfolio> LoadAsync(CancellationToken ct = default)
    {
        try
        {
            var draft = await _js.InvokeAsync<string?>("tradingAuth.getItem", DraftKey);
            if (!string.IsNullOrWhiteSpace(draft))
            {
                var fromDraft = NormalizePortfolio(ParseJson(draft));
                if (fromDraft.Positions.Count > 0)
                    return fromDraft;
            }
        }
        catch
        {
            // Ignore localStorage errors and fall back to static JSON.
        }

        try
        {
            var url = $"{PortfolioPath}?v={DateTime.UtcNow.Ticks}";
            using var resp = await _http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode)
                return new BrokerPortfolio();

            var json = await resp.Content.ReadAsStringAsync(ct);
            return NormalizePortfolio(ParseJson(json));
        }
        catch
        {
            return new BrokerPortfolio();
        }
    }

    public async Task<BrokerPortfolio> LoadFromFileAsync(CancellationToken ct = default)
    {
        try
        {
            var url = $"{PortfolioPath}?v={DateTime.UtcNow.Ticks}";
            using var resp = await _http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode)
                return new BrokerPortfolio();

            var json = await resp.Content.ReadAsStringAsync(ct);
            try
            {
                return NormalizePortfolio(ParseJson(json));
            }
            catch (InvalidOperationException)
            {
                return new BrokerPortfolio();
            }
        }
        catch
        {
            return new BrokerPortfolio();
        }
    }

    public async Task SaveDraftAsync(BrokerPortfolio portfolio, CancellationToken ct = default)
    {
        portfolio.UpdatedAt = DateTime.Now;
        var json = JsonSerializer.Serialize(NormalizePortfolio(portfolio), JsonOpts);
        await _js.InvokeVoidAsync("tradingAuth.setItem", DraftKey, json);
    }

    public async Task ClearDraftAsync(CancellationToken ct = default)
    {
        try
        {
            await _js.InvokeVoidAsync("tradingAuth.removeItem", DraftKey);
        }
        catch
        {
            // Ignore localStorage failures.
        }
    }

    public BrokerPortfolio ParseJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new BrokerPortfolio();

        try
        {
            return NormalizePortfolio(JsonSerializer.Deserialize<BrokerPortfolio>(json, JsonOpts) ?? new BrokerPortfolio());
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"JSON không hợp lệ: {ex.Message}", ex);
        }
    }

    private static BrokerPortfolio NormalizePortfolio(BrokerPortfolio portfolio)
    {
        portfolio.Positions ??= [];
        portfolio.ClosedPositions ??= [];
        portfolio.Positions = portfolio.Positions
            .Where(p => !string.IsNullOrWhiteSpace(p.Symbol))
            .Select(NormalizePosition)
            .ToList();
        portfolio.ClosedPositions = portfolio.ClosedPositions
            .Where(p => !string.IsNullOrWhiteSpace(p.Symbol))
            .Select(NormalizePosition)
            .ToList();
        return portfolio;
    }

    private static BrokerPosition NormalizePosition(BrokerPosition position)
    {
        position.Symbol = position.Symbol.Trim().ToUpperInvariant();
        position.Buys ??= [];
        position.Sells ??= [];
        position.Notes ??= [];
        position.Buys = position.Buys.Where(b => b.Price > 0).ToList();
        position.Sells = position.Sells.Where(s => s.Price > 0).ToList();
        return position;
    }

    public Task DownloadJsonAsync(BrokerPortfolio portfolio)
    {
        portfolio.UpdatedAt = DateTime.Now;
        var json = JsonSerializer.Serialize(portfolio, JsonOpts);
        return DownloadAsync("portfolio.json", json);
    }

    public Task DownloadCsvAsync(BrokerPortfolio portfolio)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Nganh,Ma CP,Gia mua,KL tong,KL con lai,Gia TB,Cat lo,Muc tieu,Ti trong,Trang thai,Realized P&L (đ),Note moi nhat");
        foreach (var p in portfolio.Positions.OrderBy(x => x.Symbol, StringComparer.OrdinalIgnoreCase))
        {
            var lots = string.Join(" | ", p.Buys.OrderBy(b => b.BoughtAt).Select((b, i) =>
            {
                var qty = b.Quantity is > 0 ? $" x{b.Quantity.Value.ToString("0.##", CultureInfo.InvariantCulture)}" : "";
                return $"L{i + 1} {b.Price.ToString("0.##", CultureInfo.InvariantCulture)}{qty}";
            }));
            var note = p.LatestNote?.Text?.Replace('"', '\'') ?? "";
            sb.AppendLine(string.Join(',',
                Csv(p.Sector),
                Csv(p.Symbol),
                Csv(lots),
                Csv(p.TotalQuantity?.ToString("0.##", CultureInfo.InvariantCulture)),
                Csv(p.RemainingQuantity?.ToString("0.##", CultureInfo.InvariantCulture)),
                Csv(p.AvgBuy?.ToString("0.##", CultureInfo.InvariantCulture)),
                Csv(p.StopLoss?.ToString("0.##", CultureInfo.InvariantCulture)),
                Csv(p.TargetPrice?.ToString("0.##", CultureInfo.InvariantCulture)),
                Csv(p.WeightPct?.ToString("0.##", CultureInfo.InvariantCulture)),
                Csv(BrokerStatusLabels.Vi(p.Status)),
                Csv(p.RealizedPnl?.ToString("N0", CultureInfo.InvariantCulture)),
                Csv(note)));
        }

        if ((portfolio.ClosedPositions?.Count ?? 0) > 0)
        {
            sb.AppendLine();
            sb.AppendLine("# Vi the da dong");
            sb.AppendLine("Ma CP,Ngay dong,KL ban,Realized P&L (đ),Realized %");
            foreach (var p in portfolio.ClosedPositions!.OrderBy(x => x.ClosedAt ?? DateTime.MinValue))
            {
                sb.AppendLine(string.Join(',',
                    Csv(p.Symbol),
                    Csv(p.ClosedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                    Csv(p.SoldQuantity?.ToString("0.##", CultureInfo.InvariantCulture)),
                    Csv(p.RealizedPnl?.ToString("N0", CultureInfo.InvariantCulture)),
                    Csv(p.RealizedPnlPct?.ToString("N2", CultureInfo.InvariantCulture))));
            }
        }

        return DownloadAsync("broker-portfolio.csv", sb.ToString());
    }

    private async Task DownloadAsync(string fileName, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        using var stream = new MemoryStream(bytes);
        using var streamRef = new DotNetStreamReference(stream);
        await _js.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }

    private static string Csv(string? value)
    {
        var v = value ?? "";
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
            return $"\"{v.Replace("\"", "\"\"")}\"";
        return v;
    }
}
