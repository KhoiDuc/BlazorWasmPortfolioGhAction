namespace BlazorWasmPortfolioGhAction.Services.Trading;

/// <summary>
/// Trading backend URLs — đổi link trong appsettings.json khi cần.
/// </summary>
public class TradingApiOptions
{
    /// <summary>Go API (Fly.io) — watchlist, journal, community, chat.</summary>
    public string BaseUrl { get; set; } = "https://trading-api-dark-sunset-2092.fly.dev";

    /// <summary>OSINT / Telegram / macro news VPS.</summary>
    public string OsintBaseUrl { get; set; } = "http://152.53.208.182:8080";

    /// <summary>RRG chart PNG host.</summary>
    public string RrgBaseUrl { get; set; } = "https://thehaohcm.alwaysdata.net";

    /// <summary>Economic calendar JSON.</summary>
    public string CalendarUrl { get; set; } = "https://nfs.faireconomy.media/ff_calendar_thisweek.json";

    /// <summary>Live FX rates JSON.</summary>
    public string FxRatesUrl { get; set; } = "https://live-rates.com/rates";
}
