using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Consistency;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Dashboard;
using WorkoutLog.UI.Lifecycle;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class DashboardPageTests : ComponentTestBase
{
    private static readonly DashboardResult Failed = new(null, 0, 0, Failed: true);

    /// <summary>Fri 23 Oct 2026, 22:30 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 23, 22, 30, 0, TimeSpan.Zero);

    private static readonly DashboardResult Loaded = new(Now.AddDays(-1), 1, 1, Failed: false);

    private FakeTimeProvider time = null!;
    private FakeDashboardService dashboard = null!;
    private FakeLastWorkoutService lastWorkout = null!;
    private FakeRecentRecordsService recentRecords = null!;
    private FakeConsistencyService consistency = null!;
    private FakeLogger<AppLifecycle> lifecycleLogger = null!;
    private AppLifecycle lifecycle = null!;

    [SetUp]
    public void SetUp()
    {
        time = new FakeTimeProvider(Now);
        time.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        Context.Services.AddSingleton<TimeProvider>(time);
        dashboard = new FakeDashboardService();
        Context.Services.AddSingleton<IDashboardService>(dashboard);
        lastWorkout = new FakeLastWorkoutService();
        Context.Services.AddSingleton<ILastWorkoutService>(lastWorkout);
        recentRecords = new FakeRecentRecordsService();
        Context.Services.AddSingleton<IRecentRecordsService>(recentRecords);
        consistency = new FakeConsistencyService();
        Context.Services.AddSingleton<IConsistencyService>(consistency);
        lifecycleLogger = new FakeLogger<AppLifecycle>();
        lifecycle = new AppLifecycle(lifecycleLogger);
        Context.Services.AddSingleton<IAppStart>(lifecycle);
    }

    [Test]
    public void WhileLoading_ShowsTheSpinnerWithWorkoutsAndNewWorkout()
    {
        var cut = Context.Render<DashboardPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=dashboard-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=last-workout-empty]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=consistency]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workouts-link]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercises-link]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=new-workout]"), Has.Count.EqualTo(1));
            Assert.That(dashboard.Calls, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Loaded_ShowsTheLastWorkoutInThePhonesTimeZoneAndTheConsistencyCard()
    {
        lastWorkout.Result = Last(new DateTimeOffset(2026, 10, 20, 23, 30, 0, TimeSpan.Zero), [187, 300, 1]);
        consistency.Result = Consistency(new WeekTally(3, new TimeSpan(3, 5, 0), 42), new WeekTally(2, new TimeSpan(2, 40, 0), 45), [2, 3, 1, 2, 3, 0, 3, 4, 2, 3, 2, 3], 6);
        var cut = Context.Render<DashboardPage>();

        dashboard.Complete(new DashboardResult(null, 2, 7, Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=last-workout-date]").TextContent, Is.EqualTo("Wed 21 Oct 2026")));
        var card = cut.FindComponent<ConsistencyCard>().Instance;
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=last-workout-when]").TextContent, Is.EqualTo("2 days ago · 1:12:04"));
            Assert.That(cut.Find("[data-testid=last-workout-exercises]").TextContent, Is.EqualTo($"Farmer's Walk, {Name(300)} +1"));
            Assert.That(card.ThisWeek, Is.EqualTo(new WeekTotals(3, new TimeSpan(3, 5, 0), 42)));
            Assert.That(card.LastWeek, Is.EqualTo(new WeekTotals(2, new TimeSpan(2, 40, 0), 45)));
            Assert.That(card.WeekCounts, Is.EqualTo(new[] { 2, 3, 1, 2, 3, 0, 3, 4, 2, 3, 2, 3 }));
            Assert.That(card.OnMuscles.HasDelegate, Is.False);
            Assert.That(cut.Find("[data-testid=consistency-streak]").TextContent, Is.EqualTo("6-week streak"));
            Assert.That(cut.Find("[data-testid=consistency-comparison-time]").TextContent, Does.Contain("3h 05m"));
            Assert.That(cut.FindAll("[data-testid=count-week]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=dashboard-loading]"), Is.Empty);
            Assert.That(lastWorkout.Calls, Is.EqualTo(1));
            Assert.That(consistency.Calls, Is.EqualTo(1));
        });
    }

    [TestCase(1, 2, "Ab Wheel Rollout, Back Extension")]
    [TestCase(1, null, "Ab Wheel Rollout")]
    public async Task Loaded_OneOrTwoExercises_ShowsTheirNamesWithNoCount(int first, int? second, string expected)
    {
        lastWorkout.Result = Last(new DateTimeOffset(2026, 10, 20, 23, 30, 0, TimeSpan.Zero), second is { } id ? [first, id] : [first]);
        var cut = Context.Render<DashboardPage>();

        dashboard.Complete(new DashboardResult(null, 0, 0, Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=last-workout-exercises]").TextContent, Is.EqualTo(expected)));
    }

    [TestCase(2026, 10, 23, 21, 0, "Today")]
    [TestCase(2026, 10, 22, 23, 0, "Today")]
    [TestCase(2026, 10, 22, 22, 59, "Yesterday")]
    [TestCase(2026, 10, 21, 0, 0, "2 days ago")]
    [TestCase(2026, 10, 25, 9, 0, "Today")]
    public async Task Loaded_DaysSince_CountsCalendarDaysInThePhonesTimeZone(int year, int month, int day, int hour, int minute, string expected)
    {
        lastWorkout.Result = Last(new DateTimeOffset(year, month, day, hour, minute, 0, TimeSpan.Zero), [1]);
        var cut = Context.Render<DashboardPage>();

        dashboard.Complete(new DashboardResult(null, 0, 0, Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=last-workout-when]").TextContent, Does.StartWith($"{expected} · ")));
    }

    [Test]
    public async Task TappingTheLastWorkout_RaisesOnOpenWorkoutWithItsId()
    {
        var opened = new List<int>();
        lastWorkout.Result = Last(new DateTimeOffset(2026, 10, 20, 23, 30, 0, TimeSpan.Zero), [1]);
        var cut = Context.Render<DashboardPage>(p => p.Add(x => x.OnOpenWorkout, id => opened.Add(id)));
        dashboard.Complete(new DashboardResult(null, 0, 0, Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => cut.Find("button[data-testid=last-workout]").Click());

        Assert.That(opened, Is.EqualTo(new[] { 42 }));
    }

    [Test]
    public async Task ConsistencyLoadFails_ShowsTheError()
    {
        consistency.Result = ConsistencyResult.FailedRead;
        var cut = Context.Render<DashboardPage>();

        dashboard.Complete(new DashboardResult(null, 0, 0, Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=dashboard-error]").TextContent, Is.EqualTo("Couldn't load the Dashboard")));
        Assert.That(cut.FindAll("[data-testid=consistency]"), Is.Empty);
    }

    [Test]
    public async Task NoWorkouts_ShowsTheEmptyStateNotTappableAndAnEmptyConsistencyCard()
    {
        var cut = Context.Render<DashboardPage>();

        dashboard.Complete(new DashboardResult(null, 0, 0, Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=last-workout-empty]").TextContent, Is.EqualTo("No workouts yet")));
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("button[data-testid=last-workout]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=consistency-bars-none]").TextContent, Is.EqualTo("No workouts in the last 12 weeks"));
            Assert.That(cut.FindAll("[data-testid=consistency-streak]"), Is.Empty);
            Assert.That(cut.FindComponent<ConsistencyCard>().Instance.ThisWeek, Is.EqualTo(new WeekTotals(0, TimeSpan.Zero, 0)));
        });
    }

    [Test]
    public async Task LastWorkoutLoadFails_ShowsTheError()
    {
        lastWorkout.Result = new LastWorkoutResult(null, Failed: true);
        var cut = Context.Render<DashboardPage>();

        dashboard.Complete(new DashboardResult(null, 1, 1, Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=dashboard-error]").TextContent, Is.EqualTo("Couldn't load the Dashboard")));
        Assert.That(cut.FindAll("[data-testid=last-workout]"), Is.Empty);
    }

    [Test]
    public async Task LoadFails_ShowsTheErrorAndFocusesTryAgain()
    {
        var cut = Context.Render<DashboardPage>();

        dashboard.Complete(Failed);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Focuses().Single().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=dashboard-retry]")));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=dashboard-error]").TextContent, Is.EqualTo("Couldn't load the Dashboard"));
            Assert.That(cut.FindAll("[data-testid=workouts-link]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercises-link]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=new-workout]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task TryAgain_ShowsTheSpinnerWhileReloadingThenTheSummary()
    {
        var cut = Context.Render<DashboardPage>();
        dashboard.Complete(Failed);
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=dashboard-retry]"), Has.Count.EqualTo(1)));

        cut.Find("[data-testid=dashboard-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(dashboard.Calls, Is.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=dashboard-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=dashboard-error]"), Is.Empty);
        });

        dashboard.Complete(new DashboardResult(null, 1, 1, Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=consistency]"), Has.Count.EqualTo(1)));
        Assert.That(cut.FindAll("[data-testid=dashboard-error]"), Is.Empty);
    }

    [Test]
    public async Task AppStarted_ReloadsWithTheSpinnerThenShowsTheNewValues()
    {
        var cut = Context.Render<DashboardPage>();
        dashboard.Complete(new DashboardResult(null, 1, 1, Failed: false));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=consistency]"), Has.Count.EqualTo(1)));

        var started = lifecycle.NotifyStartedAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=dashboard-loading]"), Has.Count.EqualTo(1)));
        Assert.That(dashboard.Calls, Is.EqualTo(2));

        consistency.Result = Consistency(new WeekTally(1, TimeSpan.FromMinutes(52), 16), WeekTally.None, [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1], 1);
        dashboard.Complete(new DashboardResult(null, 3, 5, Failed: false));
        await WaitForRendererAsync();
        await started;

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=consistency-streak]").TextContent, Is.EqualTo("1-week streak")));
        Assert.That(consistency.Calls, Is.EqualTo(2));
    }

    [Test]
    public async Task AppStarted_FailedReload_ShowsTheError()
    {
        var cut = Context.Render<DashboardPage>();
        dashboard.Complete(new DashboardResult(null, 1, 1, Failed: false));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=consistency]"), Has.Count.EqualTo(1)));

        var started = lifecycle.NotifyStartedAsync();
        dashboard.Complete(Failed);
        await WaitForRendererAsync();
        await started;

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=dashboard-error]"), Has.Count.EqualTo(1)));
    }

    [Test]
    public async Task AppStarted_WhileLoading_IsIgnored()
    {
        Context.Render<DashboardPage>();

        await lifecycle.NotifyStartedAsync();

        Assert.That(dashboard.Calls, Is.EqualTo(1));
    }

    [Test]
    public async Task Disposed_AppStartedNoLongerReloads()
    {
        var cut = Context.Render<DashboardPage>();
        dashboard.Complete(new DashboardResult(null, 0, 0, Failed: false));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=consistency]"), Has.Count.EqualTo(1)));

        await Context.DisposeComponentsAsync();
        await lifecycle.NotifyStartedAsync();

        Assert.That(dashboard.Calls, Is.EqualTo(1));
    }

    [Test]
    public async Task Disposed_WhileLoading_SkipsTheUpdate()
    {
        var clock = new ZoneReadCounter();
        Context.Services.AddSingleton<TimeProvider>(clock);
        Context.Render<DashboardPage>();

        await Context.DisposeComponentsAsync();
        dashboard.Complete(new DashboardResult(new DateTimeOffset(2026, 10, 20, 23, 30, 0, TimeSpan.Zero), 1, 1, Failed: false));
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(dashboard.Calls, Is.EqualTo(1));
            Assert.That(clock.Reads, Is.Zero);
        });
    }

    [Test]
    public async Task Disposed_WhileReloading_ReloadFinishesWithoutError()
    {
        var cut = Context.Render<DashboardPage>();
        dashboard.Complete(new DashboardResult(null, 0, 0, Failed: false));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=consistency]"), Has.Count.EqualTo(1)));
        var started = lifecycle.NotifyStartedAsync();
        cut.WaitForAssertion(() => Assert.That(dashboard.Calls, Is.EqualTo(2)));

        await Context.DisposeComponentsAsync();
        dashboard.Complete(new DashboardResult(null, 1, 1, Failed: false));
        await WaitForRendererAsync();

        Assert.That(async () => await started, Throws.Nothing);
        Assert.That(lifecycleLogger.Collector.Count, Is.Zero);
    }

    [Test]
    public async Task AppStarted_PageDisposedByAnEarlierHandler_DoesNotReload()
    {
        lifecycle.Register(() => Context.DisposeComponentsAsync());
        var cut = Context.Render<DashboardPage>();
        dashboard.Complete(new DashboardResult(null, 0, 0, Failed: false));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=consistency]"), Has.Count.EqualTo(1)));

        var started = lifecycle.NotifyStartedAsync();

        cut.WaitForAssertion(() => Assert.That(started.IsCompletedSuccessfully, Is.True));
        Assert.Multiple(() =>
        {
            Assert.That(dashboard.Calls, Is.EqualTo(1));
            Assert.That(lifecycleLogger.Collector.Count, Is.Zero);
        });
        await started;
    }

    [Test]
    public void Buttons_RaiseTheirCallbacks()
    {
        var newWorkout = 0;
        var workouts = 0;
        var exercises = 0;
        var settings = 0;
        var cut = Context.Render<DashboardPage>(p => p
            .Add(x => x.OnNewWorkout, () => newWorkout++)
            .Add(x => x.OnWorkouts, () => workouts++)
            .Add(x => x.OnExercises, () => exercises++)
            .Add(x => x.OnSettings, () => settings++));

        cut.Find("[data-testid=new-workout]").Click();
        cut.Find("[data-testid=workouts-link]").Click();
        cut.Find("[data-testid=exercises-link]").Click();
        cut.Find("[data-testid=settings]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(newWorkout, Is.EqualTo(1));
            Assert.That(workouts, Is.EqualTo(1));
            Assert.That(exercises, Is.EqualTo(1));
            Assert.That(settings, Is.EqualTo(1));
        });
    }

    [Test]
    public void AndroidBack_HasNoHandler()
    {
        Context.Render<DashboardPage>();

        Assert.That(BackButton.HasHandler, Is.False);
    }

    [Test]
    public void WhileLoading_ShowsNoRecordsCard()
    {
        var cut = Context.Render<DashboardPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=recent-records]"), Is.Empty);
            Assert.That(recentRecords.Calls, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Loaded_ShowsTheRecordsInOrderWithTheirBestAndDaysAgoInThePhonesTimeZone()
    {
        recentRecords.Result = new RecentRecordsResult(
        [
            new ExerciseRecord(6, new DateTimeOffset(2026, 10, 22, 23, 30, 0, TimeSpan.Zero), new ExerciseBest(70m, 3, null, null)),
            new ExerciseRecord(311, Now.AddDays(-2), new ExerciseBest(null, null, null, 5.2m)),
        ], Now, Failed: false);
        var cut = Context.Render<DashboardPage>();

        dashboard.Complete(Loaded);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid^=recent-records-row-]")
                .Select(r => (r.GetAttribute("data-testid"), r.QuerySelector(".text")!.TextContent, r.QuerySelector(".detail")!.TextContent)),
            Is.EqualTo(new[]
            {
                ("recent-records-row-6", "Barbell Back Squat · 70kg × 3", "Today"),
                ("recent-records-row-311", "Outdoor Run · 5.2km", "2 days ago"),
            })));
    }

    [Test]
    public async Task Loaded_LoadEndsAfterMidnight_CountsDaysAgoFromWhenTheRecordsWereRead()
    {
        recentRecords.Result = new RecentRecordsResult([new ExerciseRecord(6, Now.AddDays(-29), new ExerciseBest(70m, 3, null, null))], Now, Failed: false);
        var cut = Context.Render<DashboardPage>();

        time.Advance(TimeSpan.FromHours(1));
        dashboard.Complete(Loaded);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() =>
            Assert.That(cut.Find("[data-testid=recent-records-row-6] .detail").TextContent, Is.EqualTo("29 days ago")));
    }

    [Test]
    public async Task NoRecords_SaysSo()
    {
        var cut = Context.Render<DashboardPage>();

        dashboard.Complete(Loaded);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() =>
            Assert.That(cut.Find("[data-testid=recent-records-none]").TextContent, Is.EqualTo("No new records in the last 30 days")));
    }

    [Test]
    public async Task RecordsFailToLoad_ShowsNoRecordsCardAndTheRestOfTheDashboard()
    {
        recentRecords.Result = new RecentRecordsResult([], Now, Failed: true);
        var cut = Context.Render<DashboardPage>();

        dashboard.Complete(Loaded);
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=last-workout]"), Has.Count.EqualTo(1)));
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=recent-records]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=dashboard-error]"), Is.Empty);
        });
    }

    [Test]
    public async Task Record_Tapped_RaisesOnOpenExerciseWithItsId()
    {
        recentRecords.Result = new RecentRecordsResult([new ExerciseRecord(6, Now.AddDays(-1), new ExerciseBest(70m, 3, null, null))], Now, Failed: false);
        int? opened = null;
        var cut = Context.Render<DashboardPage>(p => p.Add(x => x.OnOpenExercise, id => opened = id));
        dashboard.Complete(Loaded);
        await WaitForRendererAsync();

        cut.WaitForElement("button[data-testid=recent-records-row-6]").Click();

        Assert.That(opened, Is.EqualTo(6));
    }

    [Test]
    public async Task AppStarted_ReloadsTheRecords()
    {
        var cut = Context.Render<DashboardPage>();
        dashboard.Complete(Loaded);
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=recent-records-none]"), Has.Count.EqualTo(1)));
        recentRecords.Result = new RecentRecordsResult([new ExerciseRecord(6, Now.AddDays(-1), new ExerciseBest(70m, 3, null, null))], Now, Failed: false);

        var started = lifecycle.NotifyStartedAsync();
        dashboard.Complete(Loaded);
        await WaitForRendererAsync();
        await started;

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=recent-records-row-6]"), Has.Count.EqualTo(1)));
        Assert.That(recentRecords.Calls, Is.EqualTo(2));
    }

    /// <summary>Workout 42, 1:12:04 long, with the exercises in order.</summary>
    private static LastWorkoutResult Last(DateTimeOffset startedAt, int[] exerciseIds) =>
        new(new WorkoutOutline(42, startedAt, startedAt + new TimeSpan(1, 12, 4), exerciseIds), Failed: false);

    private static string Name(int exerciseId) => Catalog.All.Single(e => e.Id == exerciseId).Name;

    private static ConsistencyResult Consistency(WeekTally thisWeek, WeekTally lastWeek, int[] weekCounts, int streak) =>
        new(new WeekTallies(thisWeek, lastWeek), weekCounts, streak, Failed: false);

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];

    /// <summary>Returns <see cref="Result"/> at once; no records by default.</summary>
    private sealed class FakeRecentRecordsService : IRecentRecordsService
    {
        public RecentRecordsResult Result { get; set; } = new([], Now, Failed: false);

        public int Calls { get; private set; }

        public Task<RecentRecordsResult> GetAsync()
        {
            Calls++;
            return Task.FromResult(Result);
        }
    }

    /// <summary>Each call waits until the test completes it.</summary>
    private sealed class FakeDashboardService : IDashboardService
    {
        private TaskCompletionSource<DashboardResult> pending = new();

        public int Calls { get; private set; }

        public Task<DashboardResult> GetAsync()
        {
            Calls++;
            pending = new TaskCompletionSource<DashboardResult>();
            return pending.Task;
        }

        /// <summary>Completes the latest call.</summary>
        public void Complete(DashboardResult result) => pending.SetResult(result);
    }

    /// <summary>Returns <see cref="Result"/>, no workouts unless a test sets it.</summary>
    private sealed class FakeLastWorkoutService : ILastWorkoutService
    {
        public LastWorkoutResult Result { get; set; } = new(null, Failed: false);

        public int Calls { get; private set; }

        public Task<LastWorkoutResult> GetAsync()
        {
            Calls++;
            return Task.FromResult(Result);
        }
    }

    /// <summary>Returns <see cref="Result"/> at once; no workouts by default.</summary>
    private sealed class FakeConsistencyService : IConsistencyService
    {
        public ConsistencyResult Result { get; set; } = Consistency(WeekTally.None, WeekTally.None, new int[12], 0);

        public int Calls { get; private set; }

        public Task<ConsistencyResult> GetAsync()
        {
            Calls++;
            return Task.FromResult(Result);
        }
    }

    /// <summary>Counts reads of the local time zone.</summary>
    private sealed class ZoneReadCounter : TimeProvider
    {
        public int Reads { get; private set; }

        public override TimeZoneInfo LocalTimeZone
        {
            get
            {
                Reads++;
                return TimeZoneInfo.Utc;
            }
        }
    }
}
