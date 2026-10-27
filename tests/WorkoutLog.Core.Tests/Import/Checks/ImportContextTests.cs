using WorkoutLog.Core.Import;
using WorkoutLog.Core.Import.Checks;
using static WorkoutLog.Core.Tests.Import.ImportJson;

namespace WorkoutLog.Core.Tests.Import.Checks;

public class ImportContextTests
{
    [Test]
    public void For_CountsEachValidStartIncludingMalformedWorkoutsAndSkipsInvalidOnes()
    {
        var context = ImportContext.For(
        [
            new ImportWorkout(1, Time(Start), null, []),
            new ImportWorkout(2, Time(Start), null, null),
            new ImportWorkout(3, Time(Start.AddHours(1)), null, []),
            new ImportWorkout(4, "invalid", null, []),
            new ImportWorkout(5, null, null, null),
        ]);

        Assert.That(context.StartCounts, Is.EqualTo(new Dictionary<DateTimeOffset, int> { [Start] = 2, [Start.AddHours(1)] = 1 }));
    }
}
