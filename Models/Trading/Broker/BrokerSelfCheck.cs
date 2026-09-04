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

        // Fee/tax scenario: buy 100 @ 20, sell 100 @ 25, fee 5000, tax 10000
        // Gross = (25-20)*100*1000 = 500,000. Net = 500,000 - 5,000 - 10,000 = 485,000
        var pos3 = new BrokerPosition { Symbol = "FEE", Status = BrokerPositionStatus.NamGiu };
        pos3.Buys.Add(new BrokerLot { BoughtAt = new DateTime(2026, 1, 1), Price = 20m, Quantity = 100m });
        pos3.Sells.Add(new BrokerSell { SoldAt = new DateTime(2026, 1, 10), Price = 25m, Quantity = 100m, Fee = 5000m, Tax = 10000m });

        Debug.Assert(pos3.RealizedPnl == 485_000m, $"Realized with fee/tax should be 485,000, got {pos3.RealizedPnl}");
        Debug.Assert(pos3.IsClosed, "FEE position should be closed");

        // Fee only (no tax)
        var pos4 = new BrokerPosition { Symbol = "FEE2", Status = BrokerPositionStatus.NamGiu };
        pos4.Buys.Add(new BrokerLot { BoughtAt = new DateTime(2026, 1, 1), Price = 30m, Quantity = 200m });
        pos4.Sells.Add(new BrokerSell { SoldAt = new DateTime(2026, 1, 10), Price = 35m, Quantity = 200m, Fee = 8000m });

        // Gross = (35-30)*200*1000 = 1,000,000. Net = 1,000,000 - 8,000 = 992,000
        Debug.Assert(pos4.RealizedPnl == 992_000m, $"Realized with fee only should be 992,000, got {pos4.RealizedPnl}");

        // Reopen scenario: closed position can reopen by moving back to Positions
        var portfolio2 = new BrokerPortfolio();
        portfolio2.ClosedPositions.Add(pos3);
        // Reopen: remove from Closed, add to Positions
        portfolio2.ClosedPositions.Remove(pos3);
        portfolio2.Positions.Add(pos3);
        // After reopen, position still has sells but IsClosed depends on RemainingQuantity
        Debug.Assert(pos3.IsClosed, "pos3 should still be closed (remaining = 0) until sells are edited");

        // Tag scenario
        var pos5 = new BrokerPosition { Symbol = "TAG", Status = BrokerPositionStatus.NamGiu };
        pos5.Tags ??= [];
        pos5.Tags.Add("swing");
        pos5.Tags.Add("dài hạn");
        Debug.Assert(pos5.Tags.Count == 2, $"Tags should have 2, got {pos5.Tags.Count}");
        Debug.Assert(pos5.Tags.Contains("swing"), "Should contain swing tag");

        // Lot-level tag scenario
        var pos6 = new BrokerPosition { Symbol = "LOTTAG", Status = BrokerPositionStatus.NamGiu };
        var lot = new BrokerLot { BoughtAt = new DateTime(2026, 1, 1), Price = 20m, Quantity = 100m };
        lot.Tags ??= [];
        lot.Tags.Add("lướt sóng");
        pos6.Buys.Add(lot);
        Debug.Assert(lot.Tags.Count == 1, $"Lot tag should have 1, got {lot.Tags.Count}");
        Debug.Assert(lot.Tags.Contains("lướt sóng"), "Lot should contain lướt sóng tag");

        // Performance report
        var portfolio3 = new BrokerPortfolio();
        var perfPos = new BrokerPosition { Symbol = "PERF", Status = BrokerPositionStatus.NamGiu };
        perfPos.Buys.Add(new BrokerLot { BoughtAt = new DateTime(2026, 1, 1), Price = 20m, Quantity = 100m });
        perfPos.Sells.Add(new BrokerSell { SoldAt = new DateTime(2026, 1, 15), Price = 25m, Quantity = 100m });
        portfolio3.ClosedPositions.Add(perfPos);

        var report = BrokerPortfolioStatsCalculator.ComputePerformance(portfolio3);
        Debug.Assert(report.HasData, "Performance report should have data");
        Debug.Assert(report.TotalTrades > 0, "Should have trades");
        Debug.Assert(report.TotalRealized > 0, "Realized should be positive");
        Debug.Assert(report.Months.Count > 0, "Should have at least 1 month");
    }
}