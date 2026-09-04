using System.Diagnostics;

namespace BlazorWasmPortfolioGhAction.Models.Trading.Broker;

/// <summary>
/// ponytail: inline self-check for FIFO realized P&L + close/rebuy flow.
/// Run manually in dev via BrokerSelfCheck.Run(). Remove when stable.
/// </summary>
public static class BrokerSelfCheck
{
    public static void Run()
    {
        // Scenario: buy 100 @ 20, buy 200 @ 25, sell 150 @ 30, sell 150 @ 22
        // FIFO: first 100 sold @30 against lot1 @20 → (30-20)*100*1000 = +1,000,000
        //       next 50 sold @30 against lot2 @25 → (30-25)*50*1000 = +250,000
        //       remaining 150 sold @22 against lot2 @25 → (22-25)*150*1000 = -450,000
        // Total realized = 1,000,000 + 250,000 - 450,000 = 800,000
        var pos = new BrokerPosition { Symbol = "TEST", Status = BrokerPositionStatus.NamGiu };
        pos.Buys.Add(new BrokerLot { BoughtAt = new DateTime(2026, 1, 1), Price = 20m, Quantity = 100m });
        pos.Buys.Add(new BrokerLot { BoughtAt = new DateTime(2026, 1, 5), Price = 25m, Quantity = 200m });

        pos.Sells.Add(new BrokerSell { SoldAt = new DateTime(2026, 1, 10), Price = 30m, Quantity = 150m });
        pos.Sells.Add(new BrokerSell { SoldAt = new DateTime(2026, 1, 15), Price = 22m, Quantity = 150m });

        Debug.Assert(pos.TotalQuantity == 300m, $"TotalQuantity should be 300, got {pos.TotalQuantity}");
        Debug.Assert(pos.SoldQuantity == 300m, $"SoldQuantity should be 300, got {pos.SoldQuantity}");
        Debug.Assert(pos.RemainingQuantity == 0m, $"RemainingQuantity should be 0, got {pos.RemainingQuantity}");
        Debug.Assert(pos.IsClosed, "Position should be closed");
        Debug.Assert(pos.RealizedPnl == 800_000m, $"RealizedPnl should be 800,000, got {pos.RealizedPnl}");

        // ClosedAt = last sell date
        Debug.Assert(pos.ClosedAt == new DateTime(2026, 1, 15), $"ClosedAt should be 2026-01-15, got {pos.ClosedAt}");

        // PnlAmount (unrealized) should be null when closed (no remaining)
        Debug.Assert(pos.PnlAmount(25m) is null, "PnlAmount should be null when closed");

        // Rebuy scenario: same symbol new position
        var portfolio = new BrokerPortfolio();
        portfolio.ClosedPositions.Add(pos);
        var rebuy = new BrokerPosition { Symbol = "TEST", Status = BrokerPositionStatus.NamGiu };
        rebuy.Buys.Add(new BrokerLot { BoughtAt = new DateTime(2026, 2, 1), Price = 24m, Quantity = 100m });
        portfolio.Positions.Add(rebuy);

        // AddRecommendation logic: find existing in Positions (not ClosedPositions)
        var existing = portfolio.Positions.FirstOrDefault(p => p.Symbol.Equals("TEST", StringComparison.OrdinalIgnoreCase));
        Debug.Assert(existing is not null, "Should find open position for rebuy");
        Debug.Assert(!ReferenceEquals(existing, pos), "Rebuy should be a new position, not the closed one");
        Debug.Assert(existing.Buys.Count == 1, "New position should have 1 buy lot");

        // Partial sell scenario
        var pos2 = new BrokerPosition { Symbol = "PRT", Status = BrokerPositionStatus.NamGiu };
        pos2.Buys.Add(new BrokerLot { BoughtAt = new DateTime(2026, 1, 1), Price = 50m, Quantity = 100m });
        pos2.Sells.Add(new BrokerSell { SoldAt = new DateTime(2026, 1, 10), Price = 55m, Quantity = 30m });

        Debug.Assert(!pos2.IsClosed, "Partial sell should not close position");
        Debug.Assert(pos2.RemainingQuantity == 70m, $"Remaining should be 70, got {pos2.RemainingQuantity}");
        // Realized = (55-50)*30*1000 = 150,000
        Debug.Assert(pos2.RealizedPnl == 150_000m, $"Partial realized should be 150,000, got {pos2.RealizedPnl}");
        // Unrealized on remaining 70 @ current 52: (52-50)*70*1000 = 140,000
        Debug.Assert(pos2.PnlAmount(52m) == 140_000m, $"Unrealized should be 140,000, got {pos2.PnlAmount(52m)}");
    }
}