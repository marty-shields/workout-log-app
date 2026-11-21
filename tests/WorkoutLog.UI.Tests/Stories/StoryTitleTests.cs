using System.Text.RegularExpressions;

using WorkoutLog.UI.Stories;

namespace WorkoutLog.UI.Tests.Stories;

/// <summary>Every story and page title is built from <c>StoryTitles.Prefix</c>, so the site has one top-level section.</summary>
public partial class StoryTitleTests
{
    private static IEnumerable<TestCaseData> TitleLines() =>
        StoryFiles.All()
            .SelectMany(f => File.ReadLines(f)
                .Where(line => TitleAttribute().IsMatch(line))
                .Select(line => new TestCaseData(line.Trim()).SetArgDisplayNames($"{Path.GetFileName(f)}")));

    [Test]
    public void Prefix_IsWorkoutLog()
    {
        Assert.That(StoryTitles.Prefix, Is.EqualTo("Workout Log"));
    }

    [TestCaseSource(nameof(TitleLines))]
    public void Title_UsesThePrefixConstant(string line)
    {
        Assert.That(line, Does.Match(PrefixedTitle()));
    }

    [Test]
    public void EveryStoriesFileAndPage_DeclaresATitle()
    {
        Assert.That(StoryFiles.All().Where(f => !File.ReadLines(f).Any(l => TitleAttribute().IsMatch(l))), Is.Empty);
    }

    [Test]
    public void StoryFiles_HoldNoLiteralPrefix()
    {
        Assert.That(StoryFiles.All().Where(f => File.ReadAllText(f).Contains(StoryTitles.Prefix + "/")), Is.Empty);
    }

    [Test]
    public void Titles_AreOneTopLevelSection()
    {
        Assert.That(StoryFiles.All().Select(f => StoryFiles.Title(f).Split('/')[0]).Distinct(), Is.EqualTo(new[] { StoryTitles.Prefix }));
    }

    [Test]
    public void NavigationTreeOrder_NestsEverythingUnderThePrefix()
    {
        var app = RepoFiles.Read("src", "WorkoutLog.UI.Stories.Host", "App.razor");

        Assert.That(app, Does.Match(@"NavigationTreeOrder=""@N\[StoryTitles\.Prefix, N\["));
    }

    [GeneratedRegex(@"(\[Stories\(|^\$attribute: CustomPage\()")]
    private static partial Regex TitleAttribute();

    [GeneratedRegex("""^(@attribute \[Stories\(StoryTitles\.Prefix \+ "/[^"]+"\)\]|\$attribute: CustomPage\(StoryTitles\.Prefix \+ "/[^"]+"\))$""")]
    private static partial Regex PrefixedTitle();
}
