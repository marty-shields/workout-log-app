using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.Navigation;

/// <summary>
/// The App adds no WebView back-stack entries, so the BlazorWebView's own back callback never runs (D119).
/// Navigation goes through <c>GoTo</c> only, and there are no links.
/// </summary>
public partial class NoWebViewBackStackTests
{
    private static IEnumerable<TestCaseData> AppFiles() =>
        new[] { "*.razor", "*.cs" }
            .SelectMany(pattern => RepoFiles.Find(pattern, "src", "WorkoutLog.App"))
            .Where(f => Path.GetFileName(f) != "NavigationManagerExtensions.cs")
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetFileName(f)));

    private static IEnumerable<TestCaseData> UiMarkupFiles() =>
        RepoFiles.Find("*.razor", "src", "WorkoutLog.UI")
            .Select(f => new TestCaseData(f).SetArgDisplayNames(Path.GetFileName(f)));

    [TestCaseSource(nameof(AppFiles))]
    public void App_NavigatesWithGoToOnly(string file) =>
        Assert.That(File.ReadAllText(file), Does.Not.Contain("NavigateTo("));

    [TestCaseSource(nameof(AppFiles))]
    [TestCaseSource(nameof(UiMarkupFiles))]
    public void Markup_HasNoLinks(string file) =>
        Assert.That(Link().IsMatch(File.ReadAllText(file)), Is.False);

    [GeneratedRegex(@"<a\s[^>]*href", RegexOptions.IgnoreCase)]
    private static partial Regex Link();
}
