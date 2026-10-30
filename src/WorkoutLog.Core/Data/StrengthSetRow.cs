using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Row in the StrengthSet table.</summary>
[Table("StrengthSet")]
internal sealed class StrengthSetRow
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Id of the workout exercise.</summary>
    [Indexed]
    public int WorkoutExerciseId { get; set; }

    /// <summary>Order added, from 0 to 9.</summary>
    public int Position { get; set; }

    public int? Reps { get; set; }

    /// <summary>Weight in kg; 0 is unweighted or no assist.</summary>
    [NotNull]
    public decimal WeightKg { get; set; }

    public TimeSpan? Duration { get; set; }

    /// <summary>Distance, in the exercise's unit.</summary>
    public decimal? Distance { get; set; }
}
