using WorkoutLog.UI.Components.Dashboard;
using WorkoutLog.UI.Models.Muscles;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>Sample data for the Dashboard cards and Sets per muscle group stories, and the UI tests. Today is Fri 30 Oct 2026.</summary>
public static class SampleDashboard
{
    private static readonly string[] Exercises = ["Barbell Bench Press", "Cable Fly", "Dumbbell Incline Press", "Cable Tricep Pushdown", "Dumbbell Lateral Raise"];

    /// <summary>A workout on Tue 27 Oct 2026, 1:12:04 long, with five exercises.</summary>
    public static LastWorkout LastWorkout { get; } = Workout(3, 5);

    /// <summary>A workout <paramref name="daysAgo"/> days before today with the first <paramref name="exercises"/> sample exercises.</summary>
    public static LastWorkout Workout(int daysAgo, int exercises) =>
        new(new DateTime(2026, 10, 30, 18, 5, 0).AddDays(-daysAgo), daysAgo, new TimeSpan(1, 12, 4), Exercises[..exercises]);

    /// <summary>This week so far: 3 workouts, 3h 05m, 42 sets.</summary>
    public static WeekTotals ThisWeek { get; } = new(3, new TimeSpan(3, 5, 0), 42);

    /// <summary>Last week up to the same point: 2 workouts, 2h 40m, 45 sets.</summary>
    public static WeekTotals LastWeek { get; } = new(2, new TimeSpan(2, 40, 0), 45);

    /// <summary>A week with nothing in it.</summary>
    public static WeekTotals NoWeek { get; } = new(0, TimeSpan.Zero, 0);

    /// <summary>The first workout, this week: 1 workout, 52m, 16 sets.</summary>
    public static WeekTotals FirstWeek { get; } = new(1, new TimeSpan(0, 52, 0), 16);

    /// <summary>Twelve weeks with one empty week, ending in a 6-week streak.</summary>
    public static IReadOnlyList<int> WeekCounts { get; } = [2, 3, 1, 2, 3, 0, 3, 4, 2, 3, 2, 3];

    /// <summary>A workout every week.</summary>
    public static IReadOnlyList<int> FullWeeks { get; } = [2, 3, 1, 2, 3, 2, 3, 4, 2, 3, 2, 3];

    /// <summary>Twelve weeks with nothing in them.</summary>
    public static IReadOnlyList<int> EmptyWeeks { get; } = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

    /// <summary>One workout, this week.</summary>
    public static IReadOnlyList<int> FirstWeekCounts { get; } = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1];

    /// <summary>Three records, newest first.</summary>
    public static IReadOnlyList<RecentRecord> Records { get; } =
    [
        new(115, "Deadlift", "160kg × 3", 2),
        new(8, "Barbell Bench Press", "90kg × 5", 5),
        new(263, "Pull-up", "12 reps", 9),
    ];

    /// <summary>Some groups trained in the last 7 days, some not.</summary>
    public static IReadOnlyList<MuscleGroupSets> SomeTrained { get; } = Groups(12, 9, 0, 1, 0, 4);

    /// <summary>Every group trained in the last 7 days.</summary>
    public static IReadOnlyList<MuscleGroupSets> AllTrained { get; } = Groups(12, 9, 6, 8, 15, 4);

    /// <summary>Nothing in the last 7 days.</summary>
    public static IReadOnlyList<MuscleGroupSets> NoneTrained { get; } = Groups(0, 0, 0, 0, 0, 0);

    private static IReadOnlyList<MuscleGroupSets> Groups(int chest, int back, int shoulders, int arms, int legs, int core) =>
        [new("Chest", chest), new("Back", back), new("Shoulders", shoulders), new("Arms", arms), new("Legs", legs), new("Core", core)];
}
