using WorkoutLog.UI.Components.Inputs;
using WorkoutLog.UI.Models.Workouts;
using WorkoutLog.UI.Tests.Models.Workouts;

namespace WorkoutLog.UI.Tests.Components.Inputs;

public class RangeCalendarTests : ComponentTestBase
{
    private static readonly DateOnly Today = new(2026, 11, 6);

    private static readonly WorkoutHistory History = Histories.Of(
    [
        new(4, new DateTime(2026, 11, 4, 18, 0, 0)),
        new(3, new DateTime(2025, 3, 12, 18, 0, 0)),
        new(2, new DateTime(2025, 3, 12, 7, 0, 0)),
        new(1, new DateTime(2025, 3, 3, 18, 0, 0)),
    ], Today);

    private static readonly DateSpan Autumn = new(new(2026, 8, 6), Today);

    [Test]
    public void Opens_OnTheMonthOfTheLastDayWithBothEndsSet()
    {
        var cut = Render(Autumn);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-month-name]").TextContent.Trim(), Is.EqualTo("November 2026"));
            Assert.That(cut.Find("[data-testid=range-from] .end-value").TextContent, Is.EqualTo("Thu 6 Aug 26"));
            Assert.That(cut.Find("[data-testid=range-to] .end-value").TextContent, Is.EqualTo("Fri 6 Nov 26"));
            Assert.That(cut.Find("[data-testid=range-from]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
            Assert.That(cut.Find("[data-testid=range-hint]").TextContent, Is.EqualTo("Tap the first day"));
        });
    }

    [Test]
    public void Grid_StartsTheWeekOnMonday()
    {
        var cut = Render(Autumn);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".weekday").Select(d => d.TextContent), Is.EqualTo(new[] { "M", "T", "W", "T", "F", "S", "S" }));
            Assert.That(cut.Find(".days").Children.TakeWhile(c => c.TagName == "SPAN").Count(), Is.EqualTo(7 + 6), "1 Nov 2026 is a Sunday");
        });
    }

    [Test]
    public void FirstTap_SetsFromClearsToAndMovesOnToTo()
    {
        var cut = Render(Autumn);

        cut.Find("[data-testid=range-day-2026-11-02]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-from] .end-value").TextContent, Is.EqualTo("Mon 2 Nov 26"));
            Assert.That(cut.Find("[data-testid=range-to] .end-value").TextContent, Is.EqualTo("Pick a day"));
            Assert.That(cut.Find("[data-testid=range-to]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
            Assert.That(cut.Find("[data-testid=range-hint]").TextContent, Is.EqualTo("Tap the last day, or the same day again for one day"));
        });
    }

    [Test]
    public void SecondTap_SetsTo()
    {
        var applied = new List<DateSpan>();
        var cut = Render(Autumn, applied.Add);

        cut.Find("[data-testid=range-day-2026-11-02]").Click();
        cut.Find("[data-testid=range-day-2026-11-05]").Click();
        cut.Find("[data-testid=range-apply]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(applied, Is.EqualTo(new[] { new DateSpan(new(2026, 11, 2), new(2026, 11, 5)) }));
            Assert.That(cut.Find("[data-testid=range-from]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void SecondTapBeforeFrom_SwapsTheEnds()
    {
        var applied = new List<DateSpan>();
        var cut = Render(Autumn, applied.Add);

        cut.Find("[data-testid=range-day-2026-11-05]").Click();
        cut.Find("[data-testid=range-day-2026-11-02]").Click();
        cut.Find("[data-testid=range-apply]").Click();

        Assert.That(applied, Is.EqualTo(new[] { new DateSpan(new(2026, 11, 2), new(2026, 11, 5)) }));
    }

    [Test]
    public void SameDayTwice_PicksOneDay()
    {
        var applied = new List<DateSpan>();
        var cut = Render(Autumn, applied.Add);

        cut.Find("[data-testid=range-day-2026-11-04]").Click();
        cut.Find("[data-testid=range-day-2026-11-04]").Click();
        cut.Find("[data-testid=range-apply]").Click();

        Assert.That(applied, Is.EqualTo(new[] { new DateSpan(new(2026, 11, 4), new(2026, 11, 4)) }));
    }

    [Test]
    public void ShowWorkouts_AfterOneTap_PicksThatDay()
    {
        var applied = new List<DateSpan>();
        var cut = Render(Autumn, applied.Add);

        cut.Find("[data-testid=range-day-2026-11-04]").Click();
        cut.Find("[data-testid=range-apply]").Click();

        Assert.That(applied, Is.EqualTo(new[] { new DateSpan(new(2026, 11, 4), new(2026, 11, 4)) }));
    }

    [Test]
    public void Days_FillTheRangeAndMarkBothEndsAndToday()
    {
        var cut = Render(new DateSpan(new(2026, 11, 2), new(2026, 11, 4)));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".day.in").Select(d => d.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "range-day-2026-11-02", "range-day-2026-11-03", "range-day-2026-11-04" }));
            Assert.That(cut.Find(".day.first").GetAttribute("data-testid"), Is.EqualTo("range-day-2026-11-02"));
            Assert.That(cut.Find(".day.last").GetAttribute("data-testid"), Is.EqualTo("range-day-2026-11-04"));
            Assert.That(cut.Find(".day.today").GetAttribute("data-testid"), Is.EqualTo("range-day-2026-11-06"));
        });
    }

    [Test]
    public void Days_WithAWorkout_HaveADotAndSayItToScreenReaders()
    {
        var cut = Render(Autumn);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-day-2026-11-04] .visually-hidden").TextContent, Is.EqualTo("Wed 4 Nov, workout logged"));
            Assert.That(cut.Find("[data-testid=range-day-2026-11-04] .number").GetAttribute("aria-hidden"), Is.EqualTo("true"));
            Assert.That(cut.FindAll("[data-testid=range-day-2026-11-04] .dot"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=range-day-2026-11-03] .visually-hidden").TextContent, Is.EqualTo("Tue 3 Nov"));
            Assert.That(cut.FindAll("[data-testid=range-day-2026-11-03] .dot"), Is.Empty);
        });
    }

    [Test]
    public void Days_AfterTodayOrBeforeTheFirstWorkout_AreDisabled()
    {
        var cut = Render(new DateSpan(new(2025, 3, 3), new(2025, 3, 12)));
        var november = Render(Autumn);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-day-2025-03-02]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=range-day-2025-03-03]").HasAttribute("disabled"), Is.False);
            Assert.That(november.Find("[data-testid=range-day-2026-11-06]").HasAttribute("disabled"), Is.False);
            Assert.That(november.Find("[data-testid=range-day-2026-11-07]").HasAttribute("disabled"), Is.True);
        });
    }

    [Test]
    public void MonthArrows_StopAtTheFirstWorkoutAndThisMonth()
    {
        var cut = Render(Autumn);
        var march = Render(new DateSpan(new(2025, 3, 3), new(2025, 3, 12)));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-next-month]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=range-prev-month]").HasAttribute("disabled"), Is.False);
            Assert.That(march.Find("[data-testid=range-prev-month]").HasAttribute("disabled"), Is.True);
            Assert.That(march.Find("[data-testid=range-next-month]").HasAttribute("disabled"), Is.False);
        });
    }

    [Test]
    public void PreviousMonth_ShowsIt()
    {
        var cut = Render(Autumn);

        cut.Find("[data-testid=range-prev-month]").Click();

        Assert.That(cut.Find("[data-testid=range-month-name]").TextContent.Trim(), Is.EqualTo("October 2026"));
    }

    [Test]
    public void TappingAnEndBox_ShowsItsMonthAndSetsItNext()
    {
        var cut = Render(Autumn);

        cut.Find("[data-testid=range-from]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-month-name]").TextContent.Trim(), Is.EqualTo("August 2026"));
            Assert.That(cut.Find("[data-testid=range-from]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
        });

        cut.Find("[data-testid=range-to]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-month-name]").TextContent.Trim(), Is.EqualTo("November 2026"));
            Assert.That(cut.Find("[data-testid=range-to]").GetAttribute("aria-pressed"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void ToBoxThenAnEarlierDay_SwapsTheEnds()
    {
        var applied = new List<DateSpan>();
        var cut = Render(new DateSpan(new(2026, 11, 4), new(2026, 11, 5)), applied.Add);

        cut.Find("[data-testid=range-to]").Click();
        cut.Find("[data-testid=range-day-2026-11-02]").Click();
        cut.Find("[data-testid=range-apply]").Click();

        Assert.That(applied, Is.EqualTo(new[] { new DateSpan(new(2026, 11, 2), new(2026, 11, 4)) }));
    }

    [Test]
    public void MonthName_OpensTheYearsMonthsWithTheirCounts()
    {
        var cut = Render(new DateSpan(new(2025, 3, 3), new(2025, 3, 12)));

        cut.Find("[data-testid=range-month-name]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-year]").TextContent, Is.EqualTo("2025"));
            Assert.That(cut.Find("[data-testid=range-month-2025-03]").GetAttribute("aria-label"), Is.EqualTo("March 2025, 3 workouts"));
            Assert.That(cut.Find("[data-testid=range-month-2025-03] .month-count").TextContent, Is.EqualTo("3"));
            Assert.That(cut.Find("[data-testid=range-month-2025-03]").GetAttribute("aria-current"), Is.EqualTo("true"));
            Assert.That(cut.Find("[data-testid=range-month-2025-04] .month-count").TextContent, Is.EqualTo("0"));
            Assert.That(cut.Find("[data-testid=range-hint]").TextContent, Is.EqualTo("Choose a month"));
            Assert.That(cut.FindAll(".days"), Is.Empty);
        });
    }

    [Test]
    public void Months_OutsideTheHistory_AreDisabledAndShowNoCount()
    {
        var cut = Render(new DateSpan(new(2025, 3, 3), new(2025, 3, 12)), openOnMonths: true);
        var thisYear = Render(Autumn, openOnMonths: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-month-2025-02]").HasAttribute("disabled"), Is.True);
            Assert.That(cut.Find("[data-testid=range-month-2025-02] .month-count").TextContent, Is.EqualTo("–"));
            Assert.That(cut.Find("[data-testid=range-month-2025-03]").HasAttribute("disabled"), Is.False);
            Assert.That(cut.Find("[data-testid=range-prev-year]").HasAttribute("disabled"), Is.True);
            Assert.That(thisYear.Find("[data-testid=range-month-2026-11]").HasAttribute("disabled"), Is.False);
            Assert.That(thisYear.Find("[data-testid=range-month-2026-12]").HasAttribute("disabled"), Is.True);
            Assert.That(thisYear.Find("[data-testid=range-next-year]").HasAttribute("disabled"), Is.True);
        });
    }

    [Test]
    public void ChoosingAMonth_ShowsItsDays()
    {
        var cut = Render(Autumn, openOnMonths: true);

        cut.Find("[data-testid=range-prev-year]").Click();
        cut.Find("[data-testid=range-month-2025-03]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-month-name]").TextContent.Trim(), Is.EqualTo("March 2025"));
            Assert.That(cut.FindAll("[data-testid=range-day-2025-03-12]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Cancel_RaisesOnCancelOnly()
    {
        var applied = new List<DateSpan>();
        var cancels = 0;
        var cut = Context.Render<RangeCalendar>(p => p
            .Add(x => x.History, History)
            .Add(x => x.Dates, Autumn)
            .Add(x => x.OnApply, applied.Add)
            .Add(x => x.OnCancel, () => cancels++));

        cut.Find("[data-testid=range-day-2026-11-02]").Click();
        cut.Find("[data-testid=range-cancel]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cancels, Is.EqualTo(1));
            Assert.That(applied, Is.Empty);
        });
    }

    private IRenderedComponent<RangeCalendar> Render(DateSpan dates, Action<DateSpan>? apply = null, bool openOnMonths = false) =>
        Context.Render<RangeCalendar>(p => p
            .Add(x => x.History, History)
            .Add(x => x.Dates, dates)
            .Add(x => x.OpenOnMonths, openOnMonths)
            .Add(x => x.OnApply, apply ?? (_ => { })));
}
