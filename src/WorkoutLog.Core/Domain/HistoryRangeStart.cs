namespace WorkoutLog.Core.Domain;

/// <summary>First day of a <see cref="HistoryRange"/>, so the Exercise chart and the Workouts list agree on it.</summary>
public static class HistoryRangeStart
{
    /// <summary>
    /// <paramref name="today"/> minus 3, 6 or 12 calendar months, clamped to the end of a shorter month, e.g. 28 Feb from 31 May.
    /// Null for <see cref="HistoryRange.All"/>, which has no start.
    /// </summary>
    public static DateOnly? From(this HistoryRange range, DateOnly today) => range switch
    {
        HistoryRange.ThreeMonths => today.AddMonths(-3),
        HistoryRange.SixMonths => today.AddMonths(-6),
        HistoryRange.Year => today.AddMonths(-12),
        HistoryRange.All => null,
        _ => throw new ArgumentOutOfRangeException(nameof(range), range, null),
    };

    /// <summary>True when <paramref name="date"/> is from <see cref="From"/> to <paramref name="today"/>, both inclusive.</summary>
    public static bool Includes(this HistoryRange range, DateOnly today, DateOnly date)
    {
        var start = range.From(today);
        return date <= today && (start is not { } from || date >= from);
    }
}
