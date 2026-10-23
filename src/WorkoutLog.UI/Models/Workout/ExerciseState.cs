using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.UI.Models.Fields;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>An exercise in a workout while it is edited.</summary>
/// <param name="exerciseId">Exercise id.</param>
/// <param name="name">Exercise name.</param>
/// <param name="kind">Strength or cardio.</param>
/// <param name="fields">Fields shown for each set or entry.</param>
public sealed class ExerciseState(int exerciseId, string name, ExerciseKind kind, IReadOnlyList<FieldSpec> fields)
{
    /// <summary>Exercise id.</summary>
    public int ExerciseId { get; } = exerciseId;

    /// <summary>Exercise name.</summary>
    public string Name { get; } = name;

    /// <summary>Strength or cardio.</summary>
    public ExerciseKind Kind { get; } = kind;

    /// <summary>Fields shown for each set or entry.</summary>
    public IReadOnlyList<FieldSpec> Fields { get; } = fields;

    /// <summary>Sets (strength) or the single entry (cardio), in the order added.</summary>
    public List<EntryState> Entries { get; } = new();

    /// <summary>Validation message for the exercise.</summary>
    public string? Error { get; set; }

    /// <summary>Core's code behind <see cref="Error"/>, when Save set it.</summary>
    public ExerciseError? ErrorCode { get; set; }
}
