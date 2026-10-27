using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using static WorkoutLog.Core.Tests.Import.ImportJson;
using static WorkoutLog.Core.Tests.Import.ImportWorkouts;

namespace WorkoutLog.Core.Tests.Import.Checks;

public class StartCheckTests
{
    private static readonly ImportContext Context = new(new Dictionary<DateTimeOffset, int>());

    [Test]
    public void Check_ValidStart_Passes() =>
        Assert.That(new StartCheck().Check(At(0), Context), Is.Empty);

    [TestCase(null)]
    [TestCase("2026-10-21T17:05:00Z")]
    public void Check_MissingOrInvalidStart_IsInvalidStart(string? startedAt) =>
        Assert.That(new StartCheck().Check(At(0) with { StartedAt = startedAt }, Context), Is.EqualTo(new[] { ImportSkipReason.InvalidStart }));
}
