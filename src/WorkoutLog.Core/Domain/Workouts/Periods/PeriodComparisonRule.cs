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

        var window = PeriodWindow.Of(period, today);
        return new PeriodComparison(period, Tally(workouts, window.From, window.To, zone), Tally(workouts, window.EarlierFrom, window.EarlierTo, zone));
    }

    private static WeekTally Tally(IReadOnlyList<WorkoutTally> workouts, DateOnly from, DateOnly to, TimeZoneInfo zone) =>
        WeekComparisonRule.Tally(workouts.Where(w => PeriodWindow.IsOn(w.StartedAt, from, to, zone)));
}
