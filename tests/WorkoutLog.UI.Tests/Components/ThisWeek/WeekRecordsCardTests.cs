using Shields.UI.Components.Layout;
using WorkoutLog.UI.Components.ThisWeek;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.ThisWeek;

public class WeekRecordsCardTests : ComponentTestBase
{
    [Test]
    public void Card_IsCompactWithRowsAndNormalWithThePrompt()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Render(SampleThisWeek.Records).FindComponent<Card>().Instance.Compact, Is.True, "rows supply their own padding");
            Assert.That(Render([]).FindComponent<Card>().Instance.Compact, Is.False, "the prompt needs the card's padding below it");
        });
    }

    [Test]
    public void Row_ShowsExerciseAndBestWithTheWeekdayUnder()
    {
        var cut = Render(SampleThisWeek.Records);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("section.card").GetAttribute("data-testid"), Is.EqualTo("records"));
            Assert.That(cut.Find("section.card h2.title").TextContent, Is.EqualTo("Records this week"));
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
            Assert.That(cut.FindAll("li .text").Select(t => t.TextContent).Last(), Is.EqualTo("Treadmill run · 5.2 km"));
            Assert.That(cut.Find("[data-testid=records-more]").TextContent, Is.EqualTo("2 more"));
        });
    }

    [Test]
    public void Dividers_SeparateTheRowsAndTheLastRowKeepsOneOnlyAboveAMoreLine()
    {
        var five = Render(SampleThisWeek.ManyRecords.Take(5).ToList());
        var many = Render(SampleThisWeek.ManyRecords);

        Assert.Multiple(() =>
        {
            Assert.That(five.FindAll("li .row").Select(r => r.ClassList.Contains("no-divider")), Is.EqualTo(new[] { false, false, false, false, true }));
            Assert.That(many.FindAll("li .row").Select(r => r.ClassList.Contains("no-divider")), Is.All.False);
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
