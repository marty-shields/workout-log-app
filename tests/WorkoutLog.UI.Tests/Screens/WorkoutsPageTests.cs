using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Lifecycle;
using WorkoutLog.UI.Navigation;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class WorkoutsPageTests : ComponentTestBase
{
    private static readonly DateOnly Today = new(2026, 11, 6);

    private static readonly ReadResult<IReadOnlyDictionary<DateOnly, int>> TwoYears = new ReadResult<IReadOnlyDictionary<DateOnly, int>>(new Dictionary<DateOnly, int>
    {
        [new DateOnly(2026, 11, 2)] = 1,
        [new DateOnly(2026, 7, 6)] = 1,
        [new DateOnly(2025, 3, 12)] = 2,
    });

    private static readonly ReadResult<IReadOnlyDictionary<DateOnly, int>> OneMore = new ReadResult<IReadOnlyDictionary<DateOnly, int>>(new Dictionary<DateOnly, int>(TwoYears.Loaded())
    {
        [new DateOnly(2026, 11, 5)] = 1,
    });

    private static readonly ReadResult<IReadOnlyList<WorkoutSummary>> Recent = new ReadResult<IReadOnlyList<WorkoutSummary>>([new WorkoutSummary(4, new DateTimeOffset(2026, 11, 2, 17, 0, 0, TimeSpan.Zero))]);

    private static readonly ReadResult<IReadOnlyList<WorkoutSummary>> RecentAndYesterday = new ReadResult<IReadOnlyList<WorkoutSummary>>([
        new WorkoutSummary(5, new DateTimeOffset(2026, 11, 5, 17, 0, 0, TimeSpan.Zero)),
        .. Recent.Loaded(),
    ]);

    private FakeWorkoutsService workouts = null!;
    private FakeWorkoutTotalsService totals = null!;
    private FakeTimeProvider clock = null!;
    private FakeLogger<AppLifecycle> lifecycleLogger = null!;
    private AppLifecycle lifecycle = null!;

    [SetUp]
    public void SetUp()
    {
        Clock(new DateTimeOffset(2026, 11, 6, 9, 0, 0, TimeSpan.Zero));
        workouts = new FakeWorkoutsService();
        Context.Services.AddSingleton<IWorkoutsService>(workouts);
        totals = new FakeWorkoutTotalsService();
        Context.Services.AddSingleton<IWorkoutTotalsService>(totals);
        lifecycleLogger = new FakeLogger<AppLifecycle>();
        lifecycle = new AppLifecycle(lifecycleLogger);
        Context.Services.AddSingleton<IAppStart>(lifecycle);
    }

    [Test]
    public void WhileLoading_ShowsTheSpinnerOnly()
    {
        var cut = Context.Render<WorkoutsPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=workouts-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=workouts-dates]"), Is.Empty);
            Assert.That(cut.FindAll(".rows"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workouts-empty]"), Is.Empty);
            Assert.That(workouts.DayReads, Is.EqualTo(1));
            Assert.That(workouts.RangeReads, Is.Empty);
        });
    }

    [Test]
    public async Task Opens_ReadingTheLastThreeMonths()
    {
        var cut = await OpenedAsync();

        await CompleteRangeAsync(cut, Recent);

        Assert.Multiple(() =>
        {
            Assert.That(workouts.RangeReads, Is.EqualTo(new[] { (new DateOnly(2026, 8, 6), Today) }));
            Assert.That(cut.Find("[data-testid=workouts-dates-dates]").TextContent, Is.EqualTo("6\u00A0Aug – 6\u00A0Nov\u00A02026"));
            Assert.That(cut.Find("[data-testid=workouts-dates-count]").TextContent, Is.EqualTo("1"));
            Assert.That(Checked(cut), Is.EqualTo(new[] { "workouts-range-3m" }));
            Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-4" }));
        });
    }

    [Test]
    public async Task Rows_ShowInThePhonesTimeZoneInGivenOrder()
    {
        var cut = await OpenedAsync();

        await CompleteRangeAsync(cut, new ReadResult<IReadOnlyList<WorkoutSummary>>([
            new WorkoutSummary(2, new DateTimeOffset(2026, 10, 20, 23, 30, 0, TimeSpan.Zero)),
            new WorkoutSummary(1, new DateTimeOffset(2026, 10, 20, 17, 5, 0, TimeSpan.Zero)),
        ]));

        Assert.That(cut.FindAll(".rows .text").Select(t => t.TextContent), Is.EqualTo(new[] { "Wed 21 Oct · 00:30", "Tue 20 Oct · 18:05" }));
    }

    [Test]
    public async Task Today_IsThePhonesLocalDateNotUtcs()
    {
        Clock(new DateTimeOffset(2026, 11, 6, 23, 30, 0, TimeSpan.Zero));

        await OpenedAsync();

        Assert.That(workouts.RangeReads, Is.EqualTo(new[] { (new DateOnly(2026, 8, 7), new DateOnly(2026, 11, 7)) }));
    }

    [Test]
    public async Task Segment_ReadsItsRangeWithTheDatesStillShown()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        cut.Find("[data-testid=workouts-range-1y]").Change(true);

        Assert.Multiple(() =>
        {
            Assert.That(workouts.RangeReads[^1], Is.EqualTo((new DateOnly(2025, 11, 6), Today)));
            Assert.That(cut.FindAll("[data-testid=workouts-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=workouts-dates-dates]").TextContent, Is.EqualTo("6\u00A0Nov\u00A02025 – 6\u00A0Nov\u00A02026"));
            Assert.That(Checked(cut), Is.EqualTo(new[] { "workouts-range-1y" }));
        });

        await CompleteRangeAsync(cut, Recent);

        Assert.That(cut.FindAll("[data-testid=workouts-loading]"), Is.Empty);
    }

    [Test]
    public async Task All_ReadsFromTheFirstWorkout()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        cut.Find("[data-testid=workouts-range-all]").Change(true);
        await CompleteRangeAsync(cut, new ReadResult<IReadOnlyList<WorkoutSummary>>([]));

        Assert.Multiple(() =>
        {
            Assert.That(workouts.RangeReads[^1], Is.EqualTo((new DateOnly(2025, 3, 12), Today)));
            Assert.That(Checked(cut), Is.EqualTo(new[] { "workouts-range-all" }));
            Assert.That(cut.Find("[data-testid=workouts-dates-count]").TextContent, Is.EqualTo("4"));
        });
    }

    [Test]
    public async Task PickingDates_ReadsThemAndClearsTheSegment()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        cut.Find("[data-testid=workouts-dates]").Click();
        cut.Find("[data-testid=range-month-name]").Click();
        cut.Find("[data-testid=range-prev-year]").Click();
        cut.Find("[data-testid=range-month-2025-03]").Click();
        cut.Find("[data-testid=range-day-2025-03-12]").Click();
        cut.Find("[data-testid=range-day-2025-03-12]").Click();
        cut.Find("[data-testid=range-apply]").Click();
        await CompleteRangeAsync(cut, new ReadResult<IReadOnlyList<WorkoutSummary>>([new WorkoutSummary(1, new DateTimeOffset(2025, 3, 12, 17, 0, 0, TimeSpan.Zero))]));

        Assert.Multiple(() =>
        {
            Assert.That(workouts.RangeReads[^1], Is.EqualTo((new DateOnly(2025, 3, 12), new DateOnly(2025, 3, 12))));
            Assert.That(cut.Find("[data-testid=workouts-dates-dates]").TextContent, Is.EqualTo("Wed\u00A012\u00A0Mar\u00A02025"));
            Assert.That(cut.FindAll("[data-testid=range-calendar]"), Is.Empty);
            Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-1" }));
            Assert.That(Checked(cut), Is.Empty);
        });
    }

    [Test]
    public async Task Cancel_KeepsTheDatesAndReadsNothing()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);
        cut.Find("[data-testid=workouts-dates]").Click();
        cut.Find("[data-testid=range-day-2026-11-02]").Click();

        cut.Find("[data-testid=range-cancel]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.RangeReads, Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=workouts-dates-dates]").TextContent, Is.EqualTo("6\u00A0Aug – 6\u00A0Nov\u00A02026"));
            Assert.That(Checked(cut), Is.EqualTo(new[] { "workouts-range-3m" }));
        });
    }

    [Test]
    public async Task AnEarlierRangeAnsweringLast_IsIgnored()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);
        cut.Find("[data-testid=workouts-range-1y]").Change(true);
        cut.Find("[data-testid=workouts-range-all]").Change(true);

        workouts.CompleteRange(new ReadResult<IReadOnlyList<WorkoutSummary>>([new WorkoutSummary(1, new DateTimeOffset(2025, 3, 12, 17, 0, 0, TimeSpan.Zero))]));
        workouts.CompleteRange(Recent, index: 1);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-1" })));
        Assert.That(Checked(cut), Is.EqualTo(new[] { "workouts-range-all" }));
    }

    [Test]
    public async Task Opens_LoadsTheTotalsOfTheSameDatesAsTheList()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        Assert.Multiple(() =>
        {
            Assert.That(totals.Reads, Is.EqualTo(new[] { (new DateOnly(2026, 8, 6), Today) }));
            Assert.That(cut.Find("[data-testid=workout-totals-workouts]").TextContent, Is.EqualTo("3"));
            Assert.That(cut.Find("[data-testid=workout-totals-per-week]").TextContent, Is.EqualTo("2.5"));
        });
    }

    [Test]
    public async Task RangeChange_ReloadsTheTotalsWithTheList()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);
        totals.Result = new WorkoutTotals(9, 90, TimeSpan.FromMinutes(45), 0.5);

        cut.Find("[data-testid=workouts-range-1y]").Change(true);
        await CompleteRangeAsync(cut, Recent);

        Assert.Multiple(() =>
        {
            Assert.That(totals.Reads[^1], Is.EqualTo((new DateOnly(2025, 11, 6), Today)));
            Assert.That(cut.Find("[data-testid=workout-totals-workouts]").TextContent, Is.EqualTo("9"));
            Assert.That(cut.Find("[data-testid=workout-totals-time]").TextContent, Is.EqualTo("45 min"));
        });
    }

    [Test]
    public async Task TotalsFail_HideTheCardAndShowTheList()
    {
        totals.Result = new ReadFailed();
        var cut = await OpenedAsync();

        await CompleteRangeAsync(cut, Recent);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=workout-totals]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workouts-error]"), Is.Empty);
            Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-4" }));
        });
    }

    [Test]
    public async Task TotalsFailOnALaterRange_HideTheEarlierCardAndShowTheRows()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);
        Assert.That(cut.FindAll("[data-testid=workout-totals]"), Is.Not.Empty);
        totals.Result = new ReadFailed();

        cut.Find("[data-testid=workouts-range-1y]").Change(true);
        await CompleteRangeAsync(cut, Recent);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=workout-totals]"), Is.Empty);
            Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-4" }));
        });
    }

    [Test]
    public async Task NoWorkoutsInTheRange_HideTheCard()
    {
        var cut = await OpenedAsync();

        await CompleteRangeAsync(cut, new ReadResult<IReadOnlyList<WorkoutSummary>>([]));

        Assert.That(cut.FindAll("[data-testid=workout-totals]"), Is.Empty);
    }

    [Test]
    public async Task EarlierTotalsAnsweringLast_AreIgnored()
    {
        totals.Hold = true;
        var cut = await OpenedAsync();
        cut.Find("[data-testid=workouts-range-all]").Change(true);
        workouts.CompleteRange(Recent);
        workouts.CompleteRange(Recent, index: 1);

        totals.Complete(new ReadResult<WorkoutTotals>(new WorkoutTotals(5, 50, TimeSpan.FromMinutes(50), 5)));
        await WaitForRendererAsync();
        totals.Complete(new ReadResult<WorkoutTotals>(new WorkoutTotals(1, 10, TimeSpan.FromMinutes(10), 1)), index: 1);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=workout-totals-workouts]").TextContent, Is.EqualTo("5")));
        Assert.That(Checked(cut), Is.EqualTo(new[] { "workouts-range-all" }));
    }

    [Test]
    public async Task NoWorkouts_ShowsNoWorkoutsYetAndReadsNoRange()
    {
        var cut = Context.Render<WorkoutsPage>();

        workouts.CompleteDays(new ReadResult<IReadOnlyDictionary<DateOnly, int>>(new Dictionary<DateOnly, int>()));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=workouts-empty]").TextContent, Does.Contain("No workouts yet")));
        Assert.That(workouts.RangeReads, Is.Empty);
    }

    [Test]
    public async Task CountsFail_ShowTheErrorAndTryAgainReadsBoth()
    {
        var cut = Context.Render<WorkoutsPage>();
        workouts.CompleteDays(new ReadFailed());
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Focuses().Single().Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=workouts-retry]")));
        Assert.That(cut.Find("[data-testid=workouts-error]").TextContent, Is.EqualTo("Couldn't load your workouts"));

        cut.Find("[data-testid=workouts-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(workouts.DayReads, Is.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=workouts-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=workouts-error]"), Is.Empty);
        });

        workouts.CompleteDays(TwoYears);
        await WaitForRendererAsync();
        await CompleteRangeAsync(cut, Recent);

        Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-4" }));
    }

    [Test]
    public async Task RangeFails_ShowsTheErrorAndTryAgainReadsThatRangeOnly()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);
        cut.Find("[data-testid=workouts-range-1y]").Change(true);
        workouts.CompleteRange(new ReadFailed());
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workouts-retry]"), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=workouts-retry]").Click();
        await CompleteRangeAsync(cut, Recent);

        Assert.Multiple(() =>
        {
            Assert.That(workouts.DayReads, Is.EqualTo(1));
            Assert.That(workouts.RangeReads.Skip(1), Is.All.EqualTo((new DateOnly(2025, 11, 6), Today)));
            Assert.That(workouts.RangeReads, Has.Count.EqualTo(3));
            Assert.That(cut.FindAll("[data-testid=workouts-error]"), Is.Empty);
            Assert.That(Checked(cut), Is.EqualTo(new[] { "workouts-range-1y" }));
        });
    }

    [Test]
    public async Task CalendarOpenedDuringARead_ShowsNoSpinnerAndAFailedReadClosesIt()
    {
        var cut = await OpenedAsync();

        cut.Find("[data-testid=workouts-dates]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=range-calendar]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=workouts-loading]"), Is.Empty);
        });

        workouts.CompleteRange(new ReadFailed());
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workouts-retry]"), Has.Count.EqualTo(1)));
        cut.Find("[data-testid=workouts-retry]").Click();
        await CompleteRangeAsync(cut, Recent);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=range-calendar]"), Is.Empty);
            Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-4" }));
        });
    }

    [Test]
    public async Task TryAgain_FailsAgain_FocusesTryAgainAgain()
    {
        var cut = Context.Render<WorkoutsPage>();
        workouts.CompleteDays(new ReadFailed());
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=workouts-retry]").Click();
        workouts.CompleteDays(new ReadFailed());
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(2)));
        Focuses()[^1].Arguments[0].ShouldBeElementReferenceTo(cut.Find("[data-testid=workouts-retry]"));
    }

    [Test]
    public async Task TapRow_RaisesOnOpenWithTheWorkoutId()
    {
        var opened = new List<int>();
        var cut = Context.Render<WorkoutsPage>(p => p.Add(x => x.OnOpen, (int id) => opened.Add(id)));
        workouts.CompleteDays(TwoYears);
        await WaitForRendererAsync();
        await CompleteRangeAsync(cut, Recent);

        cut.Find("[data-testid=workouts-row-4]").Click();

        Assert.That(opened, Is.EqualTo(new[] { 4 }));
    }

    [Test]
    public async Task AppStarted_ReloadsWithTheSpinnerThenShowsAWorkoutAddedElsewhere()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        var started = lifecycle.NotifyStartedAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workouts-loading]"), Has.Count.EqualTo(1)));
        workouts.CompleteDays(OneMore);
        await WaitForRendererAsync();
        await CompleteRangeAsync(cut, RecentAndYesterday);
        await started;

        Assert.Multiple(() =>
        {
            Assert.That(workouts.DayReads, Is.EqualTo(2));
            Assert.That(workouts.RangeReads[^1], Is.EqualTo((new DateOnly(2026, 8, 6), Today)));
            Assert.That(cut.Find("[data-testid=workouts-dates-count]").TextContent, Is.EqualTo("2"));
            Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-5", "workouts-row-4" }));
        });
    }

    [Test]
    public async Task AppStarted_AfterMidnight_MovesAQuickRangeToTheNewToday()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);
        cut.Find("[data-testid=workouts-range-6m]").Change(true);
        await CompleteRangeAsync(cut, Recent);
        clock.Advance(TimeSpan.FromDays(1));

        var started = lifecycle.NotifyStartedAsync();
        workouts.CompleteDays(TwoYears);
        await WaitForRendererAsync();
        await CompleteRangeAsync(cut, Recent);
        await started;

        Assert.Multiple(() =>
        {
            Assert.That(workouts.RangeReads[^1], Is.EqualTo((new DateOnly(2026, 5, 7), new DateOnly(2026, 11, 7))));
            Assert.That(Checked(cut), Is.EqualTo(new[] { "workouts-range-6m" }));
        });
    }

    [Test]
    public async Task AppStarted_AfterMidnight_KeepsDatesPickedInTheCalendar()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);
        cut.Find("[data-testid=workouts-dates]").Click();
        cut.Find("[data-testid=range-day-2026-11-02]").Click();
        cut.Find("[data-testid=range-day-2026-11-06]").Click();
        cut.Find("[data-testid=range-apply]").Click();
        await CompleteRangeAsync(cut, Recent);
        clock.Advance(TimeSpan.FromDays(1));

        var started = lifecycle.NotifyStartedAsync();
        workouts.CompleteDays(TwoYears);
        await WaitForRendererAsync();
        await CompleteRangeAsync(cut, Recent);
        await started;

        Assert.Multiple(() =>
        {
            Assert.That(workouts.RangeReads[^1], Is.EqualTo((new DateOnly(2026, 11, 2), Today)));
            Assert.That(Checked(cut), Is.Empty);
        });
    }

    [Test]
    public async Task AppStarted_FailedReload_ShowsTheError()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        var started = lifecycle.NotifyStartedAsync();
        workouts.CompleteDays(new ReadFailed());
        await started;

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workouts-error]"), Has.Count.EqualTo(1)));
    }

    [Test]
    public async Task AppStarted_CountsFail_TryAgainReadsTheCountsAgain()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        var started = lifecycle.NotifyStartedAsync();
        workouts.CompleteDays(new ReadFailed());
        await started;

        await TryAgainAfterFailedCountsShowsTheNewCountsAndRowsAsync(cut);
    }

    [Test]
    public async Task Disposed_AppStartedNoLongerReloads()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        await Context.DisposeComponentsAsync();
        await lifecycle.NotifyStartedAsync();

        Assert.That(workouts.DayReads, Is.EqualTo(1));
    }

    [Test]
    public async Task Disposed_WhileReloading_ReloadFinishesWithoutError()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);
        var started = lifecycle.NotifyStartedAsync();
        cut.WaitForAssertion(() => Assert.That(workouts.DayReads, Is.EqualTo(2)));

        await Context.DisposeComponentsAsync();
        workouts.CompleteDays(OneMore);
        await WaitForRendererAsync();

        Assert.That(async () => await started, Throws.Nothing);
        Assert.Multiple(() =>
        {
            Assert.That(lifecycleLogger.Collector.Count, Is.Zero);
            Assert.That(workouts.RangeReads, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Refresh_FastReload_KeepsTheSpinnerUpForHalfASecond()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        workouts.CompleteDays(OneMore);
        await WaitForRendererAsync();
        workouts.CompleteRange(RecentAndYesterday);
        await WaitForRendererAsync();
        clock.Advance(TimeSpan.FromMilliseconds(499));
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=workouts-loading]"), Has.Count.EqualTo(1));
            Assert.That(Shown(cut), Is.Empty);
        });
        clock.Advance(TimeSpan.FromMilliseconds(1));
        await refresh;
        Assert.Multiple(() =>
        {
            Assert.That(workouts.DayReads, Is.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=workouts-loading]"), Is.Empty);
            Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-5", "workouts-row-4" }));
            Assert.That(cut.Find("[data-testid=workouts-dates-count]").TextContent, Is.EqualTo("2"));
        });
    }

    [Test]
    public async Task Refresh_SlowReload_ShowsTheNewRowsAsSoonAsItFinishes()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        clock.Advance(TimeSpan.FromMilliseconds(800));
        await WaitForRendererAsync();

        Assert.That(cut.FindAll("[data-testid=workouts-loading]"), Has.Count.EqualTo(1));
        workouts.CompleteDays(OneMore);
        await WaitForRendererAsync();
        await CompleteRangeAsync(cut, RecentAndYesterday);
        await refresh;
        Assert.Multiple(() =>
        {
            Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-5", "workouts-row-4" }));
        });
    }

    [Test]
    public async Task Refresh_WhileLoading_IsIgnored()
    {
        var cut = Context.Render<WorkoutsPage>();

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);

        Assert.That(workouts.DayReads, Is.EqualTo(1));
        workouts.CompleteDays(TwoYears);
        await WaitForRendererAsync();
        await CompleteRangeAsync(cut, Recent);
        await refresh;
        Assert.Multiple(() =>
        {
            Assert.That(workouts.RangeReads, Has.Count.EqualTo(1));
            Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-4" }));
        });
    }

    [Test]
    public async Task Refresh_Failed_ShowsTheErrorAfterHalfASecond()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        workouts.CompleteDays(OneMore);
        await WaitForRendererAsync();
        workouts.CompleteRange(new ReadFailed());
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=workouts-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=workouts-error]"), Is.Empty);
        });
        clock.Advance(TimeSpan.FromMilliseconds(500));
        await refresh;
        Assert.That(cut.FindAll("[data-testid=workouts-error]"), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Refresh_CountsFail_TryAgainReadsTheCountsAgain()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        await cut.InvokeAsync(async () =>
        {
            var refresh = cut.FindComponent<AppScreen>().Instance.RefreshAsync();
            workouts.CompleteDays(new ReadFailed());
            clock.Advance(TimeSpan.FromMilliseconds(500));
            await refresh;
        });

        await TryAgainAfterFailedCountsShowsTheNewCountsAndRowsAsync(cut);
    }

    [Test]
    public async Task Refresh_OtherDatesChosenDuringIt_TheirRowsWin()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        cut.Find("[data-testid=workouts-range-all]").Change(true);
        workouts.CompleteDays(OneMore);
        await WaitForRendererAsync();
        await CompleteRangeAsync(cut, new ReadResult<IReadOnlyList<WorkoutSummary>>([new WorkoutSummary(1, new DateTimeOffset(2025, 3, 12, 17, 0, 0, TimeSpan.Zero))]));
        await refresh;

        Assert.Multiple(() =>
        {
            Assert.That(workouts.RangeReads, Has.Count.EqualTo(2));
            Assert.That(Checked(cut), Is.EqualTo(new[] { "workouts-range-all" }));
            Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-1" }));
        });
    }

    [Test]
    public async Task Refresh_OtherDatesChosenDuringTheHold_TheirRowsShowWhenRead()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        workouts.CompleteDays(OneMore);
        await WaitForRendererAsync();
        workouts.CompleteRange(RecentAndYesterday);
        await WaitForRendererAsync();
        cut.Find("[data-testid=workouts-range-all]").Change(true);
        clock.Advance(TimeSpan.FromMilliseconds(500));
        await refresh;

        Assert.That(cut.FindAll("[data-testid=workouts-loading]"), Has.Count.EqualTo(1));
        await CompleteRangeAsync(cut, new ReadResult<IReadOnlyList<WorkoutSummary>>([new WorkoutSummary(1, new DateTimeOffset(2025, 3, 12, 17, 0, 0, TimeSpan.Zero))]));
        Assert.Multiple(() =>
        {
            Assert.That(workouts.RangeReads, Has.Count.EqualTo(3));
            Assert.That(Checked(cut), Is.EqualTo(new[] { "workouts-range-all" }));
            Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-1" }));
        });
    }

    [Test]
    public async Task Refresh_OtherDatesReadBeforeTheHoldEnds_TheirRowsShowAtOnce()
    {
        var cut = await OpenedAsync();
        await CompleteRangeAsync(cut, Recent);

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        workouts.CompleteDays(OneMore);
        await WaitForRendererAsync();
        workouts.CompleteRange(RecentAndYesterday);
        await WaitForRendererAsync();
        cut.Find("[data-testid=workouts-range-all]").Change(true);
        await CompleteRangeAsync(cut, new ReadResult<IReadOnlyList<WorkoutSummary>>([new WorkoutSummary(1, new DateTimeOffset(2025, 3, 12, 17, 0, 0, TimeSpan.Zero))]));

        Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-1" }));
        clock.Advance(TimeSpan.FromMilliseconds(500));
        await refresh;
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=workouts-loading]"), Is.Empty);
            Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-1" }));
        });
    }

    [Test]
    public void NavigationBar_RaisesOnNavigate()
    {
        var navigated = new List<NavDestination>();
        var cut = Context.Render<WorkoutsPage>(p => p.Add(x => x.OnNavigate, navigated.Add));

        cut.Find("[data-testid=dashboard-link]").Click();

        Assert.That(navigated, Is.EqualTo(new[] { NavDestination.Dashboard }));
    }

    [Test]
    public async Task AndroidBack_NavigatesToTheDashboard()
    {
        var navigated = new List<NavDestination>();
        var cut = Context.Render<WorkoutsPage>(p => p.Add(x => x.OnNavigate, navigated.Add));

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(navigated, Is.EqualTo(new[] { NavDestination.Dashboard }));
    }

    [Test]
    public async Task AndroidBack_WhileTheCalendarIsOpen_ClosesItThenGoesToTheDashboard()
    {
        var navigated = new List<NavDestination>();
        var cut = Context.Render<WorkoutsPage>(p => p.Add(x => x.OnNavigate, navigated.Add));
        workouts.CompleteDays(TwoYears);
        await WaitForRendererAsync();
        await CompleteRangeAsync(cut, Recent);
        cut.Find("[data-testid=workouts-dates]").Click();

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=range-calendar]"), Is.Empty);
            Assert.That(navigated, Is.Empty);
        });

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(navigated, Is.EqualTo(new[] { NavDestination.Dashboard }));
    }

    private void Clock(DateTimeOffset now)
    {
        clock = new FakeTimeProvider(now);
        clock.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        Context.Services.AddSingleton<TimeProvider>(clock);
    }

    /// <summary>Renders the page and answers the per-day read with <see cref="TwoYears"/>; the 3M read is left pending.</summary>
    private async Task<IRenderedComponent<WorkoutsPage>> OpenedAsync()
    {
        var cut = Context.Render<WorkoutsPage>();
        workouts.CompleteDays(TwoYears);
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(workouts.RangeReads, Has.Count.EqualTo(1)));
        return cut;
    }

    private async Task TryAgainAfterFailedCountsShowsTheNewCountsAndRowsAsync(IRenderedComponent<WorkoutsPage> cut)
    {
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workouts-retry]"), Has.Count.EqualTo(1)));

        var dayReads = workouts.DayReads;

        cut.Find("[data-testid=workouts-retry]").Click();

        Assert.That(workouts.DayReads, Is.EqualTo(dayReads + 1));

        workouts.CompleteDays(OneMore);
        await WaitForRendererAsync();
        await CompleteRangeAsync(cut, RecentAndYesterday);

        Assert.Multiple(() =>
        {
            Assert.That(workouts.RangeReads[^1], Is.EqualTo((new DateOnly(2026, 8, 6), Today)));
            Assert.That(cut.Find("[data-testid=workouts-dates-count]").TextContent, Is.EqualTo("2"));
            Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-5", "workouts-row-4" }));
            Assert.That(cut.FindAll("[data-testid=workouts-error]"), Is.Empty);
        });
    }

    private async Task CompleteRangeAsync(IRenderedComponent<WorkoutsPage> cut, ReadResult<IReadOnlyList<WorkoutSummary>> result)
    {
        workouts.CompleteRange(result);
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=workouts-loading]"), Is.Empty));
    }

    private static IEnumerable<string?> Shown(IRenderedComponent<WorkoutsPage> cut) =>
        cut.FindAll("[data-testid^=workouts-row-]").Select(r => r.GetAttribute("data-testid"));

    private static IEnumerable<string?> Checked(IRenderedComponent<WorkoutsPage> cut) =>
        cut.FindAll("[data-testid=workouts-range] input[checked]").Select(i => i.GetAttribute("data-testid"));

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];

    /// <summary>Each read waits until the test completes it.</summary>
    private sealed class FakeWorkoutsService : IWorkoutsService
    {
        private readonly List<TaskCompletionSource<ReadResult<IReadOnlyList<WorkoutSummary>>>> ranges = [];
        private TaskCompletionSource<ReadResult<IReadOnlyDictionary<DateOnly, int>>> days = new();

        public int DayReads { get; private set; }

        /// <summary>The days of each range read, in order.</summary>
        public List<(DateOnly From, DateOnly To)> RangeReads { get; } = [];

        public Task<ReadResult<IReadOnlyDictionary<DateOnly, int>>> GetDaysAsync()
        {
            DayReads++;
            days = new TaskCompletionSource<ReadResult<IReadOnlyDictionary<DateOnly, int>>>();
            return days.Task;
        }

        public Task<ReadResult<IReadOnlyList<WorkoutSummary>>> GetBetweenAsync(DateOnly from, DateOnly to)
        {
            RangeReads.Add((from, to));
            var pending = new TaskCompletionSource<ReadResult<IReadOnlyList<WorkoutSummary>>>();
            ranges.Add(pending);
            return pending.Task;
        }

        public void CompleteDays(ReadResult<IReadOnlyDictionary<DateOnly, int>> result) => days.SetResult(result);

        /// <summary>Completes a pending range read: the latest, or the one <paramref name="index"/> from the end.</summary>
        public void CompleteRange(ReadResult<IReadOnlyList<WorkoutSummary>> result, int index = 0) => ranges[^(index + 1)].SetResult(result);
    }
}
