using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Models.Dashboard;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class DashboardViewTests : ComponentTestBase
{
    private static readonly DateTime SampleStart = new(2026, 10, 20, 18, 5, 0);

    [Test]
    public void LastWorkout_ShowsDayAndDate()
    {
        var cut = Context.Render<DashboardView>(p => p
            .Add(x => x.Summary, new DashboardSummary(new DateTime(2026, 10, 20, 18, 5, 0), 2, 7)));

        Assert.That(cut.Find("[data-testid=last-workout]").TextContent, Is.EqualTo("Tue 20 Oct 2026"));
    }

    [Test]
    public void NoWorkouts_ShowsEmptyState()
    {
        var cut = Context.Render<DashboardView>(p => p
            .Add(x => x.Summary, new DashboardSummary(null, 0, 0)));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=last-workout]"), Is.Empty);
            Assert.That(cut.Find("[data-testid=last-workout-empty]").TextContent, Does.Contain("No workouts yet"));
        });
    }

    [Test]
    public void Counts_AreShown()
    {
        var cut = Context.Render<DashboardView>(p => p
            .Add(x => x.Summary, new DashboardSummary(null, 2, 7)));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=count-week] .value").TextContent, Is.EqualTo("2"));
            Assert.That(cut.Find("[data-testid=count-month] .value").TextContent, Is.EqualTo("7"));
        });
    }

    [Test]
    public void Buttons_RaiseTheirCallbacks()
    {
        var newWorkout = false;
        var workouts = false;
        var exercises = false;
        var cut = Context.Render<DashboardView>(p => p
            .Add(x => x.Summary, new DashboardSummary(null, 0, 0))
            .Add(x => x.OnNewWorkout, () => newWorkout = true)
            .Add(x => x.OnWorkouts, () => workouts = true)
            .Add(x => x.OnExercises, () => exercises = true));

        cut.Find("[data-testid=new-workout]").Click();
        cut.Find("[data-testid=workouts-link]").Click();
        cut.Find("[data-testid=exercises-link]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(newWorkout, Is.True);
            Assert.That(workouts, Is.True);
            Assert.That(exercises, Is.True);
        });
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void SettingsGear_RaisesOnSettingsInEveryState(bool loading, bool failed)
    {
        var settings = 0;
        var cut = Context.Render<DashboardView>(p => p
            .Add(x => x.Summary, new DashboardSummary(null, 0, 0))
            .Add(x => x.Loading, loading)
            .Add(x => x.Failed, failed)
            .Add(x => x.OnSettings, () => settings++));

        var gear = cut.Find("[data-testid=settings]");
        gear.Click();

        Assert.Multiple(() =>
        {
            Assert.That(gear.GetAttribute("aria-label"), Is.EqualTo("Settings"));
            Assert.That(settings, Is.EqualTo(1));
        });
    }

    [Test]
    public void SettingsGear_IsInTopBar()
    {
        var cut = Context.Render<DashboardView>(p => p
            .Add(x => x.Summary, new DashboardSummary(null, 0, 0)));

        Assert.That(cut.Find("header [data-testid=settings]"), Is.Not.Null);
    }

    [Test]
    public void SettingsGear_IsTheSettingsIcon()
    {
        var cut = Context.Render<DashboardView>(p => p
            .Add(x => x.Summary, new DashboardSummary(null, 0, 0)));

        var gear = cut.FindComponent<IconButton>().Instance;

        Assert.That((gear.TestId, gear.Icon, gear.Muted, gear.Size), Is.EqualTo(("settings", IconName.Settings, false, IconSize.Default)));
    }

    [Test]
    public void NewWorkout_IsInBottomBar()
    {
        var cut = Context.Render<DashboardView>(p => p
            .Add(x => x.Summary, new DashboardSummary(null, 0, 0)));

        Assert.That(cut.Find("[data-testid=bottom-bar] [data-testid=new-workout]"), Is.Not.Null);
    }

    [Test]
    public void Loading_ShowsTheSpinnerInsteadOfTheSummary()
    {
        var cut = Context.Render<DashboardView>(p => p
            .Add(x => x.Summary, new DashboardSummary(SampleStart, 2, 7))
            .Add(x => x.Loading, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=dashboard-loading]").TextContent, Does.Contain("Loading…"));
            Assert.That(cut.FindAll("[data-testid=last-workout]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=count-week]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=count-month]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=dashboard-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workouts-link]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercises-link]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=new-workout]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Failed_ShowsTheErrorAndTryAgainInsteadOfTheSummary()
    {
        var cut = Context.Render<DashboardView>(p => p
            .Add(x => x.Summary, new DashboardSummary(SampleStart, 2, 7))
            .Add(x => x.Failed, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=dashboard-error]").TextContent, Is.EqualTo("Couldn't load the Dashboard"));
            Assert.That(cut.Find("[data-testid=dashboard-error]").GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(cut.Find("[data-testid=dashboard-retry]").TextContent.Trim(), Is.EqualTo("Try again"));
            Assert.That(cut.FindAll("[data-testid=last-workout]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=count-week]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workouts-link]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercises-link]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=new-workout]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void FailedWhileLoading_ShowsTheSpinner()
    {
        var cut = Context.Render<DashboardView>(p => p
            .Add(x => x.Summary, new DashboardSummary(null, 0, 0))
            .Add(x => x.Loading, true)
            .Add(x => x.Failed, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=dashboard-loading]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=dashboard-error]"), Is.Empty);
        });
    }

    [Test]
    public void TryAgain_RaisesOnRetry()
    {
        var retries = 0;
        var cut = Context.Render<DashboardView>(p => p
            .Add(x => x.Summary, new DashboardSummary(null, 0, 0))
            .Add(x => x.Failed, true)
            .Add(x => x.OnRetry, () => retries++));

        cut.Find("[data-testid=dashboard-retry]").Click();

        Assert.That(retries, Is.EqualTo(1));
    }

    [Test]
    public void Failed_FocusesTryAgain()
    {
        var cut = Context.Render<DashboardView>(p => p.Add(x => x.Summary, new DashboardSummary(null, 0, 0)).Add(x => x.Failed, true));

        cut.WaitForAssertion(() => Focuses().Single().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=dashboard-retry]")));
    }

    [Test]
    public void Failed_RenderedAgain_DoesNotFocusAgain()
    {
        var cut = Context.Render<DashboardView>(p => p.Add(x => x.Summary, new DashboardSummary(null, 0, 0)).Add(x => x.Failed, true));
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Failed, true));

        Assert.That(Focuses(), Has.Count.EqualTo(1));
    }

    [Test]
    public void FailedAfterLoading_FocusesTryAgainAgain()
    {
        var cut = Context.Render<DashboardView>(p => p.Add(x => x.Summary, new DashboardSummary(null, 0, 0)).Add(x => x.Failed, true));
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Loading, true));
        cut.Render(p => p.Add(x => x.Loading, false));

        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(2)));
    }

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];
}
