using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Tests.Models.Workout;

public class LastTimeLoaderTests
{
    private const int Squat = 6;

    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();
    private static readonly DateTimeOffset Before = new(2026, 10, 20, 23, 30, 0, TimeSpan.Zero);

    private FakeLastSessionService sessions = null!;
    private LastTimeLoader loader = null!;

    [SetUp]
    public void SetUp()
    {
        sessions = new FakeLastSessionService();
        loader = new LastTimeLoader(sessions, Catalog);
    }

    [Test]
    public async Task LoadAsync_WithALastSession_SetsItsText()
    {
        sessions.Set(new StrengthExercise(Squat, [new StrengthSet(8, 60m, null, null), new StrengthSet(6, 62.5m, null, null)]));
        var exercise = ExerciseFields.New(Catalog.All.Single(e => e.Id == Squat));

        await loader.LoadAsync(exercise);

        Assert.That(exercise.LastTime!.Entries, Is.EqualTo(new[] { "60×8", "62.5×6" }));
    }

    [Test]
    public async Task LoadAsync_NoLastSession_LeavesNone()
    {
        var exercise = ExerciseFields.New(Catalog.All.Single(e => e.Id == Squat));

        await loader.LoadAsync(exercise);

        Assert.That(exercise.LastTime, Is.Null);
    }

    [Test]
    public async Task LoadAsync_FailedRead_LeavesNone()
    {
        sessions.Failing = true;
        var exercise = ExerciseFields.New(Catalog.All.Single(e => e.Id == Squat));

        await loader.LoadAsync(exercise);

        Assert.That(exercise.LastTime, Is.Null);
    }

    [Test]
    public async Task LoadAsync_Before_PassesTheCutoffOn()
    {
        var exercise = ExerciseFields.New(Catalog.All.Single(e => e.Id == Squat));

        await loader.LoadAsync(exercise, Before);

        Assert.That(sessions.Requests, Is.EqualTo(new[] { (Squat, (DateTimeOffset?)Before) }));
    }

    [Test]
    public async Task LoadAsync_ExerciseNotInTheList_ReadsNothing()
    {
        var exercise = new ExerciseState(99999, "Unknown", ExerciseKind.Strength, []);

        await loader.LoadAsync(exercise);

        Assert.Multiple(() =>
        {
            Assert.That(exercise.LastTime, Is.Null);
            Assert.That(sessions.Requests, Is.Empty);
        });
    }
}
