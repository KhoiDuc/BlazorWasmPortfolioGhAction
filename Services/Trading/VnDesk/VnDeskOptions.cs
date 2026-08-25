namespace BlazorWasmPortfolioGhAction.Services.Trading.VnDesk;

public sealed class VnDeskOptions
{
    public string VndirectBaseUrl { get; set; } = "https://api-finfo.vndirect.com.vn";
    public string CafefIndexUrl { get; set; } = "https://banggia.cafef.vn/stockhandler.ashx?index=true";
    public string CafefAllStocksUrl { get; set; } = "https://banggia.cafef.vn/stockhandler.ashx?allstocks=true";
    public int HistorySessions { get; set; } = 250;
    public int ScreenerDays { get; set; } = 60;
}
