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
        string? Colour(string item) =>
            ((string?)project.Descendants().Single(e => e.Name.LocalName == item).Attribute("Color"))?.ToLowerInvariant();

        Assert.Multiple(() =>
        {
            Assert.That(Colour("MauiIcon"), Is.AnyOf([.. Allowed]));
            Assert.That(Colour("MauiSplashScreen"), Is.EqualTo(Palette["splash-light"]));
        });
    }

    [Test]
    public void DarkSplashColour_IsThePalettesDarkSplash()
    {
        var colours = XDocument.Parse(RepoFiles.Read("src", "WorkoutLog.App", "Platforms", "Android", "Resources", "values-night", "colors.xml"));

        var splash = colours.Descendants("color").Single(e => (string?)e.Attribute("name") == "maui_splash_color").Value;

        Assert.That(splash.ToLowerInvariant(), Is.EqualTo(Palette["splash-dark"]));
    }

    [Test]
    public void LightSplash_MatchesTheAppsLightPage()
    {
        Assert.That(Palette["splash-light"], Is.EqualTo(DesignTokens.Colour("--color-bg").Light.ToString()));
    }
}
