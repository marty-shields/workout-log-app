namespace WorkoutLog.UI.Tests.DesignSystem;

public class ContrastTests
{
    private const double Text = 4.5;
    private const double LargeTextOrControl = 3.0;

    // Foreground token, background token, WCAG AA minimum.
    private static readonly (string Foreground, string Background, double Minimum)[] Pairs =
    [
        ("--color-text", "--color-bg", Text),
        ("--color-text", "--color-surface", Text),
        ("--color-text", "--color-selected", Text),
        ("--color-text-muted", "--color-bg", Text),
        ("--color-text-muted", "--color-surface", Text),
        ("--color-error", "--color-bg", Text),
        ("--color-error", "--color-surface", Text),
        ("--color-on-bar", "--color-bar", Text),
        ("--color-on-bar-muted", "--color-bar", Text),
        ("--color-on-primary", "--color-primary", Text),
        ("--color-on-secondary", "--color-secondary", Text),
        ("--color-on-danger", "--color-danger", Text),
        ("--color-accent", "--color-surface", LargeTextOrControl),
        ("--color-border-strong", "--color-bg", LargeTextOrControl),
        ("--color-border-strong", "--color-surface", LargeTextOrControl),
        ("--color-focus", "--color-bg", LargeTextOrControl),
        ("--color-focus", "--color-surface", LargeTextOrControl),
    ];

    private static IEnumerable<TestCaseData> Cases() =>
        Pairs.SelectMany(p => new[] { "light", "dark" }.Select(theme =>
            new TestCaseData(p.Foreground, p.Background, p.Minimum, theme)
                .SetArgDisplayNames($"{p.Foreground} on {p.Background} ({theme})")));

    [TestCaseSource(nameof(Cases))]
    public void Pair_MeetsWcagAA(string foreground, string background, double minimum, string theme)
    {
        var fg = Pick(DesignTokens.Colour(foreground), theme);
        var bg = Pick(DesignTokens.Colour(background), theme);

        Assert.That(Rgb.Contrast(fg, bg), Is.GreaterThanOrEqualTo(minimum), $"{fg} on {bg}");
    }

    private static Rgb Pick((Rgb Light, Rgb Dark) colour, string theme) => theme == "light" ? colour.Light : colour.Dark;
}
