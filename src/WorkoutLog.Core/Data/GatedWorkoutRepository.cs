using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Data;

/// <summary>Runs each <see cref="WorkoutRepository"/> call through the <see cref="DatabaseGate"/>.</summary>
internal sealed class GatedWorkoutRepository(WorkoutRepository inner, DatabaseGate gate) : IWorkoutRepository
{
    /// <inheritdoc />
    public Task<int> AddAsync(NewWorkout workout) => gate.RunAsync(() => inner.AddAsync(workout));

    /// <inheritdoc />
    public Task ReplaceExercisesAsync(int workoutId, IReadOnlyList<WorkoutExercise> exercises) =>
        gate.RunAsync(() => inner.ReplaceExercisesAsync(workoutId, exercises));

    /// <inheritdoc />
    public Task<Workout?> GetAsync(int id) => gate.RunAsync(() => inner.GetAsync(id));

    /// <inheritdoc />
    public Task<IReadOnlyList<WorkoutSummary>> ListAsync() => gate.RunAsync(() => inner.ListAsync());

    /// <inheritdoc />
    public Task DeleteAsync(int id) => gate.RunAsync(() => inner.DeleteAsync(id));
}
