using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Navigation;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class WorkoutsPageTests : ComponentTestBase
{
    private static readonly DateOnly Today = new(2026, 11, 6);

    private static readonly WorkoutDaysResult TwoYears = new(new Dictionary<DateOnly, int>
    {
        [new DateOnly(2026, 11, 2)] = 1,
        [new DateOnly(2026, 7, 6)] = 1,
        [new DateOnly(2025, 3, 12)] = 2,
    }, Failed: false);

    private static readonly WorkoutsResult Recent = new([new WorkoutSummary(4, new DateTimeOffset(2026, 11, 2, 17, 0, 0, TimeSpan.Zero))], Failed: false);

    private FakeWorkoutsService workouts = null!;

    [SetUp]
    public void SetUp()
    {
        Clock(new DateTimeOffset(2026, 11, 6, 9, 0, 0, TimeSpan.Zero));
        workouts = new FakeWorkoutsService();
        Context.Services.AddSingleton<IWorkoutsService>(workouts);
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

        await CompleteRangeAsync(cut, new WorkoutsResult(
        [
            new WorkoutSummary(2, new DateTimeOffset(2026, 10, 20, 23, 30, 0, TimeSpan.Zero)),
            new WorkoutSummary(1, new DateTimeOffset(2026, 10, 20, 17, 5, 0, TimeSpan.Zero)),
        ], Failed: false));

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
        await CompleteRangeAsync(cut, new WorkoutsResult([], Failed: false));

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
        await CompleteRangeAsync(cut, new WorkoutsResult([new WorkoutSummary(1, new DateTimeOffset(2025, 3, 12, 17, 0, 0, TimeSpan.Zero))], Failed: false));

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

        workouts.CompleteRange(new WorkoutsResult([new WorkoutSummary(1, new DateTimeOffset(2025, 3, 12, 17, 0, 0, TimeSpan.Zero))], Failed: false));
        workouts.CompleteRange(Recent, index: 1);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(Shown(cut), Is.EqualTo(new[] { "workouts-row-1" })));
        Assert.That(Checked(cut), Is.EqualTo(new[] { "workouts-range-all" }));
    }

    [Test]
    public async Task NoWorkouts_ShowsNoWorkoutsYetAndReadsNoRange()
    {
        var cut = Context.Render<WorkoutsPage>();

        workouts.CompleteDays(new WorkoutDaysResult(new Dictionary<DateOnly, int>(), Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=workouts-empty]").TextContent, Does.Contain("No workouts yet")));
        Assert.That(workouts.RangeReads, Is.Empty);
    }

    [Test]
    public async Task CountsFail_ShowTheErrorAndTryAgainReadsBoth()
    {
        var cut = Context.Render<WorkoutsPage>();
        workouts.CompleteDays(new WorkoutDaysResult(new Dictionary<DateOnly, int>(), Failed: true));
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
        workouts.CompleteRange(new WorkoutsResult([], Failed: true));
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

        workouts.CompleteRange(new WorkoutsResult([], Failed: true));
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
        workouts.CompleteDays(new WorkoutDaysResult(new Dictionary<DateOnly, int>(), Failed: true));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=workouts-retry]").Click();
        workouts.CompleteDays(new WorkoutDaysResult(new Dictionary<DateOnly, int>(), Failed: true));
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
        var clock = new FakeTimeProvider(now);
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

    private async Task CompleteRangeAsync(IRenderedComponent<WorkoutsPage> cut, WorkoutsResult result)
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
        private readonly List<TaskCompletionSource<WorkoutsResult>> ranges = [];
        private TaskCompletionSource<WorkoutDaysResult> days = new();

        public int DayReads { get; private set; }

        /// <summary>The days of each range read, in order.</summary>
        public List<(DateOnly From, DateOnly To)> RangeReads { get; } = [];

        public Task<WorkoutDaysResult> GetDaysAsync()
        {
            DayReads++;
            days = new TaskCompletionSource<WorkoutDaysResult>();
            return days.Task;
        }

        public Task<WorkoutsResult> GetBetweenAsync(DateOnly from, DateOnly to)
        {
            RangeReads.Add((from, to));
            var pending = new TaskCompletionSource<WorkoutsResult>();
            ranges.Add(pending);
            return pending.Task;
        }

        public void CompleteDays(WorkoutDaysResult result) => days.SetResult(result);

        /// <summary>Completes a pending range read: the latest, or the one <paramref name="index"/> from the end.</summary>
        public void CompleteRange(WorkoutsResult result, int index = 0) => ranges[^(index + 1)].SetResult(result);
    }
}
