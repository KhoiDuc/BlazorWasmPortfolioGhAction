namespace BlazorWasmPortfolioGhAction.Models.Trading.Broker;

public enum BrokerLevelInputMode
{
    Price,
    Percent
}

public static class BrokerLevelHelper
{
    public static decimal? ResolveStop(decimal buyPrice, BrokerLevelInputMode? mode, decimal? input)
    {
        if (input is not > 0 || buyPrice <= 0) return null;
        return mode == BrokerLevelInputMode.Percent
            ? buyPrice * (1 - input.Value / 100m)
            : input;
    }

    public static decimal? ResolveTarget(decimal buyPrice, BrokerLevelInputMode? mode, decimal? input)
    {
        if (input is not > 0 || buyPrice <= 0) return null;
        return mode == BrokerLevelInputMode.Percent
            ? buyPrice * (1 + input.Value / 100m)
            : input;
    }

    public static bool TryValidateStop(decimal buyPrice, BrokerLevelInputMode mode, decimal? input, out decimal? resolved, out string? error)
    {
        resolved = null;
        if (input is null or 0)
        {
            error = null;
            return true;
        }

        if (mode == BrokerLevelInputMode.Percent)
        {
            if (input <= 0 || input >= 100)
            {
                error = "Trading_BrokerLevel_StopPctRange";
                return false;
            }
            resolved = ResolveStop(buyPrice, mode, input);
            if (resolved is not > 0)
            {
                error = "Trading_BrokerLevel_StopPctTooLarge";
                return false;
            }
        }
        else
        {
            if (input >= buyPrice)
            {
                error = "Trading_BrokerLevel_StopPriceBelowBuy";
                return false;
            }
            resolved = input;
        }

        error = null;
        return true;
    }

    public static bool TryValidateTarget(decimal buyPrice, BrokerLevelInputMode mode, decimal? input, out decimal? resolved, out string? error)
    {
        resolved = null;
        if (input is null or 0)
        {
            error = null;
            return true;
        }

        if (mode == BrokerLevelInputMode.Percent)
        {
            if (input <= 0)
            {
                error = "Trading_BrokerLevel_TargetPctPositive";
                return false;
            }
            resolved = ResolveTarget(buyPrice, mode, input);
        }
        else
        {
            if (input <= buyPrice)
            {
                error = "Trading_BrokerLevel_TargetPriceAboveBuy";
                return false;
            }
            resolved = input;
        }

        error = null;
        return true;
    }

    public static string FormatDisplay(decimal? price, BrokerLevelInputMode? mode, decimal? input, bool isStop)
    {
        if (price is not > 0) return "—";
        if (mode == BrokerLevelInputMode.Percent && input is > 0)
        {
            var sign = isStop ? "-" : "+";
            return $"{price:N2} ({sign}{input:N1}%)";
        }
        return price.Value.ToString("N2");
    }
}
