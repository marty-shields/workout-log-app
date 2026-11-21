using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.Stories;

public partial class ScreenStoryTests
{
    [Test]
    public void ScreenStories_DeclareEveryScreenState()
    {
        var declared = RepoFiles.Find("*.stories.razor", "src", "WorkoutLog.UI.Stories", "Stories", "Screens")
            .SelectMany(file => StoryName().Matches(File.ReadAllText(file))
                .Select(m => $"{StoryFiles.Title(file)}/{m.Groups[1].Value}"));

        Assert.That(declared, Is.EquivalentTo(ScreenStates.All.Select(s => s.ToString())));
    }

    [GeneratedRegex("""<Story Name="([^"]+)">""")]
    private static partial Regex StoryName();
}
