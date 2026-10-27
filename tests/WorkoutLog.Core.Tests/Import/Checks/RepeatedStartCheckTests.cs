using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using static WorkoutLog.Core.Tests.Import.ImportJson;
using static WorkoutLog.Core.Tests.Import.ImportWorkouts;

namespace WorkoutLog.Core.Tests.Import.Checks;

public class RepeatedStartCheckTests
{
    [Test]
    public void Check_StartOnceInTheFile_Passes() =>
        Assert.That(new RepeatedStartCheck().Check(At(0), Counts(1)), Is.Empty);

    [Test]
    public void Check_StartMoreThanOnceInTheFile_IsRepeatedStart() =>
        Assert.That(new RepeatedStartCheck().Check(At(0), Counts(2)), Is.EqualTo(new[] { ImportSkipReason.RepeatedStart }));

    [Test]
    public void Check_InvalidStart_LeftToStartCheck() =>
        Assert.That(new RepeatedStartCheck().Check(At(0) with { StartedAt = null }, Counts(2)), Is.Empty);

    private static ImportContext Counts(int count) => new(new Dictionary<DateTimeOffset, int> { [Start] = count });
}
