using WorkoutLog.UI.Components.ThisWeek;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.ThisWeek;

public class DayStripTests : ComponentTestBase
{
    [Test]
    public void Days_AreMondayToSundayWithAbbreviations()
    {
        var cut = Render(SampleThisWeek.Wednesday, SampleThisWeek.TrainedDays);

        Assert.That(cut.FindAll(".name").Select(n => n.TextContent), Is.EqualTo(new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" }));
    }

    [Test]
    public void TrainedDays_ShowATickAndNotTheDate()
    {
        var cut = Render(SampleThisWeek.Wednesday, SampleThisWeek.TrainedDays);

        Assert.Multiple(() =>
        {
            Assert.That(Day(cut, "monday").ClassList, Does.Contain("done"));
            Assert.That(Day(cut, "monday").QuerySelectorAll(".dot svg"), Has.Count.EqualTo(1));
            Assert.That(TestText.Normalise(Day(cut, "monday").QuerySelector(".dot")!.TextContent), Is.Empty);
            Assert.That(Day(cut, "wednesday").ClassList, Does.Contain("done"));
        });
    }

    [Test]
    public void UntrainedDays_ShowTheirDate()
    {
        var cut = Render(SampleThisWeek.Wednesday, [SampleThisWeek.Monday]);

        Assert.Multiple(() =>
        {
            Assert.That(Day(cut, "tuesday").ClassList, Does.Not.Contain("done"));
            Assert.That(Day(cut, "tuesday").QuerySelector(".dot")!.TextContent.Trim(), Is.EqualTo("17"));
            Assert.That(Day(cut, "sunday").QuerySelector(".dot")!.TextContent.Trim(), Is.EqualTo("22"));
        });
    }

    [Test]
    public void Today_HasARing_AndIsTheOnlyCurrentDate()
    {
        var cut = Render(SampleThisWeek.Wednesday, SampleThisWeek.TrainedDays);

        Assert.Multiple(() =>
        {
            Assert.That(cut.FindAll(".today").Select(d => d.GetAttribute("data-testid")), Is.EqualTo(new[] { "strip-wednesday" }));
            Assert.That(cut.FindAll("[aria-current=date]"), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void FutureDays_AreDimmed_PastAndTodayAreNot()
    {
        var cut = Render(SampleThisWeek.Wednesday, SampleThisWeek.TrainedDays);

        Assert.That(cut.FindAll(".future").Select(d => d.GetAttribute("data-testid")),
            Is.EqualTo(new[] { "strip-thursday", "strip-friday", "strip-saturday", "strip-sunday" }));
    }

    [Test]
    public void EachDay_HasATextAlternativeForTheStateNotJustColour()
    {
        var cut = Render(SampleThisWeek.Wednesday, [SampleThisWeek.Monday, SampleThisWeek.Wednesday]);

        var spoken = cut.FindAll(".day").Select(d => d.QuerySelector(".visually-hidden")!.TextContent);

        Assert.That(spoken, Is.EqualTo(new[]
        {
            "Monday 16 November, trained",
            "Tuesday 17 November, no workout",
            "Wednesday 18 November, trained, today",
            "Thursday 19 November, coming up",
            "Friday 20 November, coming up",
            "Saturday 21 November, coming up",
            "Sunday 22 November, coming up",
        }));
    }

    [Test]
    public void TodayWithNothingLogged_HasARingAndAnAlternative()
    {
        var cut = Render(SampleThisWeek.Monday, []);

        Assert.Multiple(() =>
        {
            Assert.That(Day(cut, "monday").ClassList, Does.Contain("today").And.Not.Contain("done"));
            Assert.That(Day(cut, "monday").QuerySelector(".visually-hidden")!.TextContent, Is.EqualTo("Monday 16 November, no workout, today"));
        });
    }

    private IRenderedComponent<DayStrip> Render(DateOnly today, IReadOnlyCollection<DateOnly> trained) =>
        Context.Render<DayStrip>(p => p
            .Add(x => x.Monday, SampleThisWeek.Monday)
            .Add(x => x.Today, today)
            .Add(x => x.Trained, trained)
            .Add(x => x.TestId, "strip"));

    private static AngleSharp.Dom.IElement Day(IRenderedComponent<DayStrip> cut, string day) => cut.Find($"[data-testid=strip-{day}]");
}
