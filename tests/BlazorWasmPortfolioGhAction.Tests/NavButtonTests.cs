using Bunit;
using Xunit;

namespace BlazorWasmPortfolioGhAction.Tests;

public class NavButtonTests : TestContext
{
    [Fact]
    public void Language_toggle_exposes_an_accessible_name()
    {
        var cut = Render(builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "type", "button");
            builder.AddAttribute(2, "aria-label", "Language");
            builder.AddContent(3, "VI");
            builder.CloseElement();
        });

        Assert.Equal("Language", cut.Find("button").GetAttribute("aria-label"));
    }
}
