using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public sealed class SectorQuantService
{
    private readonly IVnMarketClient _market;
    private readonly VnSectorService _sectors;
    private readonly IVnDeskStore _store;

    public SectorQuantService(IVnMarketClient market, VnSectorService sectors, IVnDeskStore store)
    {
        _market = market;
        _sectors = sectors;
        _store = store;
    }

    /// <summary>Phase 1: latest quotes → Breadth/Maker/Score (VolR = 1 placeholder).</summary>
    public async Task<SectorQuantBoard> BuildPhase1Async(IProgress<int>? progress = null, CancellationToken ct = default)
    {
        var sectorMap = await _sectors.GetMapAsync();
        var allSymbols = sectorMap.SelectMany(s => s.Value).Distinct().ToList();
        var stocks = await _market.GetLatestManyAsync(allSymbols, progress, ct: ct);

        var prevRanks = await _store.GetSectorRankHistoryAsync();
        var sectors = new List<SectorRankCard>();
        var totalUp = 0; var totalDown = 0; var totalUnch = 0; var totalVol = 0m;

        foreach (var kv in sectorMap)
        {
            var list = stocks.Where(s => kv.Value.Contains(s.Symbol, StringComparer.OrdinalIgnoreCase)).ToList();
            if (list.Count == 0) continue;

            var card = new SectorRankCard { Name = kv.Key, StockCount = list.Count };
            card.UpCount = list.Count(s => s.PercentChange > 0);
            card.DownCount = list.Count(s => s.PercentChange < 0);
            card.Breadth = (double)card.UpCount / card.StockCount * 100;
            card.AvgChange = (double)list.Average(s => s.PercentChange);
            card.TotalVolume = list.Sum(s => s.Volume);
            card.Maker = CalcMakerProxy(list);
            card.VolR = 1.0; // placeholder until phase 2
            card.VolRLoaded = false;
            card.Score = CalcScore(card.Breadth, card.VolR, card.AvgChange, card.Maker);

            var topList = list.OrderByDescending(s => s.PercentChange).Take(3).ToList();
            if (topList.Count > 0)
            {
                card.TopGainerSymbol = topList[0].Symbol;
                card.TopGainerPct = (double)topList[0].PercentChange;
                card.TopSymbols = topList.Select(s => s.Symbol).ToList();
            }

            card.PrevRank = prevRanks.GetValueOrDefault(kv.Key);
            sectors.Add(card);

            totalUp += card.UpCount;
            totalDown += card.DownCount;
            totalUnch += card.StockCount - card.UpCount - card.DownCount;
            totalVol += card.TotalVolume;
        }

        sectors.Sort((a, b) => b.Score.CompareTo(a.Score));
        for (var i = 0; i < sectors.Count; i++)
            sectors[i].Rank = i + 1;

        var summary = BuildSummary(sectors, totalUp, totalDown, totalUnch, totalVol);
        return new SectorQuantBoard { Summary = summary, Sectors = sectors, UpdatedAt = DateTime.Now, Phase2Complete = false };
    }

    /// <summary>Phase 2: fetch 21-session history per symbol → update VolR + re-rank. Mutates board in place.</summary>
    public async Task LoadPhase2VolRAsync(SectorQuantBoard board, IProgress<int>? progress = null, CancellationToken ct = default)
    {
        var sectorMap = await _sectors.GetMapAsync();
        var done = 0;
        var totalSyms = sectorMap.SelectMany(s => s.Value).Distinct().Count();

        foreach (var card in board.Sectors)
        {
            if (!sectorMap.TryGetValue(card.Name, out var syms)) continue;
            var volrs = new List<double>();
            foreach (var sym in syms)
            {
                done++;
                progress?.Report(done);
                try
                {
                    var hist = await _market.GetHistoricalAsync(sym, 21, ct: ct);
                    if (hist.Count < 2) continue;
                    var avgVol = hist.Take(20).Average(h => h.Volume);
                    if (avgVol <= 0) continue;
                    volrs.Add((double)(hist[^1].Volume / avgVol));
                }
                catch { /* skip */ }
            }
            if (volrs.Count > 0)
            {
                card.VolR = volrs.Average();
                card.VolRLoaded = true;
                card.Score = CalcScore(card.Breadth, card.VolR, card.AvgChange, card.Maker);
            }
        }

        board.Sectors.Sort((a, b) => b.Score.CompareTo(a.Score));
        for (var i = 0; i < board.Sectors.Count; i++)
            board.Sectors[i].Rank = i + 1;

        board.Summary = BuildSummary(board.Sectors, board.Summary.TotalUp, board.Summary.TotalDown,
            board.Summary.TotalUnchanged, board.Summary.TotalVolume);
        board.Phase2Complete = true;
        board.UpdatedAt = DateTime.Now;

        var ranks = board.Sectors.ToDictionary(s => s.Name, s => s.Rank);
        await _store.SaveSectorRankHistoryAsync(ranks);
    }

    private static double CalcMakerProxy(List<StockData> stocks)
    {
        var weighted = stocks
            .Where(s => s.Range > 0)
            .Select(s => new { M = (double)((s.Close - s.Open) / s.Range), W = (double)s.Volume })
            .ToList();
        if (weighted.Count == 0 || weighted.Sum(x => x.W) == 0) return 0;
        var totalW = weighted.Sum(x => x.W);
        return Math.Clamp(weighted.Sum(x => x.M * x.W) / totalW, -1, 1);
    }

    private static double CalcScore(double breadth, double volR, double avgChange, double maker)
    {
        var normChange = Math.Clamp(avgChange / 5.0, -1, 1); // ±5% → ±1
        var score = 0.45 * breadth + 25 * Math.Min(volR, 2) + 15 * normChange + 10 * Math.Max(maker, 0);
        return Math.Clamp(score, 0, 100);
    }

    private static MarketQuantSummary BuildSummary(List<SectorRankCard> sectors, int totalUp, int totalDown, int totalUnch, decimal totalVol)
    {
        var totalStocks = totalUp + totalDown + totalUnch;
        var breadth = totalStocks > 0 ? (double)totalUp / totalStocks * 100 : 0;
        var avgVolR = sectors.Count > 0 ? sectors.Average(s => s.VolR) : 0;
        var avgMaker = sectors.Count > 0 ? sectors.Average(s => s.Maker) : 0;
        var avgScore = sectors.Count > 0 ? sectors.Average(s => s.Score) : 0;

        var upSec = sectors.Count(s => s.AvgChange > 0.3);
        var downSec = sectors.Count(s => s.AvgChange < -0.3);
        var flatSec = sectors.Count - upSec - downSec;
        var probUp = sectors.Count > 0 ? (double)upSec / sectors.Count * 100 : 0;
        var probDown = sectors.Count > 0 ? (double)downSec / sectors.Count * 100 : 0;
        var probFlat = sectors.Count > 0 ? (double)flatSec / sectors.Count * 100 : 0;

        var highLiq = sectors.Count(s => s.VolR >= 1.0);
        var highLiqRatio = sectors.Count > 0 ? (double)highLiq / sectors.Count * 100 : 0;

        var state = breadth >= 45 && avgVolR >= 1.05 ? "MỞ RỘNG" : "CO HẸP";

        return new MarketQuantSummary
        {
            AvgScore = avgScore,
            AvgMaker = avgMaker,
            AvgVolR = avgVolR,
            MarketBreadth = breadth,
            TotalUp = totalUp,
            TotalDown = totalDown,
            TotalUnchanged = totalUnch,
            TotalVolume = totalVol,
            MarketState = state,
            ProbUp = probUp,
            ProbDown = probDown,
            ProbFlat = probFlat,
            HighLiquidityRatio = highLiqRatio
        };
    }
}