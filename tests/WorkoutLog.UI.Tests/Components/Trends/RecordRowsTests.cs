using WorkoutLog.UI.Components.ThisWeek;
using WorkoutLog.UI.Components.Trends;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.Trends;

public class RecordRowsTests : ComponentTestBase
{
    [Test]
    public void ShowsEachRecordWithItsDateInPlaceOfTheWeekday()
    {
        var cut = Render(SampleTrends.MonthRecords);

        Assert.That(cut.FindAll(".detail").Select(d => d.TextContent), Is.EqualTo(new[] { "Wed 18 Nov", "Mon 16 Nov", "Sat 14 Nov", "Fri 6 Nov" }));
    }

    [Test]
    public void WithoutAWhen_ShowsTheWeekday()
    {
        var cut = Render([new WeekRecord(8, "Squat", "100kg × 5", DayOfWeek.Monday)]);

        Assert.That(cut.Find(".detail").TextContent, Is.EqualTo("Monday"));
    }

    [Test]
    public void MoreThanFive_ShowsFiveAndTheRest()
    {
        var cut = Render(SampleThisWeek.ManyRecords);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".rows li"), Has.Count.EqualTo(5));
            Assert.That(cut.Find("[data-testid=records-more]").TextContent, Is.EqualTo($"{SampleThisWeek.ManyRecords.Count - 5} more"));
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
    public void None_ShowsTheEmptyText() =>
        Assert.That(Render([]).Find("[data-testid=records-none]").TextContent, Is.EqualTo("No new records this month"));

    [Test]
    public void TappingARow_RaisesOnOpenWithTheExerciseId()
    {
        var opened = new List<int>();
        var cut = Context.Render<RecordRows>(p => p
            .Add(x => x.Records, SampleTrends.MonthRecords)
            .Add(x => x.EmptyText, "None")
            .Add(x => x.OnOpen, opened.Add)
            .Add(x => x.TestId, "records"));

        cut.Find($"[data-testid=records-row-{SampleTrends.MonthRecords[0].ExerciseId}]").Click();

        Assert.That(opened, Is.EqualTo(new[] { SampleTrends.MonthRecords[0].ExerciseId }));
    }

    [Test]
    public void WithoutOnOpen_RowsAreNotButtons() =>
        Assert.That(Render(SampleTrends.MonthRecords).FindAll("button"), Is.Empty);

    [Test]
    public void WithoutATestId_RendersNoTestIdAttributes() =>
        Assert.That(Context.Render<RecordRows>(p => p.Add(x => x.Records, SampleTrends.MonthRecords).Add(x => x.EmptyText, "None")).FindAll("[data-testid]"), Is.Empty);

    private IRenderedComponent<RecordRows> Render(IReadOnlyList<WeekRecord> records) =>
        Context.Render<RecordRows>(p => p
            .Add(x => x.Records, records)
            .Add(x => x.EmptyText, "No new records this month")
            .Add(x => x.TestId, "records"));
}
