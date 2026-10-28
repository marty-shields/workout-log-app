namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>
/// Longest distance and duration, fastest (lowest) pace, most floors and calories, from each session's best values.
/// A tie across sessions takes the earliest start.
/// </summary>
internal sealed class CardioRecordsRule : ICardioRecordsRule
{
    /// <inheritdoc />
    public CardioRecords Records(IReadOnlyList<CardioSessionProgress> sessions)
    {
        var byStart = sessions.OrderBy(s => s.Session.StartedAt).ToList();
        return new CardioRecords(
            Highest(byStart, s => s.Best.Distance),
            Highest(byStart, s => s.Best.Duration),
            Lowest(byStart, s => s.Best.Pace),
            Highest(byStart, s => s.Best.Floors),
            Highest(byStart, s => s.Best.Calories));
    }

    private static ProgressRecord<T>? Highest<T>(List<CardioSessionProgress> byStart, Func<CardioSessionProgress, T?> value)
        where T : struct =>
        byStart.Where(s => value(s) is not null)
            .OrderByDescending(s => value(s))
            .Select(s => new ProgressRecord<T>(value(s)!.Value, s.Session.StartedAt))
            .FirstOrDefault();

    private static ProgressRecord<T>? Lowest<T>(List<CardioSessionProgress> byStart, Func<CardioSessionProgress, T?> value)
        where T : struct =>
        byStart.Where(s => value(s) is not null)
            .OrderBy(s => value(s))
            .Select(s => new ProgressRecord<T>(value(s)!.Value, s.Session.StartedAt))
            .FirstOrDefault();
}
