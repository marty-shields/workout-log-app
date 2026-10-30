using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Row in the CardioEntry table. One per workout exercise.</summary>
[Table("CardioEntry")]
internal sealed class CardioEntryRow
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Id of the workout exercise. Unique.</summary>
    [Unique]
    public int WorkoutExerciseId { get; set; }

    public TimeSpan Duration { get; set; }

    /// <summary>Distance, in the exercise's unit.</summary>
    public decimal? Distance { get; set; }

    /// <summary>Floors climbed.</summary>
    public int? Floors { get; set; }

    /// <summary>Resistance, damper or speed level.</summary>
    public int? Level { get; set; }

    /// <summary>Incline, percent.</summary>
    public decimal? Incline { get; set; }

    public int? Calories { get; set; }

    /// <summary>Rate, in the exercise's unit.</summary>
    public decimal? Rate { get; set; }
}
