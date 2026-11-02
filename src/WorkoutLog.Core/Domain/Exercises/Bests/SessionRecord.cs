using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

internal sealed record SessionRecord(WorkoutSession Session, ExerciseBest Best);
