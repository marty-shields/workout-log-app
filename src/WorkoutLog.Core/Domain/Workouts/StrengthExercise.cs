namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>A strength exercise in a workout.</summary>
/// <param name="ExerciseId">Id of the exercise in the fixed list.</param>
/// <param name="Sets">Sets, in the order added.</param>
public sealed record StrengthExercise(int ExerciseId, IReadOnlyList<StrengthSet> Sets) : WorkoutExercise(ExerciseId);
