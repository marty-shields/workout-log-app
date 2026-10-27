using Moq;
using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using static WorkoutLog.Core.Tests.Import.ImportWorkouts;

namespace WorkoutLog.Core.Tests.Import.Checks;

public class ImportCheckerTests
{
    private static readonly ImportContext Context = new(new Dictionary<DateTimeOffset, int>());

    [Test]
    public void Check_EveryCheckPasses_ReturnsNothing()
    {
        var workout = At(0);
        var checker = new ImportChecker([Returning(workout), Returning(workout)]);

        Assert.That(checker.Check(workout, Context), Is.Empty);
    }

    [Test]
    public void Check_RunsEveryCheckAndReturnsEachReasonOnceInReasonOrder()
    {
        var workout = At(0);
        var checker = new ImportChecker(
        [
            Returning(workout, ImportSkipReason.FailsSaveRules, ImportSkipReason.InvalidStart),
            Returning(workout),
            Returning(workout, ImportSkipReason.InvalidStart, ImportSkipReason.UnknownExercise),
        ]);

        Assert.That(checker.Check(workout, Context),
            Is.EqualTo(new[] { ImportSkipReason.InvalidStart, ImportSkipReason.UnknownExercise, ImportSkipReason.FailsSaveRules }));
    }

    [Test]
    public void Check_Malformed_ReturnsMalformedAloneWithoutRunningTheChecks()
    {
        var check = new Mock<IImportCheck>(MockBehavior.Strict);

        Assert.That(new ImportChecker([check.Object]).Check(new ImportWorkout(1, null, null, null), Context),
            Is.EqualTo(new[] { ImportSkipReason.Malformed }));
    }

    private static IImportCheck Returning(ImportWorkout workout, params ImportSkipReason[] reasons)
    {
        var check = new Mock<IImportCheck>(MockBehavior.Strict);
        check.Setup(c => c.Check(workout, Context)).Returns(reasons);
        return check.Object;
    }
}
