using WorkoutLog.UI.Components.Dashboard;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.Dashboard;

public class ConsistencyCardTests : ComponentTestBase
{
    [Test]
    public void Card_ShowsTheComparisonThenTheWeeksThenTheMusclesRow()
    {
        var cut = Render(6, () => { });

        var parts = cut.FindAll("[data-testid=consistency-comparison], [data-testid=consistency-bars], [data-testid=consistency-muscles]")
            .Select(e => e.GetAttribute("data-testid"));

        Assert.Multiple(() =>
        {
            Assert.That(parts, Is.EqualTo(new[] { "consistency-comparison", "consistency-bars", "consistency-muscles" }));
            Assert.That(cut.Find("h2").TextContent, Is.EqualTo("Last 12 weeks"));
            Assert.That(cut.FindComponent<WeekComparison>().Instance.ThisWeek, Is.EqualTo(SampleDashboard.ThisWeek));
            Assert.That(cut.FindComponent<WeekBars>().Instance.Counts, Is.EqualTo(SampleDashboard.WeekCounts));
        });
    }

    [TestCase(1, "1-week streak")]
    [TestCase(6, "6-week streak")]
    public void Streak_IsShown(int streak, string expected)
    {
        var cut = Render(streak);

        Assert.That(cut.Find("[data-testid=consistency-streak]").TextContent, Is.EqualTo(expected));
    }

    [Test]
    public void NoStreak_IsHidden()
    {
        var cut = Render(0);

        Assert.That(cut.FindAll("[data-testid=consistency-streak]"), Is.Empty);
    }

    [Test]
    public void MusclesRow_WithOnMuscles_RaisesIt()
    {
        var opened = 0;
        var cut = Render(6, () => opened++);

        cut.Find("button[data-testid=consistency-muscles]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(opened, Is.EqualTo(1));
            Assert.That(cut.Find("[data-testid=consistency-muscles] .text").TextContent, Is.EqualTo("Sets per muscle group"));
        });
    }

    [Test]
    public void MusclesRow_WithoutOnMuscles_IsNotTappable()
    {
        var cut = Render(6);

        Assert.That(cut.FindAll("button"), Is.Empty);
    }

    private IRenderedComponent<ConsistencyCard> Render(int streak, Action? onMuscles = null) =>
        Context.Render<ConsistencyCard>(p =>
        {
            p.Add(x => x.ThisWeek, SampleDashboard.ThisWeek)
                .Add(x => x.LastWeek, SampleDashboard.LastWeek)
                .Add(x => x.WeekCounts, SampleDashboard.WeekCounts)
                .Add(x => x.Streak, streak)
                .Add(x => x.TestId, "consistency");
            if (onMuscles is not null)
            {
                p.Add(x => x.OnMuscles, onMuscles);
            }
        });
}
