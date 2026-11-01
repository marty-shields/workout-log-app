using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using static WorkoutLog.Core.Tests.Import.ImportJson;
using static WorkoutLog.Core.Tests.Import.ImportWorkouts;

namespace WorkoutLog.Core.Tests.Import.Checks;

public class FutureCheckTests
{
    private static readonly DateTimeOffset Finish = Start.AddHours(1);

    private static IEnumerable<ImportSkipReason> CheckAt(DateTimeOffset now, ImportWorkout workout) =>
        new FutureCheck().Check(workout, new ImportContext(new Dictionary<DateTimeOffset, int>(), now));

    [Test]
    public void Check_BothInThePast_Passes() =>
        Assert.That(CheckAt(Finish.AddDays(1), At(0)), Is.Empty);

    [Test]
    public void Check_StartEqualToNow_Passes() =>
        Assert.That(CheckAt(Start, At(0) with { FinishedAt = Time(Start) }), Is.Empty);

    [Test]
    public void Check_FinishEqualToNow_Passes() =>
        Assert.That(CheckAt(Finish, At(0)), Is.Empty);

    [Test]
    public void Check_StartATickAfterNow_IsInFuture() =>
        Assert.That(CheckAt(Start.AddTicks(-1), At(0) with { FinishedAt = Time(Start.AddTicks(-1)) }),
            Is.EqualTo(new[] { ImportSkipReason.InFuture }));

    [Test]
    public void Check_FinishAfterNow_IsInFuture() =>
        Assert.That(CheckAt(Finish.AddTicks(-1), At(0)), Is.EqualTo(new[] { ImportSkipReason.InFuture }));

    [Test]
    public void Check_StartAndFinishAfterNow_IsReportedOnce() =>
        Assert.That(CheckAt(Start.AddDays(-1), At(0)), Is.EqualTo(new[] { ImportSkipReason.InFuture }));

    [Test]
    public void Check_InvalidStartOrFinish_ChecksTheOtherOnly()
    {
        Assert.That(CheckAt(Start.AddDays(1), At(0) with { StartedAt = null }), Is.Empty);
        Assert.That(CheckAt(Start.AddDays(1), At(0) with { FinishedAt = "tomorrow" }), Is.Empty);
        Assert.That(CheckAt(Start.AddDays(-1), At(0) with { StartedAt = null }), Is.EqualTo(new[] { ImportSkipReason.InFuture }));
    }
}
