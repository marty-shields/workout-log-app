using System.Text.Json;
using SQLite;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Data;

/// <summary>Reads and writes the <see cref="UnfinishedTables"/>.</summary>
internal sealed class UnfinishedWorkoutRepository(SQLiteAsyncConnection connection) : IUnfinishedWorkoutRepository
{
    /// <inheritdoc />
    public async Task<UnfinishedWorkout?> GetAsync()
    {
        var workout = await connection.Table<UnfinishedWorkoutRow>().FirstOrDefaultAsync();
        if (workout is null)
        {
            return null;
        }

        var exercises = await connection.Table<UnfinishedExerciseRow>().OrderBy(e => e.Position).ToListAsync();
        var entries = (await connection.Table<UnfinishedEntryRow>().OrderBy(e => e.Position).ToListAsync()).ToLookup(e => e.UnfinishedExerciseId);
        return new UnfinishedWorkout(
            workout.StartedAt,
            workout.Note,
            exercises.Select(e => new UnfinishedExercise(
                e.ExerciseId,
                e.Note,
                e.IsDone,
                entries[e.Id].Select(entry => new UnfinishedEntry(
                    JsonSerializer.Deserialize<Dictionary<string, string?>>(entry.FieldValues) ?? [])).ToList())).ToList());
    }

    /// <inheritdoc />
    public Task ReplaceAsync(UnfinishedWorkout workout) =>
        connection.RunInTransactionAsync(db =>
        {
            UnfinishedTables.Clear(db);
            if (workout.Exercises.Count == 0)
            {
                return;
            }

            db.Insert(new UnfinishedWorkoutRow { StartedAt = workout.StartedAt, Note = workout.Note });
            for (var position = 0; position < workout.Exercises.Count; position++)
            {
                var exercise = workout.Exercises[position];
                var row = new UnfinishedExerciseRow { ExerciseId = exercise.ExerciseId, Position = position, Note = exercise.Note, IsDone = exercise.IsDone };
                db.Insert(row);
                for (var entry = 0; entry < exercise.Entries.Count; entry++)
                {
                    db.Insert(new UnfinishedEntryRow
                    {
                        UnfinishedExerciseId = row.Id,
                        Position = entry,
                        FieldValues = JsonSerializer.Serialize(exercise.Entries[entry].Values),
                    });
                }
            }
        });

    /// <inheritdoc />
    public Task DeleteAsync() => connection.RunInTransactionAsync(UnfinishedTables.Clear);
}
