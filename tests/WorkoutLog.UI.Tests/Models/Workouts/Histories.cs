using WorkoutLog.UI.Models.Workouts;

namespace WorkoutLog.UI.Tests.Models.Workouts;

/// <summary>Builds a <see cref="WorkoutHistory"/> from rows, counting each by its local day as Core does.</summary>
public static class Histories
{
    public static WorkoutHistory Of(IReadOnlyList<WorkoutsRow> rows, DateOnly today) =>
        new(rows.CountBy(r => DateOnly.FromDateTime(r.StartedAt)).ToDictionary(), today);

    /// <summary>The rows in <paramref name="dates"/>, as the range read returns them.</summary>
    public static IReadOnlyList<WorkoutsRow> In(IReadOnlyList<WorkoutsRow> rows, DateSpan dates) =>
        [.. rows.Where(r => dates.Contains(DateOnly.FromDateTime(r.StartedAt)))];
}
