using BlazorWasmPortfolioGhAction.Models.Trading.VnDesk;

namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public sealed class PositionService
{
    public static readonly (string Key, string Name, decimal FeePct)[] Brokers =
    [
        ("A", "VPS", 0.15m),
        ("B", "SSI", 0.15m),
        ("C", "TCBS", 0.10m),
        ("D", "MBS", 0.12m)
    ];

    public PositionResult Calculate(string symbol, decimal entryPrice, int shares, decimal currentPrice, string brokerKey, decimal? customFee = null)
    {
        // VN quotes are often in thousands; keep user input as typed (dong or nghin — user types actual).
        var broker = Brokers.FirstOrDefault(b => b.Key.Equals(brokerKey, StringComparison.OrdinalIgnoreCase));
        var feePct = customFee ?? (broker.Name is null ? 0.15m : broker.FeePct);
        var name = broker.Name ?? "Khac";
        const decimal sellTaxRate = 0.001m;

        decimal buyValue = entryPrice * shares;
        decimal sellValue = currentPrice * shares;
        decimal buyFee = buyValue * (feePct / 100);
        decimal sellFee = sellValue * (feePct / 100);
        decimal sellTax = sellValue * sellTaxRate;
        decimal totalCost = buyValue + buyFee;
        decimal net = sellValue - sellFee - sellTax;
        decimal pnl = net - totalCost;
        decimal pct = totalCost == 0 ? 0 : pnl / totalCost * 100;

        var note = pct < -5 ? "Lo > 5%. Can nhac cat lo / theo doi."
            : pct > 10 ? "Lai > 10%. Can nhac chot mot phan."
            : "Vi the on dinh. Tiep tuc theo doi.";

        return new PositionResult
        {
            Symbol = symbol,
            Broker = name,
            FeeRatePct = feePct,
            EntryPrice = entryPrice,
            CurrentPrice = currentPrice,
            Shares = shares,
            BuyValue = buyValue,
            BuyFee = buyFee,
            TotalCost = totalCost,
            SellValue = sellValue,
            SellFee = sellFee,
            SellTax = sellTax,
            NetSellValue = net,
            ProfitLoss = pnl,
            ProfitLossPercent = pct,
            Note = note
        };
    }

    public SizeResult Size(decimal capital, decimal riskPct, decimal price, decimal stop, decimal? target = null)
    {
        var result = new SizeResult { Capital = capital, RiskPct = riskPct, Price = price, Stop = stop };
        if (price <= 0 || capital <= 0 || riskPct <= 0)
        {
            result.NoTrade = true;
            result.Reason = "Thieu von / gia / % risk.";
            return result;
        }
        var dist = Math.Abs(price - stop);
        if (dist <= 0)
        {
            result.NoTrade = true;
            result.Reason = "Thieu stop hoac stop = gia. No-trade.";
            return result;
        }
        result.StopDistance = dist;
        result.RiskAmount = capital * (riskPct / 100);
        result.Shares = (int)Math.Floor(result.RiskAmount / dist);
        result.Shares -= result.Shares % 100; // lot 100 VN
        if (result.Shares < 100)
        {
            result.NoTrade = true;
            result.Reason = "Size < 1 lot. No-trade.";
            return result;
        }
        result.PositionValue = result.Shares * price;
        if (target is > 0)
        {
            result.Reward = Math.Abs(target.Value - price) * result.Shares;
            result.RiskReward = result.RiskAmount == 0 ? 0 : result.Reward / (dist * result.Shares);
            if (result.RiskReward < 1.2m)
            {
                result.NoTrade = true;
                result.Reason = $"R:R {result.RiskReward:N2} < 1.2. No-trade.";
            }
        }
        return result;
    }
}
