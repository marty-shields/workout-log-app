using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Data;

/// <summary>Runs each call to the inner repository through the <see cref="DatabaseGate"/>.</summary>
internal sealed class GatedWorkoutRepository(IWorkoutRepository inner, DatabaseGate gate) : IWorkoutRepository
{
    /// <inheritdoc />
    public Task<int> AddAsync(NewWorkout workout) => gate.RunAsync(() => inner.AddAsync(workout));

    /// <inheritdoc />
    public Task ReplaceExercisesAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises) =>
        gate.RunAsync(() => inner.ReplaceExercisesAsync(workoutId, exercises));

    /// <inheritdoc />
    public Task<Workout?> GetAsync(int id) => gate.RunAsync(() => inner.GetAsync(id));

    /// <inheritdoc />
    public Task<IReadOnlyList<WorkoutSummary>> ListAsync(DateTimeOffset? from, DateTimeOffset? to, int? limit) =>
        gate.RunAsync(() => inner.ListAsync(from, to, limit));

    /// <inheritdoc />
    public Task<IReadOnlyList<WorkoutWeights>> GetWeightsAsync(int exerciseId) => gate.RunAsync(() => inner.GetWeightsAsync(exerciseId));

    /// <inheritdoc />
    public Task DeleteAsync(int id) => gate.RunAsync(() => inner.DeleteAsync(id));
}
