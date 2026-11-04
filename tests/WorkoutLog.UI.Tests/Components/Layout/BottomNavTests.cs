using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests.Components.Layout;

public class BottomNavTests : ComponentTestBase
{
    [Test]
    public void Items_AreDashboardWorkoutsExercisesWithIconAndLabel()
    {
        var cut = Context.Render<BottomNav>(p => p.Add(x => x.Current, NavDestination.Dashboard));

        var items = cut.FindAll("nav button").Select(b => (b.GetAttribute("data-testid"), b.TextContent.Trim(), b.QuerySelectorAll("svg").Length));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("nav").GetAttribute("aria-label"), Is.EqualTo("Main"));
            Assert.That(items, Is.EqualTo(new[] { ("dashboard-link", "Dashboard", 1), ("workouts-link", "Workouts", 1), ("exercises-link", "Exercises", 1) }));
        });
    }

    [TestCase(NavDestination.Dashboard, "dashboard-link")]
    [TestCase(NavDestination.Workouts, "workouts-link")]
    [TestCase(NavDestination.Exercises, "exercises-link")]
    public void CurrentItem_IsTheOnlyCurrentPage(NavDestination current, string testId)
    {
        var cut = Context.Render<BottomNav>(p => p.Add(x => x.Current, current));

        var marked = cut.FindAll("[aria-current]").Select(b => (b.GetAttribute("data-testid"), b.GetAttribute("aria-current"), b.ClassList.Contains("current")));

        Assert.That(marked, Is.EqualTo(new[] { (testId, "page", true) }));
    }

    [Test]
    public void TappingAnotherItem_RaisesOnNavigateWithIt()
    {
        var navigated = new List<NavDestination>();
        var cut = Context.Render<BottomNav>(p => p
            .Add(x => x.Current, NavDestination.Workouts)
            .Add(x => x.OnNavigate, navigated.Add));

        cut.Find("[data-testid=exercises-link]").Click();
        cut.Find("[data-testid=dashboard-link]").Click();

        Assert.That(navigated, Is.EqualTo(new[] { NavDestination.Exercises, NavDestination.Dashboard }));
    }

    [Test]
    public void TappingTheCurrentItem_DoesNothing()
    {
        var navigated = new List<NavDestination>();
        var cut = Context.Render<BottomNav>(p => p
            .Add(x => x.Current, NavDestination.Workouts)
            .Add(x => x.OnNavigate, navigated.Add));

        cut.Find("[data-testid=workouts-link]").Click();

        Assert.That(navigated, Is.Empty);
    }
}
