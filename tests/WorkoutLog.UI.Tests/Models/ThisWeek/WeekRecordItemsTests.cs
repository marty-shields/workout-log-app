using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.ThisWeek;
using WorkoutLog.UI.Models.ThisWeek;

namespace WorkoutLog.UI.Tests.Models.ThisWeek;

public class WeekRecordItemsTests
{
    private const int Squat = 6;
    private const int PushUp = 264;
    private const int Plank = 258;

    /// <summary>Wed 18 Nov 2026, 12:00 UTC.</summary>
    private static readonly DateTimeOffset Now = new(2026, 11, 18, 12, 0, 0, TimeSpan.Zero);

    private static readonly TimeZoneInfo Zone = TimeZoneInfo.CreateCustomTimeZone("UTC+1", TimeSpan.FromHours(1), "UTC+1", "UTC+1");

    private static readonly IExerciseCatalog Catalog = new ExerciseCatalog();

    [Test]
    public void Items_KeepTheOrderWithNameBestAndWeekday()
    {
        var items = WeekRecordItems.Items(
        [
            new ExerciseRecord(Squat, Now.AddHours(-1), new ExerciseBest(70m, 3, null, null)),
            new ExerciseRecord(PushUp, Now.AddDays(-1), new ExerciseBest(null, 25, null, null)),
            new ExerciseRecord(Plank, Now.AddDays(-2), new ExerciseBest(null, null, TimeSpan.FromSeconds(105), null)),
        ], Catalog, Zone);

        Assert.That(items, Is.EqualTo(new[]
        {
            new WeekRecord(Squat, "Barbell Back Squat", "70kg × 3", DayOfWeek.Wednesday),
            new WeekRecord(PushUp, "Push-up", "25 reps", DayOfWeek.Tuesday),
            new WeekRecord(Plank, "Plank", "1:45", DayOfWeek.Monday),
        }));
    }

    [TestCase(23, 0, DayOfWeek.Wednesday)]
    [TestCase(22, 59, DayOfWeek.Tuesday)]
    [TestCase(23, 59, DayOfWeek.Wednesday)]
    public void Items_Weekday_IsTheDayInTheZone(int utcHour, int utcMinute, DayOfWeek expected)
    {
        var started = new DateTimeOffset(2026, 11, 17, utcHour, utcMinute, 0, TimeSpan.Zero);

        var items = WeekRecordItems.Items([new ExerciseRecord(Squat, started, new ExerciseBest(70m, 3, null, null))], Catalog, Zone);

        Assert.That(items.Single().Day, Is.EqualTo(expected));
    }

    [Test]
    public void Items_AnExerciseWithSeveralRecords_ShowsOnlyTheNewest()
    {
        var items = WeekRecordItems.Items(
        [
            new ExerciseRecord(Squat, Now, new ExerciseBest(80m, 3, null, null)),
            new ExerciseRecord(Squat, Now.AddDays(-1), new ExerciseBest(70m, 3, null, null)),
        ], Catalog, Zone);

        Assert.That(items, Is.EqualTo(new[] { new WeekRecord(Squat, "Barbell Back Squat", "80kg × 3", DayOfWeek.Wednesday) }));
    }

    [Test]
    public void Items_BestItCantShow_IsLeftOut() =>
        Assert.That(WeekRecordItems.Items([new ExerciseRecord(Squat, Now, new ExerciseBest(null, null, null, null))], Catalog, Zone), Is.Empty);

    [Test]
    public void Items_NewestRecordItCantShow_FallsBackToTheNextOneOfTheExercise()
    {
        var items = WeekRecordItems.Items(
        [
            new ExerciseRecord(Squat, Now, new ExerciseBest(null, null, null, null)),
            new ExerciseRecord(Squat, Now.AddDays(-1), new ExerciseBest(70m, 3, null, null)),
        ], Catalog, Zone);

        Assert.That(items, Is.EqualTo(new[] { new WeekRecord(Squat, "Barbell Back Squat", "70kg × 3", DayOfWeek.Tuesday) }));
    }
}
