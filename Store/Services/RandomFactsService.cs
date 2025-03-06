using BlazorWasmPortfolioGhAction.Shared.Model;
using Newtonsoft.Json;

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
            // Fetch random fact from the API
            var response = await _httpClient.GetAsync("https://uselessfacts.jsph.pl/api/v2/facts/random");

            // Read the response content
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to fetch random fact");
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RandomFact>(content);
            }
        }
    }
}
