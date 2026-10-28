using WorkoutLog.UI.Components.Lists;

namespace WorkoutLog.UI.Tests.Components.Lists;

public class SessionRowTests : ComponentTestBase
{
    [Test]
    public void Renders_DateHeadlineAndDetail()
    {
        var cut = Render();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".date").TextContent, Is.EqualTo("Tue 20 Oct"));
            Assert.That(cut.Find(".headline").TextContent, Is.EqualTo("70kg × 3"));
            Assert.That(cut.Find(".detail").TextContent, Is.EqualTo("60×8 · 62.5×8 · 70×3"));
            Assert.That(cut.Find(".row").GetAttribute("data-testid"), Is.EqualTo("session"));
        });
    }

    [Test]
    public void IsNotTappable()
    {
        var cut = Render();

        Assert.That(cut.FindAll("*").Where(e => e.LocalName is "button" or "a" or "input" || e.Attributes.Any(a => a.Name == "blazor:onclick")), Is.Empty);
    }

    private IRenderedComponent<SessionRow> Render() =>
        Context.Render<SessionRow>(p => p
            .Add(x => x.Date, "Tue 20 Oct")
            .Add(x => x.Headline, "70kg × 3")
            .Add(x => x.Detail, "60×8 · 62.5×8 · 70×3")
            .Add(x => x.TestId, "session"));
}
