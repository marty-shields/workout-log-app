using Shields.UI.Components.Actions;
using WorkoutLog.UI.Components.Layout;
using WorkoutLog.UI.Navigation;

namespace WorkoutLog.UI.Tests.Components.Layout;

public class MainTabBarTests : ComponentTestBase
{
    [Test]
    public void Items_AreThisWeekNewTrendsWithIconAndLabel()
    {
        var cut = Context.Render<MainTabBar>(p => p.Add(x => x.Current, MainTab.ThisWeek));

        var items = cut.FindAll("nav button").Select(b => (b.GetAttribute("data-testid"), TestText.Normalise(b.TextContent), b.QuerySelectorAll("svg").Length));
        var icons = cut.FindComponents<Icon>().Select(i => i.Instance.Name);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("nav").GetAttribute("aria-label"), Is.EqualTo("Main"));
            Assert.That(items, Is.EqualTo(new[] { ("this-week-link", "This week", 1), ("new-link", "New", 1), ("trends-link", "Trends", 1) }));
            Assert.That(icons, Is.EqualTo(new[] { IconName.Calendar, IconName.Plus, IconName.BarChart }));
        });
    }

    [TestCase(MainTab.ThisWeek, "this-week-link")]
    [TestCase(MainTab.Trends, "trends-link")]
    public void CurrentTab_IsTheOnlyCurrentPage_AndIsMarkedBeyondColour(MainTab current, string testId)
    {
        var cut = Context.Render<MainTabBar>(p => p.Add(x => x.Current, current));

        var marked = cut.FindAll("[aria-current]").Select(b => (b.GetAttribute("data-testid"), b.GetAttribute("aria-current"), b.ClassList.Contains("current")));

        Assert.That(marked, Is.EqualTo(new[] { (testId, "page", true) }));
    }

    [Test]
    public void NewButton_IsNeverCurrent()
    {
        var cut = Context.Render<MainTabBar>(p => p.Add(x => x.Current, MainTab.Trends));

        Assert.That(cut.Find("[data-testid=new-link]").HasAttribute("aria-current"), Is.False);
    }

    [Test]
    public void TappingAnotherTab_RaisesOnNavigateWithIt()
    {
        var navigated = new List<MainTab>();
        var cut = Context.Render<MainTabBar>(p => p
            .Add(x => x.Current, MainTab.ThisWeek)
            .Add(x => x.OnNavigate, navigated.Add));

        cut.Find("[data-testid=trends-link]").Click();

        Assert.That(navigated, Is.EqualTo(new[] { MainTab.Trends }));
    }

    [Test]
    public void TappingTheCurrentTab_RaisesNothing()
    {
        var navigated = new List<MainTab>();
        var cut = Context.Render<MainTabBar>(p => p
            .Add(x => x.Current, MainTab.Trends)
            .Add(x => x.OnNavigate, navigated.Add));

        cut.Find("[data-testid=trends-link]").Click();

        Assert.That(navigated, Is.Empty);
    }

    [Test]
    public void New_RaisesOnNew()
    {
        var raised = 0;
        var cut = Context.Render<MainTabBar>(p => p
            .Add(x => x.Current, MainTab.ThisWeek)
            .Add(x => x.OnNew, () => raised++));

        cut.Find("[data-testid=new-link]").Click();

        Assert.That(raised, Is.EqualTo(1));
    }
}
