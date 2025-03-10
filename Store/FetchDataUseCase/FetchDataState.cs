﻿using BlazorWasmPortfolioGhAction.Shared.Model;
using Fluxor;

namespace BlazorWasmPortfolioGhAction.Store.FetchDataUseCase
{
    [FeatureState]
    public record FetchDataState
    {
        public bool IsLoading { get; init; }
        public IEnumerable<WeatherForecast>? Forecasts { get; init; }
        public string? Error { get; init; }

        private FetchDataState() { }
        public FetchDataState(bool isLoading, IEnumerable<WeatherForecast>? forecasts, string? error)
        {
            IsLoading = isLoading;
            Forecasts = forecasts;
            Error = error;
        }
    }
}
