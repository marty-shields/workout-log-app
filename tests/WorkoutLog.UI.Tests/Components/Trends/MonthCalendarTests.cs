using WorkoutLog.UI.Components.Trends;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.Trends;

public class MonthCalendarTests : ComponentTestBase
{
    [Test]
    public void ShowsEveryDayOfTheMonth()
    {
        var cut = Render(new DateOnly(2026, 11, 1), SampleTrends.MonthTrainedDays);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".day"), Has.Count.EqualTo(30));
            Assert.That(cut.Find("ul.days").GetAttribute("aria-label"), Is.EqualTo("Days in November 2026"));
        });
    }

    [TestCase(11, "grid-column-start: 7")]
    [TestCase(10, "grid-column-start: 4")]
    [TestCase(6, "grid-column-start: 1")]
    public void TheFirstDayStartsInItsWeekdayColumn(int month, string expected)
    {
        // 1 Nov 2026 is a Sunday, 1 Oct a Thursday and 1 Jun a Monday.
        var cut = Render(new DateOnly(2026, month, 1), []);

        Assert.That(cut.Find("[data-testid=cal-1]").GetAttribute("style"), Is.EqualTo(expected));
    }

    [Test]
    public void TrainedDays_AreSolidAndSaySo()
    {
        var cut = Render(new DateOnly(2026, 11, 1), SampleTrends.MonthTrainedDays);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".done"), Has.Count.EqualTo(11));
            Assert.That(cut.Find("[data-testid=cal-16]").ClassList, Does.Contain("done"));
            Assert.That(cut.Find("[data-testid=cal-16] .visually-hidden").TextContent, Is.EqualTo("Monday 16 November, trained"));
            Assert.That(cut.Find("[data-testid=cal-3]").ClassList, Does.Not.Contain("done"));
            Assert.That(cut.Find("[data-testid=cal-3] .visually-hidden").TextContent, Is.EqualTo("Tuesday 3 November, no workout"));
        });
    }

    [Test]
    public void Today_IsRingedAndMarkedAsTheCurrentDate()
    {
        var cut = Render(new DateOnly(2026, 11, 1), SampleTrends.MonthTrainedDays);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".today"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=cal-18]").GetAttribute("aria-current"), Is.EqualTo("date"));
            Assert.That(cut.Find("[data-testid=cal-18] .visually-hidden").TextContent, Is.EqualTo("Wednesday 18 November, trained, today"));
        });
    }

    [Test]
    public void DaysAfterToday_AreDimmedAndSaidToBeComingUp()
    {
        var cut = Render(new DateOnly(2026, 11, 1), SampleTrends.MonthTrainedDays);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".future"), Has.Count.EqualTo(12));
            Assert.That(cut.Find("[data-testid=cal-19] .visually-hidden").TextContent, Is.EqualTo("Thursday 19 November, coming up"));
            Assert.That(cut.Find("[data-testid=cal-18]").ClassList, Does.Not.Contain("future"));
        });
    }

    [Test]
    public void FinishedMonth_HasNoRingAndNoDimmedDays()
    {
        var cut = Render(new DateOnly(2026, 10, 1), SampleTrends.PastMonthTrainedDays);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".today"), Is.Empty);
            Assert.That(cut.FindAll(".future"), Is.Empty);
            Assert.That(cut.FindAll(".day"), Has.Count.EqualTo(31));
            Assert.That(cut.FindAll(".done"), Has.Count.EqualTo(15));
        });
    }

    [Test]
    public void MonthAfterToday_HasEveryDayDimmedAndNoRing()
    {
        var cut = Render(new DateOnly(2026, 12, 1), []);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".day"), Has.Count.EqualTo(31));
            Assert.That(cut.FindAll(".future"), Has.Count.EqualTo(31));
            Assert.That(cut.FindAll(".today"), Is.Empty);
        });
    }

    [Test]
    public void Legend_NamesTrainedNoWorkoutAndToday() =>
        Assert.That(Render(new DateOnly(2026, 11, 1), []).FindAll(".legend li").Select(li => li.TextContent),
            Is.EqualTo(new[] { "Trained", "No workout", "Today" }));

    private IRenderedComponent<MonthCalendar> Render(DateOnly month, IReadOnlyCollection<DateOnly> trained) =>
        Context.Render<MonthCalendar>(p => p
            .Add(x => x.Month, month)
            .Add(x => x.Today, SampleTrends.Today)
            .Add(x => x.Trained, trained)
            .Add(x => x.TestId, "cal"));
}
