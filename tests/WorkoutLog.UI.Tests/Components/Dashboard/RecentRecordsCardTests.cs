using WorkoutLog.UI.Components.Dashboard;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.Dashboard;

public class RecentRecordsCardTests : ComponentTestBase
{
    private static readonly (string, string)[] Expected =
    [
        ("Deadlift · 160kg × 3", "2 days ago"),
        ("Barbell Bench Press · 90kg × 5", "5 days ago"),
        ("Pull-up · 12 reps", "9 days ago"),
    ];

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void Records_ShowAsRowsInOrder(int count)
    {
        var cut = Render(SampleDashboard.Records.Take(count).ToList(), _ => { });

        var rows = cut.FindAll("[data-testid^=records-row-]").Select(r => (r.QuerySelector(".text")!.TextContent, r.QuerySelector(".detail")!.TextContent));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("h2").TextContent, Is.EqualTo("Recent records"));
            Assert.That(rows, Is.EqualTo(Expected.Take(count)));
        });
    }

    [Test]
    public void Row_RaisesOnOpenWithTheExerciseId()
    {
        int? opened = null;
        var cut = Render(SampleDashboard.Records, id => opened = id);

        cut.Find("button[data-testid=records-row-8]").Click();

        Assert.That(opened, Is.EqualTo(8));
    }

    [Test]
    public void WithoutOnOpen_RowsAreNotTappable()
    {
        var cut = Render(SampleDashboard.Records);

        Assert.That(cut.FindAll("button"), Is.Empty);
    }

    [Test]
    public void NoRecords_SaysSo()
    {
        var cut = Render([], _ => { });

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=records-none]").TextContent, Is.EqualTo("No new records in the last 30 days"));
            Assert.That(cut.FindAll("li"), Is.Empty);
        });
    }

    private IRenderedComponent<RecentRecordsCard> Render(IReadOnlyList<RecentRecord> records, Action<int>? onOpen = null) =>
        Context.Render<RecentRecordsCard>(p =>
        {
            p.Add(x => x.Records, records).Add(x => x.TestId, "records");
            if (onOpen is not null)
            {
                p.Add(x => x.OnOpen, onOpen);
            }
        });
}
