﻿namespace BlazorWasmPortfolioGhAction.Pages.CryptoManagerComponents
{
    public class Settings
    {
        public int NumberOfTickers { get; set; } = 10;
        public string QuoteUnit { get; set; } = "USDT"; // Default to USDT
        public string SortBy { get; set; } = "MarketCap"; // Default to MarketCap
    }
}
