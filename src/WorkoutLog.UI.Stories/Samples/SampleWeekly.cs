namespace WorkoutLog.UI.Stories.Samples;

/// <summary>Sample weekly counts for the WeekBars stories and the UI tests.</summary>
public static class SampleWeekly
{
    /// <summary>Twelve weeks with one empty week, ending in a 6-week streak.</summary>
    public static IReadOnlyList<int> WeekCounts { get; } = [2, 3, 1, 2, 3, 0, 3, 4, 2, 3, 2, 3];

    /// <summary>A workout every week.</summary>
    public static IReadOnlyList<int> FullWeeks { get; } = [2, 3, 1, 2, 3, 2, 3, 4, 2, 3, 2, 3];

    /// <summary>Twelve weeks with nothing in them.</summary>
    public static IReadOnlyList<int> EmptyWeeks { get; } = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

    /// <summary>One workout, this week.</summary>
    public static IReadOnlyList<int> FirstWeekCounts { get; } = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1];
}
