using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorWasmPortfolioGhAction.Pages;
using Xunit;

namespace BlazorWasmPortfolioGhAction.Tests;

public class ToeicJsonTest
{
    private static ToeicData LoadToeicData()
    {
        var path = Path.GetFullPath("../../../../../wwwroot/sample-data/toeic.json");
        Assert.True(File.Exists(path), $"File not found at: {path}");
        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<ToeicData>(json, options);
        Assert.NotNull(data);
        return data;
    }

    [Fact]
    public void TestToeicJsonDeserialization()
    {
        var data = LoadToeicData();
        Assert.NotEmpty(data.VocabCategories);
        Assert.NotEmpty(data.ConfusingWords);
        Assert.NotEmpty(data.PhrasalVerbs);
        Assert.NotEmpty(data.Paraphrases);
    }

    [Fact]
    public void TestVocabCategoriesCountAndItemsPerCategory()
    {
        var data = LoadToeicData();
        Assert.Equal(41, data.VocabCategories.Count);

        foreach (var category in data.VocabCategories)
        {
            Assert.False(string.IsNullOrWhiteSpace(category.Id), "Category ID must not be empty");
            Assert.False(string.IsNullOrWhiteSpace(category.Name), $"Category {category.Id} Name must not be empty");
            Assert.False(string.IsNullOrWhiteSpace(category.NameVi), $"Category {category.Id} NameVi must not be empty");
            Assert.Equal(50, category.Items.Count);
        }
    }

    [Fact]
    public void TestAllWordsHaveRequiredFieldsAndNoUntrimmedWhitespace()
    {
        var data = LoadToeicData();

        foreach (var category in data.VocabCategories)
        {
            foreach (var item in category.Items)
            {
                Assert.False(string.IsNullOrWhiteSpace(item.En), $"{category.Id}: En must not be empty");
                Assert.Equal(item.En.Trim(), item.En);

                Assert.False(string.IsNullOrWhiteSpace(item.Pos), $"{category.Id}: {item.En} Pos must not be empty");
                Assert.Equal(item.Pos.Trim(), item.Pos);

                Assert.False(string.IsNullOrWhiteSpace(item.Ipa), $"{category.Id}: {item.En} Ipa must not be empty");
                Assert.Equal(item.Ipa.Trim(), item.Ipa);
                Assert.Matches(@"^/.+/$", item.Ipa);

                Assert.False(string.IsNullOrWhiteSpace(item.Vi), $"{category.Id}: {item.En} Vi must not be empty");
                Assert.Equal(item.Vi.Trim(), item.Vi);

                Assert.False(string.IsNullOrWhiteSpace(item.Collocation), $"{category.Id}: {item.En} Collocation must not be empty");
                Assert.Equal(item.Collocation.Trim(), item.Collocation);

                Assert.False(string.IsNullOrWhiteSpace(item.Example), $"{category.Id}: {item.En} Example must not be empty");
                Assert.Equal(item.Example.Trim(), item.Example);
            }
        }
    }

    [Fact]
    public void TestNoDuplicateWordsWithinSameCategory()
    {
        var data = LoadToeicData();

        foreach (var category in data.VocabCategories)
        {
            var seen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var item in category.Items)
            {
                var en = item.En.Trim();
                Assert.True(seen.Add(en), $"Duplicate word '{en}' found in category {category.Id}");
            }
        }
    }

    [Fact]
    public void TestNoDuplicateVietnameseMeaningsWithinSameCategory()
    {
        var data = LoadToeicData();

        foreach (var category in data.VocabCategories)
        {
            var seenVi = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var item in category.Items)
            {
                var vi = item.Vi.Trim();
                Assert.True(seenVi.Add(vi), $"Duplicate Vietnamese translation '{vi}' for word '{item.En}' in category {category.Id}");
            }
        }
    }

    [Fact]
    public void TestNoForeignLeakedCharactersInVietnamese()
    {
        var data = LoadToeicData();
        var cjkOrCyrillicRegex = new Regex(@"[\p{IsCJKUnifiedIdeographs}\p{IsCyrillic}]", RegexOptions.Compiled);

        foreach (var category in data.VocabCategories)
        {
            foreach (var item in category.Items)
            {
                Assert.DoesNotMatch(cjkOrCyrillicRegex, item.Vi);
            }
        }
    }

    [Fact]
    public void TestAllWordsHaveWordFamily()
    {
        var data = LoadToeicData();

        foreach (var category in data.VocabCategories)
        {
            foreach (var item in category.Items)
            {
                Assert.NotNull(item.WordFamily);
                var hasAnySlot = !string.IsNullOrWhiteSpace(item.WordFamily.V) ||
                                 !string.IsNullOrWhiteSpace(item.WordFamily.N) ||
                                 !string.IsNullOrWhiteSpace(item.WordFamily.Adj) ||
                                 !string.IsNullOrWhiteSpace(item.WordFamily.Adv);
                Assert.True(hasAnySlot, $"{category.Id}: {item.En} must have at least one wordFamily slot");
            }
        }
    }
}
