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

    /// <summary>The card is shown as one line. Only the screen's view of the exercise: it is never saved and is not a change.</summary>
    public bool IsDone { get; set; }

    /// <summary>The exercise, its note or one of its sets or entries shows an error.</summary>
    public bool HasError => Error is not null || NoteError is not null || Entries.Any(e => e.Errors.Count > 0);

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

    /// <summary>New Workout: the exercise's last saved workout, shown beside each set or above the entry. Null when there is none: the exercise's first time.</summary>
    public LastTime? LastTime { get; set; }

    /// <summary>Note on the exercise in this workout. Null when there is none.</summary>
    public string? Note { get; set; }

    /// <summary>The note the exercise had in the last saved workout with it. Null hides the last note line.</summary>
    public string? LastNote { get; set; }

    /// <summary>Validation message for <see cref="Note"/>, shown in the note field.</summary>
    public string? NoteError { get; set; }
}
