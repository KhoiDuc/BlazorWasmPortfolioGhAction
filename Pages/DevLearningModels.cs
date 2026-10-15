using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Pages;

public class DevLearningData
{
    [JsonPropertyName("domains")] public List<DevDomain> Domains { get; set; } = new();
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

public class DevInteractiveParam
{
    [JsonPropertyName("key")] public string Key { get; set; } = "";
    [JsonPropertyName("label")] public string Label { get; set; } = "";
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
