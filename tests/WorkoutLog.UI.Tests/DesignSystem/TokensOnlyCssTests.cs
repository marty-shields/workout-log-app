using Shields.UI.Testing;

namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>The kit's tokens-only checks over this app's styles and markup; the kit's own base.css is checked by the kit's tests.</summary>
public class TokensOnlyCssTests
{
    /// <summary>The folder every style and touch-target check scans: the whole project.</summary>
    internal static readonly string UiFolder = RepoFiles.PathOf("src", "WorkoutLog.UI");

    // This app's component styles; it keeps no tokens file, so every stylesheet is checked.
    private static IEnumerable<TestCaseData> StyleFiles() =>
        RepoFiles.Find("*.css", "src", "WorkoutLog.UI")
            .Select(f => new TestCaseData(File.ReadAllText(f), Path.GetFileName(f)).SetArgDisplayNames(Path.GetFileName(f)));

    [Test]
    public void Checks_CoverTheWholeProject_NotJustScreens()
    {
        var css = RepoFiles.Find("*.css", "src", "WorkoutLog.UI").Select(f => Path.GetRelativePath(UiFolder, f).Replace('\\', '/')).ToList();

        Assert.That(css, Does.Contain("Components/Charts/Chart.razor.css").And.Contain("Screens/ExerciseView.razor.css"));
    }

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
        Assert.That(TokensOnlyCheck.UnknownTokens(css), Is.Empty, name);
    }

    [TestCaseSource(nameof(StyleFiles))]
    public void Css_FillsWithBackgroundColor_SoThePressedLayerShows(string css, string name)
    {
        Assert.That(TokensOnlyCheck.BackgroundShorthands(css), Is.Empty, $"{name}: base.css's pressed layer is a background-image, which the background shorthand resets");
    }
}
