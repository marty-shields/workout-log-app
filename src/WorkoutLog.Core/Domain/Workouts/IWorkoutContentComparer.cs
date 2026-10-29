namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>Compares a saved workout with one from an import file.</summary>
internal interface IWorkoutContentComparer
{
    /// <summary>
    /// True when the two would export the same: finish to the tick; exercises and sets in order;
    /// decimals rounded by <see cref="Exercises.Decimals.Round2"/>; durations to the second.
    /// Starts and exercise names aren't compared.
    /// </summary>
    bool AreSame(Workout saved, NewWorkout imported);
}
