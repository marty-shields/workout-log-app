using System.Globalization;

namespace WorkoutLog.UI.Components.Weekly;

/// <summary>A total compared between two weeks: its label, the testid suffix, its value in whole units and how to show it.</summary>
internal sealed record WeekMetric(string Label, string Key, Func<WeekTotals, int> Value, Func<int, string> Format)
{
    public static readonly WeekMetric[] All =
    [
        new("Workouts", "workouts", w => w.Workouts, Count),
        new("Time", "time", w => (int)w.Time.TotalMinutes, DisplayFormat.Minutes),
        new("Sets", "sets", w => w.Sets, Count),
    ];

    internal static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);
}
