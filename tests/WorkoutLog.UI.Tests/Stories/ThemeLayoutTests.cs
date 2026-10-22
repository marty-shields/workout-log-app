using Microsoft.AspNetCore.Components;
using WorkoutLog.UI.Stories.Shared;

namespace WorkoutLog.UI.Tests.Stories;

public class ThemeLayoutTests : ComponentTestBase
{
    private IRenderedComponent<ThemeLayout> RenderLayout() =>
        Context.Render<ThemeLayout>(p => p.Add(x => x.Body, (RenderFragment)(b => b.AddMarkupContent(0, "<p data-testid=\"story\"></p>"))));

    [Test]
    public void Render_ShowsTheStoryAndFollowsSystemTheme()
    {
        var cut = RenderLayout();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=preview] [data-testid=story]"), Is.Not.Null);
            Assert.That(cut.Find("[data-testid=preview]").HasAttribute("data-theme"), Is.False);
            Assert.That(cut.Find("[data-testid=theme-system]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
        });
    }

    [TestCase("light")]
    [TestCase("dark")]
    public void Theme_SetsThemeOnThePreview(string theme)
    {
        var cut = RenderLayout();

        cut.Find($"[data-testid=theme-{theme}]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=preview]").GetAttribute("data-theme"), Is.EqualTo(theme));
            Assert.That(cut.Find($"[data-testid=theme-{theme}]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
            Assert.That(cut.Find("[data-testid=theme-system]").GetAttribute("aria-pressed"), Is.EqualTo("false"));
        });
    }

    [Test]
    public void System_ClearsTheme()
    {
        var cut = RenderLayout();
        cut.Find("[data-testid=theme-dark]").Click();

        cut.Find("[data-testid=theme-system]").Click();

        Assert.That(cut.Find("[data-testid=preview]").HasAttribute("data-theme"), Is.False);
    }
}
