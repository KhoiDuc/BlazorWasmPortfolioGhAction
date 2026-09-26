using BlazorWasmPortfolioGhAction.Shared.Model;
using Fluxor;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace BlazorWasmPortfolioGhAction.Store.FetchDataUseCase
{
    public class Effects
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<Effects> _logger;

        public Effects(HttpClient httpClient, ILogger<Effects> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        [EffectMethod(typeof(FetchDataAction))]
        public async Task HandleAsync(IDispatcher dispatcher)
        {
            try
            {
                var forecasts = await _httpClient.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json");
                dispatcher.Dispatch(new FetchDataSuccessAction(forecasts ??= []));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load weather sample data");
                dispatcher.Dispatch(new FetchDataErrorAction(ex.Message));
            }
        }
    }
}
