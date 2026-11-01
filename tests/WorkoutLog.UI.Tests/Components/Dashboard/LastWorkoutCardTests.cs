using WorkoutLog.UI.Components.Actions;
using WorkoutLog.UI.Components.Dashboard;
using WorkoutLog.UI.Stories.Samples;

namespace WorkoutLog.UI.Tests.Components.Dashboard;

public class LastWorkoutCardTests : ComponentTestBase
{
    [Test]
    public void Workout_ShowsDateHowLongAgoAndDuration()
    {
        var cut = Render(SampleDashboard.Workout(3, 5));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=last-date]").TextContent, Is.EqualTo("Tue 27 Oct 2026"));
            Assert.That(cut.Find("[data-testid=last-when]").TextContent, Is.EqualTo("3 days ago · 1:12:04"));
        });
    }

    [TestCase(0, "Today")]
    [TestCase(1, "Yesterday")]
    public void RecentWorkout_SaysTodayOrYesterday(int daysAgo, string expected)
    {
        var cut = Render(SampleDashboard.Workout(daysAgo, 5));

        Assert.That(cut.Find("[data-testid=last-when]").TextContent, Does.StartWith($"{expected} · "));
    }

    [TestCase(1, "Barbell Bench Press")]
    [TestCase(2, "Barbell Bench Press, Cable Fly")]
    [TestCase(3, "Barbell Bench Press, Cable Fly +1")]
    [TestCase(5, "Barbell Bench Press, Cable Fly +3")]
    public void Exercises_ShowTheFirstTwoThenTheRestAsACount(int exercises, string expected)
    {
        var cut = Render(SampleDashboard.Workout(3, exercises));

        Assert.That(cut.Find("[data-testid=last-exercises]").TextContent, Is.EqualTo(expected));
    }

    [Test]
    public void WithOnOpen_IsAButtonWithAChevronThatRaisesIt()
    {
        var opened = 0;
        var cut = Render(SampleDashboard.LastWorkout, () => opened++);

        cut.Find("button[data-testid=last]").Click();

        Assert.Multiple(() =>
        {
            Assert.That(opened, Is.EqualTo(1));
            Assert.That(cut.FindComponent<Icon>().Instance.Name, Is.EqualTo(IconName.Chevron));
        });
    }

    [Test]
    public void WithoutOnOpen_IsNotTappable()
    {
        var cut = Render(SampleDashboard.LastWorkout);

        Assert.That(cut.FindAll("button"), Is.Empty);
    }

    [Test]
    public void NoWorkout_SaysNoWorkoutsYetAndIsNotTappable()
    {
        var cut = Render(null, () => { });

        Assert.Multiple(() =>
        {
            Assert.That(cut.Find("[data-testid=last-empty]").TextContent, Is.EqualTo("No workouts yet"));
            Assert.That(cut.FindAll("button"), Is.Empty);
            Assert.That(cut.FindAll("[data-testid=last-date]"), Is.Empty);
        });
    }

    private IRenderedComponent<LastWorkoutCard> Render(LastWorkout? workout, Action? onOpen = null) =>
        Context.Render<LastWorkoutCard>(p =>
        {
            p.Add(x => x.Workout, workout).Add(x => x.TestId, "last");
            if (onOpen is not null)
            {
                p.Add(x => x.OnOpen, onOpen);
            }
        });
}
