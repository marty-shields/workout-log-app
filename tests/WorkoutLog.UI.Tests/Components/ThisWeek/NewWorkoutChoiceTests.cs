using Microsoft.Extensions.DependencyInjection;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Components.ThisWeek;

namespace WorkoutLog.UI.Tests.Components.ThisWeek;

/// <summary>What every + button goes through: This week and Trends call <see cref="NewWorkoutChoice.AskAsync"/> and render the kit sheet from its state.</summary>
public class NewWorkoutChoiceTests : ComponentTestBase
{
    private List<string> raised = [];

    [SetUp]
    public void Reset() => raised = [];

    private NewWorkoutChoice Choice()
    {
        var choice = Context.Services.GetRequiredService<NewWorkoutChoice>();
        choice.OnNew = () => { raised.Add("new"); return Task.CompletedTask; };
        choice.OnResume = () => { raised.Add("resume"); return Task.CompletedTask; };
        return choice;
    }

    [Test]
    public async Task Ask_NothingUnfinished_RaisesOnNewAndOpensNothing()
    {
        var choice = Choice();

        await choice.AskAsync();

        Assert.That(raised, Is.EqualTo(new[] { "new" }));
        Assert.That(choice.IsOpen, Is.False);
    }

    [Test]
    public async Task Ask_WithAnUnfinishedWorkout_OpensSayingWhatIsLostAndRaisesNothing()
    {
        Unfinished();
        var choice = Choice();

        await choice.AskAsync();

        Assert.Multiple(() =>
        {
            Assert.That(raised, Is.Empty);
            Assert.That(choice.IsOpen, Is.True);
            Assert.That(choice.Message, Does.Contain("1 exercise, 2 sets will be lost"));
        });
    }

    [Test]
    public async Task Ask_ReadFails_RaisesNothingSoAStoredWorkoutIsNeverStartedOver()
    {
        Unfinished();
        UnfinishedWorkout.FailReads = true;
        var choice = Choice();

        await choice.AskAsync();

        Assert.That(raised, Is.Empty);
        Assert.That(choice.IsOpen, Is.False);
        Assert.That(UnfinishedWorkout.Stored, Is.Not.Null);
    }

    [Test]
    public async Task Ask_ReadsAfreshOnEveryTap()
    {
        var choice = Choice();
        await choice.AskAsync();
        Unfinished();

        await choice.AskAsync();

        Assert.That(raised, Is.EqualTo(new[] { "new" }), "the second tap opens the sheet instead");
        Assert.That(choice.IsOpen, Is.True);
    }

    [Test]
    public async Task Ask_WhileAnotherAskIsReading_IsIgnored()
    {
        var choice = Choice();
        var gate = new TaskCompletionSource();
        choice.OnNew = async () => { raised.Add("new"); await gate.Task; };

        var first = choice.AskAsync();
        await choice.AskAsync();
        gate.SetResult();
        await first;

        Assert.That(raised, Is.EqualTo(new[] { "new" }));
    }

    [Test]
    public async Task Resume_RaisesOnResumeClosesAndKeepsTheWorkout()
    {
        Unfinished();
        var choice = Choice();
        await choice.AskAsync();

        await choice.ResumeAsync();

        Assert.That(raised, Is.EqualTo(new[] { "resume" }));
        Assert.That(choice.IsOpen, Is.False);
        Assert.That(UnfinishedWorkout.Stored, Is.Not.Null);
    }

    [Test]
    public async Task DiscardAndStart_DeletesTheWorkoutThenRaisesOnNew()
    {
        Unfinished();
        var choice = Choice();
        await choice.AskAsync();

        await choice.DiscardAndStartAsync();

        Assert.That(UnfinishedWorkout.Stored, Is.Null);
        Assert.That(raised, Is.EqualTo(new[] { "new" }));
        Assert.That(choice.IsOpen, Is.False);
    }

    [Test]
    public async Task DiscardAndStart_WriteFails_KeepsTheWorkoutAndStartsNothing()
    {
        Unfinished();
        var choice = Choice();
        await choice.AskAsync();
        UnfinishedWorkout.FailWrites = true;

        await choice.DiscardAndStartAsync();

        Assert.That(UnfinishedWorkout.Stored, Is.Not.Null);
        Assert.That(raised, Is.Empty);
    }

    [Test]
    public async Task Close_KeepsTheWorkoutAndStartsNothing()
    {
        Unfinished();
        var choice = Choice();
        await choice.AskAsync();

        choice.Close();

        Assert.That(choice.IsOpen, Is.False);
        Assert.That((raised, UnfinishedWorkout.Stored is not null), Is.EqualTo((new List<string>(), true)));
    }

    [Test]
    public void NotAsked_IsClosedWithNoMessage()
    {
        var choice = Choice();

        Assert.That((choice.IsOpen, choice.Message), Is.EqualTo((false, (string?)null)));
    }

    /// <summary>One strength exercise with two sets, started two hours ago.</summary>
    private void Unfinished() =>
        UnfinishedWorkout.Stored = new UnfinishedWorkout(DateTimeOffset.UnixEpoch, null,
        [
            new UnfinishedExercise(Catalog.All.First(e => e.Type == ExerciseType.Strength).Id, null, false,
                [new UnfinishedEntry(new Dictionary<string, string?>()), new UnfinishedEntry(new Dictionary<string, string?>())]),
        ]);
}
