namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>Maps a primary muscle to its muscle group.</summary>
internal sealed class MuscleGroupMap
{
    /// <summary>The muscle's group. Throws <see cref="ArgumentOutOfRangeException"/> for a muscle with no group.</summary>
    public MuscleGroup GroupOf(Muscle muscle) => muscle switch
    {
        Muscle.Chest => MuscleGroup.Chest,
        Muscle.Lats or Muscle.MiddleBack or Muscle.LowerBack or Muscle.Traps or Muscle.Neck => MuscleGroup.Back,
        Muscle.Shoulders => MuscleGroup.Shoulders,
        Muscle.Biceps or Muscle.Triceps or Muscle.Forearms => MuscleGroup.Arms,
        Muscle.Quadriceps or Muscle.Hamstrings or Muscle.Glutes or Muscle.Calves or Muscle.Abductors or Muscle.Adductors => MuscleGroup.Legs,
        Muscle.Abdominals => MuscleGroup.Core,
        _ => throw new ArgumentOutOfRangeException(nameof(muscle), muscle, "No muscle group for this muscle."),
    };
}
