using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.Docs;

/// <summary>
/// The README's screenshots and Project layout against the docs and folders that must list them. Screenshot names come from the README's
/// <c>docs/images/readme/&lt;name&gt;-light.png</c> references and the first column of the table in docs/dev-setup-build.md.
/// Layout names are the <c>WorkoutLog.*</c> entries in the README's fenced Project layout block.
/// </summary>
public partial class ReadmeDocsTests
{
    private static readonly string Readme = RepoFiles.Read("README.md");

    [Test]
    public void Screenshots_AreAllInTheSetupTable()
    {
        var table = ScreenshotRow().Matches(RepoFiles.Read("docs", "dev-setup-build.md")).Select(m => m.Groups[1].Value);

        Assert.That(table, Is.EquivalentTo(ReadmeScreenshot().Matches(Readme).Select(m => m.Groups[1].Value).Distinct()));
    }

    [Test]
    public void Screenshots_HaveALightAndADarkImage()
    {
        var names = ScreenshotRow().Matches(RepoFiles.Read("docs", "dev-setup-build.md")).Select(m => m.Groups[1].Value);

        Assert.That(MissingImages(names, name => File.Exists(RepoFiles.PathOf("docs", "images", "readme", name))), Is.Empty);
    }

    [Test]
    public void MissingImages_ListsAMissingDarkImage()
    {
        var present = new HashSet<string> { "home-light.png", "home-dark.png", "cardio-light.png" };

        Assert.That(MissingImages(["home", "cardio"], present.Contains), Is.EqualTo(new[] { "cardio-dark.png" }));
    }

    [Test]
    public void SourceProjects_AreAllInTheProjectLayout()
    {
        var layout = LayoutEntry().Matches(Readme).Select(m => m.Groups[1].Value);
        var projects = Directory.GetDirectories(RepoFiles.PathOf("src"), "WorkoutLog.*").Select(Path.GetFileName);

        Assert.That(layout, Is.SupersetOf(projects));
    }

    private static IEnumerable<string> MissingImages(IEnumerable<string> names, Func<string, bool> exists) =>
        names.SelectMany(name => new[] { $"{name}-light.png", $"{name}-dark.png" }).Where(file => !exists(file));

    [GeneratedRegex(@"docs/images/readme/([a-z-]+)-light\.png")]
    private static partial Regex ReadmeScreenshot();

    [GeneratedRegex(@"^\| `([a-z-]+)` \| `[^`]+` \|\r?$", RegexOptions.Multiline)]
    private static partial Regex ScreenshotRow();

    [GeneratedRegex(@"^  (WorkoutLog\.[A-Za-z0-9.]+)/ ", RegexOptions.Multiline)]
    private static partial Regex LayoutEntry();
}
