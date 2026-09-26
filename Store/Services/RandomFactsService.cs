using BlazorWasmPortfolioGhAction.Shared.Model;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace BlazorWasmPortfolioGhAction.Store.Services
{
    public class RandomFactsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RandomFactsService> _logger;

        public RandomFactsService(HttpClient httpClient, ILogger<RandomFactsService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<RandomFact?> GetRandomFact()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<RandomFact>("https://uselessfacts.jsph.pl/api/v2/facts/random");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Random fact request failed");
                return null;
            }
        }
    }
}
