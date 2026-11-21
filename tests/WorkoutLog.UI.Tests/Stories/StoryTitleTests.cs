using System.Text.RegularExpressions;

using WorkoutLog.UI.Stories;

namespace WorkoutLog.UI.Tests.Stories;

/// <summary>Every story and page title is built from <c>StoryTitles.Prefix</c>, so the site has one top-level section.</summary>
public partial class StoryTitleTests
{
    private static readonly string StoriesFolder = RepoFiles.PathOf("src", "WorkoutLog.UI.Stories", "Stories");

    private static IEnumerable<TestCaseData> TitleLines() =>
        Directory.EnumerateFiles(StoriesFolder, "*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".stories.razor") || f.EndsWith(".md"))
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
        var files = Directory.EnumerateFiles(StoriesFolder, "*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".stories.razor") || f.EndsWith(".md"));

        Assert.That(files.Where(f => !File.ReadLines(f).Any(l => TitleAttribute().IsMatch(l))), Is.Empty);
    }

    [Test]
    public void StoryFiles_HoldNoLiteralPrefix()
    {
        var files = Directory.EnumerateFiles(StoriesFolder, "*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".stories.razor") || f.EndsWith(".md"));

        Assert.That(files.Where(f => File.ReadAllText(f).Contains(StoryTitles.Prefix + "/")), Is.Empty);
    }

    [Test]
    public void Titles_AreOneTopLevelSection()
    {
        Assert.That(StoryFiles.Titles().Select(t => t.Split('/')[0]).Distinct(), Is.EqualTo(new[] { StoryTitles.Prefix }));
    }

    [GeneratedRegex(@"(\[Stories\(|^\$attribute: CustomPage\()")]
    private static partial Regex TitleAttribute();

    [GeneratedRegex("""^(@attribute \[Stories\(|\$attribute: CustomPage\()StoryTitles\.Prefix \+ "/[^"]+"\)\]?$""")]
    private static partial Regex PrefixedTitle();
}

/// <summary>The resolved title of each *.stories.razor file: the prefix, then the part after it.</summary>
internal static partial class StoryFiles
{
    public static IEnumerable<string> Titles() =>
        RepoFiles.Find("*.stories.razor", "src", "WorkoutLog.UI.Stories", "Stories").Select(f => Title(File.ReadAllText(f)));

    public static string Title(string source) => $"{StoryTitles.Prefix}/{Rest().Match(source).Groups[1].Value}";

    [GeneratedRegex("""\[Stories\(StoryTitles\.Prefix \+ "/([^"]+)"\)\]""")]
    private static partial Regex Rest();
}
