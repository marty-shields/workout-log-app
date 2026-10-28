using static WorkoutLog.Core.Domain.Exercises.Progress.RecordPick;

namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>
/// Longest distance and duration, fastest (lowest) pace, most floors and calories, from each session's best values.
/// A tie across sessions takes the earliest start.
/// </summary>
internal sealed class CardioRecordsRule : ICardioRecordsRule
{
    /// <inheritdoc />
    public CardioRecords Records(IReadOnlyList<CardioSessionProgress> sessions) => new(
        Highest(sessions, s => s.Session.StartedAt, s => s.Best.Distance),
        Highest(sessions, s => s.Session.StartedAt, s => s.Best.Duration),
        Lowest(sessions, s => s.Session.StartedAt, s => s.Best.Pace),
        Highest(sessions, s => s.Session.StartedAt, s => s.Best.Floors),
        Highest(sessions, s => s.Session.StartedAt, s => s.Best.Calories));
}
