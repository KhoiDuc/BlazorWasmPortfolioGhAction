using BlazorWasmPortfolioGhAction.Shared.Model;

namespace BlazorWasmPortfolioGhAction.Store.FetchDataUseCase
{
    public record FetchDataAction();
    public record FetchDataSuccessAction(IEnumerable<WeatherForecast> Forecasts);
    public record FetchDataErrorAction(string Error);
}