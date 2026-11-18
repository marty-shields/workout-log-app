using WorkoutLog.UI.Components.Dashboard;
using WorkoutLog.UI.Components.ThisWeek;
using WorkoutLog.UI.Models.Muscles;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>Sample data for the This week stories and the UI tests. Today is Wed 18 Nov 2026; the week is Mon 16 to Sun 22 Nov.</summary>
public static class SampleThisWeek
{
    public static DateOnly Monday { get; } = new(2026, 11, 16);

    public static DateOnly Wednesday { get; } = new(2026, 11, 18);

    /// <summary>Monday to Wednesday, all trained.</summary>
    public static IReadOnlyList<DateOnly> TrainedDays { get; } = [Monday, Monday.AddDays(1), Wednesday];

    /// <summary>3 workouts, 2h 35m, 34 sets.</summary>
    public static WeekTotals ThisWeek { get; } = new(3, new TimeSpan(2, 35, 0), 34);

    /// <summary>Monday to Wednesday last week: 2 workouts, 2h, 33 sets.</summary>
    public static WeekTotals LastWeekToDate { get; } = new(2, new TimeSpan(2, 0, 0), 33);

    public static WeekTotals Nothing { get; } = new(0, TimeSpan.Zero, 0);

    /// <summary>All of last week: 4 workouts, 4h 10m, 67 sets.</summary>
    public static WeekTotals LastWeek { get; } = new(4, new TimeSpan(4, 10, 0), 67);

    /// <summary>Three workouts, oldest first.</summary>
    public static IReadOnlyList<WeekWorkout> Workouts { get; } =
    [
        new(31, new DateTime(2026, 11, 16, 18, 5, 0), ["Bench press", "Overhead press", "Lateral raise", "Tricep pushdown"], new TimeSpan(1, 2, 0)),
        new(32, new DateTime(2026, 11, 17, 18, 30, 0), ["Treadmill run"], new TimeSpan(0, 35, 0)),
        new(33, new DateTime(2026, 11, 18, 7, 10, 0), ["Squat", "Leg press", "Leg curl"], new TimeSpan(0, 58, 0)),
    ];

    /// <summary>One record.</summary>
    public static IReadOnlyList<WeekRecord> Records { get; } = [new(8, "Bench press", "82.5kg × 5", DayOfWeek.Monday)];

    /// <summary>Seven records, newest first.</summary>
    public static IReadOnlyList<WeekRecord> ManyRecords { get; } =
    [
        new(115, "Deadlift", "160kg × 3", DayOfWeek.Wednesday),
        new(22, "Squat", "120kg × 5", DayOfWeek.Wednesday),
        new(24, "Leg press", "240kg × 8", DayOfWeek.Wednesday),
        new(263, "Pull-up", "12 reps", DayOfWeek.Tuesday),
        new(41, "Treadmill run", "5.2 km", DayOfWeek.Tuesday),
        new(8, "Bench press", "82.5kg × 5", DayOfWeek.Monday),
        new(9, "Overhead press", "52.5kg × 5", DayOfWeek.Monday),
    ];

    /// <summary>Four groups trained, Back at zero.</summary>
    public static IReadOnlyList<MuscleGroupSets> Muscles { get; } =
        [new("Legs", 14), new("Chest", 9), new("Shoulders", 6), new("Arms", 3), new("Core", 2), new("Back", 0)];

    /// <summary>Every group trained.</summary>
    public static IReadOnlyList<MuscleGroupSets> AllTrained { get; } =
        [new("Legs", 14), new("Chest", 9), new("Shoulders", 6), new("Arms", 3), new("Core", 2), new("Back", 1)];

    /// <summary>No group trained.</summary>
    public static IReadOnlyList<MuscleGroupSets> NoneTrained { get; } =
        [new("Legs", 0), new("Chest", 0), new("Shoulders", 0), new("Arms", 0), new("Core", 0), new("Back", 0)];
}
