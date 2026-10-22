namespace WorkoutLog.Core.Domain.Workouts.Validation;

/// <summary>Save rules for a workout (FR-09), checked against each exercise's definition.</summary>
internal interface IWorkoutValidator
{
    /// <summary>Checks the workout: at least one exercise, and each exercise as in <see cref="Validate(WorkoutExercise)"/>.</summary>
    WorkoutErrors Validate(IReadOnlyList<WorkoutExercise> exercises);

    /// <summary>
    /// Checks one exercise: it is in the list, its type matches, a strength exercise has a set,
    /// and each set or entry has its required metrics, values within limits and no metrics the exercise doesn't show.
    /// </summary>
    ExerciseErrors Validate(WorkoutExercise exercise);
}
