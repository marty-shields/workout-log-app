using System.Text.RegularExpressions;
using WorkoutLog.UI.Stories.Stories.DesignSystem;

namespace WorkoutLog.UI.Tests.Stories;

public partial class TokenSheetTests : ComponentTestBase
{
    private const string Module = "./Stories/DesignSystem/TokenSheet.razor.js";

    [Test]
    public void Render_ShowsEveryTokenInTokensCss()
    {
        Context.JSInterop.SetupModule(Module).Setup<Dictionary<string, string>>("read", _ => true).SetResult([]);

        var cut = Context.Render<TokenSheet>();

        var shown = cut.FindAll("[data-token]").Select(e => e.GetAttribute("data-token"));
        Assert.That(shown, Is.EquivalentTo(TokensInCss()));
    }

    [Test]
    public void Colours_ShowLightAndDarkValues()
    {
        Context.JSInterop.SetupModule(Module).Setup<Dictionary<string, string>>("read", _ => true)
            .SetResult(new() { ["--color-bar light"] = "#305252", ["--color-bar dark"] = "#305252" });

        var cut = Context.Render<TokenSheet>();

        var cells = cut.Find("[data-token=\"--color-bar\"]").QuerySelectorAll("td[data-theme]");
        Assert.Multiple(() =>
        {
            Assert.That(cells.Select(c => c.GetAttribute("data-theme")), Is.EqualTo(new[] { "light", "dark" }));
            Assert.That(cells.Select(c => c.QuerySelector(".value")!.TextContent), Is.EqualTo(new[] { "#305252", "#305252" }));
        });
    }

    private static IEnumerable<string> TokensInCss()
    {
        var css = RepoFiles.Read("src", "WorkoutLog.UI", "wwwroot", "css", "tokens.css");
        return TokenName().Matches(css).Select(m => m.Groups[1].Value);
    }

    [GeneratedRegex(@"^\s*(--[a-z0-9-]+):", RegexOptions.Multiline)]
    private static partial Regex TokenName();
}
