namespace BlazorWasmPortfolioGhAction.Services.Trading;

public enum VnSession
{
    Closed,
    Lunch,
    Ato,
    Continuous,
    Atc,
    Plo,
}

/// <summary>
/// Vietnam cash-equity rules shared by the order ticket, quick sell, and settlement calendar.
/// Holiday dates are a static list through 2027 and need a yearly update.
/// </summary>
public static class VnMarketRules
{
    public const decimal FeeRate = 0.0015m;
    public const decimal SellTaxRate = 0.001m;

    private static readonly HashSet<DateOnly> Holidays =
    [
        new(2025, 1, 1),
        new(2025, 1, 27), new(2025, 1, 28), new(2025, 1, 29), new(2025, 1, 30), new(2025, 1, 31),
        new(2025, 2, 1), new(2025, 2, 2),
        new(2025, 4, 7),
        new(2025, 4, 30), new(2025, 5, 1),
        new(2025, 9, 1), new(2025, 9, 2),

        new(2026, 1, 1),
        new(2026, 2, 14), new(2026, 2, 15), new(2026, 2, 16), new(2026, 2, 17), new(2026, 2, 18),
        new(2026, 2, 19), new(2026, 2, 20), new(2026, 2, 21), new(2026, 2, 22),
        new(2026, 4, 26),
        new(2026, 4, 30), new(2026, 5, 1),
        new(2026, 9, 2),

        new(2027, 1, 1),
        new(2027, 2, 5), new(2027, 2, 6), new(2027, 2, 7), new(2027, 2, 8), new(2027, 2, 9),
        new(2027, 2, 10), new(2027, 2, 11), new(2027, 2, 12), new(2027, 2, 13), new(2027, 2, 14),
        new(2027, 4, 16),
        new(2027, 4, 30), new(2027, 5, 1),
        new(2027, 9, 2),
    ];

    public static DateTime ToVietnam(DateTime instant)
    {
        if (instant.Kind != DateTimeKind.Utc) return instant;
        foreach (var id in new[] { "SE Asia Standard Time", "Asia/Ho_Chi_Minh" })
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(id);
                return TimeZoneInfo.ConvertTimeFromUtc(instant, tz);
            }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return instant.AddHours(7);
    }

    public static bool IsHoliday(DateOnly day) => Holidays.Contains(day);

    public static bool IsBusinessDay(DateTime day)
    {
        if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) return false;
        return !IsHoliday(DateOnly.FromDateTime(day));
    }

    public static DateTime AddBusinessDays(DateTime date, int days)
    {
        var cursor = date.Date;
        var left = days;
        while (left > 0)
        {
            cursor = cursor.AddDays(1);
            if (IsBusinessDay(cursor)) left--;
        }
        return cursor;
    }

    /// <summary>13:00 on business day T+2. Shares and sale proceeds become available then.</summary>
    public static DateTime SettledAt(DateTime tradeDate) =>
        AddBusinessDays(tradeDate, 2).Date.AddHours(13);

    public static bool IsSettled(DateTime tradeDate, DateTime now) =>
        ToVietnam(now) >= SettledAt(tradeDate);

    public static decimal SettledQuantity(IEnumerable<(DateTime BoughtAt, decimal Qty)> lots, DateTime now) =>
        lots.Where(lot => lot.Qty > 0 && IsSettled(lot.BoughtAt, now)).Sum(lot => lot.Qty);

    public static decimal SellableQuantity(IEnumerable<(DateTime BoughtAt, decimal Qty)> lots, decimal sold, DateTime now) =>
        Math.Max(0, SettledQuantity(lots, now) - Math.Max(0, sold));

    public static decimal PendingQuantity(IEnumerable<(DateTime BoughtAt, decimal Qty)> lots, DateTime now)
    {
        var total = lots.Where(lot => lot.Qty > 0).Sum(lot => lot.Qty);
        return Math.Max(0, total - SettledQuantity(lots, now));
    }

    public static int TickVnd(string? exchange, int priceVnd)
    {
        var board = (exchange ?? "").Trim().ToUpperInvariant();
        if (board is "HNX" or "UPCOM") return 100;
        if (priceVnd < 10_000) return 10;
        if (priceVnd < 50_000) return 50;
        return 100;
    }

    public static int ToVnd(decimal deskPrice) =>
        (int)Math.Round(deskPrice * 1000m, MidpointRounding.AwayFromZero);

    public static decimal TickDesk(string? exchange, decimal deskPrice) =>
        TickVnd(exchange, ToVnd(deskPrice)) / 1000m;

    public static decimal RoundToTick(string? exchange, decimal deskPrice)
    {
        var step = TickDesk(exchange, deskPrice);
        if (step <= 0) return deskPrice;
        var steps = Math.Round(deskPrice / step, MidpointRounding.AwayFromZero);
        return steps * step;
    }

    public static bool OnTick(string? exchange, decimal deskPrice)
    {
        var vnd = ToVnd(deskPrice);
        var step = TickVnd(exchange, vnd);
        return step > 0 && vnd % step == 0;
    }

    public static decimal BandPercent(string? exchange) => (exchange ?? "").Trim().ToUpperInvariant() switch
    {
        "HNX" => 0.10m,
        "UPCOM" => 0.15m,
        _ => 0.07m,
    };

    public static (decimal Floor, decimal Ceil) Band(string? exchange, decimal refDesk)
    {
        var pct = BandPercent(exchange);
        var floor = RoundToTick(exchange, refDesk * (1 - pct));
        var ceil = RoundToTick(exchange, refDesk * (1 + pct));
        return (floor, ceil);
    }

    public static VnSession SessionAt(string? exchange, DateTime now)
    {
        var local = ToVietnam(now);
        if (!IsBusinessDay(local)) return VnSession.Closed;
        var minutes = local.Hour * 60 + local.Minute;
        var board = (exchange ?? "HOSE").Trim().ToUpperInvariant();
        if (minutes >= 11 * 60 + 30 && minutes < 13 * 60) return VnSession.Lunch;

        if (board == "HOSE")
        {
            if (minutes >= 9 * 60 && minutes < 9 * 60 + 15) return VnSession.Ato;
            if (minutes >= 9 * 60 + 15 && minutes < 11 * 60 + 30) return VnSession.Continuous;
            if (minutes >= 13 * 60 && minutes < 14 * 60 + 30) return VnSession.Continuous;
            if (minutes >= 14 * 60 + 30 && minutes < 14 * 60 + 45) return VnSession.Atc;
            return VnSession.Closed;
        }

        if (board == "HNX")
        {
            if (minutes >= 9 * 60 && minutes < 11 * 60 + 30) return VnSession.Continuous;
            if (minutes >= 13 * 60 && minutes < 14 * 60 + 30) return VnSession.Continuous;
            if (minutes >= 14 * 60 + 30 && minutes < 14 * 60 + 45) return VnSession.Atc;
            if (minutes >= 14 * 60 + 45 && minutes < 15 * 60) return VnSession.Plo;
            return VnSession.Closed;
        }

        if (minutes >= 9 * 60 && minutes < 11 * 60 + 30) return VnSession.Continuous;
        if (minutes >= 13 * 60 && minutes < 15 * 60) return VnSession.Continuous;
        return VnSession.Closed;
    }

    public static string SessionLabel(VnSession session) => session switch
    {
        VnSession.Ato => "ATO",
        VnSession.Continuous => "Liên tục",
        VnSession.Atc => "ATC",
        VnSession.Plo => "PLO",
        VnSession.Lunch => "Nghỉ trưa",
        _ => "Đóng cửa",
    };

    public static IReadOnlyList<string> PriceTypes(string? exchange, VnSession session)
    {
        var board = (exchange ?? "HOSE").Trim().ToUpperInvariant();
        return (board, session) switch
        {
            ("HOSE", VnSession.Ato) => ["ATO"],
            ("HOSE", VnSession.Continuous) => ["LO", "MP"],
            ("HOSE", VnSession.Atc) => ["ATC"],
            ("HNX", VnSession.Continuous) => ["LO", "MTL", "MOK", "MAK"],
            ("HNX", VnSession.Atc) => ["ATC"],
            ("HNX", VnSession.Plo) => ["PLO"],
            ("UPCOM", VnSession.Continuous) => ["LO"],
            _ => ["LO"],
        };
    }

    public static bool PriceTypeAllowed(string? exchange, string? priceType, DateTime now)
    {
        var type = (priceType ?? "LO").Trim().ToUpperInvariant();
        if (type == "LO") return true;
        var session = SessionAt(exchange, now);
        return PriceTypes(exchange, session).Contains(type, StringComparer.OrdinalIgnoreCase);
    }

    public static string? QuantityError(int quantity, string? priceType)
    {
        if (quantity <= 0) return "Khối lượng phải > 0.";
        var type = (priceType ?? "LO").Trim().ToUpperInvariant();
        if (quantity < 100)
        {
            if (type != "LO") return "Lô lẻ (1–99 cổ) chỉ đặt giá LO.";
            return null;
        }
        if (quantity % 100 != 0) return "Khối lượng lô chẵn phải là bội số của 100.";
        return null;
    }
}
