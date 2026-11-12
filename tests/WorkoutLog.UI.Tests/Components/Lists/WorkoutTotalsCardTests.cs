using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Lists;

namespace WorkoutLog.UI.Tests.Components.Lists;

public class WorkoutTotalsCardTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsTheFourLabelledValues()
    {
        var cut = Context.Render<WorkoutTotalsCard>(p => p
            .Add(x => x.Totals, new WorkoutTotals(38, 452, new TimeSpan(12, 30, 40), 2.46))
            .Add(x => x.TestId, "totals"));

        Assert.That(cut.FindAll(".item").Select(i => (i.QuerySelector("dt")!.TextContent, i.QuerySelector("dd")!.TextContent)), Is.EqualTo(new[]
        {
            ("Workouts", "38"),
            ("Sets", "452"),
            ("Time trained", "12 h 30 min"),
            ("Per week", "2.5"),
        }));
    }

    [Test]
    public void Render_GivesTheCardAndEachValueATestId()
    {
        var cut = Context.Render<WorkoutTotalsCard>(p => p
            .Add(x => x.Totals, new WorkoutTotals(3, 30, TimeSpan.FromMinutes(45), 3))
            .Add(x => x.TestId, "workout-totals"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=workout-totals]"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=workout-totals-workouts]").TextContent, Is.EqualTo("3"));
            Assert.That(cut.Find("[data-testid=workout-totals-sets]").TextContent, Is.EqualTo("30"));
            Assert.That(cut.Find("[data-testid=workout-totals-time]").TextContent, Is.EqualTo("45 min"));
            Assert.That(cut.Find("[data-testid=workout-totals-per-week]").TextContent, Is.EqualTo("3.0"));
        });
    }
}
