using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Shared.Model
{
    public class Flags
    {
        [JsonPropertyName("nsfw")]
        public bool Nsfw { get; set; }

        [JsonPropertyName("religious")]
        public bool Religious { get; set; }

        [JsonPropertyName("political")]
        public bool Political { get; set; }

        [JsonPropertyName("racist")]
        public bool Racist { get; set; }

        [JsonPropertyName("sexist")]
        public bool Sexist { get; set; }

        [JsonPropertyName("explicit")]
        public bool Explicit { get; set; }

        public override string ToString()
        {
            return $"Nsfw: {Nsfw}, Religious: {Religious}, Political: {Political}, Racist: {Racist}, Sexist: {Sexist}, Explicit: {Explicit}";
        }
    }

    public class Joke
    {
        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("setup")]
        public string? Setup { get; set; }

        [JsonPropertyName("delivery")]
        public string? Delivery { get; set; }

        [JsonPropertyName("flags")]
        public Flags? Flags { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("safe")]
        public bool Safe { get; set; }

        [JsonPropertyName("lang")]
        public string? Lang { get; set; }
    }

    public class JokeApiError
    {
        [JsonPropertyName("error")]
        [Required]
        public bool Error { get; set; }

        [JsonPropertyName("internalError")]
        public bool InternalError { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("causedBy")]
        public string[]? CausedBy { get; set; }

        [JsonPropertyName("additionalInfo")]
        public string? AdditionalInfo { get; set; }

        [JsonPropertyName("timeStamp")]
        public long TimeStamp { get; set; }
    }
}
