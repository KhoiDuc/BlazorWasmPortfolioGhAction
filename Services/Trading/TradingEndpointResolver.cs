namespace BlazorWasmPortfolioGhAction.Services.Trading;

/// <summary>
/// Maps proxy paths to external owner URLs (VnDirect, CafeF, Yahoo, SJC, TCBS, DNSE, Coingecko, calendar, FX).
/// No internal backend routing — RRG/Fly/OSINT removed.
/// </summary>
public class TradingEndpointResolver
{
    private readonly TradingApiOptions _options;

    public TradingEndpointResolver(TradingApiOptions options) => _options = options;

    /// <summary>Absolute URL for img/iframe src (gold, silver…).</summary>
    public string ResolveProxyUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var p = Normalize(path);

        if (p.Equals("phuquygold", StringComparison.OrdinalIgnoreCase))
            return "https://giabac.phuquygroup.vn/PhuQuyPrice/SilverPricePartial";

        if (p.StartsWith("goldprice/", StringComparison.OrdinalIgnoreCase))
            return RewritePrefix(p, "goldprice", "https://sjc.com.vn/GoldPrice");

        if (p.StartsWith("silverprice/", StringComparison.OrdinalIgnoreCase))
            return RewritePrefix(p, "silverprice", "https://giabac.phuquygroup.vn/PhuQuyPrice");

        if (p.StartsWith("yahoo/", StringComparison.OrdinalIgnoreCase))
            p = "yahoo-finance/" + p["yahoo/".Length..];

        if (p.StartsWith("yahoo-finance/", StringComparison.OrdinalIgnoreCase))
        {
            var direct = RewritePrefix(p, "yahoo-finance", "https://query1.finance.yahoo.com");
            return WrapCorsProxy(direct);
        }

        if (p.StartsWith("tcanalysis/", StringComparison.OrdinalIgnoreCase))
            return RewritePrefix(p, "tcanalysis", "https://apipubaws.tcbs.com.vn/tcanalysis");

        if (p.StartsWith("stock-insight/", StringComparison.OrdinalIgnoreCase))
            return RewritePrefix(p, "stock-insight", "https://apipubaws.tcbs.com.vn/stock-insight");

        if (p.StartsWith("dnse-", StringComparison.OrdinalIgnoreCase))
            return $"https://services.entrade.com.vn/{p}";

        if (p.StartsWith("cafef/", StringComparison.OrdinalIgnoreCase))
            return RewritePrefix(p, "cafef", "https://banggia.cafef.vn");

        if (p.StartsWith("v4/", StringComparison.OrdinalIgnoreCase))
            return RewritePrefix(p, "v4", "https://api-finfo.vndirect.com.vn/v4");

        if (p.StartsWith("cg/", StringComparison.OrdinalIgnoreCase))
            return RewritePrefix(p, "cg", "https://api.coingecko.com");

        return string.Empty;
    }

    /// <summary>Absolute URL for JSON fetch (calendar, rates, proxy JSON).</summary>
    public string ResolveFetchUrl(string path)
    {
        var p = Normalize(path);

        if (p.Equals("ff_calendar_thisweek.json", StringComparison.OrdinalIgnoreCase))
            return _options.CalendarUrl;

        if (p.Equals("api/rates", StringComparison.OrdinalIgnoreCase))
            return _options.FxRatesUrl;

        var direct = ResolveProxyUrl(path);
        return Uri.IsWellFormedUriString(direct, UriKind.Absolute) ? direct : string.Empty;
    }

    private static string RewritePrefix(string path, string prefix, string targetBase)
    {
        var suffix = path[prefix.Length..].TrimStart('/');
        return string.IsNullOrEmpty(suffix)
            ? targetBase.TrimEnd('/')
            : $"{targetBase.TrimEnd('/')}/{suffix}";
    }

    private static string Normalize(string path) => path.Trim().TrimStart('/');

    /// <summary>Wraps an external URL through the CORS proxy if configured.</summary>
    private string WrapCorsProxy(string directUrl)
    {
        var proxy = _options.CorsProxyUrl?.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(proxy))
            return directUrl;
        return $"{proxy}/?url={Uri.EscapeDataString(directUrl)}";
    }
}