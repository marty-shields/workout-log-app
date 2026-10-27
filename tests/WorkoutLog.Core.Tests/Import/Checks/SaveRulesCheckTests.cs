using Moq;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using static WorkoutLog.Core.Tests.Import.ImportWorkouts;

namespace WorkoutLog.Core.Tests.Import.Checks;

public class SaveRulesCheckTests
{
    private static readonly ImportContext Context = new(new Dictionary<DateTimeOffset, int>());
    private readonly SaveRulesCheck check = new(new WorkoutValidator(new ExerciseCatalog()));

    [Test]
    public void Check_ValidWorkout_Passes() =>
        Assert.That(check.Check(At(0, Bench, Run), Context), Is.Empty);

    [Test]
    public void Check_PassesTheDomainExercisesToTheValidator()
    {
        IReadOnlyList<WorkoutExercise>? validated = null;
        var validator = new Mock<IWorkoutValidator>(MockBehavior.Strict);
        validator.Setup(v => v.Validate(It.IsAny<IReadOnlyList<WorkoutExercise>>()))
            .Callback<IReadOnlyList<WorkoutExercise>>(e => validated = e)
            .Returns(new WorkoutErrors(null, []));

        new SaveRulesCheck(validator.Object).Check(At(0, Bench, Run), Context);

        Assert.That(validated, Is.EqualTo(new[] { Bench.ToDomain(), Run.ToDomain() }).UsingPropertiesComparer());
    }

    [Test]
    public void Check_NoExercises_FailsSaveRules() =>
        Assert.That(check.Check(At(0) with { Exercises = [] }, Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_StrengthWithNoSets_FailsSaveRules() =>
        Assert.That(check.Check(At(0, Bench with { Sets = [] }), Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_MissingRequiredMetric_FailsSaveRules() =>
        Assert.That(check.Check(At(0, Bench with { Sets = [new ImportSet(WeightKg: 60m)] }), Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_ValueOverTheLimit_FailsSaveRules() =>
        Assert.That(check.Check(At(0, Bench with { Sets = [new ImportSet(8, 100_000m)] }), Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_MetricTheExerciseDoesntShow_FailsSaveRules() =>
        Assert.That(check.Check(At(0, Bench with { Sets = [new ImportSet(8, Distance: 5m)] }), Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_SetsForACardioExercise_FailsSaveRules() =>
        Assert.That(check.Check(At(0, Run with { Sets = [new ImportSet(8)], Entry = null }), Context), Is.EqualTo(new[] { ImportSkipReason.FailsSaveRules }));

    [Test]
    public void Check_UnknownExercise_LeftToUnknownExerciseCheck() =>
        Assert.That(check.Check(At(0, Bench, Bench with { Exercise = new ImportExerciseInfo(999) }), Context), Is.Empty);
}
