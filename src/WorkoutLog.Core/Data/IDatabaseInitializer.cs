namespace WorkoutLog.Core.Data;

/// <summary>Prepares the database at startup.</summary>
public interface IDatabaseInitializer
{
    /// <summary>Runs the schema upgrade steps, creates missing tables and columns, then seeds the exercise list.</summary>
    Task InitializeAsync();
}
