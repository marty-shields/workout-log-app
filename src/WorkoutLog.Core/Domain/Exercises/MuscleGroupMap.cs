namespace WorkoutLog.Core.Domain.Exercises;

/// <inheritdoc />
internal sealed class MuscleGroupMap : IMuscleGroupMap
{
    private static readonly Dictionary<Muscle, MuscleGroup> Groups = new()
    {
        [Muscle.Chest] = MuscleGroup.Chest,
        [Muscle.Lats] = MuscleGroup.Back,
        [Muscle.MiddleBack] = MuscleGroup.Back,
        [Muscle.LowerBack] = MuscleGroup.Back,
        [Muscle.Traps] = MuscleGroup.Back,
        [Muscle.Neck] = MuscleGroup.Back,
        [Muscle.Shoulders] = MuscleGroup.Shoulders,
        [Muscle.Biceps] = MuscleGroup.Arms,
        [Muscle.Triceps] = MuscleGroup.Arms,
        [Muscle.Forearms] = MuscleGroup.Arms,
        [Muscle.Quadriceps] = MuscleGroup.Legs,
        [Muscle.Hamstrings] = MuscleGroup.Legs,
        [Muscle.Glutes] = MuscleGroup.Legs,
        [Muscle.Calves] = MuscleGroup.Legs,
        [Muscle.Abductors] = MuscleGroup.Legs,
        [Muscle.Adductors] = MuscleGroup.Legs,
        [Muscle.Abdominals] = MuscleGroup.Core,
    };

    /// <inheritdoc />
    public MuscleGroup? GroupOf(Muscle muscle) => Groups.TryGetValue(muscle, out var group) ? group : null;
}
