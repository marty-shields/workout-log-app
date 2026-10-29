using System.Globalization;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Exercises.Bests;
using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Models.Exercises;

/// <summary>Builds, searches and filters the rows of the Exercises list and the add-exercise list.</summary>
public static class ExerciseListItems
{
    /// <summary>A row for the exercise, with its last session's date and best when it has one.</summary>
    /// <param name="exercise">The exercise.</param>
    /// <param name="logged">Its last session; null when not logged or not loaded.</param>
    /// <param name="zone">The phone's time zone.</param>
    /// <param name="now">The current time.</param>
    public static ExerciseListItem Item(ExerciseDefinition exercise, LoggedExercise? logged, TimeZoneInfo zone, DateTimeOffset now) =>
        new(exercise.Id, exercise.Name, ExerciseFields.Kind(exercise.Type), logged is null ? null : Last(exercise, logged, zone, now));

    /// <summary>
    /// The last session's date in <paramref name="zone"/>, with the year only when it isn't this year, then its best (D49):
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
    /// The rows for <paramref name="query"/> and <paramref name="filter"/>: with search words, ranked as <see cref="IExerciseCatalog.Search"/>;
    /// otherwise in the order given.
    /// </summary>
    public static IReadOnlyList<ExerciseListItem> Find(IReadOnlyList<ExerciseListItem> items, IExerciseCatalog catalog, string? query, ExerciseFilter filter)
    {
        IEnumerable<ExerciseListItem> found = items;
        if (catalog.HasSearchWords(query))
        {
            var byId = items.ToDictionary(i => i.Id);
            found = catalog.Search(query).Where(e => byId.ContainsKey(e.Id)).Select(e => byId[e.Id]);
        }

        return found.Where(i => filter switch
            {
                ExerciseFilter.Strength => i.Kind == ExerciseKind.Strength,
                ExerciseFilter.Cardio => i.Kind == ExerciseKind.Cardio,
                _ => true,
            })
            .ToList();
    }
}
