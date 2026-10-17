using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Pages;

public class StringOrIntJsonConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out long l))
                return $"{l} phút đọc";
            if (reader.TryGetDouble(out double d))
                return $"{d} phút đọc";
        }
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString() ?? "";
        }
        return "";
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

public class DevBlogData
{
    [JsonPropertyName("seriesList")] public List<DevSeries> SeriesList { get; set; } = new();
}

public class DevSeries
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("category")] public string Category { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("titleVi")] public string TitleVi { get; set; } = "";
    [JsonPropertyName("icon")] public string Icon { get; set; } = "";
    [JsonPropertyName("color")] public string Color { get; set; } = "";
    [JsonPropertyName("badge")] public string Badge { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("descriptionVi")] public string DescriptionVi { get; set; } = "";
    [JsonPropertyName("articles")] public List<DevArticle> Articles { get; set; } = new();
}

public class DevArticle
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("seriesId")] public string SeriesId { get; set; } = "";
    [JsonPropertyName("lessonNumber")] public string LessonNumber { get; set; } = ""; // e.g. "Bài 1", "Bài 2"
    [JsonPropertyName("level")] public string Level { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced, Production
    [JsonPropertyName("levelVi")] public string LevelVi { get; set; } = "Cơ bản"; // Cơ bản, Trung cấp, Nâng cao, Thực chiến
    [JsonPropertyName("levelOrder")] public int LevelOrder { get; set; } = 1; // 1: Cơ bản, 2: Trung cấp, 3: Nâng cao, 4: Thực chiến
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("titleVi")] public string TitleVi { get; set; } = "";
    [JsonConverter(typeof(StringOrIntJsonConverter))]
    [JsonPropertyName("readingTime")] public string ReadingTime { get; set; } = "10 phút đọc";
    [JsonPropertyName("publishedDate")] public string PublishedDate { get; set; } = "15/10/2026";
    [JsonPropertyName("summary")] public string Summary { get; set; } = "";
    [JsonPropertyName("summaryVi")] public string SummaryVi { get; set; } = "";
    [JsonPropertyName("tags")] public List<string> Tags { get; set; } = new();
    [JsonPropertyName("sections")] public List<DevArticleSection> Sections { get; set; } = new();
    [JsonPropertyName("content")] public string Content { get; set; } = "";
}

public class DevArticleSection
{
    [JsonPropertyName("heading")] public string Heading { get; set; } = "";
    [JsonPropertyName("headingVi")] public string HeadingVi { get; set; } = "";
    [JsonPropertyName("paragraphs")] public List<string> Paragraphs { get; set; } = new();
    [JsonPropertyName("paragraphsVi")] public List<string> ParagraphsVi { get; set; } = new();
    [JsonPropertyName("codeTitle")] public string CodeTitle { get; set; } = "";
    [JsonPropertyName("codeLanguage")] public string CodeLanguage { get; set; } = "bash";
    [JsonPropertyName("codeContent")] public string CodeContent { get; set; } = "";
    [JsonPropertyName("calloutType")] public string CalloutType { get; set; } = ""; // tip, warning, info
    [JsonPropertyName("calloutTitle")] public string CalloutTitle { get; set; } = "";
    [JsonPropertyName("calloutTitleVi")] public string CalloutTitleVi { get; set; } = "";
    [JsonPropertyName("calloutText")] public string CalloutText { get; set; } = "";
    [JsonPropertyName("calloutTextVi")] public string CalloutTextVi { get; set; } = "";
}
