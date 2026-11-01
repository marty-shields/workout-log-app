using WorkoutLog.UI.Models.Muscles;
using WorkoutLog.UI.Screens;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Screens;

public class MusclesViewTests : ComponentTestBase
{
    [Test]
    public void Title_IsSetsPerMuscleGroupForTheLast7Days()
    {
        var cut = Render(SampleDashboard.SomeTrained);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=screen-title]").TextContent, Is.EqualTo("Sets per muscle group"));
            Assert.That(cut.Find("[data-testid=screen-subtitle]").TextContent, Is.EqualTo("Last 7 days"));
        });
    }

    [Test]
    public void Groups_ShowTheirSetsInOrder()
    {
        var cut = Render(SampleDashboard.SomeTrained);

        Assert.That(Rows(cut), Is.EqualTo(new[]
        {
            ("muscles-chest", "Chest", "12 sets"),
            ("muscles-back", "Back", "9 sets"),
            ("muscles-shoulders", "Shoulders", "Not trained"),
            ("muscles-arms", "Arms", "1 set"),
            ("muscles-legs", "Legs", "Not trained"),
            ("muscles-core", "Core", "4 sets"),
        }));
    }

    [Test]
    public void NotTrained_IsMuted()
    {
        var cut = Render(SampleDashboard.SomeTrained);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=muscles-shoulders] .sets").ClassList, Does.Contain("none"));
            Assert.That(cut.Find("[data-testid=muscles-chest] .sets").ClassList, Does.Not.Contain("none"));
        });
    }

    [Test]
    public void NoneTrained_EveryGroupIsNotTrained()
    {
        var cut = Render(SampleDashboard.NoneTrained);

        Assert.That(Rows(cut).Select(r => r.Item3), Is.EqualTo(Enumerable.Repeat("Not trained", 6)));
    }

    [Test]
    public void Back_RaisesOnBack()
    {
        var back = 0;
        var cut = Context.Render<MusclesView>(p => p.Add(x => x.Groups, SampleDashboard.AllTrained).Add(x => x.OnBack, () => back++));

        cut.Find("[data-testid=back]").Click();

        Assert.That(back, Is.EqualTo(1));
    }

    [Test]
    public void Loading_ShowsTheSpinnerInsteadOfTheGroups()
    {
        var cut = Render(SampleDashboard.AllTrained, loading: true);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=muscles-loading]").TextContent, Does.Contain("Loading…"));
            Assert.That(cut.FindAll("li"), Is.Empty);
        });
    }

    [Test]
    public void Failed_ShowsTheErrorAndTryAgainThatRaisesOnRetry()
    {
        var retries = 0;
        var cut = Context.Render<MusclesView>(p => p
            .Add(x => x.Groups, SampleDashboard.AllTrained)
            .Add(x => x.Failed, true)
            .Add(x => x.OnRetry, () => retries++));

        cut.Find("[data-testid=muscles-retry]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=muscles-error]").TextContent, Is.EqualTo("Couldn't load your muscles"));
            Assert.That(retries, Is.EqualTo(1));
            Assert.That(cut.FindAll("li"), Is.Empty);
        });
    }

    private IRenderedComponent<MusclesView> Render(IReadOnlyList<MuscleGroupSets> groups, bool loading = false) =>
        Context.Render<MusclesView>(p => p.Add(x => x.Groups, groups).Add(x => x.Loading, loading));

    private static IEnumerable<(string?, string, string)> Rows(IRenderedComponent<MusclesView> cut) =>
        cut.FindAll("li").Select(li => (li.GetAttribute("data-testid"), li.Children[0].TextContent, li.QuerySelector(".sets")!.TextContent));
}
