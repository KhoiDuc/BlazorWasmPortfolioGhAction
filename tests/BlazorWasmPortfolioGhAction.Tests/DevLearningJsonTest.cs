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
    [InlineData("Dev $ ightarrow$ Staging", "Dev → Staging")]
    [InlineData("Dev $\r\nightarrow$ Staging", "Dev → Staging")]
    [InlineData("Dev $\nightarrow$ Staging", "Dev → Staging")]
    public void TestCleanText(string input, string expected)
    {
        var cleaned = CleanText(input);
        Assert.Equal(expected, cleaned);
    }

    public static string CleanText(string? text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        var res = System.Text.RegularExpressions.Regex.Replace(text, @"\$[^$\w]*?\\*r?ightarrow[^$\w]*?\$|\\+r?ightarrow|[\r\n\t]+ightarrow", "→", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        res = System.Text.RegularExpressions.Regex.Replace(res, @"\$[^$\w]*?\\*l?eftarrow[^$\w]*?\$|\\+l?eftarrow|[\r\n\t]+eftarrow", "←", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        res = System.Text.RegularExpressions.Regex.Replace(res, @"\$[^$\w]*?\\+to[^$\w]*?\$|\\+to", "→", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return res;
    }
}
