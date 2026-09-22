using BlazorWasmPortfolioGhAction.Models.Trading.Broker;
using BlazorWasmPortfolioGhAction.Models.Trading.Tcbs;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Tcbs;

public static class TcbsPortfolioSync
{
    public static List<TcbsSyncRow> Preview(BrokerPortfolio portfolio, IReadOnlyList<TcbsAsset> assets, IReadOnlyList<TcbsMatch> matches)
    {
        var rows = new List<TcbsSyncRow>();
        var positions = portfolio.Positions.Concat(portfolio.ClosedPositions ?? []).ToList();

        foreach (var asset in assets.Where(a => !LooksDerivative(a.Symbol)))
        {
            var pos = positions.FirstOrDefault(p => p.Symbol.Equals(asset.Symbol, StringComparison.OrdinalIgnoreCase));
            if (pos is null)
            {
                rows.Add(new TcbsSyncRow
                {
                    Symbol = asset.Symbol,
                    Action = "Thêm mã",
                    Detail = $"KL {asset.Quantity:N0} · giá TB {asset.AvgPrice:N2}",
                });
                continue;
            }

            var remain = pos.RemainingQuantity ?? 0;
            if (remain != asset.Quantity)
            {
                rows.Add(new TcbsSyncRow
                {
                    Symbol = asset.Symbol,
                    Action = "Khớp KL",
                    Detail = $"Sổ tay {remain:N0} → TCBS {asset.Quantity:N0} · giá TB {asset.AvgPrice:N2}",
                });
            }
        }

        foreach (var match in matches.Where(m => !LooksDerivative(m.Symbol)))
        {
            var pos = positions.FirstOrDefault(p => p.Symbol.Equals(match.Symbol, StringComparison.OrdinalIgnoreCase));
            var buy = IsBuy(match.Side);
            var exists = pos is not null && (buy
                ? pos.Buys.Any(b => SameLot(b.Price, b.Quantity, match))
                : pos.Sells.Any(s => SameLot(s.Price, s.Quantity, match)));
            if (exists) continue;
            rows.Add(new TcbsSyncRow
            {
                Symbol = match.Symbol,
                Action = buy ? "Thêm lần mua" : "Thêm lần bán",
                Detail = $"{match.Quantity:N0} @ {match.Price:N2}" + (match.Time is null ? "" : $" · {match.Time:dd/MM/yyyy HH:mm}"),
            });
        }

        return rows;
    }

    public static void Apply(BrokerPortfolio portfolio, IReadOnlyList<TcbsSyncRow> selected, IReadOnlyList<TcbsAsset> assets, IReadOnlyList<TcbsMatch> matches)
    {
        bool Take(string symbol, string action) =>
            selected.Any(r => r.Selected && r.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase) && r.Action == action);

        foreach (var asset in assets.Where(a => !LooksDerivative(a.Symbol)))
        {
            var pos = FindOrNull(portfolio, asset.Symbol);
            if (pos is null && Take(asset.Symbol, "Thêm mã"))
            {
                pos = new BrokerPosition
                {
                    Symbol = asset.Symbol,
                    Status = BrokerPositionStatus.NamGiu,
                    Buys =
                    [
                        new BrokerLot
                        {
                            BoughtAt = DateTime.Today,
                            Price = asset.AvgPrice,
                            Quantity = asset.Quantity,
                            Note = "Đồng bộ từ TCBS",
                        }
                    ],
                    Notes =
                    [
                        new BrokerNote { Kind = BrokerNoteKind.Self, Text = "Tạo từ tài sản TCBS." }
                    ],
                };
                portfolio.Positions.Add(pos);
                continue;
            }

            if (pos is not null && Take(asset.Symbol, "Khớp KL"))
            {
                pos.Buys =
                [
                    new BrokerLot
                    {
                        BoughtAt = DateTime.Today,
                        Price = asset.AvgPrice,
                        Quantity = asset.Quantity,
                        Note = "KL đồng bộ từ TCBS",
                    }
                ];
                pos.Sells = [];
                pos.Status = BrokerPositionStatus.NamGiu;
            }
        }

        foreach (var match in matches.Where(m => !LooksDerivative(m.Symbol)))
        {
            var buy = IsBuy(match.Side);
            var action = buy ? "Thêm lần mua" : "Thêm lần bán";
            if (!Take(match.Symbol, action)) continue;
            var pos = FindOrNull(portfolio, match.Symbol) ?? portfolio.Positions.FirstOrDefault(p => p.Symbol.Equals(match.Symbol, StringComparison.OrdinalIgnoreCase));
            if (pos is null)
            {
                pos = new BrokerPosition { Symbol = match.Symbol, Status = BrokerPositionStatus.NamGiu };
                portfolio.Positions.Add(pos);
            }
            if (buy)
            {
                if (pos.Buys.Any(b => SameLot(b.Price, b.Quantity, match))) continue;
                pos.Buys.Add(new BrokerLot
                {
                    BoughtAt = match.Time?.Date ?? DateTime.Today,
                    Price = match.Price,
                    Quantity = match.Quantity,
                    Note = "Khớp TCBS " + match.TradeId,
                });
            }
            else
            {
                if (pos.Sells.Any(s => SameLot(s.Price, s.Quantity, match))) continue;
                pos.Sells.Add(new BrokerSell
                {
                    SoldAt = match.Time?.Date ?? DateTime.Today,
                    Price = match.Price,
                    Quantity = match.Quantity,
                    Note = "Khớp TCBS " + match.TradeId,
                });
            }
        }
    }

    private static BrokerPosition? FindOrNull(BrokerPortfolio portfolio, string symbol) =>
        portfolio.Positions.FirstOrDefault(p => p.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase))
        ?? portfolio.ClosedPositions?.FirstOrDefault(p => p.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));

    private static bool SameLot(decimal price, decimal? qty, TcbsMatch match) =>
        qty is not null && qty.Value == match.Quantity && Math.Abs(price - match.Price) < 0.001m;

    private static bool IsBuy(string side)
    {
        var s = side.Trim().ToUpperInvariant();
        return s is "B" or "NB" or "BUY" or "MUA";
    }

    private static bool LooksDerivative(string symbol) =>
        symbol.Contains("VN30F", StringComparison.OrdinalIgnoreCase)
        || symbol.Contains("VN100F", StringComparison.OrdinalIgnoreCase);
}
