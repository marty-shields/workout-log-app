using WorkoutLog.Core.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Domain.Workouts.Periods;

/// <summary>A week (Monday to Sunday), calendar month or calendar year, by local date.</summary>
public sealed record Period
{
    private Period(PeriodKind kind, DateOnly start)
    {
        Kind = kind;
        Start = start;
    }

    public PeriodKind Kind { get; }

    /// <summary>The first day: a Monday, the 1st, or 1 January.</summary>
    public DateOnly Start { get; }

    /// <summary>The last day, inclusive.</summary>
    public DateOnly End => Next().Start.AddDays(-1);

    /// <summary>The period of <paramref name="kind"/> that holds <paramref name="day"/>.</summary>
    public static Period Containing(PeriodKind kind, DateOnly day) => new(kind, kind switch
    {
        PeriodKind.Week => LocalWeek.Monday(day),
        PeriodKind.Month => new DateOnly(day.Year, day.Month, 1),
        _ => new DateOnly(day.Year, 1, 1),
    });

    public Period Previous() => Containing(Kind, Start.AddDays(-1));

    public Period Next() => Kind switch
    {
        PeriodKind.Week => new Period(Kind, Start.AddDays(7)),
        PeriodKind.Month => new Period(Kind, Start.AddMonths(1)),
        _ => new Period(Kind, Start.AddYears(1)),
    };

    public bool Contains(DateOnly day) => day >= Start && day <= End;

    /// <summary>
    /// The day in the period before that is the same point in it as <paramref name="day"/> is in this one: the same weekday, the same day of
    /// the month or the same date. A day the earlier period lacks becomes its last day (31 Mar gives 28 Feb; 29 Feb gives 28 Feb).
    /// </summary>
    public DateOnly SamePointBefore(DateOnly day) => Kind switch
    {
        PeriodKind.Week => day.AddDays(-7),
        PeriodKind.Month => day.AddMonths(-1),
        _ => day.AddYears(-1),
    };
}
