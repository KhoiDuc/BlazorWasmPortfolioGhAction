namespace BlazorWasmPortfolioGhAction.Services.Trading;

/// <summary>
/// Maps proxy paths to owner URLs — same targets as vercel.json / trading_api/internal/proxy/proxy.go.
/// </summary>
public class TradingEndpointResolver
{
    private readonly TradingApiOptions _options;

    private static readonly Dictionary<string, string> RrgFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["assets_rrgchart"] = "assets_rrgchart.png",
        ["crypto_rrgchart"] = "crypto_rrgchart.png",
        ["cryto_rrgchart"] = "crypto_rrgchart.png",
        ["futures_rrgchart"] = "futures_rrgchart.png",
        ["vnstock_rrgchart"] = "vnstock_rrgchart.png",
        ["forex_rrgchart"] = "forex_rrgchart.png",
    };

    public TradingEndpointResolver(TradingApiOptions options) => _options = options;

    public string FlyBase => _options.BaseUrl.TrimEnd('/');

    public string OsintBase => _options.OsintBaseUrl.TrimEnd('/');

    public bool IsOsintPath(string path)
    {
        var p = Normalize(path);
        return p.StartsWith("api/osint", StringComparison.OrdinalIgnoreCase)
            || p.StartsWith("api/news/telegram", StringComparison.OrdinalIgnoreCase)
            || p.StartsWith("api/news-groups", StringComparison.OrdinalIgnoreCase)
            || p.StartsWith("api/news-items", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Absolute URL for img/iframe src (RRG, gold, petrolimex…).</summary>
    public string ResolveProxyUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return FlyBase;

        var p = Normalize(path);

        if (RrgFiles.TryGetValue(p, out var rrgFile))
            return $"{_options.RrgBaseUrl.TrimEnd('/')}/{rrgFile}";

        if (p.Equals("phuquygold", StringComparison.OrdinalIgnoreCase))
            return "https://giabac.phuquygroup.vn/PhuQuyPrice/SilverPricePartial";

        if (p.StartsWith("goldprice/", StringComparison.OrdinalIgnoreCase))
            return RewritePrefix(p, "goldprice", "https://sjc.com.vn/GoldPrice");

        if (p.StartsWith("silverprice/", StringComparison.OrdinalIgnoreCase))
            return RewritePrefix(p, "silverprice", "https://giabac.phuquygroup.vn/PhuQuyPrice");

        if (p.StartsWith("petrolimex/", StringComparison.OrdinalIgnoreCase))
        {
            if (p.Equals("petrolimex/search", StringComparison.OrdinalIgnoreCase)
                || p.Equals("petrolimex", StringComparison.OrdinalIgnoreCase))
            {
                return "https://portals.petrolimex.com.vn/~apis/portals/cms.item/search?object-identity=search&x-request=eyJGaWx0ZXJCeSI6eyJBbmQiOlt7IlN5c3RlbUlEIjp7IkVxdWFscyI6IjY3ODNkYzEyNzFmZjQ0OWU5NWI3NGE5NTIwOTY0MTY5In19LHsiUmVwb3NpdG9yeUlEIjp7IkVxdWFscyI6ImE5NTQ1MWUyM2I0NzRmZTU4ODZiZmI3Y2Y4NDNmNTNjIn19LHsiUmVwb3NpdG9yeUVudGl0eUlEIjp7IkVxdWFscyI6IjM4MDEzNzhmZTFlMDQ1YjFhZmExMGRlN2M1Nzc2MTI0In19LHsiU3RhdHVzIjp7IkVxdWFscyI6IlB1Ymxpc2hlZCJ9fV19LCJTb3J0QnkiOnsiTGFzdE1vZGlmaWVkIjoiRGVzY2VuZGluZyJ9LCJQYWdpbmF0aW9uIjp7IlRvdGFsUmVjb3JkcyI6LTEsIlRvdGFsUGFnZXMiOjAsIlBhZ2VTaXplIjowLCJQYWdlTnVtYmVyIjowfX0=";
            }
            return RewritePrefix(p, "petrolimex", "https://portals.petrolimex.com.vn");
        }

        if (p.StartsWith("yahoo/", StringComparison.OrdinalIgnoreCase))
            p = "yahoo-finance/" + p["yahoo/".Length..];

        if (p.StartsWith("yahoo-finance/", StringComparison.OrdinalIgnoreCase))
            return RewritePrefix(p, "yahoo-finance", "https://query1.finance.yahoo.com");

        if (p.StartsWith("tcanalysis/", StringComparison.OrdinalIgnoreCase))
            return RewritePrefix(p, "tcanalysis", "https://apipubaws.tcbs.com.vn/tcanalysis");

        if (p.StartsWith("stock-insight/", StringComparison.OrdinalIgnoreCase))
            return RewritePrefix(p, "stock-insight", "https://apipubaws.tcbs.com.vn/stock-insight");

        if (p.StartsWith("dnse-", StringComparison.OrdinalIgnoreCase))
            return $"https://services.entrade.com.vn/{p}";

        if (p.StartsWith("v4/", StringComparison.OrdinalIgnoreCase))
            return RewritePrefix(p, "v4", "https://api-finfo.vndirect.com.vn/v4");

        if (p.StartsWith("cg/", StringComparison.OrdinalIgnoreCase))
            return RewritePrefix(p, "cg", "https://api.coingecko.com");

        return $"{FlyBase}/{p}";
    }

    /// <summary>Absolute URL for JSON fetch (calendar, rates, proxy JSON).</summary>
    public string ResolveFetchUrl(string path)
    {
        var p = Normalize(path);

        if (p.Equals("ff_calendar_thisweek.json", StringComparison.OrdinalIgnoreCase))
            return _options.CalendarUrl;

        if (p.Equals("api/rates", StringComparison.OrdinalIgnoreCase))
            return _options.FxRatesUrl;

        if (Uri.TryCreate(ResolveProxyUrl(path), UriKind.Absolute, out var direct)
            && (direct.Scheme == Uri.UriSchemeHttps || direct.Host != new Uri(FlyBase).Host))
            return direct.ToString();

        return $"{FlyBase}/{p}";
    }

    private static string RewritePrefix(string path, string prefix, string targetBase)
    {
        var suffix = path[prefix.Length..].TrimStart('/');
        return string.IsNullOrEmpty(suffix)
            ? targetBase.TrimEnd('/')
            : $"{targetBase.TrimEnd('/')}/{suffix}";
    }

    private static string Normalize(string path) => path.Trim().TrimStart('/');
}
