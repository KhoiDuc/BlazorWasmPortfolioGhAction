using System.IO;
using System.Text.Json;
using BlazorWasmPortfolioGhAction.Pages;
using Xunit;

namespace BlazorWasmPortfolioGhAction.Tests;

public class DevLearningJsonTest
{
    [Fact]
    public void TestDevLearningJsonDeserialization()
    {
        var path = Path.GetFullPath("../../../../../wwwroot/sample-data/dev-learning.json");
        Assert.True(File.Exists(path), $"File not found at: {path}");
        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<DevBlogData>(json, options);
        Assert.NotNull(data);
        Assert.NotEmpty(data.SeriesList);
    }

    [Theory]
    [InlineData(@"Dev $\rightarrow$ Staging", "Dev → Staging")]
    [InlineData(@"Dev $\\rightarrow$ Staging", "Dev → Staging")]
    [InlineData(@"Dev \rightarrow Staging", "Dev → Staging")]
    [InlineData(@"$\rightarrow$", "→")]
    [InlineData(@"$\\rightarrow$", "→")]
    public void TestCleanText(string input, string expected)
    {
        var cleaned = CleanText(input);
        Assert.Equal(expected, cleaned);
    }

    public static string CleanText(string? text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        var res = System.Text.RegularExpressions.Regex.Replace(text, @"\$?\\*rightarrow\$?", "→");
        res = System.Text.RegularExpressions.Regex.Replace(res, @"\$?\\*leftarrow\$?", "←");
        return res;
    }
}
