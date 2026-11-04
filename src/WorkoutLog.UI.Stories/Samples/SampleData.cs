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
        Exercise(catalog, 6, ["60", "8"], ["60", "8"], ["62.5", "6"]),
        Exercise(catalog, 218, ["25", "10"], ["25", "8"]),
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

    /// <summary>Marks the exercise's set or entry at <paramref name="index"/> as the record, as New Workout does for the set that beats the best, and returns the exercise.</summary>
    public static ExerciseState WithRecord(ExerciseState exercise, int index)
    {
        exercise.RecordEntry = exercise.Entries[index];
        return exercise;
    }

    /// <summary>Sets the exercise's last time, as New Workout does after a pick, and returns it.</summary>
    public static ExerciseState WithLastTime(ExerciseState exercise, params string[] entries)
    {
        exercise.LastTime = new LastTime(entries);
        return exercise;
    }

    /// <summary>
    /// New Workout with last time's values: a squat with as many sets as last time, a curl with more, an assisted pull-up with fewer, a plank, a carry and a treadmill run, and an outdoor run with no last time.
    /// </summary>
    public static List<ExerciseState> LastTimeWorkout(IExerciseCatalog catalog) =>
    [
        WithLastTime(Exercise(catalog, 6, ["60", "8"], ["60", "8"], ["62.5", "6"]), "60×8", "60×8", "62.5×6"),
        WithLastTime(Exercise(catalog, 132, ["10", "12"], ["10", "10"], ["12", "8"]), "10×12", "10×10"),
        WithLastTime(Exercise(catalog, 218, ["25", "10"]), "25kg×10", "25kg×8"),
        WithLastTime(Exercise(catalog, 258, ["0:01:00", null]), "1:30"),
        WithLastTime(Exercise(catalog, 187, ["40", "32", "0:00:45"], ["40", "32", "0:00:50"], ["30", "32", null]), "32kg · 40m · 0:45", "32kg · 40m · 0:50"),
        WithLastTime(Exercise(catalog, 300, ["0:20:00", "3.2", "1.5", "210"]), "3km · 20:00 · 9 km/h · 190 calories · 1.5%"),
        Exercise(catalog, 311, ["0:25:30", "5", null]),
    ];

    /// <summary>A short note.</summary>
    public const string Note = "Left knee twinge on the last set";

    /// <summary>A short note from the last workout.</summary>
    public const string LastNote = "Belt on from set 3, felt strong";

    /// <summary>A note too long for one line on a phone.</summary>
    public const string LongNote = "Slow tempo, three seconds down and a pause at the bottom. Grip gave out before the legs did, so try straps next time. Warm up with the empty bar and two lighter sets first.";

    /// <summary>A note of <see cref="WorkoutLimits.MaxNoteLength"/> + 1 characters.</summary>
    public static readonly string NoteTooLong = string.Concat(Enumerable.Repeat("Felt heavy today. ", 28))[..(WorkoutLimits.MaxNoteLength + 1)];

    /// <summary>The note field's error for a note over <see cref="WorkoutLimits.MaxNoteLength"/> characters.</summary>
    public static readonly string NoteTooLongError = WorkoutChecker.NoteTooLong;

    /// <summary>Sets the exercise's note and last note, as New Workout does after a pick, and returns it.</summary>
    public static ExerciseState WithNotes(ExerciseState exercise, string? note, string? lastNote)
    {
        exercise.Note = note;
        exercise.LastNote = lastNote;
        return exercise;
    }

    /// <summary>Sets the notes of the first three exercises: both, last time's only, and today's only (long). The rest have none. Returns the list.</summary>
    public static List<ExerciseState> WithSampleNotes(List<ExerciseState> workout)
    {
        WithNotes(workout[0], Note, LastNote);
        WithNotes(workout[1], null, LongNote);
        WithNotes(workout[2], LongNote, null);
        return workout;
    }

    /// <summary>Sets a note over the limit and its error on the first exercise. Returns the list.</summary>
    public static List<ExerciseState> WithNoteTooLong(List<ExerciseState> workout)
    {
        workout[0].Note = NoteTooLong;
        workout[0].NoteError = NoteTooLongError;
        return workout;
    }

    /// <summary><see cref="StrengthWorkout"/> with each exercise's <see cref="Weights"/>.</summary>
    public static List<ExerciseState> StrengthWorkoutWithWeights(IExerciseCatalog catalog) =>
        StrengthWorkout(catalog).Select(e => WithWeights(e, Weights[e.ExerciseId].LastKg, Weights[e.ExerciseId].BestKg)).ToList();

    /// <summary>A saved workout's exercises, given oldest first, in the order Edit Workout shows them: newest first.</summary>
    public static List<ExerciseState> Opened(List<ExerciseState> saved) => [.. saved.AsEnumerable().Reverse()];

    /// <summary>A saved workout's exercises, given oldest first, as Edit Workout loads them: newest first, each one done.</summary>
    public static List<ExerciseState> Loaded(List<ExerciseState> saved) => MarkDone(Opened(saved), [.. Enumerable.Range(0, saved.Count)]);

    /// <summary>Deletes the set (from 0) of the exercise at this position of the screen order, as its trash does, and returns the deletion Undo puts back.</summary>
    public static PendingUndo DeleteSet(List<ExerciseState> exercises, int position, int set)
    {
        var exercise = exercises[position];
        var removed = new RemovedSet(exercise, exercise.Entries[set], exercise.Entries[..set]);
        exercise.Entries.RemoveAt(set);
        return removed;
    }

    /// <summary>Deletes the exercise at this position of the screen order, as its trash does, and returns the deletion Undo puts back.</summary>
    public static PendingUndo DeleteExercise(List<ExerciseState> exercises, int position)
    {
        var exercise = exercises[position];
        exercises.RemoveAt(position);
        return new RemovedExercise(exercise, position);
    }

    /// <summary>Marks the exercises at these positions of the screen order done, and returns the list.</summary>
    public static List<ExerciseState> MarkDone(List<ExerciseState> exercises, params int[] positions)
    {
        foreach (var position in positions)
        {
            exercises[position].IsDone = true;
        }

        return exercises;
    }

    /// <summary>A squat with two sets, and a carry with three sets: two filled, the last one partly filled.</summary>
    public static List<ExerciseState> CarryWorkout(IExerciseCatalog catalog) =>
    [
        Exercise(catalog, 6, ["60", "8"], ["60", "8"]),
        Exercise(catalog, 187, ["40", "32", "0:00:45"], ["40", "32", "0:00:50"], ["30", "32", null]),
    ];

    /// <summary>A saved workout after a failed save: a set missing reps, a carry set missing distance.</summary>
    public static List<ExerciseState> EditWorkoutWithErrors(IExerciseCatalog catalog, IWorkoutChecker checker)
    {
        List<ExerciseState> exercises =
        [
            Exercise(catalog, 6, ["60", "8"], ["60", null]),
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

    /// <summary>A squat whose last set is the record, a carry whose first set is the record and a run that is the record, each with its last and PB.</summary>
    public static List<ExerciseState> RecordsWorkout(IExerciseCatalog catalog) =>
    [
        WithRecord(WithWeights(Exercise(catalog, 6, ["8", "60"], ["8", "60"], ["6", "62.5"]), 60m, 60m), 2),
        WithRecord(Exercise(catalog, 187, ["40", "36", "0:00:45"], ["40", "32", "0:00:50"], ["30", "32", null]), 0),
        WithRecord(Exercise(catalog, 311, ["0:25:30", "5.2", null]), 0),
    ];

    /// <summary>Ten strength exercises, so Add exercise is off.</summary>
    public static List<ExerciseState> FullWorkout(IExerciseCatalog catalog) =>
        [.. new[] { 6, 8, 132, 222, 218, 258, 187, 263, 264, 13 }.Select(id => Exercise(catalog, id, Sets(catalog, id)))];

    /// <summary>A workout after a failed save: a set missing reps, an exercise with no sets, a rower with no duration and a damper of 5.5, and a run with no distance.</summary>
    public static List<ExerciseState> WorkoutWithErrors(IExerciseCatalog catalog, IWorkoutChecker checker)
    {
        List<ExerciseState> exercises =
        [
            Exercise(catalog, 222, ["50", "10"], ["50", null]),
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

    /// <summary>One filled set of the exercise, in its field order: 20 kg, reps 8, 0:00:45, 40.</summary>
    private static string?[] Sets(IExerciseCatalog catalog, int id) =>
        [.. ExerciseFields.For(Find(catalog, id)).Select(f => f.Metric switch
        {
            Metric.Reps => "8",
            Metric.WeightKg => "20",
            Metric.Duration => "0:00:45",
            _ => "40",
        })];
}
