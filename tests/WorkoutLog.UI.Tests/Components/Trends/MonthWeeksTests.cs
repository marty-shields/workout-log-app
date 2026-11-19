using WorkoutLog.UI.Components.Trends;
using WorkoutLog.UI.Models.Workouts;
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

    [Test]
    public void NoOnOpen_NoRowIsTappable() =>
        Assert.That(Render([new MonthWeek("2 – 8 Nov", 3, new TimeSpan(2, 0, 0), Week)]).FindAll("button"), Is.Empty);

    [Test]
    public void WithOnOpen_OnlyRowsWithWorkoutsAndDatesAreTappableAndRaiseTheirDates()
    {
        var opened = new List<DateSpan>();
        var cut = Context.Render<MonthWeeks>(p => p
            .Add(x => x.Weeks, new[]
            {
                new MonthWeek("1 Nov", 0, TimeSpan.Zero, new DateSpan(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 1))),
                new MonthWeek("2 – 8 Nov", 3, new TimeSpan(2, 0, 0), Week),
                new MonthWeek("9 – 15 Nov", 2, new TimeSpan(1, 0, 0)),
            })
            .Add(x => x.OnOpen, opened.Add)
            .Add(x => x.TestId, "weeks"));

        cut.Find("[data-testid=weeks-2-open]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("button").Select(b => b.GetAttribute("data-testid")), Is.EqualTo(new[] { "weeks-2-open" }));
            Assert.That(opened, Is.EqualTo(new[] { Week }));
            Assert.That(cut.Find("[data-testid=weeks-2-open] .dates").TextContent, Is.EqualTo("2 – 8 Nov"));
        });
    }

    private static readonly DateSpan Week = new(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 8));

    private IRenderedComponent<MonthWeeks> Render(IReadOnlyList<MonthWeek> weeks) =>
        Context.Render<MonthWeeks>(p => p.Add(x => x.Weeks, weeks).Add(x => x.TestId, "weeks"));
}
