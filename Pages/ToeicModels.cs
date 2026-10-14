using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Pages;

public class ToeicData
{
    [JsonPropertyName("modules")] public List<ToeicTopicModule> Modules { get; set; } = new();
    [JsonPropertyName("traps")] public List<ToeicTrap> Traps { get; set; } = new();
    [JsonPropertyName("quickTricks")] public List<ToeicQuickTrick> QuickTricks { get; set; } = new();
    [JsonPropertyName("vocabCategories")] public List<ToeicVocabCategory> VocabCategories { get; set; } = new();
    [JsonPropertyName("paraphrases")] public List<ToeicParaphraseGroup> Paraphrases { get; set; } = new();
    [JsonPropertyName("grammarTopics")] public List<ToeicGrammarTopic> GrammarTopics { get; set; } = new();
    [JsonPropertyName("speakingPrompts")] public List<ToeicSpeakingPrompt> SpeakingPrompts { get; set; } = new();
    [JsonPropertyName("writingTemplates")] public List<ToeicWritingTemplate> WritingTemplates { get; set; } = new();
}

public class ToeicTopicModule
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("num")] public string Num { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("titleVi")] public string TitleVi { get; set; } = "";
    [JsonPropertyName("category")] public string Category { get; set; } = ""; // Traps, Vocab, Grammar, Speaking, Writing, Skills
    [JsonPropertyName("skill")] public string Skill { get; set; } = ""; // Listening, Reading, Speaking, Writing, General
    [JsonPropertyName("icon")] public string Icon { get; set; } = "bi-journal-code";
    [JsonPropertyName("accent")] public string Accent { get; set; } = "gold"; // gold, blue, emerald, purple, crimson
    [JsonPropertyName("summary")] public string Summary { get; set; } = "";
    [JsonPropertyName("highlights")] public List<string> Highlights { get; set; } = new();
    [JsonPropertyName("keyPoints")] public List<string> KeyPoints { get; set; } = new();
    [JsonPropertyName("cheatItems")] public List<ToeicCheatEntry> CheatItems { get; set; } = new();
    [JsonPropertyName("tags")] public List<string> Tags { get; set; } = new();
}

public class ToeicCheatEntry
{
    [JsonPropertyName("term")] public string Term { get; set; } = "";
    [JsonPropertyName("meaning")] public string Meaning { get; set; } = "";
    [JsonPropertyName("formulaOrExample")] public string FormulaOrExample { get; set; } = "";
}

public class ToeicTrap
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("part")] public string Part { get; set; } = ""; // Part 1 .. Part 7
    [JsonPropertyName("skill")] public string Skill { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("dangerLevel")] public string DangerLevel { get; set; } = "High"; // Extreme, High, Medium
    [JsonPropertyName("exampleQuestion")] public string ExampleQuestion { get; set; } = "";
    [JsonPropertyName("trapChoice")] public string TrapChoice { get; set; } = "";
    [JsonPropertyName("correctChoice")] public string CorrectChoice { get; set; } = "";
    [JsonPropertyName("explanation")] public string Explanation { get; set; } = "";
    [JsonPropertyName("proTip")] public string ProTip { get; set; } = "";
}

public class ToeicQuickTrick
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("part")] public string Part { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("rule")] public string Rule { get; set; } = "";
    [JsonPropertyName("example")] public string Example { get; set; } = "";
    [JsonPropertyName("timeSaveSeconds")] public int TimeSaveSeconds { get; set; } = 15;
}

public class ToeicVocabCategory
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("nameVi")] public string NameVi { get; set; } = "";
    [JsonPropertyName("icon")] public string Icon { get; set; } = "bi-folder2";
    [JsonPropertyName("items")] public List<ToeicWordItem> Items { get; set; } = new();
}

public class ToeicWordItem
{
    [JsonPropertyName("en")] public string En { get; set; } = "";
    [JsonPropertyName("pos")] public string Pos { get; set; } = ""; // n, v, adj, adv, phr
    [JsonPropertyName("ipa")] public string Ipa { get; set; } = "";
    [JsonPropertyName("vi")] public string Vi { get; set; } = "";
    [JsonPropertyName("collocation")] public string Collocation { get; set; } = "";
    [JsonPropertyName("example")] public string Example { get; set; } = "";
    [JsonPropertyName("isMustKnow")] public bool IsMustKnow { get; set; } = true;
}

public class ToeicParaphraseGroup
{
    [JsonPropertyName("type")] public string Type { get; set; } = ""; // Verbs, Nouns, Collocations
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("pairs")] public List<ToeicParaphraseItem> Pairs { get; set; } = new();
}

public class ToeicParaphraseItem
{
    [JsonPropertyName("original")] public string Original { get; set; } = "";
    [JsonPropertyName("paraphrases")] public List<string> Paraphrases { get; set; } = new();
    [JsonPropertyName("vi")] public string Vi { get; set; } = "";
    [JsonPropertyName("context")] public string Context { get; set; } = "";
}

public class ToeicGrammarTopic
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("formula")] public string Formula { get; set; } = "";
    [JsonPropertyName("rules")] public List<string> Rules { get; set; } = new();
    [JsonPropertyName("cheatTable")] public List<ToeicGrammarRow> CheatTable { get; set; } = new();
    [JsonPropertyName("miniQuiz")] public List<ToeicMiniQuiz> MiniQuiz { get; set; } = new();
}

public class ToeicGrammarRow
{
    [JsonPropertyName("label")] public string Label { get; set; } = "";
    [JsonPropertyName("patterns")] public string Patterns { get; set; } = "";
    [JsonPropertyName("examples")] public string Examples { get; set; } = "";
    [JsonPropertyName("note")] public string Note { get; set; } = "";
}

public class ToeicMiniQuiz
{
    [JsonPropertyName("question")] public string Question { get; set; } = "";
    [JsonPropertyName("options")] public List<string> Options { get; set; } = new();
    [JsonPropertyName("correctIndex")] public int CorrectIndex { get; set; }
    [JsonPropertyName("explanation")] public string Explanation { get; set; } = "";
}

public class ToeicSpeakingPrompt
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("part")] public string Part { get; set; } = ""; // Part 2, Part 3, Part 4, Part 5
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("promptText")] public string PromptText { get; set; } = "";
    [JsonPropertyName("timePrepSeconds")] public int TimePrepSeconds { get; set; } = 45;
    [JsonPropertyName("timeResponseSeconds")] public int TimeResponseSeconds { get; set; } = 30;
    [JsonPropertyName("blueprint")] public List<string> Blueprint { get; set; } = new();
    [JsonPropertyName("sampleAnswer")] public string SampleAnswer { get; set; } = "";
    [JsonPropertyName("highScorePhrases")] public List<string> HighScorePhrases { get; set; } = new();
    [JsonPropertyName("tips")] public string Tips { get; set; } = "";
}

public class ToeicWritingTemplate
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("task")] public string Task { get; set; } = ""; // Task 1 (Picture), Task 2 (Email), Task 3 (Essay)
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("scenario")] public string Scenario { get; set; } = "";
    [JsonPropertyName("requiredPoints")] public List<string> RequiredPoints { get; set; } = new();
    [JsonPropertyName("structureBlueprint")] public List<string> StructureBlueprint { get; set; } = new();
    [JsonPropertyName("sampleFullResponse")] public string SampleFullResponse { get; set; } = "";
    [JsonPropertyName("connectors")] public List<string> Connectors { get; set; } = new();
    [JsonPropertyName("commonMistakes")] public List<string> CommonMistakes { get; set; } = new();
}
