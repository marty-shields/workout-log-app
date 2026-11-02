namespace WorkoutLog.Core.Domain.Exercises;

/// <summary>Muscle group of a strength exercise's primary muscle, in display order.</summary>
public enum MuscleGroup
{
    Chest,

    /// <summary>Lats, middle back, lower back, traps and neck.</summary>
    Back,

    Shoulders,

    /// <summary>Biceps, triceps and forearms.</summary>
    Arms,

    /// <summary>Quadriceps, hamstrings, glutes, calves, abductors and adductors.</summary>
    Legs,

    /// <summary>Abdominals.</summary>
    Core,
}
