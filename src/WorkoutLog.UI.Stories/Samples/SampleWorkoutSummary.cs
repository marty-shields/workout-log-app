using WorkoutLog.UI.Models.Summary;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>Sample data for the Workout Summary stories and the UI tests.</summary>
public static class SampleWorkoutSummary
{
    private static readonly SummaryChange Squat = new(6, "Barbell Back Squat", "100kg × 5", SinceLastTime.Equal);
    private static readonly SummaryChange Curl = new(132, "Dumbbell Curl", "12kg × 10", SinceLastTime.Down("2kg"));
    private static readonly SummaryChange Plank = new(258, "Plank", "1:00", SinceLastTime.FirstTime);

    /// <summary>Two records, and one change of each kind: equal, up, down and first time. Up includes an assisted exercise, where less assist is up.</summary>
    public static WorkoutSummary RecordsAndChanges { get; } = new(
        new TimeSpan(1, 12, 4),
        6,
        17,
        [new SummaryRecord(8, "Barbell Bench Press", "90kg × 5"), new SummaryRecord(115, "Deadlift", "160kg × 3")],
        [
            Squat,
            new SummaryChange(8, "Barbell Bench Press", "90kg × 5", SinceLastTime.Up("2.5kg")),
            new SummaryChange(115, "Deadlift", "160kg × 3", SinceLastTime.Up("5kg")),
            new SummaryChange(218, "Machine Assisted Pull-up", "20kg × 8", SinceLastTime.Up("5kg")),
            Curl,
            Plank,
        ]);

    /// <summary>Changes but no records.</summary>
    public static WorkoutSummary NoRecords { get; } = new(new TimeSpan(0, 48, 20), 3, 8, [], [Squat, Curl, Plank]);

    /// <summary>The first workout: nothing to compare with.</summary>
    public static WorkoutSummary FirstWorkout { get; } = new(new TimeSpan(0, 52, 0), 3, 9, [], []);

    /// <summary>A workout of exercises not done before.</summary>
    public static WorkoutSummary AllFirstTime { get; } = new(
        new TimeSpan(0, 35, 40),
        3,
        6,
        [],
        [
            new SummaryChange(311, "Outdoor Run", "5.2km", SinceLastTime.FirstTime),
            new SummaryChange(187, "Farmer's Walk", "32kg · 40m", SinceLastTime.FirstTime),
            Plank,
        ]);
}
