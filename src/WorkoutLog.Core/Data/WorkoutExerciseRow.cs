using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Row in the WorkoutExercise table.</summary>
[Table("WorkoutExercise")]
internal sealed class WorkoutExerciseRow
{
    /// <summary>Row id.</summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Id of the workout.</summary>
    [Indexed]
    public int WorkoutId { get; set; }

    /// <summary>Id of the exercise.</summary>
    public int ExerciseId { get; set; }

    /// <summary>Order added or last picked, from 0 (D42).</summary>
    public int Position { get; set; }
}
