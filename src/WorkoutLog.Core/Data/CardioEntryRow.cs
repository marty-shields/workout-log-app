using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Row in the CardioEntry table. One per workout exercise.</summary>
[Table("CardioEntry")]
internal sealed class CardioEntryRow
{
    /// <summary>Row id.</summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Id of the workout exercise. Unique.</summary>
    [Unique]
    public int WorkoutExerciseId { get; set; }

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Distance, in the exercise's unit.</summary>
    public decimal? Distance { get; set; }

    /// <summary>Floors climbed.</summary>
    public int? Floors { get; set; }

    /// <summary>Resistance, damper or speed level.</summary>
    public int? Level { get; set; }

    /// <summary>Incline, percent.</summary>
    public decimal? Incline { get; set; }

    /// <summary>Calories.</summary>
    public int? Calories { get; set; }

    /// <summary>Rate, in the exercise's unit.</summary>
    public decimal? Rate { get; set; }

    /// <summary>Pace: time per the exercise's pace unit.</summary>
    public TimeSpan? Pace { get; set; }
}
