using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Domain.Workouts.Periods;

/// <summary>Volume is weight times reps over a period's sets, in decimals so that sums carry no rounding drift.</summary>
internal sealed class VolumeRule
{
    /// <summary>
    /// A lift's weight as entered (a dumbbell's weight is not doubled) times its reps; a bodyweight move's added weight times its reps.
    /// Other kinds, and a set with no reps or no weight, add nothing.
    /// </summary>
    public decimal Of(ExerciseCategory category, int? reps, decimal? weightKg) =>
        category is ExerciseCategory.Lift or ExerciseCategory.Bodyweight && reps is > 0 && weightKg is > 0 ? weightKg.Value * reps.Value : 0m;

    /// <summary>
    /// Volume of the period and of the one before. A period that holds today is cut at today, the earlier one at the same point;
    /// any other period is compared whole to whole. A set is in the period of its workout's local start date.
    /// </summary>
    public PeriodVolume Compare(IReadOnlyList<SetVolume> sets, Period period, DateTimeOffset now, TimeZoneInfo zone)
    {
        var window = PeriodWindow.Of(period, DateOnly.FromDateTime(LocalWeek.Local(now, zone)));
        return new PeriodVolume(period, Sum(sets, window.From, window.To, zone), Sum(sets, window.EarlierFrom, window.EarlierTo, zone));
    }

    private static decimal Sum(IReadOnlyList<SetVolume> sets, DateOnly from, DateOnly to, TimeZoneInfo zone) =>
        sets.Where(s => PeriodWindow.IsOn(s.StartedAt, from, to, zone)).Sum(s => s.Kg);
}
