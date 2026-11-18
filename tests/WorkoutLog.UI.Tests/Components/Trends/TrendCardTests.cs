using WorkoutLog.UI.Components.Trends;

namespace WorkoutLog.UI.Tests.Components.Trends;

public class TrendCardTests : ComponentTestBase
{
    [Test]
    public void ShowsTheTitleAsAHeadingWithTheAsideAndContent()
    {
        var cut = Context.Render<TrendCard>(p => p
            .Add(x => x.Title, "Records")
            .Add(x => x.Aside, "1 this week")
            .Add(x => x.ChildContent, "<p class=\"inside\">Hello</p>")
            .Add(x => x.TestId, "card"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("h2").TextContent, Is.EqualTo("Records"));
            Assert.That(cut.Find("[data-testid=card-aside]").TextContent, Is.EqualTo("1 this week"));
            Assert.That(cut.Find(".inside").TextContent, Is.EqualTo("Hello"));
        });
    }

    [Test]
    public void WithoutAnAside_ShowsNone()
    {
        var cut = Context.Render<TrendCard>(p => p.Add(x => x.Title, "Weeks").Add(x => x.TestId, "card"));

        Assert.That(cut.FindAll("[data-testid=card-aside]"), Is.Empty);
    }
}
