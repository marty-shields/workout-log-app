namespace WorkoutLog.UI.Models.Summary;

/// <summary>An exercise's best in a workout against its last session before it. Up is better and Down is worse, so for an assisted exercise less assist is Up.</summary>
public sealed record SinceLastTime
{
    private SinceLastTime(ChangeKind kind, string? difference)
    {
        Kind = kind;
        Difference = difference;
    }

    /// <summary>The exercise has no earlier session.</summary>
    public static SinceLastTime FirstTime { get; } = new(ChangeKind.FirstTime, null);

    /// <summary>The best is the same as last time.</summary>
    public static SinceLastTime Equal { get; } = new(ChangeKind.Equal, null);

    /// <summary>How the best compares.</summary>
    public ChangeKind Kind { get; }

    /// <summary>How much better or worse, e.g. 2.5kg. Null unless <see cref="Kind"/> is Up or Down.</summary>
    public string? Difference { get; }

    /// <summary>A best better than last time by <paramref name="difference"/>.</summary>
    public static SinceLastTime Up(string difference) => new(ChangeKind.Up, Required(difference));

    /// <summary>A best worse than last time by <paramref name="difference"/>.</summary>
    public static SinceLastTime Down(string difference) => new(ChangeKind.Down, Required(difference));

    private static string Required(string difference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(difference);
        return difference;
    }
}
