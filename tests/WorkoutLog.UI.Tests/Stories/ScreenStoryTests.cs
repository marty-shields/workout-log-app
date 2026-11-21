using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.Stories;

public partial class ScreenStoryTests
{
    [Test]
    public void ScreenStories_DeclareEveryScreenState()
    {
        var declared = RepoFiles.Find("*.stories.razor", "src", "WorkoutLog.UI.Stories", "Stories", "Screens")
            .Select(File.ReadAllText)
            .SelectMany(source => StoryName().Matches(source)
                .Select(m => $"{StoryFiles.Title(source)}/{m.Groups[1].Value}"));

        Assert.That(declared, Is.EquivalentTo(ScreenStates.All.Select(s => s.ToString())));
    }

    [GeneratedRegex("""<Story Name="([^"]+)">""")]
    private static partial Regex StoryName();
}
