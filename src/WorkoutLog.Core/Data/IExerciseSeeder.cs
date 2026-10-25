namespace WorkoutLog.Core.Data;

/// <summary>Brings the Exercise table in line with the exercise list.</summary>
internal interface IExerciseSeeder
{
    /// <summary>Inserts missing Ids and updates Name and Type by Id. Never deletes rows. The table must exist.</summary>
    Task SeedAsync();
}
