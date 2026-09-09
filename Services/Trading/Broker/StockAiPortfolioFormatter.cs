using System.Text;
using BlazorWasmPortfolioGhAction.Models.Trading.Broker;

namespace BlazorWasmPortfolioGhAction.Services.Trading.Broker;

// ponytail: overlaps BrokerPromptLibrary.BuildPortfolioContext — both serialize a BrokerPortfolio to text.
// Different output shape (this: symbol+price+qty summary; that: full sector/status/stop/target context).
// Merge when: a single configurable formatter serving both call sites is warranted.
public static class StockAiPortfolioFormatter
{
    public static string Format(BrokerPortfolio portfolio, IReadOnlyDictionary<string, decimal> quotes)
    {
        var sb = new StringBuilder();
        foreach (var p in portfolio.Positions.OrderBy(x => x.Symbol, StringComparer.OrdinalIgnoreCase))
        {
            quotes.TryGetValue(p.Symbol, out var current);
            var currentText = current > 0 ? current.ToString("N2") : "—";
            sb.AppendLine($"{p.Symbol}: Giá TB {p.AvgBuy?.ToString("N2") ?? "—"}, Hiện tại {currentText}, KL {BrokerFormat.Quantity(p.TotalQuantity)}, TT {BrokerStatusLabels.Vi(p.Status)}");
        }
        return sb.ToString().Trim();
    }
}
