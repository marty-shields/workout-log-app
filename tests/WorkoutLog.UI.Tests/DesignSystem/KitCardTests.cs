using System.Text.RegularExpressions;
using Shields.UI.Testing;

namespace WorkoutLog.UI.Tests.DesignSystem;

public partial class KitCardTests
{
    private static readonly string[] CardStyles =
    [
        Path.Combine("Components", "ThisWeek", "WeekWorkoutsCard.razor.css"),
        Path.Combine("Components", "ThisWeek", "WeekRecordsCard.razor.css"),
    ];

    [TestCaseSource(nameof(CardStyles))]
    public void CardStyles_LeaveBorderCornersSurfaceAndTitleToTheKit(string file)
    {
        var css = File.ReadAllText(RepoFiles.PathOf("src", "WorkoutLog.UI", file));

        Assert.Multiple(() =>
        {
            Assert.That(css, Does.Not.Contain(".card"));
            Assert.That(css, Does.Not.Contain(".title"));
            Assert.That(css, Does.Not.Contain("--color-surface"));
            Assert.That(css, Does.Not.Contain("--radius-lg"));
            Assert.That(css, Does.Not.Contain("--radius-md"));
            Assert.That(css, Does.Not.Match(BorderShorthand()));
        });
    }

    /// <summary>
    /// Rules that draw a border, corners or a surface and are not cards, keyed by file and selector, each with its reason.
    /// Anything else that draws all of a card's look fails <see cref="NoStyleSheet_DrawsACardItself"/>: use the kit's Card.
    /// </summary>
    private static readonly Dictionary<(string File, string Selector), string> NotCards = [];

    private static IEnumerable<string> UiStyleSheets() =>
        Directory.EnumerateFiles(RepoFiles.PathOf("src", "WorkoutLog.UI"), "*.razor.css", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Select(file => Path.GetRelativePath(RepoFiles.PathOf("src", "WorkoutLog.UI"), file))
            .Order();

    [Test]
    public void NoStyleSheet_DrawsACardItself()
    {
        var drawn = new List<(string File, string Selector)>();
        foreach (var file in UiStyleSheets())
        {
            var css = CssComment().Replace(File.ReadAllText(RepoFiles.PathOf("src", "WorkoutLog.UI", file)), "");
            foreach (Match rule in CssRule().Matches(css))
            {
                var body = rule.Groups[2].Value;
                if (Surface().IsMatch(body) || (DrawnBorder().IsMatch(body) && body.Contains("border-radius")))
                {
                    drawn.Add((file, rule.Groups[1].Value.Trim()));
                }
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(drawn.Where(rule => !NotCards.ContainsKey(rule)).Select(rule => $"{rule.File} {rule.Selector}"), Is.Empty,
                "a rule draws a card's border, corners or surface: build it on the kit's Card, or add it to NotCards with a reason");
            Assert.That(NotCards.Keys.Except(drawn).Select(rule => $"{rule.File} {rule.Selector}"), Is.Empty,
                "NotCards lists a rule that no longer draws a border, corners or surface: remove it");
        });
    }

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CssComment();

    [GeneratedRegex(@"([^{}]+)\{([^{}]*)\}")]
    private static partial Regex CssRule();

    [GeneratedRegex(@"(^|[;\s])border(-block|-inline)?(-style)?\s*:[^;]*\b(solid|dashed)\b")]
    private static partial Regex DrawnBorder();

    [GeneratedRegex(@"--color-surface[-\w]*")]
    private static partial Regex Surface();

    [GeneratedRegex(@"(^|[;{\s])border\s*:")]
    private static partial Regex BorderShorthand();

    [Test]
    public void KitCardTitle_IsTheMediumFontSize_Of16Pixels()
    {
        var title = TitleRule().Match(KitCss.Bundle);

        Assert.Multiple(() =>
        {
            Assert.That(title.Success, Is.True, "no Card .title rule in the kit's bundle");
            Assert.That(title.Groups[1].Value, Does.Contain("font-size: var(--font-size-md)"));
            Assert.That(DesignTokens.Value("--font-size-md"), Is.EqualTo("1rem"));
        });
    }

    [GeneratedRegex(@"\.title\[[^\]]*\]\s*\{([^}]*overflow-wrap[^}]*)\}")]
    private static partial Regex TitleRule();
}
