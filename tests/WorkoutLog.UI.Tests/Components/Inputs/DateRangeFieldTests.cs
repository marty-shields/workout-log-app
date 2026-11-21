using WorkoutLog.UI.Components.Inputs;
using WorkoutLog.UI.Models.Workouts;
using WorkoutLog.UI.Tests.Models.Workouts;

namespace WorkoutLog.UI.Tests.Components.Inputs;

public class DateRangeFieldTests : ComponentTestBase
{
    private static readonly DateOnly Today = new(2026, 11, 6);

    private static readonly WorkoutHistory History = Histories.Of([new(1, new DateTime(2025, 3, 12, 18, 0, 0))], Today);

    [TestCase(2026, 8, 6, 2026, 11, 6, "6\u00A0Aug – 6\u00A0Nov\u00A02026", "6 Aug – 6 Nov 2026")]
    [TestCase(2025, 11, 6, 2026, 11, 6, "6\u00A0Nov\u00A02025 – 6\u00A0Nov\u00A02026", "6 Nov 2025 – 6 Nov 2026")]
    [TestCase(2025, 3, 12, 2025, 3, 12, "Wed\u00A012\u00A0Mar\u00A02025", "Wed 12 Mar 2025")]
    public void Field_ShowsTheDatesKeptWholeAndNamesThemWithPlainSpacesAndTheCount(int fromYear, int fromMonth, int fromDay, int toYear, int toMonth, int toDay, string shown, string text)
    {
        var cut = Render(new DateSpan(new(fromYear, fromMonth, fromDay), new(toYear, toMonth, toDay)), count: 1);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=dates-dates]").TextContent, Is.EqualTo(shown));
            Assert.That(cut.Find("[data-testid=dates-count]").TextContent, Is.EqualTo("1"));
            Assert.That(cut.Find("[data-testid=dates]").GetAttribute("aria-label"), Is.EqualTo($"Dates shown: {text}. 1 workout"));
            Assert.That(cut.Find("[data-testid=dates]").GetAttribute("aria-expanded"), Is.EqualTo("false"));
            Assert.That(cut.FindAll("[data-testid=range]"), Is.Empty);
        });
    }

    [Test]
    public void AccessibleName_CountsWorkoutsInThePlural()
    {
        var cut = Render(new DateSpan(new(2026, 8, 6), Today), count: 38);

        Assert.That(cut.Find("[data-testid=dates]").GetAttribute("aria-label"), Is.EqualTo("Dates shown: 6 Aug – 6 Nov 2026. 38 workouts"));
    }

    [TestCase(false, true)]
    [TestCase(true, false)]
    public void TappingTheField_RaisesExpandedChangedWithTheOtherState(bool expanded, bool raised)
    {
        var changes = new List<bool>();
        var cut = Render(new DateSpan(Today, Today), expanded: expanded, expandedChanged: changes.Add);

        cut.Find("[data-testid=dates]").Click();

        Assert.That(changes, Is.EqualTo(new[] { raised }));
    }

    [Test]
    public void Expanded_ShowsTheCalendar()
    {
        var cut = Render(new DateSpan(Today, Today), expanded: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=dates]").GetAttribute("aria-expanded"), Is.EqualTo("true"));
            Assert.That(cut.FindAll("[data-testid=range]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Expanded_ShowsTheCurrentDatesInTheFromAndToBoxes()
    {
        var cut = Render(new DateSpan(new(2026, 8, 6), Today), expanded: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-from-value]").TextContent, Is.EqualTo("Thu 6 Aug 26"));
            Assert.That(cut.Find("[data-testid=range-to-value]").TextContent, Is.EqualTo("Fri 6 Nov 26"));
        });
    }

    [Test]
    public void Expanded_DisablesDaysBeforeTheFirstWorkoutAndAfterToday()
    {
        var cut = Render(new DateSpan(new(2026, 11, 1), Today), expanded: true);

        cut.Find("[data-testid=range-month-name]").Click();
        cut.Find("[data-testid=range-prev-year]").Click();
        cut.Find("[data-testid=range-month-2025-03]").Click();
        var before = cut.Find("[data-testid=range-day-2025-03-11]").HasAttribute("disabled");
        var first = cut.Find("[data-testid=range-day-2025-03-12]").HasAttribute("disabled");
        cut.Find("[data-testid=range-month-name]").Click();
        cut.Find("[data-testid=range-next-year]").Click();
        cut.Find("[data-testid=range-month-2026-11]").Click();
        var today = cut.Find("[data-testid=range-day-2026-11-06]").HasAttribute("disabled");
        var after = cut.Find("[data-testid=range-day-2026-11-07]").HasAttribute("disabled");

        Assert.Multiple(() =>
        {
            Assert.That(before, Is.True, "the day before the first workout");
            Assert.That(first, Is.False, "the first workout's day");
            Assert.That(today, Is.False, "today");
            Assert.That(after, Is.True, "the day after today");
        });
    }

    [Test]
    public void Expanded_MarksAWorkoutDayAndCountsTheMonthsWorkouts()
    {
        var cut = Render(new DateSpan(new(2025, 3, 1), new(2025, 3, 31)), expanded: true);

        cut.Find("[data-testid=range-month-name]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-month-2025-03]").GetAttribute("aria-label"), Is.EqualTo("March 2025, 1 workout"));
            Assert.That(cut.Find("[data-testid=range-month-count-2025-03]").TextContent, Is.EqualTo("1"));
            Assert.That(cut.Find("[data-testid=range-month-count-2025-04]").TextContent, Is.EqualTo("0"));
        });

        cut.Find("[data-testid=range-month-2025-03]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=range-day-2025-03-12] .visually-hidden").TextContent, Does.Contain("workout logged"));
            Assert.That(cut.Find("[data-testid=range-day-2025-03-13] .visually-hidden").TextContent, Does.Not.Contain("workout logged"));
        });
    }

    [Test]
    public void ShowWorkouts_ClosesThenRaisesDatesChanged()
    {
        var events = new List<string>();
        var cut = Render(new DateSpan(new(2026, 8, 6), Today), expanded: true, expandedChanged: e => events.Add($"expanded {e}"),
            datesChanged: d => events.Add($"dates {d.From:yyyy-MM-dd} {d.To:yyyy-MM-dd}"));

        cut.Find("[data-testid=range-day-2026-11-02]").Click();
        cut.Find("[data-testid=range-apply]").Click();

        Assert.That(events, Is.EqualTo(new[] { "expanded False", "dates 2026-11-02 2026-11-02" }));
    }

    [Test]
    public void Cancel_ClosesWithoutChangingTheDates()
    {
        var events = new List<string>();
        var cut = Render(new DateSpan(new(2026, 8, 6), Today), expanded: true, expandedChanged: e => events.Add($"expanded {e}"),
            datesChanged: _ => events.Add("dates"));

        cut.Find("[data-testid=range-day-2026-11-02]").Click();
        cut.Find("[data-testid=range-cancel]").Click();

        Assert.That(events, Is.EqualTo(new[] { "expanded False" }));
    }

    [Test]
    public async Task AndroidBack_WhileExpanded_Closes()
    {
        var changes = new List<bool>();
        var cut = Render(new DateSpan(Today, Today), expanded: true, expandedChanged: changes.Add);

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(changes, Is.EqualTo(new[] { false }));
    }

    [Test]
    public void Closed_DoesNotHandleAndroidBack()
    {
        Render(new DateSpan(Today, Today));

        Assert.That(BackButton.HasHandler, Is.False);
    }

    private IRenderedComponent<DateRangeField> Render(
        DateSpan dates, int count = 0, bool expanded = false, Action<bool>? expandedChanged = null, Action<DateSpan>? datesChanged = null) =>
        Context.Render<DateRangeField>(p => p
            .Add(x => x.Dates, dates)
            .Add(x => x.Count, count)
            .Add(x => x.History, History)
            .Add(x => x.Expanded, expanded)
            .Add(x => x.ExpandedChanged, expandedChanged ?? (_ => { }))
            .Add(x => x.DatesChanged, datesChanged ?? (_ => { }))
            .Add(x => x.TestId, "dates"));
}
