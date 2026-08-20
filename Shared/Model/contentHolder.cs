using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Shared.Model
{
    public class ContentHolder
    {
        [JsonPropertyName("page")]
        [Newtonsoft.Json.JsonProperty("page")]
        public string? Page { get; set; }

        [JsonPropertyName("section")]
        [Newtonsoft.Json.JsonProperty("section")]
        public string? Section { get; set; }

        [JsonPropertyName("subSection")]
        [Newtonsoft.Json.JsonProperty("subSection")]
        public string? SubSection { get; set; }

        [JsonPropertyName("content")]
        [Newtonsoft.Json.JsonProperty("content")]
        public string? Content { get; set; }

        [JsonPropertyName("id")]
        [Newtonsoft.Json.JsonProperty("id")]
        public int Id { get; set; }

        // Backwards compatibility getters/setters for legacy code
        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string? page { => Page; set => Page = value; }

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string? section { => Section; set => Section = value; }

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string? subSection { => SubSection; set => SubSection = value; }

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string? content { => Content; set => Content = value; }

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public int id { => Id; set => Id = value; }
    }
}
