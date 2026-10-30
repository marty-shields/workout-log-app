using System.Globalization;
using WorkoutLog.UI.Components.Dashboard;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.Dashboard;

public class WeekBarsTests : ComponentTestBase
{
    [Test]
    public void Figure_IsNamedAndTheDrawingHidden()
    {
        var cut = Render(SampleDashboard.FullWeeks);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("figure").GetAttribute("aria-label"), Is.EqualTo("Workouts per week, last 12 weeks"));
            Assert.That(cut.Find("svg").GetAttribute("aria-hidden"), Is.EqualTo("true"));
        });
    }

    [Test]
    public void Bars_AreProportionalToTheMost()
    {
        var cut = Render([1, 2, 4]);

        var heights = cut.FindAll("rect").Select(r => double.Parse(r.GetAttribute("height")!, CultureInfo.InvariantCulture));

        Assert.That(heights, Is.EqualTo(new[] { 15d, 30d, 60d }));
    }

    [Test]
    public void Full_HasABarPerWeekWithThisWeekLastAndHighlighted()
    {
        var cut = Render(SampleDashboard.FullWeeks);

        var bars = cut.FindAll("rect");

        Assert.Multiple(() =>
        {
            Assert.That(bars, Has.Count.EqualTo(12));
            Assert.That(bars.Select(b => b.ClassList.Contains("this-week")), Is.EqualTo(Enumerable.Repeat(false, 11).Append(true)));
        });
    }

    [Test]
    public void Gaps_EmptyWeeksHaveNoBar()
    {
        var cut = Render(SampleDashboard.WeekCounts);

        Assert.That(cut.FindAll("rect"), Has.Count.EqualTo(11));
    }

    [Test]
    public void AllEmpty_ShowsOnlyTheBaseline()
    {
        var cut = Render(SampleDashboard.EmptyWeeks);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("rect"), Is.Empty);
            Assert.That(cut.FindAll("line.baseline"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void OneWeek_HasOneFullHeightBar()
    {
        var cut = Render(SampleDashboard.FirstWeekCounts);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("rect"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("rect").GetAttribute("height"), Is.EqualTo("60"));
            Assert.That(cut.Find("rect").ClassList, Does.Contain("this-week"));
        });
    }

    [Test]
    public void TextAlternative_ListsEachWeekOldestFirst()
    {
        var cut = Render([2, 0, 3, 1]);

        Assert.That(cut.FindAll("[data-testid=bars-values] li").Select(li => li.TextContent),
            Is.EqualTo(new[] { "3 weeks ago: 2 workouts", "2 weeks ago: 0 workouts", "Last week: 3 workouts", "This week: 1 workout" }));
    }

    private IRenderedComponent<WeekBars> Render(IReadOnlyList<int> counts) =>
        Context.Render<WeekBars>(p => p.Add(x => x.Counts, counts).Add(x => x.TestId, "bars"));
}
