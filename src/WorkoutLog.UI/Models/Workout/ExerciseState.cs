using WorkoutLog.Core.Domain.Workouts;
using WorkoutLog.Core.Domain.Workouts.Validation;
using WorkoutLog.UI.Models.Fields;

namespace WorkoutLog.UI.Models.Workout;

/// <summary>An exercise in a workout while it is edited.</summary>
/// <param name="exerciseId">Exercise id.</param>
/// <param name="name">Exercise name.</param>
/// <param name="kind">Strength or cardio.</param>
/// <param name="fields">Fields shown for each set or entry.</param>
/// <param name="workedOut">Cardio: the value shown worked out from distance and duration.</param>
public sealed class ExerciseState(int exerciseId, string name, ExerciseKind kind, IReadOnlyList<FieldSpec> fields, WorkedOutKind workedOut = WorkedOutKind.None)
{
    /// <summary>Exercise id.</summary>
    public int ExerciseId { get; } = exerciseId;

    /// <summary>Exercise name.</summary>
    public string Name { get; } = name;

    /// <summary>Strength or cardio.</summary>
    public ExerciseKind Kind { get; } = kind;

    /// <summary>Fields shown for each set or entry.</summary>
    public IReadOnlyList<FieldSpec> Fields { get; } = fields;

    /// <summary>Cardio: the value shown worked out from distance and duration.</summary>
    public WorkedOutKind WorkedOut { get; } = workedOut;

    /// <summary>Strength: the exercise has <see cref="WorkoutLimits.MaxSets"/> sets, so Add set is off.</summary>
    public bool SetsFull => Kind == ExerciseKind.Strength && Entries.Count >= WorkoutLimits.MaxSets;

    /// <summary>Sets (strength) or the single entry (cardio), in the order added.</summary>
    public List<EntryState> Entries { get; } = new();

    /// <summary>Validation message for the exercise.</summary>
    public string? Error { get; set; }

    /// <summary>Core's code behind <see cref="Error"/>, when Save set it.</summary>
    public ExerciseError? ErrorCode { get; set; }

    /// <summary>New Workout: the set or entry in <see cref="Entries"/> that beats the exercise's best in every saved workout, so shows the Record badge. At most one. Null shows no badge.</summary>
    public EntryState? RecordEntry { get; set; }

    /// <summary>New Workout, strength: best weight in the last saved workout with the exercise, in kg. Null when that workout has none.</summary>
    public decimal? LastKg { get; set; }

    /// <summary>New Workout, strength: best weight in any saved workout (PB), in kg. Null hides the last and PB line.</summary>
    public decimal? BestKg { get; set; }
}
