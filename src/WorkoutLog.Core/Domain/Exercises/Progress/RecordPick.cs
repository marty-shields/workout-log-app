namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Picks a record from each session's value. Sessions with no value are left out; a tie takes the earliest start.</summary>
internal static class RecordPick
{
    /// <summary>The highest value, or null when no session has one.</summary>
    public static ProgressRecord<T>? Highest<TSession, T>(IEnumerable<TSession> sessions, Func<TSession, DateTimeOffset> start, Func<TSession, T?> value)
        where T : struct =>
        Values(sessions, start, value).OrderByDescending(r => r.Value).FirstOrDefault();

    /// <summary>The lowest value, or null when no session has one.</summary>
    public static ProgressRecord<T>? Lowest<TSession, T>(IEnumerable<TSession> sessions, Func<TSession, DateTimeOffset> start, Func<TSession, T?> value)
        where T : struct =>
        Values(sessions, start, value).OrderBy(r => r.Value).FirstOrDefault();

    /// <summary>The first value in <paramref name="order"/>, or null when no session has one.</summary>
    public static ProgressRecord<T>? First<TSession, T>(
        IEnumerable<TSession> sessions,
        Func<TSession, DateTimeOffset> start,
        Func<TSession, T?> value,
        Func<IEnumerable<ProgressRecord<T>>, IOrderedEnumerable<ProgressRecord<T>>> order)
        where T : class =>
        order(sessions.Select(s => (Value: value(s), Start: start(s)))
                .Where(v => v.Value is not null)
                .OrderBy(v => v.Start)
                .Select(v => new ProgressRecord<T>(v.Value!, v.Start)))
            .FirstOrDefault();

    private static IEnumerable<ProgressRecord<T>> Values<TSession, T>(IEnumerable<TSession> sessions, Func<TSession, DateTimeOffset> start, Func<TSession, T?> value)
        where T : struct =>
        sessions.Select(s => (Value: value(s), Start: start(s)))
            .Where(v => v.Value is not null)
            .OrderBy(v => v.Start)
            .Select(v => new ProgressRecord<T>(v.Value!.Value, v.Start));
}
