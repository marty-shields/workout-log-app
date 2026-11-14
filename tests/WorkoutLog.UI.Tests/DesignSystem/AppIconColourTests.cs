using System.Text.Json;
using System.Xml.Linq;

namespace WorkoutLog.UI.Tests.DesignSystem;

/// <summary>The App's icon and splash use only the colours in the icon's <c>palette.json</c>.</summary>
public class AppIconColourTests
{
    private static readonly Dictionary<string, string> Palette =
        JsonSerializer.Deserialize<Dictionary<string, string>>(RepoFiles.Read("src", "WorkoutLog.App", "Resources", "AppIcon", "palette.json"))!;

    private static readonly HashSet<string> Allowed = [.. Palette.Values];

    private static IEnumerable<TestCaseData> ArtFiles() =>
        new[] { "AppIcon", "Splash" }
            .SelectMany(folder => RepoFiles.Find("*.svg", "src", "WorkoutLog.App", "Resources", folder))
            .Append(RepoFiles.PathOf("docs", "images", "readme", "icon.svg"))
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetFileName(f)));

    [TestCaseSource(nameof(ArtFiles))]
    public void Svg_FillsOnlyWithPaletteColours(string file)
    {
        var elements = XDocument.Load(file).Descendants().ToList();
        var gradientIds = elements
            .Where(e => e.Name.LocalName == "linearGradient")
            .Select(e => ((string?)e.Attribute("id"))?.ToLowerInvariant())
            .ToHashSet();
        var stopsWithoutColour = elements
            .Where(e => e.Name.LocalName == "stop" && e.Parent?.Name.LocalName == "linearGradient" && e.Attribute("stop-color") is null)
            .ToList();
        var colours = elements
            .SelectMany(e => e.Attributes().Where(a => a.Name.LocalName is "fill" or "stroke" or "stop-color"))
            .Select(a => a.Value.ToLowerInvariant())
            .ToList();
        var references = colours.Where(c => c.StartsWith("url(", StringComparison.Ordinal)).ToList();
        var solid = colours.Except(references).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(elements.Where(e => e.Attribute("style") is not null), Is.Empty);
            Assert.That(stopsWithoutColour, Is.Empty);
            Assert.That(solid, Is.Not.Empty);
            Assert.That(solid.Where(c => !Allowed.Contains(c)), Is.Empty);
            // Only a gradient defined in the same file; MAUI's rasteriser is not known to support other paint servers.
            Assert.That(references.Where(r => !(r.StartsWith("url(#", StringComparison.Ordinal) && r.EndsWith(')') && gradientIds.Contains(r[5..^1]))), Is.Empty);
        });
    }

    [Test]
    public void AppProject_IconAndSplashColours_ArePaletteColours()
    {
        var project = XDocument.Parse(RepoFiles.Read("src", "WorkoutLog.App", "WorkoutLog.App.csproj"));
        string? Colour(string item, string attribute) =>
            ((string?)project.Descendants().Single(e => e.Name.LocalName == item).Attribute(attribute))?.ToLowerInvariant();

        Assert.Multiple(() =>
        {
            Assert.That(Colour("MauiIcon", "Color"), Is.AnyOf([.. Allowed]));
            Assert.That(Colour("MauiSplashScreen", "Color"), Is.EqualTo(Palette["splash-light"]));
            Assert.That(Colour("MauiSplashScreen", "DarkColor"), Is.EqualTo(Palette["splash-dark"]));
        });
    }

    [Test]
    public void LightSplash_MatchesTheAppsLightPage()
    {
        Assert.That(Palette["splash-light"], Is.EqualTo(DesignTokens.Colour("--color-bg").Light.ToString()));
    }
}
