using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.ThisWeek;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Models.ThisWeek;

/// <summary>Builds the rows of the Records this week card.</summary>
public static class WeekRecordItems
{
    /// <summary>
    /// One row per exercise, in the order given: its name, its best as on the Exercises list, and the weekday of the session in <paramref name="zone"/>.
    /// An exercise that set several records shows its first in <paramref name="records"/>, the newest. A best the list can't show is left out.
    /// </summary>
    public static IReadOnlyList<WeekRecord> Items(IReadOnlyList<ExerciseRecord> records, IExerciseCatalog catalog, TimeZoneInfo zone)
    {
        var definitions = catalog.All.ToDictionary(e => e.Id);
        return records
            .DistinctBy(r => r.ExerciseId)
            .Select(r => (Record: r, Exercise: definitions[r.ExerciseId]))
            .Select(r => ExerciseListItems.Best(r.Exercise, r.Record.Best) is { } best
                ? new WeekRecord(r.Exercise.Id, r.Exercise.Name, best, TimeZoneInfo.ConvertTime(r.Record.StartedAt, zone).DayOfWeek)
                : null)
            .OfType<WeekRecord>()
            .ToList();
    }
}
