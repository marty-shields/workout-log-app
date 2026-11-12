using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Lists;

namespace WorkoutLog.UI.Tests.Components.Lists;

public class WorkoutTotalsCardTests : ComponentTestBase
{
    [Test]
    public void Render_ShowsTheFourLabelledValuesInOrder()
    {
        var cut = Render(new WorkoutTotals(32, 384, new TimeSpan(12, 30, 40), 2.5));

        Assert.That(cut.FindAll(".item").Select(i => (i.QuerySelector("dt")!.TextContent, i.QuerySelector("dd")!.TextContent)), Is.EqualTo(new[]
        {
            ("Workouts", "32"),
            ("Sets", "384"),
            ("Time trained", "12 h 30 min"),
            ("Per week", "2.5"),
        }));
    }

    [Test]
    public void Render_GivesEachValueAndTheCardATestId()
    {
        var cut = Render(new WorkoutTotals(32, 384, new TimeSpan(0, 45, 0), 3));

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("[data-testid=totals]"), Has.Count.EqualTo(1));
            Assert.That(cut.Find("[data-testid=totals-workouts]").TextContent, Is.EqualTo("32"));
            Assert.That(cut.Find("[data-testid=totals-sets]").TextContent, Is.EqualTo("384"));
            Assert.That(cut.Find("[data-testid=totals-time]").TextContent, Is.EqualTo("45 min"));
            Assert.That(cut.Find("[data-testid=totals-per-week]").TextContent, Is.EqualTo("3.0"));
        });
    }

    [Test]
    public void Card_IsANamedSection() =>
        Assert.That(Render(new WorkoutTotals(1, 1, TimeSpan.Zero, 1)).Find("section").GetAttribute("aria-label"), Is.EqualTo("Totals for these dates"));

    private IRenderedComponent<WorkoutTotalsCard> Render(WorkoutTotals totals) =>
        Context.Render<WorkoutTotalsCard>(p => p.Add(x => x.Totals, totals).Add(x => x.TestId, "totals"));
}
