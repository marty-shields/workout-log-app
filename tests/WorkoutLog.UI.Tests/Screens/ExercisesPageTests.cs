using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Lifecycle;
using WorkoutLog.UI.Navigation;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class ExercisesPageTests : ComponentTestBase
{
    private const int Squat = 6;
    private const int Curl = 132;
    private const int Run = 311;

    /// <summary>Wed 28 Oct 2026 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private static readonly LoggedExercisesResult Logged = new(
    [
        new LoggedExercise(Run, Now.AddDays(-1), new ExerciseBest(null, null, null, 5.2m)),
        new LoggedExercise(Squat, Now.AddDays(-8), new ExerciseBest(70m, 3, null, null)),
        new LoggedExercise(Curl, Now.AddYears(-1), null),
    ], Failed: false);

    private static readonly LoggedExercisesResult LoggedAgain = new(
    [
        new LoggedExercise(Squat, Now.AddHours(-1), new ExerciseBest(80m, 2, null, null)),
        .. Logged.Exercises,
    ], Failed: false);

    private FakeTimeProvider clock = null!;
    private FakeLogger<AppLifecycle> lifecycleLogger = null!;
    private AppLifecycle lifecycle = null!;

    [SetUp]
    public void SetUp()
    {
        clock = new FakeTimeProvider(Now);
        clock.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1"));
        Context.Services.AddSingleton<TimeProvider>(clock);
        LoggedExercises.Result = Logged;
        lifecycleLogger = new FakeLogger<AppLifecycle>();
        lifecycle = new AppLifecycle(lifecycleLogger);
        Context.Services.AddSingleton<IAppStart>(lifecycle);
    }

    [Test]
    public void WhileLoading_ShowsTheSpinnerOnly()
    {
        LoggedExercises.Hold = true;

        var cut = Context.Render<ExercisesPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercises-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercises-empty]"), Is.Empty);
            Assert.That(cut.FindAll(".toolbar"), Is.Empty);
        });
    }

    [Test]
    public void Loaded_ListsTheExercisesInOrderWithTheirLastSessionAndKind()
    {
        var cut = Context.Render<ExercisesPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid^=exercises-row-]").Select(r => r.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "exercises-row-311", "exercises-row-6", "exercises-row-132" }));
            Assert.That(cut.FindAll("[data-testid^=exercises-row-] .detail").Select(d => d.TextContent),
                Is.EqualTo(new[] { "Last Tue 27 Oct · 5.2km", "Last Tue 20 Oct · 70kg × 3", "Last Tue 28 Oct 2025" }));
            Assert.That(cut.FindAll("[data-testid^=exercises-row-] .aside").Select(a => a.TextContent), Is.EqualTo(new[] { "Cardio", "Strength", "Strength" }));
            Assert.That(cut.FindAll("button[data-testid^=exercises-row-]").Select(r => r.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "exercises-row-311", "exercises-row-6", "exercises-row-132" }));
        });
    }

    [Test]
    public void TappingALift_RaisesOnOpenWithItsId()
    {
        var opened = new List<int>();
        var cut = Context.Render<ExercisesPage>(p => p.Add(x => x.OnOpen, (int id) => opened.Add(id)));

        cut.Find("[data-testid=exercises-row-132]").Click();

        Assert.That(opened, Is.EqualTo(new[] { Curl }));
    }

    [Test]
    public void TappingACardioRow_RaisesOnOpenWithItsId()
    {
        var opened = new List<int>();
        var cut = Context.Render<ExercisesPage>(p => p.Add(x => x.OnOpen, (int id) => opened.Add(id)));

        cut.Find("[data-testid=exercises-row-311]").Click();

        Assert.That(opened, Is.EqualTo(new[] { Run }));
    }

    [Test]
    public void NothingLogged_ShowsTheMessage()
    {
        LoggedExercises.Result = new LoggedExercisesResult([], Failed: false);

        var cut = Context.Render<ExercisesPage>();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercises-empty]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercises-search]"), Is.Empty);
        });
    }

    [Test]
    public void LoadFailed_ShowsTheErrorAndTryAgainLoadsAgain()
    {
        LoggedExercises.Result = new LoggedExercisesResult([], Failed: true);
        var cut = Context.Render<ExercisesPage>();
        Assume.That(cut.Find("[data-testid=exercises-error]").TextContent, Is.EqualTo("Couldn't load your exercises"));
        LoggedExercises.Result = Logged;

        cut.Find("[data-testid=exercises-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(LoggedExercises.Loads, Is.EqualTo(2));
            Assert.That(cut.FindAll("[data-testid=exercises-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid^=exercises-row-]"), Has.Count.EqualTo(3));
        });
    }

    [Test]
    public void Search_RanksTheMatchesAmongTheLoggedExercises()
    {
        var cut = Context.Render<ExercisesPage>();

        cut.Find("[data-testid=exercises-search]").Input("barbell");

        Assert.That(cut.FindAll("[data-testid^=exercises-row-]").Select(r => r.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercises-row-6" }));
    }

    [Test]
    public void Search_NoMatch_ShowsNoExercisesMatch()
    {
        var cut = Context.Render<ExercisesPage>();

        cut.Find("[data-testid=exercises-search]").Input("zumba");

        Assert.That(cut.Find("[data-testid=exercises-none]").TextContent, Is.EqualTo("No exercises match"));
    }

    [TestCase("exercises-filter-strength", null, new[] { "exercises-row-6", "exercises-row-132" })]
    [TestCase("exercises-filter-cardio", null, new[] { "exercises-row-311" })]
    [TestCase("exercises-filter-strength", "curl", new[] { "exercises-row-132" })]
    [TestCase("exercises-filter-cardio", "curl", new string[0])]
    public void Chip_FiltersWithAndWithoutSearch(string chip, string? query, string[] expected)
    {
        var cut = Context.Render<ExercisesPage>();
        if (query is not null)
        {
            cut.Find("[data-testid=exercises-search]").Input(query);
        }

        cut.Find($"[data-testid={chip}]").Change(true);

        Assert.That(cut.FindAll("[data-testid^=exercises-row-]").Select(r => r.GetAttribute("data-testid")), Is.EqualTo(expected));
    }

    [Test]
    public async Task AppStarted_ReloadsWithTheSpinnerThenShowsTheNewValues()
    {
        var cut = Context.Render<ExercisesPage>();
        LoggedExercises.Hold = true;
        LoggedExercises.Result = LoggedAgain;

        var started = lifecycle.NotifyStartedAsync();

        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=exercises-loading]"), Has.Count.EqualTo(1)));
        LoggedExercises.Complete();

        cut.WaitForAssertion(() => Assert.That(cut.Find("[data-testid=exercises-row-6] .detail").TextContent, Is.EqualTo("Last Wed 28 Oct · 80kg × 2")));
        Assert.That(LoggedExercises.Loads, Is.EqualTo(2));
        await started;
    }

    [Test]
    public async Task AppStarted_KeepsTheSearchAndChip()
    {
        var cut = Context.Render<ExercisesPage>();
        cut.Find("[data-testid=exercises-search]").Input("curl");
        cut.Find("[data-testid=exercises-filter-strength]").Change(true);

        await cut.InvokeAsync(lifecycle.NotifyStartedAsync);

        Assert.Multiple(() =>
        {
            Assert.That(LoggedExercises.Loads, Is.EqualTo(2));
            Assert.That(cut.Find("[data-testid=exercises-search]").GetAttribute("value"), Is.EqualTo("curl"));
            Assert.That(cut.FindAll("[data-testid^=exercises-row-]").Select(r => r.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercises-row-132" }));
        });
    }

    [Test]
    public async Task AppStarted_FailedReload_ShowsTheError()
    {
        var cut = Context.Render<ExercisesPage>();
        LoggedExercises.Result = new LoggedExercisesResult([], Failed: true);

        await cut.InvokeAsync(lifecycle.NotifyStartedAsync);

        Assert.That(cut.FindAll("[data-testid=exercises-error]"), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Disposed_AppStartedNoLongerReloads()
    {
        Context.Render<ExercisesPage>();

        await Context.DisposeComponentsAsync();
        await lifecycle.NotifyStartedAsync();

        Assert.That(LoggedExercises.Loads, Is.EqualTo(1));
    }

    [Test]
    public async Task Disposed_WhileReloading_ReloadFinishesWithoutError()
    {
        var cut = Context.Render<ExercisesPage>();
        LoggedExercises.Hold = true;
        var started = lifecycle.NotifyStartedAsync();
        cut.WaitForAssertion(() => Assert.That(LoggedExercises.Loads, Is.EqualTo(2)));

        await Context.DisposeComponentsAsync();
        LoggedExercises.Complete();
        await WaitForRendererAsync();

        Assert.That(async () => await started, Throws.Nothing);
        Assert.That(lifecycleLogger.Collector.Count, Is.Zero);
    }

    [Test]
    public async Task Refresh_FastReload_KeepsTheSpinnerUpForHalfASecond()
    {
        var cut = Context.Render<ExercisesPage>();
        LoggedExercises.Result = LoggedAgain;

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        await WaitForRendererAsync();
        clock.Advance(TimeSpan.FromMilliseconds(499));
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercises-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid^=exercises-row-]"), Is.Empty);
        });
        clock.Advance(TimeSpan.FromMilliseconds(1));
        await refresh;
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercises-loading]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=exercises-row-6] .detail").TextContent, Is.EqualTo("Last Wed 28 Oct · 80kg × 2"));
        });
    }

    [Test]
    public async Task Refresh_SlowReload_ShowsTheNewValuesAsSoonAsItFinishes()
    {
        var cut = Context.Render<ExercisesPage>();
        LoggedExercises.Hold = true;
        LoggedExercises.Result = LoggedAgain;

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        clock.Advance(TimeSpan.FromMilliseconds(800));
        await WaitForRendererAsync();

        Assert.That(cut.FindAll("[data-testid=exercises-loading]"), Has.Count.EqualTo(1));
        LoggedExercises.Complete();
        cut.WaitForAssertion(() => Assert.That(cut.FindAll("[data-testid=exercises-loading]"), Is.Empty));
        await refresh;
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=exercises-row-6] .detail").TextContent, Is.EqualTo("Last Wed 28 Oct · 80kg × 2"));
        });
    }

    [Test]
    public async Task Refresh_KeepsTheSearchAndChip()
    {
        var cut = Context.Render<ExercisesPage>();
        cut.Find("[data-testid=exercises-search]").Input("curl");
        cut.Find("[data-testid=exercises-filter-strength]").Change(true);

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        clock.Advance(TimeSpan.FromMilliseconds(500));
        await refresh;

        Assert.Multiple(() =>
        {
            Assert.That(LoggedExercises.Loads, Is.EqualTo(2));
            Assert.That(cut.Find("[data-testid=exercises-search]").GetAttribute("value"), Is.EqualTo("curl"));
            Assert.That(cut.FindAll("[data-testid^=exercises-row-]").Select(r => r.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercises-row-132" }));
        });
    }

    [Test]
    public async Task Refresh_KeepsTheSearchAndChipsUpAndTakesChangesMadeDuringIt()
    {
        var cut = Context.Render<ExercisesPage>();
        cut.Find("[data-testid=exercises-search]").Input("curl");
        LoggedExercises.Hold = true;
        LoggedExercises.Result = Logged with
        {
            Exercises = [new LoggedExercise(Squat, Now.AddHours(-1), new ExerciseBest(80m, 2, null, null)), .. Logged.Exercises.Where(e => e.ExerciseId != Squat)],
        };

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercises-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid^=exercises-row-]"), Is.Empty);
            Assert.That(cut.Find(".toolbar [data-testid=exercises-search]").GetAttribute("value"), Is.EqualTo("curl"));
        });
        cut.Find("[data-testid=exercises-search]").Input("squat");
        cut.Find("[data-testid=exercises-filter-strength]").Change(true);
        LoggedExercises.Complete();
        clock.Advance(TimeSpan.FromMilliseconds(500));
        await refresh;
        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".toolbar [data-testid=exercises-search]").GetAttribute("value"), Is.EqualTo("squat"));
            Assert.That(cut.FindAll("[data-testid=exercises-filter] input[checked]").Select(i => i.GetAttribute("data-testid")),
                Is.EqualTo(new[] { "exercises-filter-strength" }));
            Assert.That(cut.FindAll("[data-testid^=exercises-row-]").Select(r => r.GetAttribute("data-testid")), Is.EqualTo(new[] { "exercises-row-6" }));
            Assert.That(cut.Find("[data-testid=exercises-row-6] .detail").TextContent, Is.EqualTo("Last Wed 28 Oct · 80kg × 2"));
        });
    }

    [Test]
    public async Task Refresh_Failed_ShowsTheErrorAfterHalfASecond()
    {
        var cut = Context.Render<ExercisesPage>();
        LoggedExercises.Result = new LoggedExercisesResult([], Failed: true);

        var refresh = cut.InvokeAsync(cut.FindComponent<AppScreen>().Instance.RefreshAsync);
        await WaitForRendererAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercises-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercises-error]"), Is.Empty);
        });
        clock.Advance(TimeSpan.FromMilliseconds(500));
        await refresh;
        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=exercises-error]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll(".toolbar"), Is.Empty);
        });
    }

    [Test]
    public void NavigationBar_RaisesOnNavigate()
    {
        var navigated = new List<NavDestination>();
        var cut = Context.Render<ExercisesPage>(p => p.Add(x => x.OnNavigate, navigated.Add));

        cut.Find("[data-testid=dashboard-link]").Click();

        Assert.That(navigated, Is.EqualTo(new[] { NavDestination.Dashboard }));
    }

    [Test]
    public async Task AndroidBack_NavigatesToTheDashboard()
    {
        var navigated = new List<NavDestination>();
        var cut = Context.Render<ExercisesPage>(p => p.Add(x => x.OnNavigate, navigated.Add));

        await cut.InvokeAsync(BackButton.HandleAsync);

        Assert.That(navigated, Is.EqualTo(new[] { NavDestination.Dashboard }));
    }
}
