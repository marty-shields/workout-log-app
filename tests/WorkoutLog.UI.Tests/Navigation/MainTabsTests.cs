using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests.Navigation;

public class MainTabsTests
{
    [Test]
    public void Start_IsThisWeek() => Assert.That(MainTabs.Start, Is.EqualTo(MainTab.ThisWeek));

    [TestCase(MainTab.ThisWeek, "")]
    [TestCase(MainTab.Trends, "trends")]
    public void Route_IsTheTabsAppRoute(MainTab tab, string route) => Assert.That(MainTabs.Route(tab), Is.EqualTo(route));

    [Test]
    public void Items_AreThisWeekThenTrendsWithTheirTestIds() =>
        Assert.That(MainTabs.Items.Select(i => (i.Label, i.TestId)), Is.EqualTo(new[] { ("This week", "this-week-link"), ("Trends", "trends-link") }));

    [Test]
    public void Items_StartWithTheStartTab() => Assert.That(MainTabs.Items[0], Is.EqualTo(MainTabs.Item(MainTabs.Start)));

    [TestCase(MainTab.ThisWeek)]
    [TestCase(MainTab.Trends)]
    public void Item_AndTab_RoundTrip(MainTab tab) => Assert.That(MainTabs.Tab(MainTabs.Item(tab)), Is.EqualTo(tab));

    [Test]
    public void AddButton_IsLabelledNewWithThePlusIcon()
    {
        Assert.That(MainTabs.AddLabel, Is.EqualTo("New"));
        Assert.That(MainTabs.AddIcon, Is.EqualTo(Shields.UI.Components.Actions.IconName.Plus));
    }
}
