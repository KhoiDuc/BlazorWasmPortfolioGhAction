using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Pages;

public class DevLearningData
{
    [JsonPropertyName("domains")] public List<DevDomain> Domains { get; set; } = new();
    [JsonPropertyName("roadmaps")] public List<DevRoadmap> Roadmaps { get; set; } = new();
    [JsonPropertyName("defaultParameters")] public List<DevInteractiveParam> DefaultParameters { get; set; } = new();
    [JsonPropertyName("commands")] public List<DevCommandItem> Commands { get; set; } = new();
}

public class DevDomain
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("nameVi")] public string NameVi { get; set; } = "";
    [JsonPropertyName("icon")] public string Icon { get; set; } = "";
    [JsonPropertyName("color")] public string Color { get; set; } = "";
    [JsonPropertyName("badge")] public string Badge { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("descriptionVi")] public string DescriptionVi { get; set; } = "";
}

public class DevRoadmap
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("domainId")] public string DomainId { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("titleVi")] public string TitleVi { get; set; } = "";
    [JsonPropertyName("icon")] public string Icon { get; set; } = "";
    [JsonPropertyName("color")] public string Color { get; set; } = "";
    [JsonPropertyName("summary")] public string Summary { get; set; } = "";
    [JsonPropertyName("summaryVi")] public string SummaryVi { get; set; } = "";
    [JsonPropertyName("stages")] public List<DevRoadmapStage> Stages { get; set; } = new();
}

public class DevRoadmapStage
{
    [JsonPropertyName("level")] public string Level { get; set; } = ""; // Cơ bản, Trung cấp, Nâng cao
    [JsonPropertyName("levelEn")] public string LevelEn { get; set; } = ""; // Beginner, Intermediate, Advanced
    [JsonPropertyName("badge")] public string Badge { get; set; } = "";
    [JsonPropertyName("goal")] public string Goal { get; set; } = "";
    [JsonPropertyName("goalVi")] public string GoalVi { get; set; } = "";
    [JsonPropertyName("keyConcepts")] public List<string> KeyConcepts { get; set; } = new();
    [JsonPropertyName("keyConceptsVi")] public List<string> KeyConceptsVi { get; set; } = new();
    [JsonPropertyName("practicalTasks")] public List<string> PracticalTasks { get; set; } = new();
    [JsonPropertyName("practicalTasksVi")] public List<string> PracticalTasksVi { get; set; } = new();
    [JsonPropertyName("recommendedCommands")] public List<string> RecommendedCommands { get; set; } = new();
    [JsonPropertyName("commonGotchas")] public string CommonGotchas { get; set; } = "";
    [JsonPropertyName("commonGotchasVi")] public string CommonGotchasVi { get; set; } = "";
}

public class DevInteractiveParam
{
    [JsonPropertyName("key")] public string Key { get; set; } = "";
    [JsonPropertyName("label")] public string Label { get; set; } = "";
    [JsonPropertyName("labelVi")] public string LabelVi { get; set; } = "";
    [JsonPropertyName("defaultValue")] public string DefaultValue { get; set; } = "";
    [JsonPropertyName("placeholder")] public string Placeholder { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
}

public class DevCommandItem
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("domainId")] public string DomainId { get; set; } = "";
    [JsonPropertyName("subcategory")] public string Subcategory { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("titleVi")] public string TitleVi { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("descriptionVi")] public string DescriptionVi { get; set; } = "";
    [JsonPropertyName("rawCommand")] public string RawCommand { get; set; } = "";
    [JsonPropertyName("codeLanguage")] public string CodeLanguage { get; set; } = "bash";
    [JsonPropertyName("flags")] public List<DevFlagExpl> Flags { get; set; } = new();
    [JsonPropertyName("whenToUse")] public string WhenToUse { get; set; } = "";
    [JsonPropertyName("whenToUseVi")] public string WhenToUseVi { get; set; } = "";
    [JsonPropertyName("pitfalls")] public string Pitfalls { get; set; } = "";
    [JsonPropertyName("pitfallsVi")] public string PitfallsVi { get; set; } = "";
    [JsonPropertyName("level")] public string Level { get; set; } = "Daily Practical"; // Daily Practical, Production Tuning, Disaster Recovery, Senior Architecture
    [JsonPropertyName("tags")] public List<string> Tags { get; set; } = new();
}

public class DevFlagExpl
{
    [JsonPropertyName("flag")] public string Flag { get; set; } = "";
    [JsonPropertyName("explanation")] public string Explanation { get; set; } = "";
    [JsonPropertyName("explanationVi")] public string ExplanationVi { get; set; } = "";
}
