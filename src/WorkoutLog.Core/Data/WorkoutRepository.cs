using SQLite;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Data;

/// <summary>Stores workouts in SQLite and deletes child rows in code.</summary>
internal sealed class WorkoutRepository(SQLiteAsyncConnection connection) : IWorkoutRepository
{
    /// <inheritdoc />
    public async Task<int> AddAsync(NewWorkout workout)
    {
        var row = new WorkoutRow { StartedAt = workout.StartedAt, FinishedAt = workout.FinishedAt };
        await connection.RunInTransactionAsync(db =>
        {
            db.Insert(row);
            InsertExercises(db, row.Id, workout.Exercises);
        });
        return row.Id;
    }

    /// <inheritdoc />
    public Task ReplaceExercisesAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises) =>
        connection.RunInTransactionAsync(db =>
        {
            EnsureExists(db, workoutId);
            DeleteExercises(db, workoutId);
            InsertExercises(db, workoutId, exercises);
        });

    /// <inheritdoc />
    public async Task<Workout?> GetAsync(int id)
    {
        var row = await connection.FindAsync<WorkoutRow>(id);
        if (row is null)
        {
            return null;
        }

        var exerciseRows = await connection.Table<WorkoutExerciseRow>()
            .Where(e => e.WorkoutId == id).OrderBy(e => e.Position).ToListAsync();
        var exerciseIds = exerciseRows.Select(e => e.Id).ToList();
        var sets = (await connection.Table<StrengthSetRow>()
                .Where(s => exerciseIds.Contains(s.WorkoutExerciseId)).OrderBy(s => s.Position).ToListAsync())
            .ToLookup(s => s.WorkoutExerciseId);
        var entries = (await connection.Table<CardioEntryRow>()
                .Where(c => exerciseIds.Contains(c.WorkoutExerciseId)).ToListAsync())
            .ToDictionary(c => c.WorkoutExerciseId);

        var exercises = exerciseRows.Select(e => entries.TryGetValue(e.Id, out var entry)
                ? (WorkoutExercise)new CardioExercise(e.ExerciseId, ToEntry(entry))
                : new StrengthExercise(e.ExerciseId, sets[e.Id].Select(ToSet).ToList()))
            .ToList();
        return new Workout(row.Id, row.StartedAt, row.FinishedAt, exercises);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WorkoutSummary>> ListAsync() =>
        (await connection.Table<WorkoutRow>().OrderByDescending(w => w.StartedAt).ThenByDescending(w => w.Id).ToListAsync())
        .Select(w => new WorkoutSummary(w.Id, w.StartedAt))
        .ToList();

    /// <inheritdoc />
    public Task DeleteAsync(int id) =>
        connection.RunInTransactionAsync(db =>
        {
            EnsureExists(db, id);
            DeleteExercises(db, id);
            db.Delete<WorkoutRow>(id);
        });

    private static void EnsureExists(SQLiteConnection db, int workoutId)
    {
        if (db.Find<WorkoutRow>(workoutId) is null)
        {
            throw new KeyNotFoundException($"Workout {workoutId} not found");
        }
    }

    private static void DeleteExercises(SQLiteConnection db, int workoutId)
    {
        const string exerciseIds = "SELECT Id FROM WorkoutExercise WHERE WorkoutId = ?";
        db.Execute($"DELETE FROM StrengthSet WHERE WorkoutExerciseId IN ({exerciseIds})", workoutId);
        db.Execute($"DELETE FROM CardioEntry WHERE WorkoutExerciseId IN ({exerciseIds})", workoutId);
        db.Execute("DELETE FROM WorkoutExercise WHERE WorkoutId = ?", workoutId);
    }

    private static void InsertExercises(SQLiteConnection db, int workoutId, IReadOnlyList<WorkoutExercise> exercises)
    {
        for (var i = 0; i < exercises.Count; i++)
        {
            var exerciseRow = new WorkoutExerciseRow { WorkoutId = workoutId, ExerciseId = exercises[i].ExerciseId, Position = i };
            db.Insert(exerciseRow);

            switch (exercises[i])
            {
                case StrengthExercise strength:
                    db.InsertAll(strength.Sets.Select((s, position) => ToRow(s, exerciseRow.Id, position)), runInTransaction: false);
                    break;
                case CardioExercise cardio:
                    db.Insert(ToRow(cardio.Entry, exerciseRow.Id));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exercises), $"Unknown exercise type {exercises[i].GetType().Name}");
            }
        }
    }

    private static StrengthSetRow ToRow(StrengthSet set, int workoutExerciseId, int position) => new()
    {
        WorkoutExerciseId = workoutExerciseId,
        Position = position,
        Reps = set.Reps,
        WeightKg = set.WeightKg,
        Duration = set.Duration,
        Distance = set.Distance,
    };

    private static CardioEntryRow ToRow(CardioEntry entry, int workoutExerciseId) => new()
    {
        WorkoutExerciseId = workoutExerciseId,
        Duration = entry.Duration ?? throw new ArgumentException("Cardio entry has no duration", nameof(entry)),
        Distance = entry.Distance,
        Floors = entry.Floors,
        Level = entry.Level,
        Incline = entry.Incline,
        Calories = entry.Calories,
        Rate = entry.Rate,
        Pace = entry.Pace,
    };

    private static StrengthSet ToSet(StrengthSetRow row) => new(row.Reps, row.WeightKg, row.Duration, row.Distance);

    private static CardioEntry ToEntry(CardioEntryRow row) =>
        new(row.Duration, row.Distance, row.Floors, row.Level, row.Incline, row.Calories, row.Rate, row.Pace);
}
