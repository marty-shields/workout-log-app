using Shields.UI.Testing;

namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>The kit's tokens-only checks over this app's styles and markup; the kit's own base.css is checked by the kit's tests.</summary>
public class TokensOnlyCssTests
{
    private static readonly string UiFolder = RepoFiles.PathOf("src", "WorkoutLog.UI");

    // This app's component styles; its tokens.css is where literals belong.
    private static IEnumerable<TestCaseData> StyleFiles() =>
        RepoFiles.Find("*.css", "src", "WorkoutLog.UI")
            .Where(f => Path.GetFileName(f) != "tokens.css")
            .Select(f => new TestCaseData(File.ReadAllText(f), Path.GetFileName(f)).SetArgDisplayNames(Path.GetFileName(f)));

    [Test]
    public void Css_HasNoLiteralColoursOrSizes()
    {
        Assert.That(TokensOnlyCheck.LiteralsInFolder(UiFolder), Is.Empty);
    }

    [Test]
    public void InlineStyles_HaveNoLiteralColoursOrSizes()
    {
        Assert.That(TokensOnlyCheck.InlineStyleLiteralsInFolder(UiFolder), Is.Empty);
    }

    [TestCaseSource(nameof(StyleFiles))]
    public void Css_UsesOnlyDeclaredTokens(string css, string name)
    {
        Assert.That(TokensOnlyCheck.UnknownTokens(css, AppTokens.Names), Is.Empty, name);
    }

    [TestCaseSource(nameof(StyleFiles))]
    public void Css_FillsWithBackgroundColor_SoThePressedLayerShows(string css, string name)
    {
        Assert.That(TokensOnlyCheck.BackgroundShorthands(css), Is.Empty, $"{name}: base.css's pressed layer is a background-image, which the background shorthand resets");
    }
}
