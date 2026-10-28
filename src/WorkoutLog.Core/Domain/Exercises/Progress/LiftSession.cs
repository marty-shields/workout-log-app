using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A saved workout's sets of one lift.</summary>
/// <param name="WorkoutId">Workout id.</param>
/// <param name="StartedAt">Start time of the workout, UTC.</param>
/// <param name="Sets">Every set of the lift in the workout, in workout order.</param>
public sealed record LiftSession(int WorkoutId, DateTimeOffset StartedAt, IReadOnlyList<StrengthSet> Sets);
