using System.Globalization;
using System.Text;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Models.Trading.Broker;
using Microsoft.JSInterop;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

public interface IBrokerDeskStore
{
    Task<BrokerPortfolio> LoadAsync(CancellationToken ct = default);
    Task<BrokerPortfolio?> LoadFromApiAsync(CancellationToken ct = default);
    Task SaveDraftAsync(BrokerPortfolio portfolio, CancellationToken ct = default);
    Task DownloadJsonAsync(BrokerPortfolio portfolio);
    Task DownloadCsvAsync(BrokerPortfolio portfolio);
    BrokerPortfolio ParseJson(string json);

    // CRUD Position
    Task<(bool Ok, string? Error)> CreatePositionAsync(BrokerPosition pos);
    Task<(bool Ok, string? Error)> UpdatePositionAsync(string symbol, BrokerPosition pos);
    Task<(bool Ok, string? Error)> DeletePositionAsync(string symbol);

    // CRUD Lot
    Task<(bool Ok, string? Error)> AddLotAsync(string symbol, BrokerLot lot);
    Task<(bool Ok, string? Error)> UpdateLotAsync(string symbol, BrokerLot lot);
    Task<(bool Ok, string? Error)> DeleteLotAsync(string symbol, string lotId);

    // CRUD Sell
    Task<(bool Ok, string? Error)> AddSellAsync(string symbol, BrokerSell sell);
    Task<(bool Ok, string? Error)> UpdateSellAsync(string symbol, BrokerSell sell);
    Task<(bool Ok, string? Error)> DeleteSellAsync(string symbol, string sellId);

    // CRUD Note
    Task<(bool Ok, string? Error)> AddNoteAsync(string symbol, BrokerNote note);
    Task<(bool Ok, string? Error)> UpdateNoteAsync(string symbol, BrokerNote note);
    Task<(bool Ok, string? Error)> DeleteNoteAsync(string symbol, string noteId);

    // CRUD Dividend
    Task<(bool Ok, string? Error)> AddDividendAsync(string symbol, BrokerDividend div);
    Task<(bool Ok, string? Error)> DeleteDividendAsync(string symbol, string divId);

    // Import (PUT toàn bộ — cho ImportJson)
    Task<(bool Ok, string? Error)> ImportAsync(BrokerPortfolio portfolio);
}

public sealed class BrokerDeskStore : IBrokerDeskStore
{
    private const string DraftKey = "broker.desk.draft";

    private readonly IJSRuntime _js;
    private readonly IBrokerApiClient _api;

    public BrokerDeskStore(IJSRuntime js, IBrokerApiClient api)
    {
        _js = js;
        _api = api;
    }

    public async Task<BrokerPortfolio> LoadAsync(CancellationToken ct = default)
    {
        var fromApi = await LoadFromApiAsync(ct);
        if (fromApi is not null)
            return fromApi;

        try
        {
            var draft = await _js.InvokeAsync<string?>("tradingAuth.getItem", DraftKey);
            if (!string.IsNullOrWhiteSpace(draft))
                return NormalizePortfolio(ParseJson(draft));
        }
        catch
        {
            // Ignore localStorage errors.
        }

        return new BrokerPortfolio();
    }

    public async Task<BrokerPortfolio?> LoadFromApiAsync(CancellationToken ct = default)
    {
        try
        {
            var fromApi = await _api.GetPortfolioAsync(ct);
            if (fromApi is null)
                return null;

            var normalized = NormalizePortfolio(fromApi);
            await SaveDraftAsync(normalized, ct);
            return normalized;
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveDraftAsync(BrokerPortfolio portfolio, CancellationToken ct = default)
    {
        portfolio.UpdatedAt = DateTime.Now;
        var json = JsonSerializer.Serialize(NormalizePortfolio(portfolio), BrokerJson.Options);
        await _js.InvokeVoidAsync("tradingAuth.setItem", DraftKey, json);
    }

    public BrokerPortfolio ParseJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new BrokerPortfolio();

        try
        {
            return NormalizePortfolio(JsonSerializer.Deserialize<BrokerPortfolio>(json, BrokerJson.Options) ?? new BrokerPortfolio());
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
        position.Dividends ??= [];
        position.Tags ??= [];
        position.Buys = position.Buys.Where(b => b.Price > 0).ToList();
        position.Sells = position.Sells.Where(s => s.Price > 0).ToList();
        foreach (var lot in position.Buys)
            lot.Tags ??= [];
        return position;
    }

    // ── CRUD Position ──

    public Task<(bool Ok, string? Error)> CreatePositionAsync(BrokerPosition pos) =>
        _api.CreatePositionAsync(pos);

    public Task<(bool Ok, string? Error)> UpdatePositionAsync(string symbol, BrokerPosition pos) =>
        _api.UpdatePositionAsync(symbol, pos);

    public Task<(bool Ok, string? Error)> DeletePositionAsync(string symbol) =>
        _api.DeletePositionAsync(symbol);

    // ── CRUD Lot ──

    public Task<(bool Ok, string? Error)> AddLotAsync(string symbol, BrokerLot lot) =>
        _api.AddLotAsync(symbol, lot);

    public Task<(bool Ok, string? Error)> UpdateLotAsync(string symbol, BrokerLot lot) =>
        _api.UpdateLotAsync(symbol, lot);

    public Task<(bool Ok, string? Error)> DeleteLotAsync(string symbol, string lotId) =>
        _api.DeleteLotAsync(symbol, lotId);

    // ── CRUD Sell ──

    public Task<(bool Ok, string? Error)> AddSellAsync(string symbol, BrokerSell sell) =>
        _api.AddSellAsync(symbol, sell);

    public Task<(bool Ok, string? Error)> UpdateSellAsync(string symbol, BrokerSell sell) =>
        _api.UpdateSellAsync(symbol, sell);

    public Task<(bool Ok, string? Error)> DeleteSellAsync(string symbol, string sellId) =>
        _api.DeleteSellAsync(symbol, sellId);

    // ── CRUD Note ──

    public Task<(bool Ok, string? Error)> AddNoteAsync(string symbol, BrokerNote note) =>
        _api.AddNoteAsync(symbol, note);

    public Task<(bool Ok, string? Error)> UpdateNoteAsync(string symbol, BrokerNote note) =>
        _api.UpdateNoteAsync(symbol, note);

    public Task<(bool Ok, string? Error)> DeleteNoteAsync(string symbol, string noteId) =>
        _api.DeleteNoteAsync(symbol, noteId);

    // ── CRUD Dividend ──

    public Task<(bool Ok, string? Error)> AddDividendAsync(string symbol, BrokerDividend div) =>
        _api.AddDividendAsync(symbol, div);

    public Task<(bool Ok, string? Error)> DeleteDividendAsync(string symbol, string divId) =>
        _api.DeleteDividendAsync(symbol, divId);

    // ── Import (PUT toàn bộ) ──

    public Task<(bool Ok, string? Error)> ImportAsync(BrokerPortfolio portfolio) =>
        _api.ImportPortfolioAsync(NormalizePortfolio(portfolio));

    public Task DownloadJsonAsync(BrokerPortfolio portfolio)
    {
        portfolio.UpdatedAt = DateTime.Now;
        var json = JsonSerializer.Serialize(portfolio, BrokerJson.Options);
        return DownloadAsync("portfolio.json", json);
    }

    public Task DownloadCsvAsync(BrokerPortfolio portfolio)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Nganh,Ma CP,Gia mua,KL tong,KL con lai,Gia TB,Cat lo,Muc tieu,Ti trong,Trang thai,Realized P&L (đ),Co tuc (đ),Note moi nhat");
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
                Csv(p.TotalDividends?.ToString("N0", CultureInfo.InvariantCulture)),
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