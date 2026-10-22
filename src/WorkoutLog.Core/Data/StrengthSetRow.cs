using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Row in the StrengthSet table.</summary>
[Table("StrengthSet")]
internal sealed class StrengthSetRow
{
    /// <summary>Row id.</summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Id of the workout exercise.</summary>
    [Indexed]
    public int WorkoutExerciseId { get; set; }

    /// <summary>Order added, from 0.</summary>
    public int Position { get; set; }

    /// <summary>Repetitions.</summary>
    public int? Reps { get; set; }

    /// <summary>Weight in kg.</summary>
    public decimal? WeightKg { get; set; }

    /// <summary>Duration.</summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>Distance, in the exercise's unit.</summary>
    public decimal? Distance { get; set; }
}
