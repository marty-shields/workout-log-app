using WorkoutLog.UI.Components.Trends;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.Trends;

public class MonthWeeksTests : ComponentTestBase
{
    [Test]
    public void Row_ShowsDatesWorkoutsAndTime()
    {
        var cut = Render(SampleTrends.MonthWeeks);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".week"), Has.Count.EqualTo(4));
            Assert.That(cut.Find("[data-testid=weeks-3] .dates").TextContent, Is.EqualTo("9 – 15 Nov"));
            Assert.That(cut.Find("[data-testid=weeks-3] .totals").TextContent, Is.EqualTo("4 workouts · 4h 10m"));
            Assert.That(cut.Find("[data-testid=weeks-4] .totals").TextContent, Is.EqualTo("3 workouts · 2h 35m"));
        });
    }

    [Test]
    public void WeekWithNothing_ShowsZeros() =>
        Assert.That(Render(SampleTrends.MonthWeeks).Find("[data-testid=weeks-1] .totals").TextContent, Is.EqualTo("0 workouts · 0m"));

    [Test]
    public void OneWorkout_IsSingular() =>
        Assert.That(Render([new MonthWeek("1 Nov", 1, new TimeSpan(0, 45, 0))]).Find(".totals").TextContent, Is.EqualTo("1 workout · 45m"));

    [Test]
    public void List_IsNamed() =>
        Assert.That(Render(SampleTrends.MonthWeeks).Find("ul").GetAttribute("aria-label"), Is.EqualTo("Workouts and time for each week"));

    [Test]
    public void NoWeeks_ShowsTheMessage()
    {
        var cut = Render([]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=weeks-none]").TextContent, Is.EqualTo("No weeks yet"));
            Assert.That(cut.FindAll("li"), Is.Empty);
        });
    }

    private IRenderedComponent<MonthWeeks> Render(IReadOnlyList<MonthWeek> weeks) =>
        Context.Render<MonthWeeks>(p => p.Add(x => x.Weeks, weeks).Add(x => x.TestId, "weeks"));
}
