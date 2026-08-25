namespace BlazorWasmPortfolioGhAction.Services.Trading;

/// <summary>
/// Owner infrastructure URLs (mirrors Trading-Signals/vercel.json + proxy.go).
/// Set UseOwnerEndpoints=true to bypass broken Fly proxy routes where direct HTTPS works.
/// </summary>
public class TradingApiOptions
{
    /// <summary>Go API on Fly.io — watchlist, journal, community, chat, DNSE proxy gateway.</summary>
    public string BaseUrl { get; set; } = "https://trading-api-dark-sunset-2092.fly.dev";

    /// <summary>OSINT / Telegram / macro news VPS (vercel rewrites to this host).</summary>
    public string OsintBaseUrl { get; set; } = "http://152.53.208.182:8080";

    /// <summary>RRG chart PNG host (alwaysdata).</summary>
    public string RrgBaseUrl { get; set; } = "https://thehaohcm.alwaysdata.net";

    /// <summary>Economic calendar JSON.</summary>
    public string CalendarUrl { get; set; } = "https://nfs.faireconomy.media/ff_calendar_thisweek.json";

    /// <summary>Live FX rates JSON.</summary>
    public string FxRatesUrl { get; set; } = "https://live-rates.com/rates";

    /// <summary>Use owner direct HTTPS URLs + OSINT VPS routing (temporary shared infra).</summary>
    public bool UseOwnerEndpoints { get; set; } = true;

    /// <summary>After Fly returns 404/502, retry OSINT paths against OsintBaseUrl (works on localhost HTTP dev).</summary>
    public bool OsintFallbackDirect { get; set; } = true;
}
