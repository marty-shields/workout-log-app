using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>Builds, searches and filters the rows of the add-exercise list.</summary>
public static class ExerciseListItems
{
    /// <summary>A row for the exercise, with its last session's date and best when it has one.</summary>
    /// <param name="logged">Its last session; null when not logged or not loaded.</param>
    /// <param name="zone">The phone's time zone.</param>
    /// <param name="now">The current time.</param>
    public static ExerciseListItem Item(ExerciseDefinition exercise, LoggedExercise? logged, TimeZoneInfo zone, DateTimeOffset now) =>
        new(exercise.Id, exercise.Name, ExerciseFields.Kind(exercise.Type), logged is null ? null : Last(exercise, logged, zone, now), logged?.LastStartedAt);

    /// <summary>
    /// The last session's date in <paramref name="zone"/>, with the year only when it isn't this year, then its best:
    /// e.g. Tue 20 Oct · 70kg × 3, or the date only when there is no best or it can't be shown.
    /// </summary>
    public static string Last(ExerciseDefinition exercise, LoggedExercise logged, TimeZoneInfo zone, DateTimeOffset now)
    {
        var date = DisplayFormat.ShortDate(TimeZoneInfo.ConvertTime(logged.LastStartedAt, zone).DateTime, TimeZoneInfo.ConvertTime(now, zone).DateTime);
        return logged.Best is { } best && Best(exercise, best) is { } text ? $"{date} · {text}" : date;
    }

    /// <summary>The best as text: 70kg × 3, 25 reps, 32kg · 40m, 1:45 or 5.2km; null for a shape it can't show.</summary>
    public static string? Best(ExerciseDefinition exercise, ExerciseBest best) => best switch
    {
        { WeightKg: { } kg, Reps: { } reps } => $"{DisplayFormat.Weight(kg)} × {reps}",
        { Reps: { } reps } => $"{reps} reps",
        { WeightKg: { } kg, Distance: { } distance } when exercise.DistanceUnit is not null => $"{DisplayFormat.Weight(kg)} · {ExerciseFields.Distance(exercise, distance)}",
        { Duration: { } duration } => DisplayFormat.Clock(duration),
        { Distance: { } distance } when exercise.DistanceUnit is not null => ExerciseFields.Distance(exercise, distance),
        _ => null,
    };

    /// <summary>
    /// The rows for <paramref name="query"/> and <paramref name="filter"/>, in two groups: exercises with a saved workout, then the rest.
    /// No search words: the first group most recent first, then A–Z, the second in the order given. With search words: both by text rank as
    /// <see cref="IExerciseCatalog.Search"/>, the first also most recent first within a rank. Search ties keep the catalog's order.
    /// Grouped only when some item in <paramref name="items"/> was done, whatever the filter shows.
    /// </summary>
    public static ExerciseListGroups Find(IReadOnlyList<ExerciseListItem> items, IExerciseCatalog catalog, string? query, AddExerciseFilter filter)
    {
        IEnumerable<(ExerciseListItem Item, int Rank)> found = items.Select(i => (i, 0));
        var searching = catalog.HasSearchWords(query);
        if (searching)
        {
            var byId = items.ToDictionary(i => i.Id);
            found = catalog.SearchRanked(query).Where(r => byId.ContainsKey(r.Exercise.Id)).Select(r => (byId[r.Exercise.Id], r.Rank));
        }

        var shown = found.Where(r => filter switch
            {
                AddExerciseFilter.Strength => r.Item.Kind == ExerciseKind.Strength,
                AddExerciseFilter.Cardio => r.Item.Kind == ExerciseKind.Cardio,
                _ => true,
            })
            .ToList();
        var recent = shown.Where(r => r.Item.LastStartedAt is not null)
            .OrderBy(r => r.Rank)
            .ThenByDescending(r => r.Item.LastStartedAt)
            .ThenBy(r => searching ? "" : r.Item.Name, StringComparer.OrdinalIgnoreCase)
            .Select(r => r.Item)
            .ToList();
        var other = shown.Where(r => r.Item.LastStartedAt is null).Select(r => r.Item).ToList();
        return new ExerciseListGroups(recent, other, items.Any(i => i.LastStartedAt is not null));
    }
}
