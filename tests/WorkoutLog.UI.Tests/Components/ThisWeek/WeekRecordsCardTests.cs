using WorkoutLog.UI.Components.ThisWeek;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.ThisWeek;

public class WeekRecordsCardTests : ComponentTestBase
{
    [Test]
    public void Row_ShowsExerciseAndBestWithTheWeekdayUnder()
    {
        var cut = Render(SampleThisWeek.Records);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find(".title").TextContent, Is.EqualTo("Records this week"));
            Assert.That(cut.Find(".text").TextContent, Is.EqualTo("Bench press · 82.5kg × 5"));
            Assert.That(cut.Find(".detail").TextContent, Is.EqualTo("Monday"));
            Assert.That(cut.FindAll("[data-testid=records-more]"), Is.Empty);
        });
    }

    [Test]
    public void UpToFiveRows_ShowWithNoMore()
    {
        var cut = Render(SampleThisWeek.ManyRecords.Take(5).ToList());

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("li"), Has.Count.EqualTo(5));
            Assert.That(cut.FindAll("[data-testid=records-more]"), Is.Empty);
        });
    }

    [Test]
    public void OverFive_ShowsFiveThenHowManyMore()
    {
        var cut = Render(SampleThisWeek.ManyRecords);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll("li"), Has.Count.EqualTo(5));
            Assert.That(cut.FindAll(".text").Select(t => t.TextContent).Last(), Is.EqualTo("Treadmill run · 5.2 km"));
            Assert.That(cut.Find("[data-testid=records-more]").TextContent, Is.EqualTo("2 more"));
        });
    }

    [Test]
    public void None_ShowsAMessage()
    {
        var cut = Render([]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=records-none]").TextContent, Is.EqualTo("No new records this week"));
            Assert.That(cut.FindAll("li"), Is.Empty);
        });
    }

    [Test]
    public void TappingARow_RaisesOnOpenWithTheExerciseId()
    {
        var opened = new List<int>();
        var cut = Render(SampleThisWeek.Records, opened.Add);

        cut.Find("[data-testid=records-row-8]").Click();

        Assert.That(opened, Is.EqualTo(new[] { 8 }));
    }

    [Test]
    public void WithoutOnOpen_RowsAreNotButtons()
    {
        var cut = Render(SampleThisWeek.Records);

        Assert.That(cut.FindAll("button"), Is.Empty);
    }

    private IRenderedComponent<WeekRecordsCard> Render(IReadOnlyList<WeekRecord> records, Action<int>? onOpen = null) =>
        Context.Render<WeekRecordsCard>(p =>
        {
            p.Add(x => x.Records, records).Add(x => x.TestId, "records");
            if (onOpen is not null)
            {
                p.Add(x => x.OnOpen, onOpen);
            }
        });
}
