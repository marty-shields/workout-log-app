using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Row in the UnfinishedWorkout table: the one unfinished workout, Id 1.</summary>
[Table("UnfinishedWorkout")]
internal sealed class UnfinishedWorkoutRow
{
    [PrimaryKey]
    public int Id { get; set; } = 1;

    /// <summary>Start time. Stored as UTC ticks.</summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>The note as typed; null when none.</summary>
    public string? Note { get; set; }
}

/// <summary>Row in the UnfinishedExercise table.</summary>
[Table("UnfinishedExercise")]
internal sealed class UnfinishedExerciseRow
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int ExerciseId { get; set; }

    /// <summary>Order added, from 0, oldest first.</summary>
    public int Position { get; set; }

    public string? Note { get; set; }

    public bool IsDone { get; set; }
}

/// <summary>Row in the UnfinishedEntry table.</summary>
[Table("UnfinishedEntry")]
internal sealed class UnfinishedEntryRow
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UnfinishedExerciseId { get; set; }

    /// <summary>Order in the exercise, from 0.</summary>
    public int Position { get; set; }

    /// <summary>The typed text of each field as a JSON object of field key to text.</summary>
    public string FieldValues { get; set; } = "{}";
}
