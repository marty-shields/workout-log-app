using SQLite;

namespace WorkoutLog.Core.Data;

/// <summary>Row in the WorkoutExercise table.</summary>
[Table("WorkoutExercise")]
internal sealed class WorkoutExerciseRow
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Id of the workout.</summary>
    [Indexed]
    [Indexed(Name = "WorkoutExercise_WorkoutId_ExerciseId", Order = 1, Unique = true)]
    public int WorkoutId { get; set; }

    /// <summary>Id of the exercise. Unique within the workout.</summary>
    [Indexed(Name = "WorkoutExercise_WorkoutId_ExerciseId", Order = 2, Unique = true)]
    public int ExerciseId { get; set; }

    /// <summary>Order added or last picked, from 0 to 9.</summary>
    public int Position { get; set; }
}
