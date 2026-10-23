using System.Globalization;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.UI.Models.Dashboard;
using WorkoutLog.UI.Models.Fields;
using WorkoutLog.UI.Models.History;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>A saved workout in the prototype.</summary>
/// <param name="Id">Workout id.</param>
/// <param name="StartedAt">Start time.</param>
/// <param name="FinishedAt">Finish time.</param>
/// <param name="Exercises">Exercises and their sets or entries.</param>
public sealed record SavedWorkout(int Id, DateTime StartedAt, DateTime FinishedAt, IReadOnlyList<ExerciseState> Exercises);

/// <summary>Sample data for stories, plus simplified stand-ins for Core's save rules and counts. Exercises come from the real catalog.</summary>
public static class SampleData
{
    /// <summary>Start time used by the static stories: Tue 20 Oct 2026 18:05.</summary>
    public static readonly DateTime Started = new(2026, 10, 20, 18, 5, 0);

    /// <summary>Picker results from the catalog's search.</summary>
    public static IReadOnlyList<PickerItem> PickerItems(IExerciseCatalog catalog, string? query) =>
        catalog.Search(query).Select(ExerciseFields.PickerItem).ToList();

    /// <summary>A newly added exercise: one empty set (strength) or its empty entry (cardio).</summary>
    public static ExerciseState NewExercise(IExerciseCatalog catalog, int id) => ExerciseFields.New(Find(catalog, id));

    /// <summary>An exercise with one set or entry per array; values follow the exercise's field order.</summary>
    public static ExerciseState Exercise(IExerciseCatalog catalog, int id, params string?[][] entries)
    {
        var definition = Find(catalog, id);
        var fields = ExerciseFields.For(definition);
        var exercise = new ExerciseState(definition.Id, definition.Name, ExerciseFields.Kind(definition.Type), fields);
        foreach (var values in entries)
        {
            var entry = new EntryState();
            for (var i = 0; i < fields.Count; i++)
            {
                entry.Values[fields[i].Key] = i < values.Length ? values[i] : null;
            }

            exercise.Entries.Add(entry);
        }

        return exercise;
    }

    /// <summary>Strength exercises with filled sets, including a hold and an assisted machine.</summary>
    public static List<ExerciseState> StrengthWorkout(IExerciseCatalog catalog) =>
    [
        Exercise(catalog, 6, ["8", "60"], ["8", "60"], ["6", "62.5"]),
        Exercise(catalog, 218, ["10", "25"], ["8", "25"]),
        Exercise(catalog, 258, ["0:01:00", null]),
        Exercise(catalog, 187, ["40", "32", null]),
    ];

    /// <summary>A squat with two sets, and a carry with three sets: two filled, the last one partly filled.</summary>
    public static List<ExerciseState> CarryWorkout(IExerciseCatalog catalog) =>
    [
        Exercise(catalog, 6, ["8", "60"], ["8", "60"]),
        Exercise(catalog, 187, ["40", "32", "0:00:45"], ["40", "32", "0:00:50"], ["30", "32", null]),
    ];

    /// <summary>A saved workout after a failed save: a set missing reps, a carry set missing distance.</summary>
    public static List<ExerciseState> EditWorkoutWithErrors(IExerciseCatalog catalog)
    {
        List<ExerciseState> exercises =
        [
            Exercise(catalog, 6, ["8", "60"], [null, "60"]),
            Exercise(catalog, 187, ["40", "32", "0:00:45"], [null, "32", "0:00:40"]),
        ];
        Validate(exercises, out _);
        return exercises;
    }

    /// <summary>Cardio exercises with filled entries, including pace.</summary>
    public static List<ExerciseState> CardioWorkout(IExerciseCatalog catalog) =>
    [
        Exercise(catalog, 300, ["0:20:00", "3.2", "1.5", "210", "9.6"]),
        Exercise(catalog, 309, ["0:15:00", "60", "8", "150", null]),
        Exercise(catalog, 311, ["0:25:30", "5", null, "5:06"]),
    ];

    /// <summary>A workout after a failed save: a set missing reps, an exercise with no sets, a cardio entry missing duration.</summary>
    public static List<ExerciseState> WorkoutWithErrors(IExerciseCatalog catalog)
    {
        List<ExerciseState> exercises =
        [
            Exercise(catalog, 222, ["10", "50"], [null, "50"]),
            Exercise(catalog, 132),
            Exercise(catalog, 305, [null, "2000", "5", null, null]),
        ];
        Validate(exercises, out _);
        return exercises;
    }

    /// <summary>Saved workouts relative to <paramref name="now"/>, for the prototype.</summary>
    public static List<SavedWorkout> SavedWorkouts(IExerciseCatalog catalog, DateTime now)
    {
        var today = now.Date;
        return
        [
            new(1, today.AddDays(-9).AddHours(17.75), today.AddDays(-9).AddHours(18.9), StrengthWorkout(catalog)),
            new(2, today.AddDays(-3).AddHours(7.5), today.AddDays(-3).AddHours(8.3), CardioWorkout(catalog)),
            new(3, today.AddDays(-1).AddHours(18.1), today.AddDays(-1).AddHours(19.2), StrengthWorkout(catalog)),
        ];
    }

    /// <summary>History rows for the static stories, newest first.</summary>
    public static IReadOnlyList<HistoryRow> HistoryRows { get; } =
    [
        new(4, new DateTime(2026, 10, 20, 18, 5, 0)),
        new(3, new DateTime(2026, 10, 20, 7, 30, 0)),
        new(2, new DateTime(2026, 10, 19, 17, 45, 0)),
        new(1, new DateTime(2026, 10, 16, 18, 10, 0)),
    ];

    /// <summary>Dashboard values: latest start, and counts from Monday 00:00 and from the 1st of the month.</summary>
    public static DashboardSummary Summary(IReadOnlyCollection<SavedWorkout> saved, DateTime now)
    {
        var weekStart = now.Date.AddDays(-(((int)now.DayOfWeek + 6) % 7));
        var monthStart = new DateTime(now.Year, now.Month, 1);
        return new DashboardSummary(
            saved.Count == 0 ? null : saved.Max(w => w.StartedAt),
            saved.Count(w => w.StartedAt >= weekStart),
            saved.Count(w => w.StartedAt >= monthStart));
    }

    /// <summary>FR-09 save rules and the value rules in docs/03. Sets <see cref="ExerciseState.Error"/> and <see cref="EntryState.Errors"/>; returns false if anything fails.</summary>
    public static bool Validate(IReadOnlyList<ExerciseState> exercises, out string? workoutError)
    {
        workoutError = exercises.Count == 0 ? "Add at least one exercise" : null;
        var valid = workoutError is null;
        foreach (var exercise in exercises)
        {
            exercise.Error = exercise.Entries.Count == 0 ? "Add at least one set" : null;
            valid &= exercise.Error is null;
            foreach (var entry in exercise.Entries)
            {
                valid &= ValidateSet(exercise, entry);
            }
        }

        return valid;
    }

    /// <summary>Save rules for one set or entry. Sets <see cref="EntryState.Errors"/>; returns false if any field fails.</summary>
    public static bool ValidateSet(ExerciseState exercise, EntryState entry)
    {
        entry.Errors.Clear();
        foreach (var field in exercise.Fields)
        {
            if (FieldError(field, entry.Values.GetValueOrDefault(field.Key)) is { } message)
            {
                entry.Errors[field.Key] = message;
            }
        }

        return entry.Errors.Count == 0;
    }

    /// <summary>Deep copy, so edits can be discarded.</summary>
    public static List<ExerciseState> Clone(IEnumerable<ExerciseState> exercises) =>
        exercises.Select(e =>
        {
            var copy = new ExerciseState(e.ExerciseId, e.Name, e.Kind, e.Fields);
            foreach (var entry in e.Entries)
            {
                var entryCopy = new EntryState();
                foreach (var (key, value) in entry.Values)
                {
                    entryCopy.Values[key] = value;
                }

                copy.Entries.Add(entryCopy);
            }

            return copy;
        }).ToList();

    private static ExerciseDefinition Find(IExerciseCatalog catalog, int id) => catalog.All.Single(e => e.Id == id);

    private static bool IsFilled(string? value) => value is not null && value.Any(char.IsDigit);

    private static string? FieldError(FieldSpec field, string? value)
    {
        if (!IsFilled(value))
        {
            return field.Required ? $"Enter {field.Label.ToLowerInvariant()}" : null;
        }

        if (field.Kind is FieldKind.Duration or FieldKind.Pace)
        {
            return TimeError(field, value!);
        }

        if (!decimal.TryParse(value, out var number))
        {
            return $"{field.Label} must be a number";
        }

        if (field.Kind == FieldKind.Whole && number != decimal.Truncate(number))
        {
            return $"{field.Label} must be a whole number";
        }

        // Reps and required distance more than 0; other numbers 0 or more; none above the limit in Core's MetricLimits.
        if ((field.Key == "reps" || (field.Key == "distance" && field.Required)) && number <= 0)
        {
            return $"{field.Label} must be more than 0";
        }

        if (number < 0)
        {
            return $"{field.Label} must be 0 or more";
        }

        var max = Max(field);
        return number > max ? $"{field.Label} must be {max.ToString("#,0", CultureInfo.InvariantCulture)} or less" : null;
    }

    private static decimal Max(FieldSpec field) => (field.Key, field.Unit) switch
    {
        ("distance", "km") => 1_000,
        ("distance", _) => 100_000,
        ("level", _) or ("incline", _) => 100,
        ("calories", _) => 10_000,
        ("rate", "km/h") => 100,
        ("rate", "strokes/min") => 200,
        ("rate", "feet/min") => 1_000,
        ("rate", _) => 300,
        _ => 1_000,
    };

    // Duration (h:mm:ss): minutes and seconds 00–59, at most 23:59:59, more than 0 when required. Pace (m:ss): seconds 00–59, at most 60:00.
    private static string? TimeError(FieldSpec field, string value)
    {
        var parts = value.Split(':').Select(p => int.TryParse(p, out var n) ? n : 0).ToArray();
        var pace = field.Kind == FieldKind.Pace;
        if (parts.Skip(1).Any(p => p is < 0 or > 59))
        {
            return pace ? $"{field.Label} seconds must be 00–59" : $"{field.Label} minutes and seconds must be 00–59";
        }

        if (parts[0] < 0)
        {
            return $"{field.Label} must be 0 or more";
        }

        var seconds = parts.Aggregate(0L, (total, part) => total * 60 + part);
        if (seconds > (pace ? 3_600 : 86_399))
        {
            return $"{field.Label} must be {(pace ? "60:00" : "23:59:59")} or less";
        }

        return field.Required && seconds == 0 ? $"{field.Label} must be more than 0" : null;
    }
}
