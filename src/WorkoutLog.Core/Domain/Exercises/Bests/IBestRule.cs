using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Domain.Exercises.Bests;

/// <summary>Picks the best of a session for one type of exercise. Each exercise in the list matches one rule.</summary>
internal interface IBestRule
{
    /// <summary>True when the rule is for <paramref name="exercise"/>.</summary>
    bool AppliesTo(ExerciseDefinition exercise);

    /// <summary>Returns the best of <paramref name="logged"/>, or null when there is nothing to show.</summary>
    ExerciseBest? Best(ExerciseDefinition exercise, IReadOnlyList<WorkoutExercise> logged);
}
