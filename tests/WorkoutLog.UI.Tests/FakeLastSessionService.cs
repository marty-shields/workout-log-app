using WorkoutLog.Core.Domain.Exercises.Progress;
using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// <see cref="ILastSessionService"/> for the tests: returns the session set for the exercise with <see cref="Set"/>, no earlier workout for the
/// others, or a failed read when <see cref="Failing"/>. Records each request.
/// </summary>
public sealed class FakeLastSessionService : ILastSessionService
{
    private readonly Dictionary<int, LastSessionExercise> sessions = [];

    /// <summary>When true, every read fails.</summary>
    public bool Failing { get; set; }

    /// <summary>The exercise id and cutoff of each read, in order.</summary>
    public List<(int ExerciseId, DateTimeOffset? Before)> Requests { get; } = [];

    /// <summary>Returns <paramref name="exercise"/> as the last session of its exercise.</summary>
    public void Set(WorkoutExercise exercise, CardioBest? best = null) =>
        sessions[exercise.ExerciseId] = new LastSessionExercise(exercise, best);

    /// <inheritdoc />
    public Task<ReadResult<LastSessionExercise?>> GetAsync(int exerciseId, DateTimeOffset? before = null)
    {
        Requests.Add((exerciseId, before));
        return Task.FromResult<ReadResult<LastSessionExercise?>>(Failing ? new ReadFailed() : sessions.GetValueOrDefault(exerciseId));
    }
}
