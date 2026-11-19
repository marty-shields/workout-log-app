using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Components.ThisWeek;
using WorkoutLog.UI.Models.Exercises;

namespace WorkoutLog.UI.Models.ThisWeek;

/// <summary>Builds the rows of the Records this week card.</summary>
public static class WeekRecordItems
{
    /// <summary>
    /// One row per exercise, in the order given: its name, its best as on the add-exercise list, and the weekday of the session in <paramref name="zone"/>.
    /// With <paramref name="withDates"/> it also gives that day's date, e.g. Wed 18 Nov, for a period longer than a week.
    /// An exercise that set several records shows its first in <paramref name="records"/>, the newest. A record whose best the add-exercise list can't show is left out, so the exercise shows its newest record that it can.
    /// </summary>
    public static IReadOnlyList<WeekRecord> Items(IReadOnlyList<ExerciseRecord> records, IExerciseCatalog catalog, TimeZoneInfo zone, bool withDates = false)
    {
        var definitions = catalog.All.ToDictionary(e => e.Id);
        return records
            .Select(r => (Record: r, Exercise: definitions[r.ExerciseId]))
            .Select(r => (r.Record, r.Exercise, Local: TimeZoneInfo.ConvertTime(r.Record.StartedAt, zone)))
            .Select(r => ExerciseListItems.Best(r.Exercise, r.Record.Best) is { } best
                ? new WeekRecord(r.Exercise.Id, r.Exercise.Name, best, r.Local.DayOfWeek, withDates ? DisplayFormat.Day(DateOnly.FromDateTime(r.Local.DateTime)) : null)
                : null)
            .OfType<WeekRecord>()
            .DistinctBy(r => r.ExerciseId)
            .ToList();
    }
}
