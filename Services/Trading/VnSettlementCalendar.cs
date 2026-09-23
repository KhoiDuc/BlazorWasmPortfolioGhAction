using BlazorWasmPortfolioGhAction.Models.Trading.Broker;

namespace BlazorWasmPortfolioGhAction.Services.Trading;

public sealed record SettlementRow(DateTime At, string Symbol, string Kind, decimal Amount, string Note);

public static class VnSettlementCalendar
{
    public static IReadOnlyList<(DateTime Day, decimal Total, IReadOnlyList<SettlementRow> Rows)> Build(
        BrokerPortfolio portfolio, DateTime now)
    {
        var rows = new List<SettlementRow>();
        var positions = (portfolio.Positions ?? []).Concat(portfolio.ClosedPositions ?? []);
        foreach (var position in positions)
        {
            foreach (var buy in position.Buys.Where(b => b.Quantity is > 0 && b.Price > 0))
            {
                var at = VnMarketRules.SettledAt(buy.BoughtAt);
                if (at < now.Date) continue;
                rows.Add(new SettlementRow(at, position.Symbol, "Hàng về", 0,
                    $"{buy.Quantity:N0} cổ về T+2"));
            }
            foreach (var sell in position.Sells.Where(s => s.Quantity is > 0 && s.Price > 0))
            {
                var at = VnMarketRules.SettledAt(sell.SoldAt);
                var gross = BrokerMoney.PositionValueVnd(sell.Price, sell.Quantity!.Value);
                var fee = sell.Fee ?? decimal.Round(gross * VnMarketRules.FeeRate, 0);
                var tax = sell.Tax ?? decimal.Round(gross * VnMarketRules.SellTaxRate, 0);
                rows.Add(new SettlementRow(at, position.Symbol, "Tiền bán về", gross - fee - tax,
                    $"Sau phí {fee:N0} và thuế {tax:N0}"));
            }
            foreach (var dividend in position.Dividends.Where(d => d.PayDate is not null && d.TotalAmount != 0))
            {
                rows.Add(new SettlementRow(dividend.PayDate!.Value, position.Symbol, "Cổ tức", dividend.TotalAmount, dividend.Note ?? ""));
            }
        }

        return rows
            .GroupBy(row => row.At.Date)
            .OrderBy(group => group.Key)
            .Select(group => (group.Key, group.Sum(row => row.Amount), (IReadOnlyList<SettlementRow>)group.OrderBy(row => row.Symbol).ToList()))
            .ToList();
    }
}
