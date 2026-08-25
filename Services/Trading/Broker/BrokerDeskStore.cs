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
    Task DownloadJsonAsync(BrokerPortfolio portfolio);
    Task DownloadCsvAsync(BrokerPortfolio portfolio);
    BrokerPortfolio ParseJson(string json);
}

public sealed class BrokerDeskStore : IBrokerDeskStore
{
    public const string PortfolioPath = "trading/broker/portfolio.json";

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
            using var resp = await _http.GetAsync(PortfolioPath, ct);
            if (!resp.IsSuccessStatusCode)
                return new BrokerPortfolio();

            var json = await resp.Content.ReadAsStringAsync(ct);
            return ParseJson(json);
        }
        catch
        {
            return new BrokerPortfolio();
        }
    }

    public BrokerPortfolio ParseJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new BrokerPortfolio();

        return JsonSerializer.Deserialize<BrokerPortfolio>(json, JsonOpts) ?? new BrokerPortfolio();
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
        sb.AppendLine("Nganh,Ma CP,Gia mua,Gia TB,Cat lo,Muc tieu,Ti trong,Trang thai,Note moi nhat");
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
                Csv(p.AvgBuy?.ToString("0.##", CultureInfo.InvariantCulture)),
                Csv(p.StopLoss?.ToString("0.##", CultureInfo.InvariantCulture)),
                Csv(p.TargetPrice?.ToString("0.##", CultureInfo.InvariantCulture)),
                Csv(p.WeightPct?.ToString("0.##", CultureInfo.InvariantCulture)),
                Csv(BrokerStatusLabels.Vi(p.Status)),
                Csv(note)));
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
