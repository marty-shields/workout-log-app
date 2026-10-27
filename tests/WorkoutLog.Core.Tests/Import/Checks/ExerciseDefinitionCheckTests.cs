using Moq;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using static WorkoutLog.Core.Tests.Import.ImportWorkouts;

namespace WorkoutLog.Core.Tests.Import.Checks;

public class ExerciseDefinitionCheckTests
{
    private static readonly ImportContext Context = new(new Dictionary<DateTimeOffset, int>());
    private readonly ExerciseDefinitionCheck check = new(new ExerciseCatalog());

    [Test]
    public void Check_TypeAndUnitsAsInTheList_Passes() =>
        Assert.That(check.Check(At(0, Bench, Run), Context), Is.Empty);

    [Test]
    public void Check_DifferentName_Passes() =>
        Assert.That(check.Check(At(0, Bench with { Exercise = Bench.Exercise with { Name = "Bench" } }, Bench with { Exercise = Bench.Exercise with { Name = null } }), Context),
            Is.Empty);

    [Test]
    public void Check_UnknownId_LeftToUnknownExerciseCheck() =>
        Assert.That(check.Check(At(0, Bench with { Exercise = new ImportExerciseInfo(999, Type: ExerciseType.Cardio) }), Context), Is.Empty);

    [TestCase("Type missing")]
    [TestCase("Type different")]
    [TestCase("Distance unit missing")]
    [TestCase("Distance unit different")]
    [TestCase("Rate unit extra")]
    [TestCase("Pace unit missing")]
    [TestCase("Distance unit extra on strength")]
    public void Check_TypeOrUnitDiffers_IsExerciseMismatch(string change)
    {
        var info = change switch
        {
            "Type missing" => Run.Exercise with { Type = null },
            "Type different" => Run.Exercise with { Type = ExerciseType.Strength },
            "Distance unit missing" => Run.Exercise with { DistanceUnit = null },
            "Distance unit different" => Run.Exercise with { DistanceUnit = DistanceUnit.Metres },
            "Rate unit extra" => Run.Exercise with { RateUnit = RateUnit.KmPerHour },
            "Pace unit missing" => Run.Exercise with { PaceUnit = null },
            _ => Bench.Exercise with { DistanceUnit = DistanceUnit.Km },
        };

        Assert.That(check.Check(At(0, Bench, Run with { Exercise = info }), Context), Is.EqualTo(new[] { ImportSkipReason.ExerciseMismatch }));
    }
}
