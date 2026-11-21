using Shields.UI.Components.Layout;
using WorkoutLog.UI.Components.ThisWeek;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.ThisWeek;

public class WeekWorkoutsCardTests : ComponentTestBase
{
    [Test]
    public void Title_IsShown()
    {
        var cut = Render(SampleThisWeek.Workouts);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("section.card").GetAttribute("data-testid"), Is.EqualTo("workouts"));
            Assert.That(cut.Find("section.card h2.title").TextContent, Is.EqualTo("This week's workouts"));
        });
    }

    [Test]
    public void Rows_AreNewestFirstWhateverTheOrderGiven()
    {
        var cut = Render(SampleThisWeek.Workouts);

        Assert.That(cut.FindAll(".text").Select(t => t.TextContent), Is.EqualTo(new[] { "Wed 18 Nov · 07:10", "Tue 17 Nov · 18:30", "Mon 16 Nov · 18:05" }));
    }

    [Test]
    public void Row_ShowsFirstTwoExercisesThenCount_AndDuration()
    {
        var cut = Render(SampleThisWeek.Workouts);

        var rows = cut.FindAll("li").Select(li => (li.QuerySelector(".detail")!.TextContent, li.QuerySelector(".aside")!.TextContent));

        Assert.That(rows, Is.EqualTo(new[]
        {
            ("Squat, Leg press +1", "0:58"),
            ("Treadmill run", "0:35"),
            ("Bench press, Overhead press +2", "1:02"),
        }));
    }

    [Test]
    public void DurationOverAnHour_IsHoursAndMinutes()
    {
        var cut = Render([new WeekWorkout(1, new DateTime(2026, 11, 16, 9, 0, 0), ["Squat", "Press"], new TimeSpan(1, 5, 59))]);

        Assert.That(cut.Find(".aside").TextContent, Is.EqualTo("1:05"));
    }

    [Test]
    public void Card_IsCompactWithRowsAndNormalWithThePrompt()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Render(SampleThisWeek.Workouts).FindComponent<Card>().Instance.Compact, Is.True, "rows supply their own padding");
            Assert.That(Render([]).FindComponent<Card>().Instance.Compact, Is.False, "the prompt needs the card's padding below it");
        });
    }

    [Test]
    public void Nothing_ShowsThePrompt()
    {
        var cut = Render([]);

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=workouts-none]").TextContent, Is.EqualTo("Nothing yet this week. Tap + to start one."));
            Assert.That(cut.FindAll("li"), Is.Empty);
        });
    }

    [Test]
    public void TappingARow_RaisesOnOpenWithTheWorkoutId()
    {
        var opened = new List<int>();
        var cut = Render(SampleThisWeek.Workouts, opened.Add);

        cut.Find("[data-testid=workouts-row-32]").Click();

        Assert.That(opened, Is.EqualTo(new[] { 32 }));
    }

    [Test]
    public void WithoutOnOpen_RowsAreNotButtons()
    {
        var cut = Render(SampleThisWeek.Workouts);

        Assert.That(cut.FindAll("button"), Is.Empty);
    }

    private IRenderedComponent<WeekWorkoutsCard> Render(IReadOnlyList<WeekWorkout> workouts, Action<int>? onOpen = null) =>
        Context.Render<WeekWorkoutsCard>(p =>
        {
            p.Add(x => x.Workouts, workouts).Add(x => x.TestId, "workouts");
            if (onOpen is not null)
            {
                p.Add(x => x.OnOpen, onOpen);
            }
        });
}
