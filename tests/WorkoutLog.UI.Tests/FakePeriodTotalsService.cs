using WorkoutLog.Core.Domain.Workouts.Periods;
using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Tests;

/// <summary>
/// <see cref="IPeriodTotalsService"/> for the tests: returns what the properties hold at once, <see cref="ThisWeek"/> for the period that holds today
/// and <see cref="LastWeek"/> for any other. Records what each call asked for.
/// </summary>
public sealed class FakePeriodTotalsService(TimeProvider time) : IPeriodTotalsService
{
    private static readonly Period Week = Period.Containing(PeriodKind.Week, new DateOnly(2026, 11, 16));

    public ReadResult<PeriodComparison> ThisWeek { get; set; } = new PeriodComparison(Week, new(0, TimeSpan.Zero, 0), new(0, TimeSpan.Zero, 0));

    public ReadResult<PeriodComparison> LastWeek { get; set; } = new PeriodComparison(Week.Previous(), new(0, TimeSpan.Zero, 0), new(0, TimeSpan.Zero, 0));

    public ReadResult<PeriodSeries> Series { get; set; } = new PeriodSeries(Week, [], [], [], 0);

    public ReadResult<WeekStreaks> Streaks { get; set; } = new WeekStreaks(0, new int[12], new LongestRun(0, false));

    /// <summary>The start of each period <see cref="GetComparisonAsync"/> was asked for.</summary>
    public List<DateOnly> Comparisons { get; } = [];

    public List<DateOnly> SeriesReads { get; } = [];

    public List<int> StreakYears { get; } = [];

    /// <inheritdoc />
    public Task<ReadResult<PeriodComparison>> GetComparisonAsync(Period period)
    {
        Comparisons.Add(period.Start);
        return Task.FromResult(period.Contains(DateOnly.FromDateTime(time.GetLocalNow().DateTime)) ? ThisWeek : LastWeek);
    }

    /// <inheritdoc />
    public Task<ReadResult<PeriodSeries>> GetSeriesAsync(Period period)
    {
        SeriesReads.Add(period.Start);
        return Task.FromResult(Series);
    }

    /// <inheritdoc />
    public Task<ReadResult<WeekStreaks>> GetStreaksAsync(int year)
    {
        StreakYears.Add(year);
        return Task.FromResult(Streaks);
    }

    /// <inheritdoc />
    public Task<ReadResult<PeriodVolume>> GetVolumeAsync(Period period) => throw new NotSupportedException();

    /// <inheritdoc />
    public Task<ReadResult<PeriodLimits>> GetLimitsAsync(PeriodKind kind) => throw new NotSupportedException();
}
