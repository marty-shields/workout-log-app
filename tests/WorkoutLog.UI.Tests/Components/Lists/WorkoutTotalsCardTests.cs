using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Lists;

namespace WorkoutLog.UI.Tests.Components.Lists;

public class WorkoutTotalsCardTests : ComponentTestBase
{
    [Test]
    public void Card_ShowsTheFourLabelledValuesInOrder()
    {
        var cut = Context.Render<WorkoutTotalsCard>(p => p
            .Add(x => x.Totals, new WorkoutTotals(38, 612, new TimeSpan(12, 30, 0), 2.5))
            .Add(x => x.TestId, "workout-totals"));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("dt").Select(e => e.TextContent), Is.EqualTo(new[] { "Workouts", "Sets", "Time trained", "Per week" }));
            Assert.That(cut.Find("[data-testid=workout-totals-workouts]").TextContent, Is.EqualTo("38"));
            Assert.That(cut.Find("[data-testid=workout-totals-sets]").TextContent, Is.EqualTo("612"));
            Assert.That(cut.Find("[data-testid=workout-totals-time]").TextContent, Is.EqualTo("12 h 30 min"));
            Assert.That(cut.Find("[data-testid=workout-totals-per-week]").TextContent, Is.EqualTo("2.5"));
            Assert.That(cut.FindAll("[data-testid=workout-totals]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void UnderAnHour_ShowsMinutesOnly()
    {
        var cut = Context.Render<WorkoutTotalsCard>(p => p
            .Add(x => x.Totals, new WorkoutTotals(1, 8, TimeSpan.FromMinutes(45), 1)).Add(x => x.TestId, "t"));

        Assert.That(cut.Find("[data-testid=t-time]").TextContent, Is.EqualTo("45 min"));
    }
}
