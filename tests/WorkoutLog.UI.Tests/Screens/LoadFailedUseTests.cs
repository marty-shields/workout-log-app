using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.Screens;

/// <summary>Every load error in the UI and the App's start-up failure show Try again as a secondary button, so it does not outweigh the screen's own primary action.</summary>
public partial class LoadFailedUseTests
{
    private static IEnumerable<TestCaseData> LoadFailedTags() =>
        new[] { "WorkoutLog.UI", "WorkoutLog.App" }.SelectMany(project => RepoFiles.Find("*.razor", "src", project))
            .SelectMany(file => LoadFailedTag().Matches(File.ReadAllText(file)).Select(m => new TestCaseData(m.Value).SetArgDisplayNames(Path.GetFileName(file))));

    [Test]
    public void TheUiAndApp_ShowLoadErrorsAndTheStartupGate() => Assert.That(LoadFailedTags().Count(), Is.GreaterThanOrEqualTo(8));

    [TestCaseSource(nameof(LoadFailedTags))]
    public void EveryTryAgain_IsASecondaryButton(string tag) =>
        Assert.That(tag, Does.Contain("RetryVariant=\"ButtonVariant.Secondary\""));

    [GeneratedRegex(@"<(?:LoadFailed|StartupGate|StartupView)\b[^>]*>")]
    private static partial Regex LoadFailedTag();
}
