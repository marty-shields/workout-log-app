using WorkoutLog.UI.Components.ThisWeek;
using WorkoutLog.UI.Components.Weekly;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.ThisWeek;

public class LastWeekCardTests : ComponentTestBase
{
    [Test]
    public void Row_ShowsLastWeeksWorkoutsTimeAndSets()
    {
        var cut = Context.Render<LastWeekCard>(p => p.Add(x => x.Totals, SampleThisWeek.LastWeek).Add(x => x.TestId, "last"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".text").TextContent, Is.EqualTo("Last week"));
            Assert.That(cut.Find(".detail").TextContent, Is.EqualTo("4 workouts · 4h 10m · 67 sets"));
        });
    }

    [Test]
    public void SingularCounts_AreSingular()
    {
        var cut = Context.Render<LastWeekCard>(p => p.Add(x => x.Totals, new WeekTotals(1, new TimeSpan(0, 45, 0), 1)));

        Assert.That(cut.Find(".detail").TextContent, Is.EqualTo("1 workout · 45m · 1 set"));
    }

    [Test]
    public void TappingTheRow_RaisesOnOpen()
    {
        var opened = 0;
        var cut = Context.Render<LastWeekCard>(p => p
            .Add(x => x.Totals, SampleThisWeek.LastWeek)
            .Add(x => x.OnOpen, () => opened++)
            .Add(x => x.TestId, "last"));

        cut.Find("[data-testid=last-row]").Click();

        Assert.That(opened, Is.EqualTo(1));
    }

    [Test]
    public void WithoutOnOpen_TheRowIsNotAButton()
    {
        var cut = Context.Render<LastWeekCard>(p => p.Add(x => x.Totals, SampleThisWeek.LastWeek));

        Assert.That(cut.FindAll("button"), Is.Empty);
    }
}
