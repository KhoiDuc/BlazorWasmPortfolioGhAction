using VnDesk.Models;

namespace VnDesk.Services;

public sealed class SectorService
{
    private readonly Dictionary<string, List<string>> _map;

    public SectorService(AppConfig cfg)
    {
        var path = cfg.DataPath("sectors.json");
        _map = JsonStore.LoadOr(path, new Dictionary<string, List<string>>());
        foreach (var key in _map.Keys.ToList())
            _map[key] = _map[key].Select(s => s.ToUpperInvariant()).Distinct().ToList();
    }

    public IReadOnlyDictionary<string, List<string>> Map => _map;

    public IReadOnlyList<string> AllSymbols => _map.SelectMany(s => s.Value).Distinct().ToList();

    public string FindSector(string symbol)
    {
        symbol = symbol.ToUpperInvariant();
        foreach (var kv in _map)
        {
            if (kv.Value.Contains(symbol))
                return kv.Key;
        }
        return "";
    }

    public Dictionary<string, SectorAnalysis> Analyze(List<StockData> stocks)
    {
        var result = new Dictionary<string, SectorAnalysis>();
        foreach (var sector in _map)
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
