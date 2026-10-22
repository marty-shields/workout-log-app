namespace WorkoutLog.UI.Models.History;

/// <summary>A saved workout in the History list.</summary>
/// <param name="Id">Workout id.</param>
/// <param name="StartedAt">Start time in the phone's time zone.</param>
public sealed record HistoryRow(int Id, DateTime StartedAt);
