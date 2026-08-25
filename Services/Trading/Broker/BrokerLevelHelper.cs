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
                error = "Cắt lỗ % phải từ 0 đến 100 (VD: 5 = -5%).";
                return false;
            }
            resolved = ResolveStop(buyPrice, mode, input);
            if (resolved is not > 0)
            {
                error = "Cắt lỗ % quá lớn so với giá mua.";
                return false;
            }
        }
        else
        {
            if (input >= buyPrice)
            {
                error = "Cắt lỗ giá phải thấp hơn giá mua.";
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
                error = "Mục tiêu % phải > 0 (VD: 10 = +10%).";
                return false;
            }
            resolved = ResolveTarget(buyPrice, mode, input);
        }
        else
        {
            if (input <= buyPrice)
            {
                error = "Mục tiêu giá phải cao hơn giá mua.";
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
