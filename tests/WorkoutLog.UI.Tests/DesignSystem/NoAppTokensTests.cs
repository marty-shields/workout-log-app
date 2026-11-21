namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>This repo declares no custom properties: every token comes from the Shields.UI kit (app-ui-kit), so a missing one is requested there.</summary>
public class NoAppTokensTests
{
    [Test]
    public void AppStyles_DeclareNoCustomProperty()
    {
        Assert.That(Custom(AppCss.All()), Is.Empty, "no app tokens: use a kit token, or ask for one in https://github.com/marty-shields/app-ui-kit (AGENTS.md, 'No custom UI elements')");
    }

    [Test]
    public void AppCss_HasNoTokensFile()
    {
        Assert.That(AppCss.Files().Where(f => Path.GetFileName(f) == "tokens.css").Select(AppCss.Name), Is.Empty, "no app tokens file: the kit's tokens.css is the only one");
    }

    [Test]
    public void AppCss_ScansTheStoriesProjectsAndMarkupToo()
    {
        Assert.Multiple(() =>
        {
            Assert.That(AppCss.Projects, Is.SupersetOf(new[] { "WorkoutLog.UI.Stories", "WorkoutLog.UI.Stories.Host" }));
            Assert.That(AppCss.MarkupFiles().Select(AppCss.Name), Does.Contain("src/WorkoutLog.UI.Stories.Host/wwwroot/iframe.html")
                .And.Contain("src/WorkoutLog.App/wwwroot/index.html").And.Contain("src/WorkoutLog.UI/Components/Workout/SetRow.razor"));
            Assert.That(AppCss.Files().Select(AppCss.Name), Does.Contain("src/WorkoutLog.UI/Components/Charts/Chart.razor.css"));
        });
    }

    [TestCase(".a { --x: 1px; color: var(--y); }", ExpectedResult = "--x")]
    [TestCase(".a { --X-Y : 1px }", ExpectedResult = "--x-y")]
    [TestCase(".a { margin: 0;\n    --x: 1px; }", ExpectedResult = "--x")]
    public string Declared_FindsACustomPropertyInAStylesheet(string css) =>
        string.Join(',', Custom(AppCss.InCss("a.css", css)).Select(d => d.Split(": ")[^1]));

    [Test]
    public void Declared_FindsACustomPropertyInAnInlineStyleAndAStyleBlock()
    {
        var markup = "<div style=\"--x: 1px; margin: 0\"></div><style>.a { --y: 2px; }</style>";

        Assert.That(Custom(AppCss.InMarkup("a.razor", markup)), Is.EqualTo(new[] { "a.razor: .a: --y", "a.razor: style=\"\": --x" }));
    }

    [Test]
    public void Declared_IgnoresAComment_AndAUse()
    {
        Assert.That(Custom(AppCss.InCss("a.css", ".a { /* --x: 1px; */ margin: var(--y); }")), Is.Empty);
    }

    private static IEnumerable<string> Custom(IEnumerable<AppCss.Declaration> declarations) =>
        declarations.Where(d => d.Property.StartsWith("--", StringComparison.Ordinal)).Select(d => $"{d.File}: {d.Selector}: {d.Property}");
}
