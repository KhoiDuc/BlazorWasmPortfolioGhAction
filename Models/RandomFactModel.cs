using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Shared.Model
{
    public class RandomFact
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string id { get; set; } = string.Empty;

        [JsonProperty("text")]
        [JsonPropertyName("text")]
        public string text { get; set; } = string.Empty;

        [JsonProperty("source")]
        [JsonPropertyName("source")]
        public string source { get; set; } = string.Empty;

        [JsonProperty("source_url")]
        [JsonPropertyName("source_url")]
        public string sourceUrl { get; set; } = string.Empty;

        [JsonProperty("language")]
        [JsonPropertyName("language")]
        public string language { get; set; } = string.Empty;

        [JsonProperty("permalink")]
        [JsonPropertyName("permalink")]
        public string permaLink { get; set; } = string.Empty;
    }
}
