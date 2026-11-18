using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Periods;

namespace WorkoutLog.Core.Tests.Domain.Workouts.Periods;

internal static class PeriodTestData
{
    public static WorkoutTally Workout(string utc, int minutes = 45, int sets = 3)
    {
        var start = DateTimeOffset.Parse(utc + "Z", System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime();
        return new WorkoutTally(start, start.AddMinutes(minutes), sets);
    }

    public static DateTimeOffset At(string utc) => DateTimeOffset.Parse(utc + "Z", System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime();

    public static Period Month(int year, int month) => Period.Containing(PeriodKind.Month, new DateOnly(year, month, 1));

    public static Period Week(int year, int month, int day) => Period.Containing(PeriodKind.Week, new DateOnly(year, month, day));

    public static Period Year(int year) => Period.Containing(PeriodKind.Year, new DateOnly(year, 1, 1));
}
