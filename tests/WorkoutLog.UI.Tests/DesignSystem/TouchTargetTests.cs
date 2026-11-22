using System.Text.RegularExpressions;
using Shields.UI.Testing;

namespace WorkoutLog.UI.Tests.DesignSystem;

public partial class TouchTargetTests
{
    [Test]
    public void Controls_AreAtLeastATouchTarget()
    {
        Assert.That(TouchTargetCheck.ProblemsInFolder(TokensOnlyCssTests.UiFolder), Is.Empty);
    }

    [Test]
    public void Kit_DefinesTheTouchTargets()
    {
        Assert.Multiple(() =>
        {
            Assert.That(DesignTokens.Value("--touch-target"), Is.EqualTo("48px"));
            Assert.That(DesignTokens.Value("--touch-target-compact"), Is.EqualTo("40px"));
        });
    }

    [Test]
    public void KitCompactIconButton_IsTheCompactTouchTarget()
    {
        var css = KitCss.Bundle;
        var rule = CompactRule().Match(css);

        Assert.That(rule.Success, Is.True, "Kit IconButton has no .compact rule");
        Assert.Multiple(() =>
        {
            Assert.That(rule.Groups[1].Value, Does.Contain("min-width: var(--touch-target-compact)"));
            Assert.That(rule.Groups[1].Value, Does.Contain("min-height: var(--touch-target-compact)"));
        });
    }

    [GeneratedRegex(@"\.compact\[b-[a-z0-9]+\]\s*\{([^}]*)\}")]
    private static partial Regex CompactRule();
}
