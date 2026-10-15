using System.Text.Json.Serialization;

namespace BlazorWasmPortfolioGhAction.Pages;

public class ToeicData
{
    [JsonPropertyName("examParts")] public List<ToeicExamPart> ExamParts { get; set; } = new();
    [JsonPropertyName("timeTimeline")] public List<ToeicTimeBlock> TimeTimeline { get; set; } = new();
    [JsonPropertyName("scoreTiers")] public List<ToeicScoreTier> ScoreTiers { get; set; } = new();
    [JsonPropertyName("accentGuides")] public List<ToeicAccentGuide> AccentGuides { get; set; } = new();
    [JsonPropertyName("modules")] public List<ToeicTopicModule> Modules { get; set; } = new();
    [JsonPropertyName("traps")] public List<ToeicTrap> Traps { get; set; } = new();
    [JsonPropertyName("quickTricks")] public List<ToeicQuickTrick> QuickTricks { get; set; } = new();
    [JsonPropertyName("vocabCategories")] public List<ToeicVocabCategory> VocabCategories { get; set; } = new();
    [JsonPropertyName("paraphrases")] public List<ToeicParaphraseGroup> Paraphrases { get; set; } = new();
    [JsonPropertyName("confusingWords")] public List<ToeicConfusingWordPair> ConfusingWords { get; set; } = new();
    [JsonPropertyName("phrasalVerbs")] public List<ToeicPhrasalVerb> PhrasalVerbs { get; set; } = new();
    [JsonPropertyName("grammarTopics")] public List<ToeicGrammarTopic> GrammarTopics { get; set; } = new();
    [JsonPropertyName("speakingPrompts")] public List<ToeicSpeakingPrompt> SpeakingPrompts { get; set; } = new();
    [JsonPropertyName("writingTemplates")] public List<ToeicWritingTemplate> WritingTemplates { get; set; } = new();
    [JsonPropertyName("examTips")] public List<ToeicExamTipGuide> ExamTips { get; set; } = new();
}

public class ToeicExamPart
{
    [JsonPropertyName("part")] public string Part { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("skill")] public string Skill { get; set; } = ""; // Listening, Reading
    [JsonPropertyName("questionsCount")] public int QuestionsCount { get; set; }
    [JsonPropertyName("durationMinutes")] public string DurationMinutes { get; set; } = "";
    [JsonPropertyName("difficulty")] public string Difficulty { get; set; } = "";
    [JsonPropertyName("formatDescription")] public string FormatDescription { get; set; } = "";
    [JsonPropertyName("scoreTip")] public string ScoreTip { get; set; } = "";
    [JsonPropertyName("targetScoreStrategy")] public string TargetScoreStrategy { get; set; } = "";
}

public class ToeicTimeBlock
{
    [JsonPropertyName("phase")] public string Phase { get; set; } = "";
    [JsonPropertyName("timeWindow")] public string TimeWindow { get; set; } = "";
    [JsonPropertyName("part")] public string Part { get; set; } = "";
    [JsonPropertyName("strategy")] public string Strategy { get; set; } = "";
    [JsonPropertyName("timePerQuestion")] public string TimePerQuestion { get; set; } = "";
}

public class ToeicScoreTier
{
    [JsonPropertyName("range")] public string Range { get; set; } = "";
    [JsonPropertyName("levelName")] public string LevelName { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("listeningScore")] public string ListeningScore { get; set; } = "";
    [JsonPropertyName("readingScore")] public string ReadingScore { get; set; } = "";
    [JsonPropertyName("careerBenchmark")] public string CareerBenchmark { get; set; } = "";
}

public class ToeicAccentGuide
{
    [JsonPropertyName("accent")] public string Accent { get; set; } = "";
    [JsonPropertyName("flag")] public string Flag { get; set; } = "";
    [JsonPropertyName("characteristics")] public string Characteristics { get; set; } = "";
    [JsonPropertyName("commonExamples")] public string CommonExamples { get; set; } = "";
    [JsonPropertyName("trapAlert")] public string TrapAlert { get; set; } = "";
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

public class ToeicConfusingWordPair
{
    [JsonPropertyName("pairTitle")] public string PairTitle { get; set; } = "";
    [JsonPropertyName("word1")] public string Word1 { get; set; } = "";
    [JsonPropertyName("pos1")] public string Pos1 { get; set; } = "";
    [JsonPropertyName("meaning1")] public string Meaning1 { get; set; } = "";
    [JsonPropertyName("example1")] public string Example1 { get; set; } = "";
    [JsonPropertyName("word2")] public string Word2 { get; set; } = "";
    [JsonPropertyName("pos2")] public string Pos2 { get; set; } = "";
    [JsonPropertyName("meaning2")] public string Meaning2 { get; set; } = "";
    [JsonPropertyName("example2")] public string Example2 { get; set; } = "";
    [JsonPropertyName("distinctionTip")] public string DistinctionTip { get; set; } = "";
}

public class ToeicPhrasalVerb
{
    [JsonPropertyName("verb")] public string Verb { get; set; } = "";
    [JsonPropertyName("meaningVi")] public string MeaningVi { get; set; } = "";
    [JsonPropertyName("synonym")] public string Synonym { get; set; } = "";
    [JsonPropertyName("example")] public string Example { get; set; } = "";
    [JsonPropertyName("contextNote")] public string ContextNote { get; set; } = "";
}

public class ToeicExamTipGuide
{
    [JsonPropertyName("phase")] public string Phase { get; set; } = ""; // Trước Ngày Thi, Trong Phòng Thi, Kỹ Năng Tô Đáp Án
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("category")] public string Category { get; set; } = "";
    [JsonPropertyName("icon")] public string Icon { get; set; } = "bi-check2-circle";
    [JsonPropertyName("doList")] public List<string> DoList { get; set; } = new();
    [JsonPropertyName("dontList")] public List<string> DontList { get; set; } = new();
    [JsonPropertyName("proSecret")] public string ProSecret { get; set; } = "";
}

