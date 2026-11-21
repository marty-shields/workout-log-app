using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>This repo declares no custom properties: every token comes from the Shields.UI kit (app-ui-kit), so a missing one is requested there.</summary>
public partial class NoAppTokensTests
{
    [Test]
    public void AppCss_DeclaresNoCustomProperty()
    {
        Assert.That(Declared(AppCss.Files()), Is.Empty, "no app tokens: use a kit token, or ask for one in https://github.com/marty-shields/app-ui-kit (AGENTS.md, 'No custom UI elements')");
    }

    [Test]
    public void AppCss_HasNoTokensFile()
    {
        Assert.That(AppCss.Files().Where(f => Path.GetFileName(f) == "tokens.css").Select(AppCss.Name), Is.Empty, "no app tokens file: the kit's tokens.css is the only one");
    }

    [Test]
    public void AppCss_CoversEveryProjectsStyles()
    {
        Assert.That(AppCss.Files().Select(AppCss.Name), Does.Contain("src/WorkoutLog.UI/Components/Charts/Chart.razor.css"));
    }

    [Test]
    public void Declared_FindsACustomPropertyButNotItsUse()
    {
        Assert.That(DeclaredIn("a { --x: 1px; color: var(--y); }"), Is.EqualTo(new[] { "--x" }));
    }

    [Test]
    public void Declared_IgnoresAComment()
    {
        Assert.That(DeclaredIn("a { /* --x: 1px; */ margin: 0; }"), Is.Empty);
    }

    private static IEnumerable<string> Declared(IEnumerable<string> files) =>
        files.SelectMany(file => DeclaredIn(File.ReadAllText(file)).Select(name => $"{AppCss.Name(file)}: {name}"));

    private static IEnumerable<string> DeclaredIn(string css) =>
        Declaration().Matches(Comment().Replace(css, "")).Select(m => m.Groups[1].Value);

    [GeneratedRegex(@"(?<=[{;]\s*)(--[\w-]+)\s*:")]
    private static partial Regex Declaration();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comment();
}
