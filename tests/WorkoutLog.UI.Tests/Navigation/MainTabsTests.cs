using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests.Navigation;

public class MainTabsTests
{
    [Test]
    public void Start_IsThisWeek() => Assert.That(MainTabs.Start, Is.EqualTo(MainTab.ThisWeek));

    [TestCase(MainTab.ThisWeek, "")]
    [TestCase(MainTab.Trends, "trends")]
    public void Route_IsTheTabsAppRoute(MainTab tab, string route) => Assert.That(MainTabs.Route(tab), Is.EqualTo(route));
}
