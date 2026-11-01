using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>A session that set a record.</summary>
/// <param name="Session">The session that set it.</param>
/// <param name="Best">Its best: the record.</param>
internal sealed record SessionRecord(WorkoutSession Session, ExerciseBest Best);
