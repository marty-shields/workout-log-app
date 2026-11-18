using WorkoutLog.Core.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Domain.Workouts.Periods;

/// <summary>A workout is in the day, week, month or year of its local start date.</summary>
internal sealed class PeriodSeriesRule
{
    private const int DaysInWeek = 7;

    public PeriodSeries Build(IReadOnlyList<WorkoutTally> workouts, Period period, DateTimeOffset now, TimeZoneInfo zone)
    {
        var started = workouts.Select(w => (Day: DateOnly.FromDateTime(LocalWeek.Local(w.StartedAt, zone)), Workout: w)).ToList();
        return period.Kind switch
        {
            PeriodKind.Week => new PeriodSeries(period, Days(started, period), Days(started, period.Previous()), [], 0),
            PeriodKind.Month => new PeriodSeries(period, Days(started, period), [], WeeksIn(started, period), 0),
            _ => new PeriodSeries(period, Months(started, period), [], [], PerWeek(started, period, DateOnly.FromDateTime(LocalWeek.Local(now, zone)))),
        };
    }

    private static List<PeriodBucket> Days(List<(DateOnly Day, WorkoutTally Workout)> started, Period period) =>
        Enumerable.Range(0, period.End.DayNumber - period.Start.DayNumber + 1)
            .Select(offset => Bucket(started, period.Start.AddDays(offset), period.Start.AddDays(offset)))
            .ToList();

    private static List<PeriodBucket> WeeksIn(List<(DateOnly Day, WorkoutTally Workout)> started, Period month)
    {
        var weeks = new List<PeriodBucket>();
        for (var monday = LocalWeek.Monday(month.Start); monday <= month.End; monday = monday.AddDays(DaysInWeek))
        {
            weeks.Add(Bucket(started, monday < month.Start ? month.Start : monday, monday.AddDays(DaysInWeek - 1) > month.End ? month.End : monday.AddDays(DaysInWeek - 1)));
        }

        return weeks;
    }

    private static List<PeriodBucket> Months(List<(DateOnly Day, WorkoutTally Workout)> started, Period year) =>
        Enumerable.Range(1, 12)
            .Select(month => Period.Containing(PeriodKind.Month, new DateOnly(year.Start.Year, month, 1)))
            .Select(month => Bucket(started, month.Start, month.End))
            .ToList();

    private static double PerWeek(List<(DateOnly Day, WorkoutTally Workout)> started, Period year, DateOnly today)
    {
        var last = today < year.End ? today : year.End;
        if (last < year.Start)
        {
            return 0;
        }

        var days = last.DayNumber - year.Start.DayNumber + 1;
        return Bucket(started, year.Start, last).Workouts * (double)DaysInWeek / days;
    }

    private static PeriodBucket Bucket(List<(DateOnly Day, WorkoutTally Workout)> started, DateOnly from, DateOnly to)
    {
        var tally = WeekComparisonRule.Tally(started.Where(s => s.Day >= from && s.Day <= to).Select(s => s.Workout));
        return new PeriodBucket(from, to, tally.Workouts, tally.Time);
    }
}
