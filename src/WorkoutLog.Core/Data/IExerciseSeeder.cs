namespace WorkoutLog.Core.Data;

/// <summary>Brings the Exercise table in line with the exercise list.</summary>
public interface IExerciseSeeder
{
    /// <summary>Creates the table if missing, inserts missing Ids and updates Name and Type by Id. Never deletes rows.</summary>
    Task SeedAsync();
}
