using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Data;

/// <summary>Runs each call to the inner repository through the <see cref="DatabaseGate"/>.</summary>
internal sealed class GatedWorkoutRepository(IWorkoutRepository inner, DatabaseGate gate) : IWorkoutRepository
{
    /// <inheritdoc />
    public Task<int> AddAsync(NewWorkout workout) => gate.RunAsync(() => inner.AddAsync(workout));

    /// <inheritdoc />
    public Task ReplaceAsync(int workoutId, string? note, IReadOnlyList<WorkoutExercise> exercises) =>
        gate.RunAsync(() => inner.ReplaceAsync(workoutId, note, exercises));

    /// <inheritdoc />
    public Task<Workout?> GetAsync(int id) => gate.RunAsync(() => inner.GetAsync(id));

    /// <inheritdoc />
    public Task<IReadOnlyList<Workout>> GetAllAsync() => gate.RunAsync(inner.GetAllAsync);

    /// <inheritdoc />
    public Task<IReadOnlyList<WorkoutSummary>> ListAsync(DateTimeOffset? from, DateTimeOffset? to, int? limit) =>
        gate.RunAsync(() => inner.ListAsync(from, to, limit));

    /// <inheritdoc />
    public Task<IReadOnlyList<DateTimeOffset>> ListStartsAsync() => gate.RunAsync(inner.ListStartsAsync);

    /// <inheritdoc />
    public Task<WorkoutOutline?> GetLatestAsync() => gate.RunAsync(inner.GetLatestAsync);

    /// <inheritdoc />
    public Task<IReadOnlyList<WorkoutTally>> ListTalliesAsync(DateTimeOffset from, DateTimeOffset to) =>
        gate.RunAsync(() => inner.ListTalliesAsync(from, to));

    /// <inheritdoc />
    public Task<IReadOnlyList<WorkoutWeights>> GetWeightsAsync(int exerciseId, DateTimeOffset? before) =>
        gate.RunAsync(() => inner.GetWeightsAsync(exerciseId, before));

    /// <inheritdoc />
    public Task<IReadOnlyList<LastSession>> GetLastSessionsAsync() => gate.RunAsync(inner.GetLastSessionsAsync);

    /// <inheritdoc />
    public Task<LastSession?> GetLastSessionOfAsync(int exerciseId, DateTimeOffset? before) =>
        gate.RunAsync(() => inner.GetLastSessionOfAsync(exerciseId, before));

    /// <inheritdoc />
    public Task<IReadOnlyList<WorkoutSession>> GetSessionsAsync(int exerciseId) => gate.RunAsync(() => inner.GetSessionsAsync(exerciseId));

    /// <inheritdoc />
    public Task<IReadOnlyList<ExerciseSetCount>> GetSetCountsAsync(DateTimeOffset from, DateTimeOffset to) =>
        gate.RunAsync(() => inner.GetSetCountsAsync(from, to));

    /// <inheritdoc />
    public Task DeleteAsync(int id) => gate.RunAsync(() => inner.DeleteAsync(id));
}
