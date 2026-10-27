using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using static WorkoutLog.Core.Tests.Import.ImportJson;
using static WorkoutLog.Core.Tests.Import.ImportWorkouts;

namespace WorkoutLog.Core.Tests.Import.Checks;

public class FinishCheckTests
{
    private static readonly ImportContext Context = new(new Dictionary<DateTimeOffset, int>());

    [Test]
    public void Check_FinishAfterStart_Passes() =>
        Assert.That(new FinishCheck().Check(At(0), Context), Is.Empty);

    [Test]
    public void Check_FinishAtStart_Passes() =>
        Assert.That(new FinishCheck().Check(At(0) with { FinishedAt = Time(Start) }, Context), Is.Empty);

    [Test]
    public void Check_FinishATickBeforeStart_IsFinishBeforeStart() =>
        Assert.That(new FinishCheck().Check(At(0) with { FinishedAt = Time(Start.AddTicks(-1)) }, Context),
            Is.EqualTo(new[] { ImportSkipReason.FinishBeforeStart }));

    [TestCase(null)]
    [TestCase("tomorrow")]
    public void Check_MissingOrInvalidFinish_IsInvalidFinish(string? finishedAt) =>
        Assert.That(new FinishCheck().Check(At(0) with { FinishedAt = finishedAt }, Context), Is.EqualTo(new[] { ImportSkipReason.InvalidFinish }));

    [Test]
    public void Check_InvalidStart_ChecksOnlyTheFinish()
    {
        Assert.That(new FinishCheck().Check(At(0) with { StartedAt = null }, Context), Is.Empty);
        Assert.That(new FinishCheck().Check(At(0) with { StartedAt = null, FinishedAt = null }, Context),
            Is.EqualTo(new[] { ImportSkipReason.InvalidFinish }));
    }
}
