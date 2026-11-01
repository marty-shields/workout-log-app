using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class DashboardViewTests : ComponentTestBase
{
    private const string Cards = "[data-testid=last], [data-testid=consistency], [data-testid=records]";

    [Test]
    public void Slots_ShowTheCardsInOrderAboveTheRows()
    {
        var cut = RenderWithCards();

        var shown = cut.FindAll(Cards + ", [data-testid=workouts-link]").Select(e => e.GetAttribute("data-testid"));

        Assert.That(shown, Is.EqualTo(new[] { "last", "consistency", "records", "workouts-link" }));
    }

    [Test]
    public void Rows_AreWorkoutsThenExercises()
    {
        var cut = Context.Render<DashboardView>();

        Assert.That(cut.FindAll("[data-testid=workouts-link], [data-testid=exercises-link]").Select(r => (r.GetAttribute("data-testid"), r.QuerySelector(".text")!.TextContent)),
            Is.EqualTo(new[] { ("workouts-link", "Workouts"), ("exercises-link", "Exercises") }));
    }

    [Test]
    public void Buttons_RaiseTheirCallbacks()
    {
        var newWorkout = false;
        var workouts = false;
        var exercises = false;
        var cut = Context.Render<DashboardView>(p => p
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
        var cut = Context.Render<DashboardView>();

        Assert.That(cut.Find("header [data-testid=settings]"), Is.Not.Null);
    }

    [Test]
    public void SettingsGear_IsTheSettingsIcon()
    {
        var cut = Context.Render<DashboardView>();

        var gear = cut.FindComponent<IconButton>().Instance;

        Assert.That((gear.TestId, gear.Icon, gear.Muted, gear.Size), Is.EqualTo(("settings", IconName.Settings, false, IconSize.Default)));
    }

    [Test]
    public void NewWorkout_IsInBottomBar()
    {
        var cut = Context.Render<DashboardView>();

        Assert.That(cut.Find("[data-testid=bottom-bar] [data-testid=new-workout]"), Is.Not.Null);
    }

    [Test]
    public void Loading_ShowsTheSpinnerInsteadOfTheCards()
    {
        var cut = RenderWithCards(p => p.Add(x => x.Loading, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=dashboard-loading]").TextContent, Does.Contain("Loading…"));
            Assert.That(cut.FindAll(Cards), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=dashboard-error]"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workouts-link]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercises-link]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=new-workout]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Failed_ShowsTheErrorAndTryAgainInsteadOfTheCards()
    {
        var cut = RenderWithCards(p => p.Add(x => x.Failed, true));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=dashboard-error]").TextContent, Is.EqualTo("Couldn't load the Dashboard"));
            Assert.That(cut.Find("[data-testid=dashboard-error]").GetAttribute("role"), Is.EqualTo("alert"));
            Assert.That(cut.Find("[data-testid=dashboard-retry]").TextContent.Trim(), Is.EqualTo("Try again"));
            Assert.That(cut.FindAll(Cards), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=workouts-link]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=exercises-link]"), Has.Count.EqualTo(1));
            Assert.That(cut.FindAll("[data-testid=new-workout]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void FailedWhileLoading_ShowsTheSpinner()
    {
        var cut = Context.Render<DashboardView>(p => p
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
            .Add(x => x.Failed, true)
            .Add(x => x.OnRetry, () => retries++));

        cut.Find("[data-testid=dashboard-retry]").Click();

        Assert.That(retries, Is.EqualTo(1));
    }

    [Test]
    public void Failed_FocusesTryAgain()
    {
        var cut = Context.Render<DashboardView>(p => p.Add(x => x.Failed, true));

        cut.WaitForAssertion(() => Focuses().Single().Arguments[0]
            .ShouldBeElementReferenceTo(cut.Find("[data-testid=dashboard-retry]")));
    }

    [Test]
    public void Failed_RenderedAgain_DoesNotFocusAgain()
    {
        var cut = Context.Render<DashboardView>(p => p.Add(x => x.Failed, true));
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Failed, true));

        Assert.That(Focuses(), Has.Count.EqualTo(1));
    }

    [Test]
    public void FailedAfterLoading_FocusesTryAgainAgain()
    {
        var cut = Context.Render<DashboardView>(p => p.Add(x => x.Failed, true));
        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(1)));

        cut.Render(p => p.Add(x => x.Loading, true));
        cut.Render(p => p.Add(x => x.Loading, false));

        cut.WaitForAssertion(() => Assert.That(Focuses(), Has.Count.EqualTo(2)));
    }

    private IRenderedComponent<DashboardView> RenderWithCards(Action<ComponentParameterCollectionBuilder<DashboardView>>? parameters = null) =>
        Context.Render<DashboardView>(p =>
        {
            p.Add(x => x.LastWorkout, "<p data-testid=\"last\"></p>")
                .Add(x => x.Consistency, "<p data-testid=\"consistency\"></p>")
                .Add(x => x.RecentRecords, "<p data-testid=\"records\"></p>");
            parameters?.Invoke(p);
        });

    private IReadOnlyList<JSRuntimeInvocation> Focuses() =>
        Context.JSInterop.Invocations["Blazor._internal.domWrapper.focus"];
}
