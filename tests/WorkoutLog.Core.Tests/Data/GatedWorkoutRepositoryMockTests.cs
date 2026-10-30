using Moq;
using WorkoutLog.Core.Data;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Tests.Data;

public class GatedWorkoutRepositoryMockTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 21, 17, 5, 0, TimeSpan.Zero);
    private static readonly WorkoutExercise[] Exercises = [new StrengthExercise(1, [new StrengthSet(10, 20m, null, null)])];

    [Test]
    public async Task Calls_PassArgumentsAndResultsThrough()
    {
        var workout = new NewWorkout(Start, Start.AddHours(1), Exercises);
        var stored = new Workout(3, Start, Start.AddHours(1), Exercises);
        IReadOnlyList<Workout> all = [stored];
        IReadOnlyList<WorkoutSummary> summaries = [new WorkoutSummary(3, Start)];
        var outline = new WorkoutOutline(3, Start, Start.AddHours(1), [1]);
        IReadOnlyList<WorkoutTally> tallies = [new WorkoutTally(Start, Start.AddHours(1), 1)];
        IReadOnlyList<WorkoutWeights> weights = [new WorkoutWeights(3, [20m])];
        IReadOnlyList<LastSession> sessions = [new LastSession(1, Start, Exercises[0])];
        IReadOnlyList<WorkoutSession> workoutSessions = [new WorkoutSession(3, Start, Exercises[0])];
        IReadOnlyList<ExerciseSetCount> setCounts = [new ExerciseSetCount(3, 1, 1)];
        var inner = new Mock<IWorkoutRepository>(MockBehavior.Strict);
        inner.Setup(r => r.AddAsync(workout)).ReturnsAsync(3);
        inner.Setup(r => r.ReplaceExercisesAsync(3, Exercises)).Returns(Task.CompletedTask);
        inner.Setup(r => r.GetAsync(3)).ReturnsAsync(stored);
        inner.Setup(r => r.GetAllAsync()).ReturnsAsync(all);
        inner.Setup(r => r.ListAsync(Start, Start, 5)).ReturnsAsync(summaries);
        inner.Setup(r => r.GetLatestAsync()).ReturnsAsync(outline);
        inner.Setup(r => r.ListTalliesAsync(Start, Start.AddDays(1))).ReturnsAsync(tallies);
        inner.Setup(r => r.GetWeightsAsync(1)).ReturnsAsync(weights);
        inner.Setup(r => r.GetLastSessionsAsync()).ReturnsAsync(sessions);
        inner.Setup(r => r.GetSessionsAsync(1)).ReturnsAsync(workoutSessions);
        inner.Setup(r => r.GetSetCountsAsync(Start, Start)).ReturnsAsync(setCounts);
        inner.Setup(r => r.DeleteAsync(3)).Returns(Task.CompletedTask);
        var gated = new GatedWorkoutRepository(inner.Object, new DatabaseGate());

        Assert.That(await gated.AddAsync(workout), Is.EqualTo(3));
        await gated.ReplaceExercisesAsync(3, Exercises);
        Assert.That(await gated.GetAsync(3), Is.SameAs(stored));
        Assert.That(await gated.GetAllAsync(), Is.SameAs(all));
        Assert.That(await gated.ListAsync(Start, Start, 5), Is.SameAs(summaries));
        Assert.That(await gated.GetLatestAsync(), Is.SameAs(outline));
        Assert.That(await gated.ListTalliesAsync(Start, Start.AddDays(1)), Is.SameAs(tallies));
        Assert.That(await gated.GetWeightsAsync(1), Is.SameAs(weights));
        Assert.That(await gated.GetLastSessionsAsync(), Is.SameAs(sessions));
        Assert.That(await gated.GetSessionsAsync(1), Is.SameAs(workoutSessions));
        Assert.That(await gated.GetSetCountsAsync(Start, Start), Is.SameAs(setCounts));
        await gated.DeleteAsync(3);

        inner.VerifyAll();
    }

    [TestCase("Add", nameof(IWorkoutRepository.AddAsync))]
    [TestCase("Replace", nameof(IWorkoutRepository.ReplaceExercisesAsync))]
    [TestCase("Get", nameof(IWorkoutRepository.GetAsync))]
    [TestCase("GetAll", nameof(IWorkoutRepository.GetAllAsync))]
    [TestCase("List", nameof(IWorkoutRepository.ListAsync))]
    [TestCase("Latest", nameof(IWorkoutRepository.GetLatestAsync))]
    [TestCase("Tallies", nameof(IWorkoutRepository.ListTalliesAsync))]
    [TestCase("Weights", nameof(IWorkoutRepository.GetWeightsAsync))]
    [TestCase("LastSessions", nameof(IWorkoutRepository.GetLastSessionsAsync))]
    [TestCase("Sessions", nameof(IWorkoutRepository.GetSessionsAsync))]
    [TestCase("SetCounts", nameof(IWorkoutRepository.GetSetCountsAsync))]
    [TestCase("Delete", nameof(IWorkoutRepository.DeleteAsync))]
    public async Task Call_WhileTheGateIsHeld_DoesNotReachTheInner(string call, string method)
    {
        var inner = new Mock<IWorkoutRepository>();
        var gate = new DatabaseGate();
        IWorkoutRepository gated = new GatedWorkoutRepository(inner.Object, gate);
        var release = new TaskCompletionSource();
        var holder = gate.RunAsync(() => release.Task);

        Task pending = call switch
        {
            "Add" => gated.AddAsync(new NewWorkout(Start, Start, Exercises)),
            "Replace" => gated.ReplaceExercisesAsync(1, []),
            "Get" => gated.GetAsync(1),
            "GetAll" => gated.GetAllAsync(),
            "List" => gated.ListAsync(),
            "Latest" => gated.GetLatestAsync(),
            "Tallies" => gated.ListTalliesAsync(Start, Start),
            "Weights" => gated.GetWeightsAsync(1),
            "LastSessions" => gated.GetLastSessionsAsync(),
            "Sessions" => gated.GetSessionsAsync(1),
            "SetCounts" => gated.GetSetCountsAsync(Start, Start),
            _ => gated.DeleteAsync(1),
        };

        Assert.Multiple(() =>
        {
            Assert.That(pending.IsCompleted, Is.False);
            Assert.That(inner.Invocations, Is.Empty);
        });
        release.SetResult();
        await Task.WhenAll(holder, pending);
        Assert.That(inner.Invocations.Single().Method.Name, Is.EqualTo(method));
    }
}
