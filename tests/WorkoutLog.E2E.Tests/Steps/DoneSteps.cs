using Reqnroll;
using WorkoutLog.E2E.Tests.Screens;
using WorkoutLog.E2E.Tests.Support;

namespace WorkoutLog.E2E.Tests.Steps;

/// <summary>Marking an exercise done, opening it again with the pencil and the delete prompt on an exercise.</summary>
[Binding]
public sealed class DoneSteps(WebView web, WorkoutScreen workout)
{
    [When(@"^I tap Done on (.+)$")]
    public void WhenITapDone(string name) => workout.Done(name);

    [Given(@"^I have marked (.+) done$")]
    public void GivenIHaveMarkedDone(string name)
    {
        workout.Done(name);
        web.WaitUntil(() => workout.IsDone(name), $"{name} didn't become done");
    }

    [When(@"^I tap the pencil on (.+)$")]
    public void WhenITapThePencil(string name) => workout.Edit(name);

    [When(@"^I tap the trash on (.+)$")]
    public void WhenITapTheTrash(string name) => workout.AskRemove(name);

    [Then(@"^(.+) is shown as one line with its name, the pencil and the trash$")]
    public void ThenIsOneLine(string name) => web.Eventually(() => workout.IsOneLine(name), Is.True, $"{name} as one line");

    [Then(@"^(.+) is shown open$")]
    public void ThenIsShownOpen(string name)
    {
        web.Settle();
        Assert.That(workout.IsDone(name), Is.False, $"{name} done");
    }

    [Then(@"^every exercise is shown as one line$")]
    public void ThenEveryExerciseIsOneLine()
    {
        web.Settle();
        Assert.That(workout.ExerciseNames().Select(workout.IsOneLine), Is.All.True);
    }
}
