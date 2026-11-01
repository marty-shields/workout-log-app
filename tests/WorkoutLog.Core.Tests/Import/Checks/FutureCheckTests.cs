using Microsoft.Extensions.Time.Testing;
using WorkoutLog.Core.Import.Checks;
using static WorkoutLog.Core.Tests.Import.ImportJson;
using static WorkoutLog.Core.Tests.Import.ImportWorkouts;

namespace WorkoutLog.Core.Tests.Import.Checks;

public class FutureCheckTests
{
    private static readonly ImportContext Context = new(new Dictionary<DateTimeOffset, int>());
    private static readonly DateTimeOffset Finish = Start.AddHours(1);

    private static FutureCheck CheckAt(DateTimeOffset now) => new(new FakeTimeProvider(now));

    [Test]
    public void Check_BothInThePast_Passes() =>
        Assert.That(CheckAt(Finish.AddDays(1)).Check(At(0), Context), Is.Empty);

    [Test]
    public void Check_StartEqualToNow_Passes() =>
        Assert.That(CheckAt(Start).Check(At(0) with { FinishedAt = Time(Start) }, Context), Is.Empty);

    [Test]
    public void Check_FinishEqualToNow_Passes() =>
        Assert.That(CheckAt(Finish).Check(At(0), Context), Is.Empty);

    [Test]
    public void Check_StartATickAfterNow_IsInFuture() =>
        Assert.That(CheckAt(Start.AddTicks(-1)).Check(At(0) with { FinishedAt = Time(Start.AddTicks(-1)) }, Context),
            Is.EqualTo(new[] { ImportSkipReason.InFuture }));

    [Test]
    public void Check_FinishAfterNow_IsInFuture() =>
        Assert.That(CheckAt(Finish.AddTicks(-1)).Check(At(0), Context), Is.EqualTo(new[] { ImportSkipReason.InFuture }));

    [Test]
    public void Check_StartAndFinishAfterNow_IsReportedOnce() =>
        Assert.That(CheckAt(Start.AddDays(-1)).Check(At(0), Context), Is.EqualTo(new[] { ImportSkipReason.InFuture }));

    [Test]
    public void Check_InvalidStartOrFinish_ChecksTheOtherOnly()
    {
        var check = CheckAt(Start.AddDays(1));
        Assert.That(check.Check(At(0) with { StartedAt = null }, Context), Is.Empty);
        Assert.That(check.Check(At(0) with { FinishedAt = "tomorrow" }, Context), Is.Empty);
        Assert.That(CheckAt(Start.AddDays(-1)).Check(At(0) with { StartedAt = null }, Context), Is.EqualTo(new[] { ImportSkipReason.InFuture }));
    }
}
