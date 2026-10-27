using Moq;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using static WorkoutLog.Core.Tests.Import.ImportWorkouts;

namespace WorkoutLog.Core.Tests.Import.Checks;

public class UnknownExerciseCheckTests
{
    private static readonly ImportContext Context = new(new Dictionary<DateTimeOffset, int>());
    private readonly UnknownExerciseCheck check = new(new ExerciseCatalog());

    [Test]
    public void Check_EveryIdInTheList_Passes() =>
        Assert.That(check.Check(At(0, Bench, Run), Context), Is.Empty);

    [Test]
    public void Check_NoExercises_Passes() =>
        Assert.That(check.Check(At(0) with { Exercises = [] }, Context), Is.Empty);

    [Test]
    public void Check_AnIdNotInTheList_IsUnknownExercise() =>
        Assert.That(check.Check(At(0, Bench, Bench with { Exercise = new ImportExerciseInfo(999) }), Context),
            Is.EqualTo(new[] { ImportSkipReason.UnknownExercise }));
}
