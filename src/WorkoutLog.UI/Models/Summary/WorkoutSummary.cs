namespace WorkoutLog.UI.Models.Summary;

/// <summary>What a <see cref="Screens.WorkoutSummaryView"/> shows about a saved new workout.</summary>
/// <param name="Duration">Finish minus start.</param>
/// <param name="Exercises">Number of exercises.</param>
/// <param name="Sets">Number of strength sets.</param>
/// <param name="Records">Exercises whose best is a record, in the workout's order.</param>
/// <param name="Changes">Each exercise's best against its last session, in the workout's order. Empty when the workout is the first one.</param>
public sealed record WorkoutSummary(TimeSpan Duration, int Exercises, int Sets, IReadOnlyList<SummaryRecord> Records, IReadOnlyList<SummaryChange> Changes)
{
    /// <summary>There is no earlier workout to compare with: <see cref="Changes"/> is empty.</summary>
    public bool IsFirstWorkout => Changes.Count == 0;
}
