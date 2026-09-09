using System.Text;
using System.Text.Encodings.Web;
using Meziantou.Framework;
using Microsoft.AspNetCore.Components;

namespace BlazorWasmPortfolioGhAction.Utils;

/// <summary>
/// Diff rendering helpers. Extracted from <c>Components/OnlineTools/TextComparison.razor</c>.
/// </summary>
public static class DiffMarkupUtils
{
    public static DiffRenderResult ComputeDiffMarkup(string leftText, string rightText, TextDiffOptions options, DiffChunkMode chunkMode)
    {
        if (chunkMode == DiffChunkMode.LineAndWord)
            return ComputeLineAndWordDiffMarkup(leftText, rightText, options);

        var normalizedOptions = CloneOptions(options);
        normalizedOptions.Chunker = chunkMode switch
        {
            DiffChunkMode.Word => TextChunker.Words,
            DiffChunkMode.Character => TextChunker.Characters,
            _ => TextChunker.Lines,
        };

        var result = TextDiff.ComputeDiff(leftText, rightText, normalizedOptions);
        return new DiffRenderResult(RenderEntries(result.Entries), result.HasDifferences);
    }

    public static DiffRenderResult ComputeLineAndWordDiffMarkup(string leftText, string rightText, TextDiffOptions options)
    {
        var lineOptions = CloneOptions(options);
        lineOptions.Chunker = TextChunker.Lines;

        var lineResult = TextDiff.ComputeDiff(leftText, rightText, lineOptions);
        var sb = new StringBuilder();
        var entries = lineResult.Entries;

        for (var index = 0; index < entries.Count;)
        {
            var entry = entries[index];
            if (entry.Operation == TextDiffOperation.Delete)
            {
                var deletedLines = new List<string>();
                while (index < entries.Count && entries[index].Operation == TextDiffOperation.Delete)
                {
                    deletedLines.Add(entries[index].Text);
                    index++;
                }

                var insertedLines = new List<string>();
                while (index < entries.Count && entries[index].Operation == TextDiffOperation.Insert)
                {
                    insertedLines.Add(entries[index].Text);
                    index++;
                }

                if (insertedLines.Count == 0)
                {
                    foreach (var deletedLine in deletedLines)
                        AppendEntry(sb, TextDiffOperation.Delete, deletedLine, "diff-line");

                    continue;
                }

                var pairedLineCount = Math.Min(deletedLines.Count, insertedLines.Count);
                for (var pairIndex = 0; pairIndex < pairedLineCount; pairIndex++)
                    AppendWordLevelLineDiff(sb, deletedLines[pairIndex], insertedLines[pairIndex], options);

                for (var i = pairedLineCount; i < deletedLines.Count; i++)
                    AppendEntry(sb, TextDiffOperation.Delete, deletedLines[i], "diff-line");

                for (var i = pairedLineCount; i < insertedLines.Count; i++)
                    AppendEntry(sb, TextDiffOperation.Insert, insertedLines[i], "diff-line");

                continue;
            }

            if (entry.Operation == TextDiffOperation.Insert)
            {
                AppendEntry(sb, TextDiffOperation.Insert, entry.Text, "diff-line");
                index++;
                continue;
            }

            sb.Append(HtmlEncoder.Default.Encode(entry.Text));
            index++;
        }

        return new DiffRenderResult(new MarkupString(sb.ToString()), lineResult.HasDifferences);
    }

    public static void AppendWordLevelLineDiff(StringBuilder sb, string deletedLine, string insertedLine, TextDiffOptions options)
    {
        var wordOptions = CloneOptions(options);
        wordOptions.Chunker = TextChunker.Words;

        var wordResult = TextDiff.ComputeDiff(deletedLine, insertedLine, wordOptions);
        var deletedBuilder = new StringBuilder();
        var insertedBuilder = new StringBuilder();

        foreach (var entry in wordResult.Entries)
        {
            var encodedText = HtmlEncoder.Default.Encode(entry.Text);
            switch (entry.Operation)
            {
                case TextDiffOperation.Equal:
                    deletedBuilder.Append(encodedText);
                    insertedBuilder.Append(encodedText);
                    break;
                case TextDiffOperation.Delete:
                    deletedBuilder.Append("<del class=\"diff-word\">").Append(encodedText).Append("</del>");
                    break;
                case TextDiffOperation.Insert:
                    insertedBuilder.Append("<ins class=\"diff-word\">").Append(encodedText).Append("</ins>");
                    break;
            }
        }

        sb.Append("<del class=\"diff-line\">").Append(deletedBuilder).Append("</del>");
        sb.Append("<ins class=\"diff-line\">").Append(insertedBuilder).Append("</ins>");
    }

    public static MarkupString RenderEntries(IReadOnlyList<TextDiffEntry> entries)
    {
        var sb = new StringBuilder();
        foreach (var entry in entries)
            AppendEntry(sb, entry.Operation, entry.Text);

        return new MarkupString(sb.ToString());
    }

    public static void AppendEntry(StringBuilder sb, TextDiffOperation operation, string text, string? cssClass = null)
    {
        var encodedText = HtmlEncoder.Default.Encode(text);
        switch (operation)
        {
            case TextDiffOperation.Equal:
                sb.Append(encodedText);
                return;
            case TextDiffOperation.Delete:
                sb.Append("<del");
                break;
            case TextDiffOperation.Insert:
                sb.Append("<ins");
                break;
        }

        if (!string.IsNullOrEmpty(cssClass))
            sb.Append(" class=\"").Append(cssClass).Append('"');

        sb.Append('>');
        sb.Append(encodedText);
        sb.Append(operation == TextDiffOperation.Delete ? "</del>" : "</ins>");
    }

    public static TextDiffOptions CloneOptions(TextDiffOptions options) => new()
    {
        Algorithm = options.Algorithm,
        IgnoreCase = options.IgnoreCase,
        IgnoreWhitespace = options.IgnoreWhitespace,
        IgnoreEndOfLine = options.IgnoreEndOfLine,
    };

    public static TextDiffAlgorithm ParseAlgorithm(string? value) =>
        Enum.TryParse<TextDiffAlgorithm>(value, ignoreCase: true, out var result) ? result : TextDiffAlgorithm.Myers;

    public static DiffChunkMode ParseChunkMode(string? value) => value switch
    {
        "word" => DiffChunkMode.Word,
        "character" => DiffChunkMode.Character,
        "line-word" => DiffChunkMode.LineAndWord,
        _ => DiffChunkMode.Line,
    };

    public static string ToQueryValue(DiffChunkMode mode) => mode switch
    {
        DiffChunkMode.Word => "word",
        DiffChunkMode.Character => "character",
        DiffChunkMode.LineAndWord => "line-word",
        _ => "line",
    };

    public static bool ToBoolean(object? value)
    {
        if (value is bool boolValue)
            return boolValue;

        if (value is string text && bool.TryParse(text, out boolValue))
            return boolValue;

        return false;
    }
}

public enum DiffChunkMode { Line, Word, Character, LineAndWord }

public readonly record struct DiffRenderResult(MarkupString Markup, bool HasDifferences);