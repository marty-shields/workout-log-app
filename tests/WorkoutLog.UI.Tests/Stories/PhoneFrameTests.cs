using WorkoutLog.UI.Stories.Shared;

namespace WorkoutLog.UI.Tests.Stories;

public class PhoneFrameTests : ComponentTestBase
{
    [Test]
    public void Render_FollowsSystemTheme()
    {
        var cut = Context.Render<PhoneFrame>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=phone]").HasAttribute("data-theme"), Is.False);
            Assert.That(cut.Find("[data-testid=theme-system]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
        });
    }

    [TestCase("light")]
    [TestCase("dark")]
    public void Theme_SetsThemeOnThePhone(string theme)
    {
        var cut = Context.Render<PhoneFrame>();

        cut.Find($"[data-testid=theme-{theme}]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=phone]").GetAttribute("data-theme"), Is.EqualTo(theme));
            Assert.That(cut.Find($"[data-testid=theme-{theme}]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
            Assert.That(cut.Find("[data-testid=theme-system]").GetAttribute("aria-pressed"), Is.EqualTo("false"));
        });
    }

    [Test]
    public void System_ClearsTheme()
    {
        var cut = Context.Render<PhoneFrame>();
        cut.Find("[data-testid=theme-dark]").Click();

        cut.Find("[data-testid=theme-system]").Click();

        Assert.That(cut.Find("[data-testid=phone]").HasAttribute("data-theme"), Is.False);
    }
}
