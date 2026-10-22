using WorkoutLog.UI.Models.History;
using WorkoutLog.UI.Screens;

namespace WorkoutLog.UI.Tests.Screens;

public class HistoryViewTests : ComponentTestBase
{
    private static readonly HistoryRow[] Rows =
    [
        new(2, new DateTime(2026, 10, 20, 18, 5, 0)),
        new(1, new DateTime(2026, 10, 20, 7, 30, 0)),
    ];

    [Test]
    public void Rows_ShowDayDateAndStartTimeInGivenOrder()
    {
        var cut = Context.Render<HistoryView>(p => p.Add(x => x.Rows, Rows));

        Assert.That(cut.FindAll(".text").Select(t => t.TextContent),
            Is.EqualTo(new[] { "Tue 20 Oct 2026 · 18:05", "Tue 20 Oct 2026 · 07:30" }));
    }

    [Test]
    public void NoRows_ShowsNoWorkoutsYet()
    {
        var cut = Context.Render<HistoryView>(p => p.Add(x => x.Rows, []));

        Assert.That(cut.Find("[data-testid=history-empty]").TextContent, Does.Contain("No workouts yet"));
    }

    [Test]
    public void WithOnOpen_TappingRowRaisesIt()
    {
        HistoryRow? opened = null;
        var cut = Context.Render<HistoryView>(p => p
            .Add(x => x.Rows, Rows)
            .Add(x => x.OnOpen, (HistoryRow row) => opened = row));

        cut.Find("[data-testid=history-row-1]").Click();

        Assert.That(opened, Is.EqualTo(Rows[1]));
    }

    [Test]
    public void WithoutOnOpen_RowsAreNotTappable()
    {
        var cut = Context.Render<HistoryView>(p => p.Add(x => x.Rows, Rows));

        Assert.That(cut.FindAll(".rows button"), Is.Empty);
    }

    [Test]
    public void Back_RaisesOnBack()
    {
        var back = false;
        var cut = Context.Render<HistoryView>(p => p
            .Add(x => x.Rows, Rows)
            .Add(x => x.OnBack, () => back = true));

        cut.Find("[data-testid=back]").Click();

        Assert.That(back, Is.True);
    }
}
