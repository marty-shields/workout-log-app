using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Import;
using static WorkoutLog.Core.Tests.Import.ImportWorkouts;

namespace WorkoutLog.Core.Tests.Import;

public class ImportWorkoutTests
{
    [Test]
    public void StartAndFinish_ExportFormat_AreParsedAsUtcToTheTick()
    {
        var workout = At(0);

        Assert.That(workout.Start, Is.EqualTo(ImportJson.Start));
        Assert.That(workout.Start!.Value.Offset, Is.EqualTo(TimeSpan.Zero));
        Assert.That(workout.Finish, Is.EqualTo(ImportJson.Start.AddHours(1)));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("2026-10-21T17:05:00Z")]
    [TestCase("2026-10-21T17:05:00.1234567")]
    [TestCase("2026-10-21T17:05:00.1234567+01:00")]
    [TestCase("2026-10-21 17:05:00.1234567Z")]
    [TestCase("2026-02-30T17:05:00.1234567Z")]
    public void StartAndFinish_NotTheExportFormat_AreNull(string? text)
    {
        var workout = new ImportWorkout(1, text, text, []);

        Assert.That(workout.Start, Is.Null);
        Assert.That(workout.Finish, Is.Null);
    }

    [Test]
    public void Malformed_IsTrueOnlyWithoutExercises()
    {
        Assert.That(new ImportWorkout(1, null, null, null).Malformed, Is.True);
        Assert.That(new ImportWorkout(1, null, null, []).Malformed, Is.False);
    }

    [Test]
    public void ToNewWorkout_ReturnsTheTimesAndDomainExercises()
    {
        Assert.That(At(0, Bench, Run).ToNewWorkout(), Is.EqualTo(new NewWorkout(ImportJson.Start, ImportJson.Start.AddHours(1),
        [
            new StrengthExercise(8, [new StrengthSet(8, 60m, null, null)]),
            new CardioExercise(311, new CardioEntry(TimeSpan.FromMinutes(30), 5.2m, null, null, null, null, null)),
        ])).UsingPropertiesComparer());
    }

    [Test]
    public void ToNewWorkout_InvalidTimesOrMalformed_Throws()
    {
        Assert.That(() => new ImportWorkout(1, null, ImportJson.Time(ImportJson.Start), []).ToNewWorkout(), Throws.InvalidOperationException);
        Assert.That(() => new ImportWorkout(1, ImportJson.Time(ImportJson.Start), null, []).ToNewWorkout(), Throws.InvalidOperationException);
        Assert.That(() => new ImportWorkout(1, ImportJson.Time(ImportJson.Start), ImportJson.Time(ImportJson.Start), null).ToNewWorkout(), Throws.InvalidOperationException);
    }
}
