using WorkoutLog.UI.Models.Dashboard;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class DashboardViewTests : ComponentTestBase
{
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
        var history = false;
        var cut = Context.Render<DashboardView>(p => p
            .Add(x => x.Summary, new DashboardSummary(null, 0, 0))
            .Add(x => x.OnNewWorkout, () => newWorkout = true)
            .Add(x => x.OnHistory, () => history = true));

        cut.Find("[data-testid=new-workout]").Click();
        cut.Find("[data-testid=history-link]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(newWorkout, Is.True);
            Assert.That(history, Is.True);
        });
    }

    [Test]
    public void NewWorkout_IsInBottomBar()
    {
        var cut = Context.Render<DashboardView>(p => p
            .Add(x => x.Summary, new DashboardSummary(null, 0, 0)));

        Assert.That(cut.Find("[data-testid=bottom-bar] [data-testid=new-workout]"), Is.Not.Null);
    }
}
