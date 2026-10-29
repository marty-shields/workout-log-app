using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using static WorkoutLog.Core.Tests.Import.ImportWorkouts;

namespace WorkoutLog.Core.Tests.Import.Checks;

public class RepeatedExerciseCheckTests
{
    private static readonly ImportContext Context = new(new Dictionary<DateTimeOffset, int>());

    [Test]
    public void Check_EachExerciseOnce_Passes() =>
        Assert.That(new RepeatedExerciseCheck().Check(At(0, Bench, Run), Context), Is.Empty);

    [Test]
    public void Check_NoExercises_Passes() =>
        Assert.That(new RepeatedExerciseCheck().Check(At(0) with { Exercises = [] }, Context), Is.Empty);

    [Test]
    public void Check_ExerciseTwice_IsRepeatedExercise() =>
        Assert.That(new RepeatedExerciseCheck().Check(At(0, Bench, Run, Bench with { Sets = [new ImportSet(5, 70m)] }), Context),
            Is.EqualTo(new[] { ImportSkipReason.RepeatedExercise }));

    [Test]
    public void Check_UnknownExerciseTwice_IsRepeatedExercise() =>
        Assert.That(new RepeatedExerciseCheck().Check(At(0, Bench with { Exercise = new ImportExerciseInfo(999) }, Bench with { Exercise = new ImportExerciseInfo(999) }), Context),
            Is.EqualTo(new[] { ImportSkipReason.RepeatedExercise }));
}
