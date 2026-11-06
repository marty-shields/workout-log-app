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
            Assert.That(cut.FindAll("[data-testid=range-calendar]"), Is.Empty);
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
            Assert.That(cut.FindAll("[data-testid=range-calendar]"), Has.Count.EqualTo(1));
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
