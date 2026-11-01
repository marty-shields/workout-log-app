using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Components.Dashboard;

namespace WorkoutLog.UI.Tests.Components.Dashboard;

public class WeekComparisonTests : ComponentTestBase
{
    private static readonly WeekTotals ThisWeek = new(3, new TimeSpan(3, 5, 0), 42);
    private static readonly WeekTotals LastWeek = new(2, new TimeSpan(2, 40, 0), 45);
    private static readonly WeekTotals Nothing = new(0, TimeSpan.Zero, 0);

    [Test]
    public void TitleColumnsAndNote_AreShown()
    {
        var cut = Render(ThisWeek, LastWeek);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("caption").TextContent, Is.EqualTo("This week vs last week"));
            Assert.That(cut.FindAll("thead th").Select(th => th.TextContent), Is.EqualTo(new[] { "This week", "Last week", "Change" }));
            Assert.That(cut.Find(".note").TextContent, Is.EqualTo("Last week up to the same day and time"));
        });
    }

    [Test]
    public void Rows_AreWorkoutsTimeAndSetsWithBothWeeks()
    {
        var cut = Render(ThisWeek, LastWeek);

        var rows = cut.FindAll("tbody tr").Select(r => r.QuerySelectorAll("th, td.value").Select(c => c.TextContent).ToArray());

        Assert.That(rows, Is.EqualTo(new[]
        {
            new[] { "Workouts", "3", "2" },
            new[] { "Time", "3h 05m", "2h 40m" },
            new[] { "Sets", "42", "45" },
        }));
    }

    [Test]
    public void Increase_ShowsTheUpIconInSuccessAndTheDifference()
    {
        var cut = Render(ThisWeek, LastWeek);

        var change = cut.Find("[data-testid=week-time] .change");
        var icon = cut.FindComponents<Icon>()[1].Instance;

        Assert.Multiple(() =>
        {
            Assert.That(TestText.Normalise(change.TextContent), Is.EqualTo("up 25m"));
            Assert.That(change.QuerySelector(".diff")!.ClassList, Does.Contain("up"));
            Assert.That((icon.Name, icon.Tone), Is.EqualTo((IconName.Up, IconTone.Success)));
        });
    }

    [Test]
    public void Decrease_ShowsTheDownIconInErrorAndTheDifference()
    {
        var cut = Render(ThisWeek, LastWeek);

        var change = cut.Find("[data-testid=week-sets] .change");
        var icon = cut.FindComponents<Icon>()[2].Instance;

        Assert.Multiple(() =>
        {
            Assert.That(TestText.Normalise(change.TextContent), Is.EqualTo("down 3"));
            Assert.That(change.QuerySelector(".diff")!.ClassList, Does.Contain("down"));
            Assert.That((icon.Name, icon.Tone), Is.EqualTo((IconName.Down, IconTone.Error)));
        });
    }

    [Test]
    public void NoChange_ShowsADashWithNoIcon()
    {
        var cut = Render(ThisWeek, ThisWeek);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".change").Select(c => TestText.Normalise(c.TextContent)), Is.All.EqualTo("—no change"));
            Assert.That(cut.FindAll(".change .same"), Has.Count.EqualTo(3));
            Assert.That(cut.FindComponents<Icon>(), Is.Empty);
        });
    }

    [Test]
    public void NothingLastWeek_ShowsTheWholeWeekAsTheIncrease()
    {
        var cut = Render(ThisWeek, Nothing);

        Assert.That(cut.FindAll(".change").Select(c => TestText.Normalise(c.TextContent)), Is.EqualTo(new[] { "up 3", "up 3h 05m", "up 42" }));
    }

    [Test]
    public void NothingThisWeek_ShowsZerosAndDecreases()
    {
        var cut = Render(Nothing, LastWeek);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("td.value:nth-of-type(1)").Select(c => c.TextContent), Is.EqualTo(new[] { "0", "0m", "0" }));
            Assert.That(cut.FindAll(".change").Select(c => TestText.Normalise(c.TextContent)), Is.EqualTo(new[] { "down 2", "down 2h 40m", "down 45" }));
        });
    }

    [Test]
    public void Time_IsComparedInWholeMinutes()
    {
        var cut = Render(ThisWeek with { Time = new TimeSpan(3, 5, 40) }, ThisWeek);

        Assert.That(TestText.Normalise(cut.Find("[data-testid=week-time] .change").TextContent), Is.EqualTo("—no change"));
    }

    private IRenderedComponent<WeekComparison> Render(WeekTotals thisWeek, WeekTotals lastWeek) =>
        Context.Render<WeekComparison>(p => p.Add(x => x.ThisWeek, thisWeek).Add(x => x.LastWeek, lastWeek).Add(x => x.TestId, "week"));
}
