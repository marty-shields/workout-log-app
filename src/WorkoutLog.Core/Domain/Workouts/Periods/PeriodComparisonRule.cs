using WorkoutLog.Core.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Domain.Workouts.Periods;

/// <summary>A workout is in the period of its local start date.</summary>
internal sealed class PeriodComparisonRule(WeekComparisonRule weekRule)
{
    /// <summary>
    /// Totals of the period and of the one before. A period that holds today is cut at today, the earlier one at the same point;
    /// any other period is compared whole to whole.
    /// </summary>
    public PeriodComparison Compare(IReadOnlyList<WorkoutTally> workouts, Period period, DateTimeOffset now, TimeZoneInfo zone)
    {
        var today = DateOnly.FromDateTime(LocalWeek.Local(now, zone));
        if (!period.Contains(today))
        {
            return new PeriodComparison(period, Tally(workouts, period.Start, period.End, zone), Tally(workouts, period.Previous().Start, period.Previous().End, zone));
        }

        if (period.Kind == PeriodKind.Week)
        {
            var weeks = weekRule.Compare(workouts, now, zone);
            return new PeriodComparison(period, weeks.ThisWeek, weeks.LastWeekToDate);
        }

        return new PeriodComparison(period, Tally(workouts, period.Start, today, zone), Tally(workouts, period.Previous().Start, period.SamePointBefore(today), zone));
    }

    private static WeekTally Tally(IReadOnlyList<WorkoutTally> workouts, DateOnly from, DateOnly to, TimeZoneInfo zone) =>
        WeekComparisonRule.Tally(workouts.Where(w => IsOn(w, from, to, zone)));

    private static bool IsOn(WorkoutTally workout, DateOnly from, DateOnly to, TimeZoneInfo zone)
    {
        var day = DateOnly.FromDateTime(LocalWeek.Local(workout.StartedAt, zone));
        return day >= from && day <= to;
    }
}
