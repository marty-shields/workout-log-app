using System.Xml.Linq;

namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>The App's icon and splash use only the palette colours and white (D89).</summary>
public class AppIconColourTests
{
    // Tokens whose light value is one of the five palette colours.
    private static readonly string[] PaletteTokens =
        ["--color-bar", "--color-on-bar-muted", "--color-accent", "--color-text", "--color-border-strong"];

    private static readonly HashSet<string> Allowed =
        [.. PaletteTokens.Select(token => DesignTokens.Colour(token).Light.ToString()), "#ffffff"];

    private static IEnumerable<TestCaseData> ArtFiles() =>
        new[] { "AppIcon", "Splash" }
            .SelectMany(folder => RepoFiles.Find("*.svg", "src", "WorkoutLog.App", "Resources", folder))
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetFileName(f)));

    [TestCaseSource(nameof(ArtFiles))]
    public void Svg_FillsOnlyWithPaletteColours(string file)
    {
        var elements = XDocument.Load(file).Descendants().ToList();
        var colours = elements
            .SelectMany(e => e.Attributes().Where(a => a.Name.LocalName is "fill" or "stroke"))
            .Select(a => a.Value.ToLowerInvariant())
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(elements.Where(e => e.Attribute("style") is not null), Is.Empty);
            Assert.That(colours, Is.Not.Empty);
            Assert.That(colours.Where(c => !Allowed.Contains(c)), Is.Empty);
        });
    }

    [Test]
    public void AppProject_IconAndSplashColours_ArePaletteColours()
    {
        var project = XDocument.Parse(RepoFiles.Read("src", "WorkoutLog.App", "WorkoutLog.App.csproj"));

        var colours = project.Descendants()
            .Where(e => e.Name.LocalName is "MauiIcon" or "MauiSplashScreen")
            .Select(e => ((string?)e.Attribute("Color"))?.ToLowerInvariant())
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(colours, Is.Not.Empty);
            Assert.That(colours.Where(c => c is null || !Allowed.Contains(c)), Is.Empty);
        });
    }
}
