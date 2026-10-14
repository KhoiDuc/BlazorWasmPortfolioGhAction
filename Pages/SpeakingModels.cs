using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Pages;

public class SpeakingData
{
    [JsonPropertyName("topics")] public List<Topic> Topics { get; set; } = new();
    [JsonPropertyName("vocab")] public List<VocabItem> Vocab { get; set; } = new();
    [JsonPropertyName("source")] public string Source { get; set; } = "";
    [JsonPropertyName("personal")] public List<PersonalItem> Personal { get; set; } = new();
    [JsonPropertyName("personalQuick")] public List<PersonalQuickItem> PersonalQuick { get; set; } = new();
    [JsonPropertyName("frames")] public List<FrameItem> Frames { get; set; } = new();
}

public class Topic
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("en")] public string En { get; set; } = "";
    [JsonPropertyName("hook")] public string Hook { get; set; } = "";
    [JsonPropertyName("words")] public List<string> Words { get; set; } = new();
    [JsonPropertyName("quick")] public List<List<string>> Quick { get; set; } = new();
    [JsonPropertyName("cue")] public string Cue { get; set; } = "";
    [JsonPropertyName("points")] public List<string> Points { get; set; } = new();
    [JsonPropertyName("outline")] public string Outline { get; set; } = "";
    [JsonPropertyName("long")] public string Long { get; set; } = "";
    [JsonPropertyName("discuss")] public List<List<string>> Discuss { get; set; } = new();
    [JsonPropertyName("everyday")] public bool Everyday { get; set; }
}

public class VocabItem
{
    [JsonPropertyName("group")] public string Group { get; set; } = "";
    [JsonPropertyName("en")] public string En { get; set; } = "";
    [JsonPropertyName("kind")] public string Kind { get; set; } = "";
    [JsonPropertyName("vi")] public string Vi { get; set; } = "";
    [JsonPropertyName("ex")] public string Ex { get; set; } = "";
    [JsonPropertyName("priority")] public bool Priority { get; set; }
    [JsonPropertyName("extra")] public bool Extra { get; set; }
    [JsonPropertyName("common")] public bool Common { get; set; }
}

public class PersonalItem
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("group")] public string Group { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("question")] public string Question { get; set; } = "";
    [JsonPropertyName("outline")] public string Outline { get; set; } = "";
    [JsonPropertyName("short")] public string Short { get; set; } = "";
    [JsonPropertyName("long")] public string Long { get; set; } = "";
    [JsonPropertyName("vi")] public string Vi { get; set; } = "";
    [JsonPropertyName("focus")] public List<string> Focus { get; set; } = new();
    [JsonPropertyName("note")] public string Note { get; set; } = "";
}

public class PersonalQuickItem
{
    [JsonPropertyName("group")] public string Group { get; set; } = "";
    [JsonPropertyName("part")] public string Part { get; set; } = "";
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("q")] public string Q { get; set; } = "";
    [JsonPropertyName("hint")] public string Hint { get; set; } = "";
    [JsonPropertyName("answer")] public string Answer { get; set; } = "";
    [JsonPropertyName("sampleExperience")] public bool SampleExperience { get; set; }
}

public class FrameItem
{
    [JsonPropertyName("group")] public string Group { get; set; } = "";
    [JsonPropertyName("en")] public string En { get; set; } = "";
    [JsonPropertyName("vi")] public string Vi { get; set; } = "";
    [JsonPropertyName("ex")] public string Ex { get; set; } = "";
}