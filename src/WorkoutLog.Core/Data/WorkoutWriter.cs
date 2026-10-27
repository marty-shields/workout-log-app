using SQLite;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Data;

/// <summary>Writes workout rows on <paramref name="db"/> and deletes child rows in code.</summary>
internal sealed class WorkoutWriter(SQLiteConnection db) : IWorkoutWriter
{
    /// <inheritdoc />
    public int Add(NewWorkout workout)
    {
        var row = new WorkoutRow { StartedAt = workout.StartedAt, FinishedAt = workout.FinishedAt };
        db.Insert(row);
        InsertExercises(row.Id, workout.Exercises);
        return row.Id;
    }

    /// <inheritdoc />
    public void Replace(NewWorkout workout)
    {
        var start = workout.StartedAt;
        var row = db.Table<WorkoutRow>().FirstOrDefault(w => w.StartedAt == start)
            ?? throw new KeyNotFoundException("No workout has the start");
        row.FinishedAt = workout.FinishedAt;
        db.Update(row);
        DeleteExercises(row.Id);
        InsertExercises(row.Id, workout.Exercises);
    }

    /// <inheritdoc />
    public void ReplaceExercises(int workoutId, IReadOnlyList<WorkoutExercise> exercises)
    {
        EnsureExists(workoutId);
        DeleteExercises(workoutId);
        InsertExercises(workoutId, exercises);
    }

    /// <inheritdoc />
    public void Delete(int workoutId)
    {
        EnsureExists(workoutId);
        DeleteExercises(workoutId);
        db.Delete<WorkoutRow>(workoutId);
    }

    /// <inheritdoc />
    public void DeleteAll()
    {
        db.DeleteAll<StrengthSetRow>();
        db.DeleteAll<CardioEntryRow>();
        db.DeleteAll<WorkoutExerciseRow>();
        db.DeleteAll<WorkoutRow>();
    }

    private void EnsureExists(int workoutId)
    {
        if (db.Find<WorkoutRow>(workoutId) is null)
        {
            throw new KeyNotFoundException($"Workout {workoutId} not found");
        }
    }

    private void DeleteExercises(int workoutId)
    {
        const string exerciseIds = "SELECT Id FROM WorkoutExercise WHERE WorkoutId = ?";
        db.Execute($"DELETE FROM StrengthSet WHERE WorkoutExerciseId IN ({exerciseIds})", workoutId);
        db.Execute($"DELETE FROM CardioEntry WHERE WorkoutExerciseId IN ({exerciseIds})", workoutId);
        db.Execute("DELETE FROM WorkoutExercise WHERE WorkoutId = ?", workoutId);
    }

    private void InsertExercises(int workoutId, IReadOnlyList<WorkoutExercise> exercises)
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
}
