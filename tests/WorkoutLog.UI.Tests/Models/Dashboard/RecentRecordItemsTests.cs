using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Dashboard;
using WorkoutLog.UI.Models.Dashboard;

namespace WorkoutLog.UI.Tests.Models.Dashboard;

public class RecentRecordItemsTests
{
    private const int Squat = 6;
    private const int PushUp = 264;
    private const int Plank = 258;

    /// <summary>Fri 30 Oct 2026, 12:00 UTC; 13:00 in UTC+1.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);

    private static readonly TimeZoneInfo Zone = TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1");

    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    [Test]
    public void Items_KeepTheOrderWithNameBestAndDaysAgo()
    {
        var items = RecentRecordItems.Items(
        [
            new ExerciseRecord(Squat, Now.AddHours(-1), new ExerciseBest(70m, 3, null, null)),
            new ExerciseRecord(PushUp, Now.AddDays(-1), new ExerciseBest(null, 25, null, null)),
            new ExerciseRecord(Plank, Now.AddDays(-9), new ExerciseBest(null, null, TimeSpan.FromSeconds(105), null)),
        ], Catalog, Zone, Now);

        Assert.That(items, Is.EqualTo(new[]
        {
            new RecentRecord(Squat, "Barbell Back Squat", "70kg × 3", 0),
            new RecentRecord(PushUp, "Push-up", "25 reps", 1),
            new RecentRecord(Plank, "Plank", "1:45", 9),
        }));
    }

    [TestCase(23, 0, 0)]
    [TestCase(22, 59, 1)]
    public void Items_DaysAgo_AreCalendarDaysInTheZone(int utcHour, int utcMinute, int expected)
    {
        var started = new DateTimeOffset(2026, 10, 29, utcHour, utcMinute, 0, TimeSpan.Zero);

        var items = RecentRecordItems.Items([new ExerciseRecord(Squat, started, new ExerciseBest(70m, 3, null, null))], Catalog, Zone, Now);

        Assert.That(items.Single().DaysAgo, Is.EqualTo(expected));
    }

    [Test]
    public void Items_BestItCantShow_IsLeftOut() =>
        Assert.That(RecentRecordItems.Items([new ExerciseRecord(Squat, Now, new ExerciseBest(null, null, null, null))], Catalog, Zone, Now), Is.Empty);
}
