using Shields.UI.Testing;

namespace WorkoutLog.UI.Tests.DesignSystem;

public class ContrastTests
{
    // Minimums are WCAG AA contrast ratios.
    private static readonly (string Foreground, string Background, double Minimum)[] Pairs =
    [
        ("--color-text", "--color-bg", ContrastCheck.Text),
        ("--color-text", "--color-surface", ContrastCheck.Text),
        ("--color-text", "--color-selected", ContrastCheck.Text),
        ("--color-text-muted", "--color-bg", ContrastCheck.Text),
        ("--color-text-muted", "--color-surface", ContrastCheck.Text),
        ("--color-error", "--color-bg", ContrastCheck.Text),
        ("--color-error", "--color-surface", ContrastCheck.Text),
        ("--color-success", "--color-bg", ContrastCheck.Text),
        ("--color-success", "--color-surface", ContrastCheck.Text),
        ("--color-on-bar", "--color-bar", ContrastCheck.Text),
        ("--color-on-bar-muted", "--color-bar", ContrastCheck.Text),
        ("--color-on-primary", "--color-primary", ContrastCheck.Text),
        ("--color-on-secondary", "--color-secondary", ContrastCheck.Text),
        ("--color-on-danger", "--color-danger", ContrastCheck.Text),
        ("--color-error", "--color-selected", ContrastCheck.Text),
        ("--color-accent", "--color-bg", ContrastCheck.LargeTextOrControl),
        ("--color-accent", "--color-surface", ContrastCheck.LargeTextOrControl),
        // Calendar dots on range days, and the meter fills on their tracks.
        ("--color-accent", "--color-selected", ContrastCheck.LargeTextOrControl),
        ("--color-accent", "--color-border", ContrastCheck.LargeTextOrControl),
        ("--color-border-strong", "--color-bg", ContrastCheck.LargeTextOrControl),
        ("--color-border-strong", "--color-surface", ContrastCheck.LargeTextOrControl),
        ("--color-chart-past", "--color-bg", ContrastCheck.LargeTextOrControl),
        ("--color-chart-past", "--color-surface", ContrastCheck.LargeTextOrControl),
        ("--color-focus", "--color-bg", ContrastCheck.LargeTextOrControl),
        ("--color-focus", "--color-surface", ContrastCheck.LargeTextOrControl),
    ];

    // The kit's retuned --color-chart-past against the accent beside it: 3.01:1 in light and 3.07:1 in dark, measured, so the issue's 3:1
    // is met. The floors are the measurements rounded down.

    private static IEnumerable<TestCaseData> Cases() =>
        Pairs.SelectMany(p => Enum.GetValues<ColourScheme>().Select(scheme =>
            new TestCaseData(p.Foreground, p.Background, p.Minimum, scheme)
                .SetArgDisplayNames($"{p.Foreground} on {p.Background} ({scheme})")));

    [TestCaseSource(nameof(Cases))]
    public void Pair_MeetsWcagAA(string foreground, string background, double minimum, ColourScheme scheme)
    {
        var result = Measure(foreground, background, minimum, scheme);

        Assert.That(result.Passes, Is.True, result.ToString());
    }

    [TestCase(ColourScheme.Light, 3.0)]
    [TestCase(ColourScheme.Dark, 3.0)]
    public void AccentBar_BesideThePastBars_MeetsTheIssuesThreeToOne(ColourScheme scheme, double floor)
    {
        var result = Measure("--color-accent", "--color-chart-past", floor, scheme);

        Assert.That(result.Passes, Is.True, result.ToString());
    }

    // The App keeps the status bar's icons light (StatusBarTheme.Dark in App.xaml.cs) in both themes, and Android draws them
    // white over the top bar.
    [TestCase(ColourScheme.Light)]
    [TestCase(ColourScheme.Dark)]
    public void StatusBarIcons_OnTheTopBar_MeetWcagAA(ColourScheme scheme)
    {
        var bar = DesignTokens.Colour("--color-bar").In(scheme);

        Assert.That(Rgb.Contrast(Rgb.Parse("#ffffff"), bar), Is.GreaterThanOrEqualTo(ContrastCheck.LargeTextOrControl), $"#ffffff on {bar}");
    }

    private static ContrastResult Measure(string foreground, string background, double minimum, ColourScheme scheme) =>
        ContrastCheck.Measure(KitCss.Tokens, foreground, background, minimum, scheme);
}
