namespace WorkoutLog.Core.Domain.Workouts.Consistency;

/// <summary>Counts workouts per week for the Dashboard's bars.</summary>
internal interface IWeekBarsRule
{
    /// <summary>
    /// Workouts started in each of the last <see cref="LocalWeek.Shown"/> weeks in <paramref name="zone"/>, oldest first; the last is
    /// the week <paramref name="now"/> is in. Workouts outside those weeks are not counted.
    /// </summary>
    IReadOnlyList<int> Count(IReadOnlyList<WorkoutTally> workouts, DateTimeOffset now, TimeZoneInfo zone);
}
