using SQLite;
using WorkoutLog.Core.Domain.Exercises;

namespace WorkoutLog.Core.Data;

/// <summary>Row in the Exercise table.</summary>
[Table("Exercise")]
public sealed class Exercise
{
    /// <summary>Exercise id, from the definition file.</summary>
    [PrimaryKey]
    public int Id { get; set; }

    /// <summary>Exercise name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Strength or cardio.</summary>
    public ExerciseType Type { get; set; }
}
