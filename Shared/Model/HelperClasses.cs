using Blazor.Extensions.Canvas.WebGL;
using Newtonsoft.Json;
using QRCoder;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Shared.Model
{
    public enum EnumQrType
    {
        [Display(Name = "Normal Text")]
        Text,
        Url,
        [Display(Name = "Phone Number")]
        PhoneNumber
    }

    public enum ImageType
    {
        PNG,
        JPG,
        JPEG,

    }
    public class RandomFact
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("text")]
        public string text { get; set; }

        [JsonProperty("source")]
        public string source { get; set; }

        [JsonProperty("source_url")]
        public string sourceUrl { get; set; }

        [JsonProperty("language")]
        public string language { get; set; }

        [JsonProperty("permalink")]
        public string permaLink { get; set; }

    }
    public class QRCodeRequestModel
    {
        public string QRValue { get; set; } = "https://github.com/KhoiDuc";
        public EnumQrType QRType { get; set; } = EnumQrType.Url;
        public SvgQRCode.SvgLogo? Logo { get; set; }
        public string DarkColorHex { get; set; } = "#A9A9A9";
        public string WhiteColorHex { get; set; } = "#ffffff";
    }

    public class QRCodeResponseModel
    {
        public string? SvgString { get; set; }
        public byte[]? ByteData => SvgString != null ? Encoding.UTF8.GetBytes(SvgString) : null;
        public string? Base64String => ByteData is not null ? Convert.ToBase64String(ByteData) : null;
    }

    public class ProgramInfo
    {
        public WebGLProgram Program { get; set; }
        public UniformLocations UniformLocations { get; set; } = new UniformLocations();
        public AttribLocations AttribLocations { get; set; } = new AttribLocations();
    }

    public class UniformLocations
    {
        public WebGLUniformLocation ProjectionMatrix { get; set; }
        public WebGLUniformLocation ModelViewMatrix { get; set; }
    }

    public class AttribLocations
    {
        public int VertexPosition { get; set; }
        public int VertexColor { get; set; }
    }

    public class Buffers
    {
        public WebGLBuffer Position { get; set; }
        public WebGLBuffer Color { get; set; }
        public WebGLBuffer Indices { get; set; }
    }

    public class Camera
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; } = 1.0f;
        public float Rotation { get; set; }
        public float Zoom { get; set; } = 1.0f;
    }
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

        public void LogJokeDetails()
        {
            //Log.Information("Joke Error: {error}", Error);
            //Log.Information("Joke Category: {category}", Category);
            //Log.Information("Joke Type: {type}", Type);
            //Log.Information("Joke Setup: {setup}", Setup);
            //Log.Information("Joke Delivery: {delivery}", Delivery);
            //Log.Information("Joke Flags: {flags}", Flags);
            //Log.Information("Joke Id: {id}", Id);
            //Log.Information("Joke Safe: {safe}", Safe);
            //Log.Information("Joke Type: {type}", Type);
            //Log.Information("Joke Lang: {lang}", Lang);
        }
    }
    public class JokeApiError
    {
        [JsonPropertyName("error")]
        [Required] public bool Error { get; set; }

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

        public void LogJokeApiErrorDetails()
        {
            //Log.Information("Error Error: {error}", Error);
            //Log.Information("Error InternalError: {internalError}", InternalError);
            //Log.Information("Error Code: {code}", Code);
            //Log.Information("Error Message: {message}", Message);
            //Log.Information("Error CausedBy: {causedBy}", string.Join(", ", CausedBy!));
            //Log.Information("Error AdditionalInfo: {additionalInfo}", AdditionalInfo);
            //Log.Information("Error TimeStamp: {timeStamp}", TimeStamp);
        }
    }
}
