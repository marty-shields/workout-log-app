using WorkoutLog.Core.Domain.Workouts.Periods;

namespace WorkoutLog.Core.Services;

/// <summary>Reads workouts, time and sets for any week, month or year, for This week and Trends. Days and weeks are local to the phone's time zone.</summary>
public interface IPeriodTotalsService
{
    /// <summary>Returns the period's totals against the period before, cut to the same point when the period holds today.</summary>
    Task<ReadResult<PeriodComparison>> GetComparisonAsync(Period period);

    /// <summary>Returns the weight lifted in the period against the period before, cut to the same point when the period holds today.</summary>
    Task<ReadResult<PeriodVolume>> GetVolumeAsync(Period period);

    /// <summary>Returns what the period's charts plot.</summary>
    Task<ReadResult<PeriodSeries>> GetSeriesAsync(Period period);

    /// <summary>Returns the current streak, the last 12 weekly counts and the longest run among weeks starting in <paramref name="year"/>.</summary>
    Task<ReadResult<WeekStreaks>> GetStreaksAsync(int year);

    /// <summary>Returns the earliest and latest period of <paramref name="kind"/> the stepper can reach.</summary>
    Task<ReadResult<PeriodLimits>> GetLimitsAsync(PeriodKind kind);
}
