namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>Maps a primary muscle to its muscle group.</summary>
internal interface IMuscleGroupMap
{
    /// <summary>The muscle's group. Throws <see cref="ArgumentOutOfRangeException"/> for a muscle with no group.</summary>
    MuscleGroup GroupOf(Muscle muscle);
}
