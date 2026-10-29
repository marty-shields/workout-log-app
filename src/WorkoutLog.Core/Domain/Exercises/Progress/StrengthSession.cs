using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>A saved workout's sets of one strength exercise.</summary>
/// <param name="WorkoutId">Workout id.</param>
/// <param name="StartedAt">Start time of the workout, UTC.</param>
/// <param name="Sets">The sets of the exercise in the workout, in order.</param>
public sealed record StrengthSession(int WorkoutId, DateTimeOffset StartedAt, IReadOnlyList<StrengthSet> Sets);
