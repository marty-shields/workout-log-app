using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.Dashboard;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Models.Dashboard;

/// <summary>Builds the rows of the Recent records card.</summary>
public static class RecentRecordItems
{
    /// <summary>
    /// The rows, in the order given: each exercise's name, its best as on the Exercises list, and the calendar days from the session to today
    /// in <paramref name="zone"/>. A best the list can't show is left out.
    /// </summary>
    public static IReadOnlyList<RecentRecord> Items(IReadOnlyList<ExerciseRecord> records, IExerciseCatalog catalog, TimeZoneInfo zone, DateTimeOffset now)
    {
        var definitions = catalog.All.ToDictionary(e => e.Id);
        var today = LocalDate(now, zone);
        return records
            .Select(r => (Record: r, Exercise: definitions[r.ExerciseId]))
            .Select(r => ExerciseListItems.Best(r.Exercise, r.Record.Best) is { } best
                ? new RecentRecord(r.Exercise.Id, r.Exercise.Name, best, today.DayNumber - LocalDate(r.Record.StartedAt, zone).DayNumber)
                : null)
            .OfType<RecentRecord>()
            .ToList();
    }

    private static DateOnly LocalDate(DateTimeOffset value, TimeZoneInfo zone) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(value, zone).DateTime);
}
