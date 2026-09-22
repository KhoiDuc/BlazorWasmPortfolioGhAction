using System.Text.Json;
using System.Text.Json.Serialization;

namespace VnDesk;

public static class JsonStore
{
    public static readonly JsonSerializerOptions Pretty = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static readonly JsonSerializerOptions Read = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static T LoadOr<T>(string path, T fallback) where T : class
    {
        try
        {
            if (!File.Exists(path))
                return fallback;
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, Read) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    public static void Save(string path, object obj)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, JsonSerializer.Serialize(obj, Pretty));
    }
}
