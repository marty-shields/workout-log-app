using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Models.Dashboard;
using WorkoutLog.UI.Models.History;
using WorkoutLog.UI.Models.Workout;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>A saved workout in the prototype.</summary>
/// <param name="Id">Workout id.</param>
/// <param name="StartedAt">Start time.</param>
/// <param name="FinishedAt">Finish time.</param>
/// <param name="Exercises">Exercises and their sets or entries.</param>
public sealed record SavedWorkout(int Id, DateTime StartedAt, DateTime FinishedAt, IReadOnlyList<ExerciseState> Exercises);

/// <summary>Sample data for stories, plus stand-ins for the counts. Exercises come from the real catalog.</summary>
public static class SampleData
{
    /// <summary>Start time used by the static stories: Tue 20 Oct 2026 18:05.</summary>
    public static readonly DateTime Started = new(2026, 10, 20, 18, 5, 0);

    /// <summary>A saved workout as Core returns it, started at <see cref="Started"/> UTC and 1:04:37 long: squat, assisted pull-up, plank and a carry with two sets.</summary>
    public static readonly Workout SavedStrengthWorkout = new(
        1,
        new DateTimeOffset(Started, TimeSpan.Zero),
        new DateTimeOffset(Started, TimeSpan.Zero) + new TimeSpan(1, 4, 37),
        [
            new StrengthExercise(6, [new StrengthSet(8, 60m, null, null), new StrengthSet(8, 60m, null, null), new StrengthSet(6, 62.5m, null, null)]),
            new StrengthExercise(218, [new StrengthSet(10, 25m, null, null), new StrengthSet(8, 25m, null, null)]),
            new StrengthExercise(258, [new StrengthSet(null, null, TimeSpan.FromMinutes(1), null)]),
            new StrengthExercise(187, [new StrengthSet(null, 32m, TimeSpan.FromSeconds(45), 40m), new StrengthSet(null, 32m, TimeSpan.FromSeconds(50), 40m)]),
        ]);

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

    /// <summary>These exercises just after Dumbbell Curl is added: its card is on top, with one empty set.</summary>
    public static List<ExerciseState> NewestFirst(IExerciseCatalog catalog, List<ExerciseState> exercises) => [NewExercise(catalog, 132), .. exercises];

    /// <summary>A saved workout's exercises, given oldest first, in the order Edit Workout shows them: newest first.</summary>
    public static List<ExerciseState> Opened(List<ExerciseState> saved) => [.. saved.AsEnumerable().Reverse()];

    /// <summary>A squat with two sets, and a carry with three sets: two filled, the last one partly filled.</summary>
    public static List<ExerciseState> CarryWorkout(IExerciseCatalog catalog) =>
    [
        Exercise(catalog, 6, ["8", "60"], ["8", "60"]),
        Exercise(catalog, 187, ["40", "32", "0:00:45"], ["40", "32", "0:00:50"], ["30", "32", null]),
    ];

    /// <summary>A saved workout after a failed save: a set missing reps, a carry set missing distance.</summary>
    public static List<ExerciseState> EditWorkoutWithErrors(IExerciseCatalog catalog, IWorkoutChecker checker)
    {
        List<ExerciseState> exercises =
        [
            Exercise(catalog, 6, ["8", "60"], [null, "60"]),
            Exercise(catalog, 187, ["40", "32", "0:00:45"], [null, "32", "0:00:40"]),
        ];
        checker.Check(exercises);
        return exercises;
    }

    /// <summary>Cardio exercises with filled entries, including pace.</summary>
    public static List<ExerciseState> CardioWorkout(IExerciseCatalog catalog) =>
    [
        Exercise(catalog, 300, ["0:20:00", "3.2", "1.5", "210", "9.6"]),
        Exercise(catalog, 309, ["0:15:00", "60", "8", "150", null]),
        Exercise(catalog, 311, ["0:25:30", "5", null, "5:06"]),
    ];

    /// <summary>A workout after a failed save: a set missing reps, an exercise with no sets, a rower with no duration and a part damper, and a run with a bad pace.</summary>
    public static List<ExerciseState> WorkoutWithErrors(IExerciseCatalog catalog, IWorkoutChecker checker)
    {
        List<ExerciseState> exercises =
        [
            Exercise(catalog, 222, ["10", "50"], [null, "50"]),
            Exercise(catalog, 132),
            Exercise(catalog, 305, [null, "2000", "5.5", null, null]),
            Exercise(catalog, 311, ["0:25:30", "5", null, "5:75"]),
        ];
        checker.Check(exercises);
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
}
