using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.DesignSystem;

public partial class KitAssetsTests
{
    private static readonly string[] KitHead =
    [
        "_content/Shields.UI/theme.js",
        "_content/Shields.UI/css/tokens.css",
        "_content/Shields.UI/css/base.css",
    ];

    private const string KitTokens = "_content/Shields.UI/css/tokens.css";

    private const string AppTokens = "_content/WorkoutLog.UI/css/tokens.css";

    private const string StoriesCss = "_content/Shields.UI.Stories/css/blazor-ui.css";

    private const string StoriesTokens = "_content/Shields.UI.Stories/css/tokens.css";

    private static IEnumerable<TestCaseData> Pages() =>
    [
        new TestCaseData(RepoFiles.Read("src", "WorkoutLog.App", "wwwroot", "index.html")).SetArgDisplayNames("App index.html"),
        new TestCaseData(RepoFiles.Read("src", "WorkoutLog.UI.Stories.Host", "wwwroot", "iframe.html")).SetArgDisplayNames("Stories iframe.html"),
    ];

    private static IEnumerable<string> Loaded(string html) =>
        Asset().Matches(html).Select(m => m.Groups[1].Value).Where(url => url.StartsWith("_content/", StringComparison.Ordinal));

    [TestCaseSource(nameof(Pages))]
    public void Page_LoadsTheKitsThemeTokensAndBaseBeforeTheAppTokens(string html)
    {
        var loaded = Loaded(html).Where(url => url != StoriesCss && url != StoriesTokens).ToList();

        Assert.That(loaded, Is.EqualTo(KitHead.Append(AppTokens)));
    }

    [Test]
    public void AppIndex_LoadsNoStoriesShellAsset()
    {
        var loaded = Loaded(RepoFiles.Read("src", "WorkoutLog.App", "wwwroot", "index.html"));

        Assert.That(loaded, Has.None.StartsWith("_content/Shields.UI.Stories/"));
    }

    [Test]
    public void StoriesIframe_LoadsTheStoriesShellCssAndItsTokensAfterTheKitsTokens()
    {
        var loaded = Loaded(RepoFiles.Read("src", "WorkoutLog.UI.Stories.Host", "wwwroot", "iframe.html")).ToList();

        Assert.That(loaded.IndexOf(StoriesCss), Is.GreaterThanOrEqualTo(0));
        Assert.That(loaded.IndexOf(StoriesTokens), Is.GreaterThan(loaded.IndexOf("_content/Shields.UI/css/tokens.css")));
    }

    [Test]
    public void StoriesIndex_LoadsTheStoriesShellCssAndItsTokensAfterTheKitsTokens()
    {
        var loaded = Loaded(RepoFiles.Read("src", "WorkoutLog.UI.Stories.Host", "wwwroot", "index.html")).ToList();

        Assert.That(loaded, Is.SupersetOf(new[] { StoriesCss, StoriesTokens, KitTokens }));
        Assert.That(loaded.IndexOf(StoriesTokens), Is.GreaterThan(loaded.IndexOf(KitTokens)));
    }

    [TestCaseSource(nameof(Pages))]
    public void Page_ThemeScriptKeepsTheStorageKeyOfEarlierVersions(string html)
    {
        var tag = ThemeScript().Match(html);

        Assert.That(tag.Success, Is.True, "no theme.js script tag");
        Assert.That(tag.Value, Does.Contain("data-storage-key=\"workoutlog.theme\""));
    }

    [Test]
    public void KitAssets_AreInThePackage()
    {
        Assert.Multiple(() =>
        {
            Assert.That(DesignTokens.KitTokensCss, Does.Contain("--color-accent"));
            Assert.That(KitAssets.Read("css/base.css"), Does.Contain("--inset-top"));
            Assert.That(KitAssets.Read("theme.js"), Does.Contain("shieldsTheme"));
        });
    }

    [Test]
    public void AppTokens_DeclareNoTokenTheKitDeclares()
    {
        var duplicates = DesignTokens.Declares(DesignTokens.AppTokensCss).Intersect(DesignTokens.Declares(DesignTokens.KitTokensCss));

        Assert.That(duplicates, Is.Empty);
    }

    [Test]
    public void LocalBaseStylesAndThemeScript_AreGone()
    {
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(RepoFiles.PathOf("src", "WorkoutLog.UI", "wwwroot", "css", "base.css")), Is.False);
            Assert.That(File.Exists(RepoFiles.PathOf("src", "WorkoutLog.UI", "wwwroot", "theme.js")), Is.False);
        });
    }

    [GeneratedRegex(@"(?:href|src)=""([^""]+)""")]
    private static partial Regex Asset();

    [GeneratedRegex(@"<script\b[^>]*theme\.js[^>]*>")]
    private static partial Regex ThemeScript();
}
