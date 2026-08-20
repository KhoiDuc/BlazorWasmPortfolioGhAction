using BlazorWasmPortfolioGhAction.Shared.Model;
using System.Net.Http.Json;

namespace BlazorWasmPortfolioGhAction.Store.Services
{
    public class RandomFactsService
    {
        private readonly HttpClient _httpClient;

        public RandomFactsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RandomFact?> GetRandomFact()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<RandomFact>("https://uselessfacts.jsph.pl/api/v2/facts/random");
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
