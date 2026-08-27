using System.Text.Json;

namespace BlazorWasmPortfolioGhAction.Utils.OnlineTools;

internal static class JsonUtils
{
    private static readonly JsonSerializerOptions IndentedSerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new DictionaryObjectConverter() },
    };

    public static string ToDisplayJson(object? instance) =>
        JsonSerializer.Serialize(instance, IndentedSerializerOptions);

    private sealed class DictionaryObjectConverter : System.Text.Json.Serialization.JsonConverter<Dictionary<object, object?>>
    {
        public override Dictionary<object, object?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotSupportedException();

        public override void Write(Utf8JsonWriter writer, Dictionary<object, object?> value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            foreach (var kvp in value)
            {
                writer.WritePropertyName($"{kvp.Key}");
                JsonSerializer.Serialize(writer, kvp.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}
