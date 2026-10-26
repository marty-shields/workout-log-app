using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Row in the Workout table.</summary>
[Table("Workout")]
internal sealed class WorkoutRow
{
    /// <summary>Workout id.</summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Start time. Stored as UTC ticks. Unique.</summary>
    [Indexed(Unique = true)]
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>Finish time. Stored as UTC ticks.</summary>
    public DateTimeOffset FinishedAt { get; set; }
}
