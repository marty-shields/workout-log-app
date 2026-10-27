using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Lifecycle;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class DashboardPageTests : ComponentTestBase
{
    private static readonly DashboardResult Failed = new(null, 0, 0, Failed: true);

    private FakeDashboardService dashboard = null!;
    private FakeLogger<AppLifecycle> lifecycleLogger = null!;
    private AppLifecycle lifecycle = null!;

    [SetUp]
    public void SetUp()
    {
        var clock = new FakeTimeProvider();
        clock.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        Context.Services.AddSingleton<TimeProvider>(clock);
        dashboard = new FakeDashboardService();
        Context.Services.AddSingleton<IDashboardService>(dashboard);
        lifecycleLogger = new FakeLogger<AppLifecycle>();
        lifecycle = new AppLifecycle(lifecycleLogger);
        Context.Services.AddSingleton<IAppStart>(lifecycle);
    }

    [Test]
    public void WhileLoading_ShowsTheSpinnerWithHistoryAndNewWorkout()
    {
        var cut = Context.Render<DashboardPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=dashboard-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=last-workout-empty]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=count-week]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=history-link]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=new-workout]"), Has.Count.EqualTo(1));
            Assert.That(dashboard.Calls, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Loaded_ShowsTheLastWorkoutInThePhonesTimeZoneAndTheCounts()
    {
        var cut = Context.Render<DashboardPage>();

        dashboard.Complete(new DashboardResult(new DateTimeOffset(2026, 10, 20, 23, 30, 0, TimeSpan.Zero), 2, 7, Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=last-workout]").TextContent, Is.EqualTo("Wed 21 Oct 2026")));
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=count-week] .value").TextContent, Is.EqualTo("2"));
            Assert.That(cut.Find("[data-testid=count-month] .value").TextContent, Is.EqualTo("7"));
            Assert.That(cut.FindAll("[data-testid=dashboard-loading]"), Is.Empty);
        });
    }

    [Test]
    public async Task NoWorkouts_ShowsTheEmptyStateAndZeroCounts()
    {
        var cut = Context.Render<DashboardPage>();

        dashboard.Complete(new DashboardResult(null, 0, 0, Failed: false));
        await WaitForRendererAsync();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=last-workout-empty]").TextContent, Does.Contain("No workouts yet")));
        Assert.That(cut.Find("[data-testid=count-week] .value").TextContent, Is.EqualTo("0"));
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
            Assert.That(cut.FindAll("[data-testid=history-link]"), Has.Count.EqualTo(1));
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

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=count-week] .value").TextContent, Is.EqualTo("1")));
        Assert.That(cut.FindAll("[data-testid=dashboard-error]"), Is.Empty);
    }

    [Test]
    public async Task AppStarted_ReloadsWithTheSpinnerThenShowsTheNewValues()
    {
        var cut = Context.Render<DashboardPage>();
        dashboard.Complete(new DashboardResult(null, 1, 1, Failed: false));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=count-week]"), Has.Count.EqualTo(1)));

        var started = lifecycle.NotifyStartedAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=dashboard-loading]"), Has.Count.EqualTo(1)));
        Assert.That(dashboard.Calls, Is.EqualTo(2));

        dashboard.Complete(new DashboardResult(null, 3, 5, Failed: false));
        await WaitForRendererAsync();
        await started;

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=count-week] .value").TextContent, Is.EqualTo("3")));
        Assert.That(cut.Find("[data-testid=count-month] .value").TextContent, Is.EqualTo("5"));
    }

    [Test]
    public async Task AppStarted_FailedReload_ShowsTheError()
    {
        var cut = Context.Render<DashboardPage>();
        dashboard.Complete(new DashboardResult(null, 1, 1, Failed: false));
        await WaitForRendererAsync();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=count-week]"), Has.Count.EqualTo(1)));

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
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=count-week]"), Has.Count.EqualTo(1)));

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
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=count-week]"), Has.Count.EqualTo(1)));
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
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=count-week]"), Has.Count.EqualTo(1)));

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
        var history = 0;
        var settings = 0;
        var cut = Context.Render<DashboardPage>(p => p
            .Add(x => x.OnNewWorkout, () => newWorkout++)
            .Add(x => x.OnHistory, () => history++)
            .Add(x => x.OnSettings, () => settings++));

        cut.Find("[data-testid=new-workout]").Click();
        cut.Find("[data-testid=history-link]").Click();
        cut.Find("[data-testid=settings]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(newWorkout, Is.EqualTo(1));
            Assert.That(history, Is.EqualTo(1));
            Assert.That(settings, Is.EqualTo(1));
        });
    }

    [Test]
    public void AndroidBack_HasNoHandler()
    {
        Context.Render<DashboardPage>();

        Assert.That(BackButton.HasHandler, Is.False);
    }

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];

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
