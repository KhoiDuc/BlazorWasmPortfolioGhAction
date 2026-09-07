namespace BlazorWasmPortfolioGhAction.Services.Trading;

/// <summary>
/// External service URLs only (calendar, FX). Internal backend URLs (Fly, OSINT, RRG) removed.
/// </summary>
public class TradingApiOptions
{
    /// <summary>Economic calendar JSON.</summary>
    public string CalendarUrl { get; set; } = "https://nfs.faireconomy.media/ff_calendar_thisweek.json";

    /// <summary>Live FX rates JSON.</summary>
    public string FxRatesUrl { get; set; } = "https://live-rates.com/rates";
}