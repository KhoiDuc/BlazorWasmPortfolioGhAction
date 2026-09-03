using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public sealed class VnSectorService
{
    private readonly VnDeskDataService _data;
    private Dictionary<string, List<string>>? _map;
    private Dictionary<string, string>? _symbolIndex;

    public VnSectorService(VnDeskDataService data) => _data = data;

    private async Task<Dictionary<string, List<string>>> MapAsync()
    {
        _map ??= await _data.GetSectorsAsync();
        return _map;
    }

    private async Task EnsureIndexAsync()
    {
        if (_symbolIndex is not null) return;
        var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in await MapAsync())
        {
            foreach (var sym in kv.Value)
                index.TryAdd(sym, kv.Key);
        }
        _symbolIndex = index;
    }

    public async Task<IReadOnlyDictionary<string, List<string>>> GetMapAsync() => await MapAsync();

    public async Task<IReadOnlyList<string>> GetAllSymbolsAsync() =>
        (await MapAsync()).SelectMany(s => s.Value).Distinct().ToList();

    public async Task<string> FindSectorAsync(string symbol)
    {
        await EnsureIndexAsync();
        return _symbolIndex!.TryGetValue(symbol.ToUpperInvariant(), out var sector) ? sector : "";
    }

    public async Task<Dictionary<string, SectorAnalysis>> AnalyzeAsync(List<StockData> stocks)
    {
        var result = new Dictionary<string, SectorAnalysis>();
        foreach (var sector in await MapAsync())
        {
            var list = stocks.Where(s => sector.Value.Contains(s.Symbol)).ToList();
            if (list.Count == 0) continue;
            result[sector.Key] = new SectorAnalysis
            {
                Name = sector.Key,
                StockCount = list.Count,
                UpCount = list.Count(s => s.PercentChange > 0),
                DownCount = list.Count(s => s.PercentChange < 0),
                UnchangedCount = list.Count(s => s.PercentChange == 0),
                AverageChange = list.Average(s => s.PercentChange),
                TotalVolume = list.Sum(s => s.Volume),
                TopGainer = list.OrderByDescending(s => s.PercentChange).FirstOrDefault(),
                TopLoser = list.OrderBy(s => s.PercentChange).FirstOrDefault(),
                TopVolume = list.OrderByDescending(s => s.Volume).FirstOrDefault()
            };
        }
        return result;
    }
}
