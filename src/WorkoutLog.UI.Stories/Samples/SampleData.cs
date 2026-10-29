using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.UI.Models.Exercises;
using WorkoutLog.UI.Models.Workout;
using WorkoutLog.UI.Models.Workouts;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>Sample data for the stories and the UI tests. Exercises come from the real catalog.</summary>
public static class SampleData
{
    /// <summary>Start time of the sample workouts: Tue 20 Oct 2026 18:05.</summary>
    public static readonly DateTime Started = new(2026, 10, 20, 18, 5, 0);

    /// <summary>A saved workout as Core returns it, started at <see cref="Started"/> UTC and 1:04:37 long: squat, assisted pull-up, plank and a carry with two sets.</summary>
    public static readonly Workout SavedStrengthWorkout = new(
        1,
        new DateTimeOffset(Started, TimeSpan.Zero),
        new DateTimeOffset(Started, TimeSpan.Zero) + new TimeSpan(1, 4, 37),
        [
            new StrengthExercise(6, [new StrengthSet(8, 60m, null, null), new StrengthSet(8, 60m, null, null), new StrengthSet(6, 62.5m, null, null)]),
            new StrengthExercise(218, [new StrengthSet(10, 25m, null, null), new StrengthSet(8, 25m, null, null)]),
            new StrengthExercise(258, [new StrengthSet(null, 0m, TimeSpan.FromMinutes(1), null)]),
            new StrengthExercise(187, [new StrengthSet(null, 32m, TimeSpan.FromSeconds(45), 40m), new StrengthSet(null, 32m, TimeSpan.FromSeconds(50), 40m)]),
        ]);

    /// <summary>Add-exercise list items from the catalog's search, filtered by the chip; logged ones carry their <see cref="SampleExercises.All"/> last session unless <paramref name="withLast"/> is false.</summary>
    public static IReadOnlyList<ExerciseListItem> AddExerciseItems(IExerciseCatalog catalog, string? query, ExerciseFilter filter = ExerciseFilter.All, bool withLast = true) =>
        ExerciseListItems.Find(
            catalog.Search(null)
                .Select(e => new ExerciseListItem(e.Id, e.Name, ExerciseFields.Kind(e.Type), withLast ? SampleExercises.All.FirstOrDefault(l => l.Id == e.Id)?.Last : null))
                .ToList(),
            catalog, query, filter);

    /// <summary>A newly added exercise: one empty set (strength) or its empty entry (cardio).</summary>
    public static ExerciseState NewExercise(IExerciseCatalog catalog, int id) => ExerciseFields.New(Find(catalog, id));

    /// <summary>An exercise with one set or entry per array; values follow the exercise's field order.</summary>
    public static ExerciseState Exercise(IExerciseCatalog catalog, int id, params string?[][] entries)
    {
        var exercise = ExerciseFields.New(Find(catalog, id));
        var fields = exercise.Fields;
        exercise.Entries.Clear();
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

    /// <summary>Last and PB weights of the sample exercises, by id: squat, assisted pull-up, plank (no weight last time), carry and curl.</summary>
    public static IReadOnlyDictionary<int, (decimal? LastKg, decimal BestKg)> Weights { get; } = new Dictionary<int, (decimal?, decimal)>
    {
        [6] = (62.5m, 70m),
        [218] = (25m, 20m),
        [258] = (null, 10m),
        [187] = (32m, 36m),
        [132] = (12m, 14m),
    };

    /// <summary>Sets the exercise's last and PB weights, as New Workout does after a pick, and returns it.</summary>
    public static ExerciseState WithWeights(ExerciseState exercise, decimal? lastKg, decimal? bestKg)
    {
        exercise.LastKg = lastKg;
        exercise.BestKg = bestKg;
        return exercise;
    }

    /// <summary><see cref="StrengthWorkout"/> with each exercise's <see cref="Weights"/>.</summary>
    public static List<ExerciseState> StrengthWorkoutWithWeights(IExerciseCatalog catalog) =>
        StrengthWorkout(catalog).Select(e => WithWeights(e, Weights[e.ExerciseId].LastKg, Weights[e.ExerciseId].BestKg)).ToList();

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

    /// <summary>Cardio exercises with filled entries: a treadmill with its speed, a stair climber and a run with its pace.</summary>
    public static List<ExerciseState> CardioWorkout(IExerciseCatalog catalog) =>
    [
        Exercise(catalog, 300, ["0:20:00", "3.2", "1.5", "210"]),
        Exercise(catalog, 309, ["0:15:00", "60", "8", "150", null]),
        Exercise(catalog, 311, ["0:25:30", "5", null]),
    ];

    /// <summary>Ten strength exercises, so Add exercise is off.</summary>
    public static List<ExerciseState> FullWorkout(IExerciseCatalog catalog) =>
        [.. new[] { 6, 8, 132, 222, 218, 258, 187, 263, 264, 13 }.Select(id => Exercise(catalog, id, Sets(catalog, id)))];

    /// <summary>A workout after a failed save: a set missing reps, an exercise with no sets, a rower with no duration and a part damper, and a run with no distance.</summary>
    public static List<ExerciseState> WorkoutWithErrors(IExerciseCatalog catalog, IWorkoutChecker checker)
    {
        List<ExerciseState> exercises =
        [
            Exercise(catalog, 222, ["10", "50"], [null, "50"]),
            Exercise(catalog, 132),
            Exercise(catalog, 305, [null, "2000", "5.5", null, null]),
            Exercise(catalog, 311, ["0:25:30", null, null]),
        ];
        checker.Check(exercises);
        return exercises;
    }

    /// <summary>Workouts rows, newest first.</summary>
    public static IReadOnlyList<WorkoutsRow> WorkoutsRows { get; } =
    [
        new(4, new DateTime(2026, 10, 20, 18, 5, 0)),
        new(3, new DateTime(2026, 10, 20, 7, 30, 0)),
        new(2, new DateTime(2026, 10, 19, 17, 45, 0)),
        new(1, new DateTime(2026, 10, 16, 18, 10, 0)),
    ];

    /// <summary>A strength exercise with ten filled sets, so Add set is off.</summary>
    public static ExerciseState FullSets(IExerciseCatalog catalog, int id) => Exercise(catalog, id, [.. Enumerable.Repeat(Sets(catalog, id), 10)]);

    private static ExerciseDefinition Find(IExerciseCatalog catalog, int id) => catalog.All.Single(e => e.Id == id);

    /// <summary>One filled set of the exercise, in its field order: reps 8, 20 kg, 0:00:45, 40.</summary>
    private static string?[] Sets(IExerciseCatalog catalog, int id) =>
        [.. ExerciseFields.For(Find(catalog, id)).Select(f => f.Metric switch
        {
            Metric.Reps => "8",
            Metric.WeightKg => "20",
            Metric.Duration => "0:00:45",
            _ => "40",
        })];
}
