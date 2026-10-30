namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>Maps a primary muscle to its muscle group.</summary>
internal interface IMuscleGroupMap
{
    /// <summary>The muscle's group, or null when the muscle isn't mapped.</summary>
    MuscleGroup? GroupOf(Muscle muscle);
}
