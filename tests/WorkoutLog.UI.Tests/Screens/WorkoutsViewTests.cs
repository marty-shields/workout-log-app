using WorkoutLog.Core.Domain;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Workouts;
using WorkoutLog.UI.Navigation;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Tests.Models.Workouts;

namespace WorkoutLog.UI.Tests.Screens;

public class WorkoutsViewTests : ComponentTestBase
{
    private static readonly DateOnly Today = new(2026, 11, 6);

    private static readonly WorkoutsRow[] All =
    [
        new(5, new DateTime(2026, 11, 5, 18, 5, 0)),
        new(4, new DateTime(2026, 11, 2, 7, 30, 0)),
        new(3, new DateTime(2026, 10, 20, 18, 5, 0)),
        new(2, new DateTime(2026, 5, 1, 17, 0, 0)),
        new(1, new DateTime(2025, 3, 12, 18, 0, 0)),
    ];

    private static readonly WorkoutHistory History = Histories.Of(All, Today);

    private static readonly DateSpan ThreeMonths = History.Span(HistoryRange.ThreeMonths);

    [Test]
    public void Rows_ShowDayDateAndStartTimeWithoutTheYearInGivenOrder()
    {
        var cut = Render(ThreeMonths);

        Assert.That(cut.FindAll(".rows .text").Select(t => t.TextContent),
            Is.EqualTo(new[] { "Thu 5 Nov · 18:05", "Mon 2 Nov · 07:30", "Tue 20 Oct · 18:05" }));
    }

    [Test]
    public void Totals_ShowAsACardBetweenTheDateFieldAndTheMonths()
    {
        var cut = Render(ThreeMonths, p => p.Add(x => x.Totals, new WorkoutTotals(3, 30, TimeSpan.FromMinutes(150), 2.5)));

        var order = cut.FindAll("[data-testid=workouts-dates], [data-testid=workout-totals], [data-testid^=workouts-month-]")
            .Select(e => e.GetAttribute("data-testid"));
        Assert.Multiple(() =>
        {
            Assert.That(order, Is.EqualTo(new[] { "workouts-dates", "workout-totals", "workouts-month-2026-11", "workouts-month-2026-10" }));
            Assert.That(cut.Find("[data-testid=workout-totals-workouts]").TextContent, Is.EqualTo("3"));
            Assert.That(cut.Find("[data-testid=workout-totals-time]").TextContent, Is.EqualTo("2 h 30 min"));
        });
    }

    [Test]
    public void NoTotals_HideTheCardAndKeepTheRows()
    {
        var cut = Render(ThreeMonths);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=workout-totals]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=workouts-row-]"), Has.Count.EqualTo(3));
        });
    }

    [Test]
    public void NoWorkoutsInTheDates_HideTheCard()
    {
        var cut = Render(new DateSpan(new(2026, 9, 1), new(2026, 9, 30)), p => p.Add(x => x.Totals, new WorkoutTotals(0, 0, TimeSpan.Zero, 0)));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=workout-totals]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workouts-none]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Rows_SitUnderAHeaderPerMonthWithItsCount()
    {
        var cut = Render(ThreeMonths);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("h2.month-title").Select(h => $"{h.Children[0].TextContent} · {h.Children[1].TextContent}"),
                Is.EqualTo(new[] { "November 2026 · 2 workouts", "October 2026 · 1 workout" }));
            Assert.That(cut.FindAll("[data-testid=workouts-month-2026-11] ~ .rows [data-testid^=workouts-row-]").Select(r => r.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "workouts-row-5", "workouts-row-4" }));
        });
    }

    [Test]
    public void Dates_ShowOnlyTheWorkoutsInThem()
    {
        var cut = Render(History.Span(HistoryRange.Year));

        Assert.That(cut.FindAll("[data-testid^=workouts-row-]").Select(r => r.GetAttribute("data-testid")),
            Is.EqualTo(new[] { "workouts-row-5", "workouts-row-4", "workouts-row-3", "workouts-row-2" }));
    }

    [TestCase(HistoryRange.ThreeMonths, "workouts-range-3m")]
    [TestCase(HistoryRange.SixMonths, "workouts-range-6m")]
    [TestCase(HistoryRange.Year, "workouts-range-1y")]
    [TestCase(HistoryRange.All, "workouts-range-all")]
    public void DatesOfARange_CheckItsSegment(HistoryRange range, string testId)
    {
        var cut = Render(History.Span(range));

        Assert.That(cut.FindAll("[data-testid=workouts-range] input[checked]").Select(i => i.GetAttribute("data-testid")), Is.EqualTo(new[] { testId }));
    }

    [Test]
    public void DatesPicked_CheckNoSegment()
    {
        var cut = Render(new DateSpan(new(2026, 10, 1), Today));

        Assert.That(cut.FindAll("[data-testid=workouts-range] input[checked]"), Is.Empty);
    }

    [Test]
    public void Segments_ReadAsWordsToScreenReaders()
    {
        var cut = Render(ThreeMonths);

        Assert.That(cut.FindAll("[data-testid=workouts-range] input").Select(i => i.GetAttribute("aria-label")),
            Is.EqualTo(new[] { "Last 3 months", "Last 6 months", "Last year", null }));
    }

    [Test]
    public void TappingASegment_RaisesDatesChangedWithItsRangeUpToToday()
    {
        var changed = new List<DateSpan>();
        var cut = Render(ThreeMonths, p => p.Add(x => x.DatesChanged, (DateSpan d) => changed.Add(d)));

        cut.Find("[data-testid=workouts-range-1y]").Change(true);
        cut.Find("[data-testid=workouts-range-all]").Change(true);

        Assert.That(changed, Is.EqualTo(new[] { new DateSpan(new(2025, 11, 6), Today), new DateSpan(new(2025, 3, 12), Today) }));
    }

    [Test]
    public void Field_ShowsTheDatesAndTheirCount()
    {
        var cut = Render(ThreeMonths);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workouts-dates-dates]").TextContent, Is.EqualTo("6\u00A0Aug – 6\u00A0Nov\u00A02026"));
            Assert.That(cut.Find("[data-testid=workouts-dates-count]").TextContent, Is.EqualTo("3"));
        });
    }

    [TestCase(2026, 9, 1, 2026, 9, 30, "No workouts between these dates")]
    [TestCase(2026, 11, 4, 2026, 11, 4, "No workout on this day")]
    public void NothingInTheDates_SaysSoWithAHint(int fromYear, int fromMonth, int fromDay, int toYear, int toMonth, int toDay, string message)
    {
        var cut = Render(new DateSpan(new(fromYear, fromMonth, fromDay), new(toYear, toMonth, toDay)));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workouts-none-message]").TextContent, Is.EqualTo(message));
            Assert.That(cut.Find("[data-testid=workouts-none-hint]").TextContent, Is.EqualTo("Choose different dates or a quick range above"));
            Assert.That(cut.FindAll("[data-testid=workouts-none] button"), Is.Empty);
            Assert.That(cut.FindAll(".rows"), Is.Empty);
        });
    }

    [Test]
    public void Expanded_ShowsTheCalendarInsteadOfTheSegmentsAndRows()
    {
        var cut = Render(ThreeMonths, p => p.Add(x => x.Expanded, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workouts-dates]").GetAttribute("aria-expanded"), Is.EqualTo("true"));
            Assert.That(cut.FindAll("[data-testid=range-calendar]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=workouts-range]"), Is.Empty);
            Assert.That(cut.FindAll(".rows"), Is.Empty);
        });
    }

    [Test]
    public void NoWorkouts_HasNoFieldOrSegments()
    {
        var cut = Context.Render<WorkoutsView>(p => p.Add(x => x.History, new WorkoutHistory(new Dictionary<DateOnly, int>(), Today)));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=workouts-dates]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workouts-range]"), Is.Empty);
        });
    }

    [Test]
    public void NoRows_ShowsNoWorkoutsYet()
    {
        var cut = Context.Render<WorkoutsView>(p => p.Add(x => x.History, new WorkoutHistory(new Dictionary<DateOnly, int>(), Today)));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workouts-empty]").TextContent, Does.Contain("No workouts yet"));
            Assert.That(cut.Find("[data-testid=workouts-empty-hint]").TextContent, Is.EqualTo("Tap New Workout on the Dashboard to log one"));
        });
    }

    [Test]
    public void WithOnOpen_TappingRowRaisesIt()
    {
        WorkoutsRow? opened = null;
        var cut = Render(ThreeMonths, p => p.Add(x => x.OnOpen, (WorkoutsRow row) => opened = row));

        cut.Find("[data-testid=workouts-row-4]").Click();

        Assert.That(opened, Is.EqualTo(All[1]));
    }

    [Test]
    public void Loading_ShowsTheSpinnerOnly()
    {
        var cut = Context.Render<WorkoutsView>(p => p.Add(x => x.Loading, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workouts-loading]").TextContent, Does.Contain("Loading…"));
            Assert.That(cut.FindAll(".rows"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workouts-empty]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workouts-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workouts-dates]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=bottom-nav]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void LoadingNewDates_KeepsTheDatesAndSegmentsOverTheSpinner()
    {
        var cut = Render(ThreeMonths, p => p.Add(x => x.Loading, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=workouts-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll(".rows"), Is.Empty);
            Assert.That(cut.Find("[data-testid=workouts-dates-dates]").TextContent, Is.EqualTo("6\u00A0Aug – 6\u00A0Nov\u00A02026"));
            Assert.That(cut.FindAll("[data-testid=workouts-range]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Failed_ShowsTheErrorAndTryAgainInsteadOfTheRows()
    {
        var cut = Context.Render<WorkoutsView>(p => p.Add(x => x.Failed, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workouts-error]").TextContent, Is.EqualTo("Couldn't load your workouts"));
            Assert.That(cut.Find("[data-testid=workouts-error]").GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(cut.Find("[data-testid=workouts-retry]").TextContent.Trim(), Is.EqualTo("Try again"));
            Assert.That(cut.FindAll("[data-testid=workouts-empty]"), Is.Empty);
        });
    }

    [Test]
    public void TryAgain_RaisesOnRetry()
    {
        var retries = 0;
        var cut = Context.Render<WorkoutsView>(p => p
            .Add(x => x.Failed, true)
            .Add(x => x.OnRetry, () => retries++));

        cut.Find("[data-testid=workouts-retry]").Click();

        Assert.That(retries, Is.EqualTo(1));
    }

    [Test]
    public void Failed_FocusesTryAgain()
    {
        var cut = Context.Render<WorkoutsView>(p => p.Add(x => x.Failed, true));

        cut.WaitForAssertion(() => Focuses().Single().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=workouts-retry]")));
    }

    [Test]
    public void Failed_RenderedAgain_DoesNotFocusAgain()
    {
        var cut = Context.Render<WorkoutsView>(p => p.Add(x => x.Failed, true));
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Failed, true));

        Assert.That(Focuses(), Has.Count.EqualTo(1));
    }

    [Test]
    public void FailedAfterLoading_FocusesTryAgainAgain()
    {
        var cut = Context.Render<WorkoutsView>(p => p.Add(x => x.Failed, true));
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Loading, true));
        cut.Render(p => p.Add(x => x.Loading, false));

        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(2)));
    }

    [Test]
    public void IsTopLevel_WithNoBackArrowAndItsNavigationItemCurrent()
    {
        var cut = Render(ThreeMonths);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=back]"), Is.Empty);
            Assert.That(cut.Find("[aria-current=page]").GetAttribute("data-testid"), Is.EqualTo("workouts-link"));
        });
    }

    [Test]
    public async Task AndroidBack_NavigatesToTheDashboard()
    {
        var navigated = new List<NavDestination>();
        var cut = Render(ThreeMonths, p => p.Add(x => x.OnNavigate, navigated.Add));

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(navigated, Is.EqualTo(new[] { NavDestination.Dashboard }));
    }

    [Test]
    public void NavigationBar_RaisesOnNavigate()
    {
        var navigated = new List<NavDestination>();
        var cut = Render(ThreeMonths, p => p.Add(x => x.OnNavigate, navigated.Add));

        cut.Find("[data-testid=exercises-link]").Click();

        Assert.That(navigated, Is.EqualTo(new[] { NavDestination.Exercises }));
    }

    [Test]
    public async Task AndroidBack_WhileExpanded_ClosesTheCalendarAndStays()
    {
        var navigated = new List<NavDestination>();
        var expanded = new List<bool>();
        var cut = Render(ThreeMonths, p => p
            .Add(x => x.Expanded, true)
            .Add(x => x.ExpandedChanged, expanded.Add)
            .Add(x => x.OnNavigate, navigated.Add));

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(expanded, Is.EqualTo(new[] { false }));
            Assert.That(navigated, Is.Empty);
        });
    }

    private IRenderedComponent<WorkoutsView> Render(DateSpan dates, Action<ComponentParameterCollectionBuilder<WorkoutsView>>? more = null) =>
        Context.Render<WorkoutsView>(p =>
        {
            p.Add(x => x.History, History).Add(x => x.Rows, Histories.In(All, dates)).Add(x => x.Dates, dates);
            more?.Invoke(p);
        });

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];
}
