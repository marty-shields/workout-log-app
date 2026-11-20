namespace WorkoutLog.UI.Tests.DesignSystem;

public class ContrastTests
{
    private const double Text = 4.5;
    private const double LargeTextOrControl = 3.0;

    // Minimums are WCAG AA contrast ratios.
    private static readonly (string Foreground, string Background, double Minimum)[] Pairs =
    [
        ("--color-text", "--color-bg", Text),
        ("--color-text", "--color-surface", Text),
        ("--color-text", "--color-selected", Text),
        ("--color-text-muted", "--color-bg", Text),
        ("--color-text-muted", "--color-surface", Text),
        ("--color-error", "--color-bg", Text),
        ("--color-error", "--color-surface", Text),
        ("--color-success", "--color-bg", Text),
        ("--color-success", "--color-surface", Text),
        ("--color-on-bar", "--color-bar", Text),
        ("--color-on-bar-muted", "--color-bar", Text),
        ("--color-on-primary", "--color-primary", Text),
        ("--color-on-secondary", "--color-secondary", Text),
        ("--color-on-danger", "--color-danger", Text),
        ("--color-error", "--color-selected", Text),
        ("--color-accent", "--color-bg", LargeTextOrControl),
        ("--color-accent", "--color-surface", LargeTextOrControl),
        // Calendar dots on range days, and the meter fills on their tracks.
        ("--color-accent", "--color-selected", LargeTextOrControl),
        ("--color-accent", "--color-border", LargeTextOrControl),
        ("--color-border-strong", "--color-bg", LargeTextOrControl),
        ("--color-border-strong", "--color-surface", LargeTextOrControl),
        ("--color-bar-past", "--color-bg", LargeTextOrControl),
        ("--color-bar-past", "--color-surface", LargeTextOrControl),
        ("--color-focus", "--color-bg", LargeTextOrControl),
        ("--color-focus", "--color-surface", LargeTextOrControl),
    ];

    // The kit's darker accent against --color-bar-past: 2.6:1 in light and 2.2:1 in dark, short of the 3:1 the issue asks for
    // (it was 1.2:1 and 1.8:1 with the old accent). No colour keeps 3:1 against both the page and the accent, so this guards
    // against a regression until the owner decides; the bars are also told apart by position and the legend.
    private const double AccentBesidePastBars = 2.2;

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

    [TestCase("light")]
    [TestCase("dark")]
    public void AccentBar_BesideThePastBars_StaysAsDistinctAsItIsToday(string theme)
    {
        var accent = Pick(DesignTokens.Colour("--color-accent"), theme);
        var past = Pick(DesignTokens.Colour("--color-bar-past"), theme);

        Assert.That(Rgb.Contrast(accent, past), Is.GreaterThanOrEqualTo(AccentBesidePastBars), $"{accent} beside {past}");
    }

    // The App keeps the status bar's icons light (StatusBarTheme.Dark in App.xaml.cs) in both themes, and Android draws them
    // white over the top bar.
    [TestCase("light")]
    [TestCase("dark")]
    public void StatusBarIcons_OnTheTopBar_MeetWcagAA(string theme)
    {
        var bar = Pick(DesignTokens.Colour("--color-bar"), theme);

        Assert.That(Rgb.Contrast(Rgb.Parse("#ffffff"), bar), Is.GreaterThanOrEqualTo(LargeTextOrControl), $"#ffffff on {bar}");
    }

    private static Rgb Pick((Rgb Light, Rgb Dark) colour, string theme) => theme == "light" ? colour.Light : colour.Dark;
}
