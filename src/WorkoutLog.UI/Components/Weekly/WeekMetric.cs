using System.Globalization;
using Shields.UI.Components.Lists;

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

    /// <summary>How a total moved, as the kit's tile shows it: up, down or none, with the size of the difference.</summary>
    internal static StatChange Change(int difference, Func<int, string> format) =>
        difference == 0
            ? new StatChange(ChangeDirection.None, "")
            : new StatChange(difference > 0 ? ChangeDirection.Up : ChangeDirection.Down, format(Math.Abs(difference)));

    internal static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);
}
