using System.Text.RegularExpressions;

namespace WorkoutLog.UI.Tests.DesignSystem;

public partial class KitCardTests
{
    private static readonly string[] CardStyles =
    [
        Path.Combine("Components", "ThisWeek", "WeekWorkoutsCard.razor.css"),
        Path.Combine("Components", "ThisWeek", "WeekRecordsCard.razor.css"),
        Path.Combine("Components", "ThisWeek", "MuscleSetsCard.razor.css"),
        Path.Combine("Components", "ThisWeek", "LastWeekCard.razor.css"),
        Path.Combine("Components", "Trends", "LongestStreakCard.razor.css"),
        Path.Combine("Components", "Lists", "RecordsCard.razor.css"),
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

    [TestCase("Components/ThisWeek/LastWeekCard.razor.css")]
    [TestCase("Components/Trends/LongestStreakCard.razor.css")]
    public void SingleRowCards_DropTheRowsBottomBorder(string file)
    {
        var css = File.ReadAllText(RepoFiles.PathOf("src", "WorkoutLog.UI", file));

        Assert.That(css, Does.Match(SingleRowBorderRule()));
    }

    [GeneratedRegex(@"(^|[;{\s])border\s*:")]
    private static partial Regex BorderShorthand();

    [GeneratedRegex(@"\.single ::deep \.row\s*\{[^}]*border-bottom:\s*none")]
    private static partial Regex SingleRowBorderRule();

    [Test]
    public void KitCardTitle_IsTheMediumFontSize_Of16Pixels()
    {
        var title = TitleRule().Match(KitAssets.ReadScopedBundle());

        Assert.Multiple(() =>
        {
            Assert.That(title.Success, Is.True, "no Card .title rule in the kit's bundle");
            Assert.That(title.Groups[1].Value, Does.Contain("font-size: var(--font-size-md)"));
            Assert.That(Regex.Match(DesignTokens.KitTokensCss, @"--font-size-md:\s*([^;]+);").Groups[1].Value, Is.EqualTo("1rem"));
        });
    }

    [GeneratedRegex(@"\.title\[[^\]]*\]\s*\{([^}]*overflow-wrap[^}]*)\}")]
    private static partial Regex TitleRule();
}
