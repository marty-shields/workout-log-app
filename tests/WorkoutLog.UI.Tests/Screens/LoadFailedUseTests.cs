using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.Screens;

/// <summary>Every load error in the UI shows Try again as a secondary button, so it does not outweigh the screen's own primary action.</summary>
public partial class LoadFailedUseTests
{
    private static IEnumerable<TestCaseData> LoadFailedTags() =>
        RepoFiles.Find("*.razor", "src", "WorkoutLog.UI")
            .SelectMany(file => LoadFailedTag().Matches(File.ReadAllText(file)).Select(m => new TestCaseData(m.Value).SetArgDisplayNames(Path.GetFileName(file))));

    [Test]
    public void TheUi_ShowsLoadErrors() => Assert.That(LoadFailedTags().Count(), Is.GreaterThanOrEqualTo(7));

    [TestCaseSource(nameof(LoadFailedTags))]
    public void LoadFailed_RetriesWithASecondaryButton(string tag) =>
        Assert.That(tag, Does.Contain("RetryVariant=\"ButtonVariant.Secondary\""));

    [GeneratedRegex(@"<LoadFailed\b[^>]*>")]
    private static partial Regex LoadFailedTag();
}
