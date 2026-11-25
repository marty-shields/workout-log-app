using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.ThisWeek;

namespace WorkoutLog.UI.Tests.Components.ThisWeek;

/// <summary>The sheet every + button goes through: This week and Trends call <see cref="NewWorkoutChoice.AskAsync"/> and nothing else.</summary>
public class NewWorkoutChoiceTests : ComponentTestBase
{
    [SetUp]
    public void Loose() => Context.JSInterop.Mode = JSRuntimeMode.Loose;

    [Test]
    public async Task Ask_NothingUnfinished_RaisesOnNewAndOpensNoSheet()
    {
        var (cut, raised) = Render();

        await cut.InvokeAsync(() => cut.Instance.AskAsync());

        Assert.That(raised, Is.EqualTo(new[] { "new" }));
        Assert.That(cut.FindAll("[data-testid=new-unfinished]"), Is.Empty);
    }

    [Test]
    public async Task Ask_WithAnUnfinishedWorkout_OpensTheSheetSayingWhatIsLostAndRaisesNothing()
    {
        Unfinished();
        var (cut, raised) = Render();

        await cut.InvokeAsync(() => cut.Instance.AskAsync());

        Assert.Multiple(() =>
        {
            Assert.That(raised, Is.Empty);
            Assert.That(cut.Find("[data-testid=new-unfinished] h2").TextContent, Is.EqualTo("Workout in progress"));
            Assert.That(cut.Find("[data-testid=new-unfinished-message]").TextContent, Does.Contain("1 exercise, 2 sets will be lost"));
            Assert.That(cut.Find("[data-testid=new-unfinished-resume]").TextContent.Trim(), Is.EqualTo("Resume"));
            Assert.That(cut.Find("[data-testid=new-unfinished-discard]").TextContent.Trim(), Is.EqualTo("Discard and start new"));
            Assert.That(cut.Find("[data-testid=new-unfinished-discard]").ClassList, Does.Contain("danger"));
        });
    }

    [Test]
    public async Task Ask_ReadFails_RaisesNothingSoAStoredWorkoutIsNeverStartedOver()
    {
        Unfinished();
        UnfinishedWorkout.FailReads = true;
        var (cut, raised) = Render();

        await cut.InvokeAsync(() => cut.Instance.AskAsync());

        Assert.That(raised, Is.Empty);
        Assert.That(cut.FindAll("[data-testid=new-unfinished]"), Is.Empty);
        Assert.That(UnfinishedWorkout.Stored, Is.Not.Null);
    }

    [Test]
    public async Task Ask_ReadsAfreshOnEveryTap()
    {
        var (cut, raised) = Render();
        await cut.InvokeAsync(() => cut.Instance.AskAsync());
        Unfinished();

        await cut.InvokeAsync(() => cut.Instance.AskAsync());

        Assert.That(raised, Is.EqualTo(new[] { "new" }), "the second tap opens the sheet instead");
        Assert.That(cut.FindAll("[data-testid=new-unfinished]"), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Resume_RaisesOnResumeClosesTheSheetAndKeepsTheWorkout()
    {
        Unfinished();
        var (cut, raised) = Render();
        await cut.InvokeAsync(() => cut.Instance.AskAsync());

        cut.Find("[data-testid=new-unfinished-resume]").Click();

        Assert.That(raised, Is.EqualTo(new[] { "resume" }));
        Assert.That(cut.FindAll("[data-testid=new-unfinished]"), Is.Empty);
        Assert.That(UnfinishedWorkout.Stored, Is.Not.Null);
    }

    [Test]
    public async Task DiscardAndStartNew_DeletesTheWorkoutThenRaisesOnNew()
    {
        Unfinished();
        var (cut, raised) = Render();
        await cut.InvokeAsync(() => cut.Instance.AskAsync());

        cut.Find("[data-testid=new-unfinished-discard]").Click();

        Assert.That(UnfinishedWorkout.Stored, Is.Null);
        Assert.That(raised, Is.EqualTo(new[] { "new" }));
        Assert.That(cut.FindAll("[data-testid=new-unfinished]"), Is.Empty);
    }

    [Test]
    public async Task DiscardAndStartNew_WriteFails_KeepsTheWorkoutAndStartsNothing()
    {
        Unfinished();
        var (cut, raised) = Render();
        await cut.InvokeAsync(() => cut.Instance.AskAsync());
        UnfinishedWorkout.FailWrites = true;

        cut.Find("[data-testid=new-unfinished-discard]").Click();

        Assert.That(UnfinishedWorkout.Stored, Is.Not.Null);
        Assert.That(raised, Is.Empty);
    }

    [Test]
    public async Task SheetClosed_KeepsTheWorkoutAndStartsNothing()
    {
        Unfinished();
        var (cut, raised) = Render();
        await cut.InvokeAsync(() => cut.Instance.AskAsync());

        cut.Find("[data-testid=new-unfinished-scrim]").Click();

        Assert.That(cut.FindAll("[data-testid=new-unfinished]"), Is.Empty);
        Assert.That((raised, UnfinishedWorkout.Stored is not null), Is.EqualTo((new List<string>(), true)));
    }

    [Test]
    public void NotAsked_ShowsNoSheet()
    {
        var (cut, _) = Render();

        Assert.That(cut.FindAll("[data-testid=new-unfinished]"), Is.Empty);
    }

    private (IRenderedComponent<NewWorkoutChoice> Cut, List<string> Raised) Render()
    {
        var raised = new List<string>();
        var cut = Context.Render<NewWorkoutChoice>(p => p
            .Add(x => x.OnNew, () => raised.Add("new"))
            .Add(x => x.OnResume, () => raised.Add("resume")));
        return (cut, raised);
    }

    /// <summary>One strength exercise with two sets, started two hours ago.</summary>
    private void Unfinished() =>
        UnfinishedWorkout.Stored = new UnfinishedWorkout(DateTimeOffset.UnixEpoch, null,
        [
            new UnfinishedExercise(Catalog.All.First(e => e.Type == ExerciseType.Strength).Id, null, false,
                [new UnfinishedEntry(new Dictionary<string, string?>()), new UnfinishedEntry(new Dictionary<string, string?>())]),
        ]);
}
