using WorkoutLog.UI.Components.Trends;

namespace WorkoutLog.UI.Tests.Components.Trends;

public class LongestStreakCardTests : ComponentTestBase
{
    [Test]
    public void Running_ShowsTheWeeksAndStillGoing()
    {
        var cut = Render(23, running: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".text").TextContent, Is.EqualTo("Longest streak"));
            Assert.That(cut.Find(".aside").TextContent, Is.EqualTo("23 weeks"));
            Assert.That(cut.Find(".detail").TextContent, Is.EqualTo("Still going"));
        });
    }

    [Test]
    public void Ended_ShowsNoDetail()
    {
        var cut = Render(14, running: false);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".aside").TextContent, Is.EqualTo("14 weeks"));
            Assert.That(cut.FindAll(".detail"), Is.Empty);
        });
    }

    [Test]
    public void OneWeek_IsSingular() =>
        Assert.That(Render(1, running: false).Find(".aside").TextContent, Is.EqualTo("1 week"));

    [Test]
    public void None_SaysNoneYet() =>
        Assert.That(Render(0, running: false).Find(".aside").TextContent, Is.EqualTo("None"));

    [Test]
    public void Row_IsNotTappable() =>
        Assert.That(Render(23, running: true).FindAll("button"), Is.Empty);

    private IRenderedComponent<LongestStreakCard> Render(int weeks, bool running) =>
        Context.Render<LongestStreakCard>(p => p.Add(x => x.Weeks, weeks).Add(x => x.Running, running).Add(x => x.TestId, "streak"));
}
