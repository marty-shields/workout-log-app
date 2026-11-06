using WorkoutLog.Core.Domain;

namespace WorkoutLog.UI.Models.Workouts;

/// <summary>How many workouts started on each local day, for the Workouts screen's dates, segments and calendar.</summary>
public sealed class WorkoutHistory
{
    private readonly IReadOnlyDictionary<DateOnly, int> perDay;

    /// <param name="perDay">Workouts started on each day in the phone's time zone; days with none are left out.</param>
    /// <param name="today">Today in the phone's time zone.</param>
    public WorkoutHistory(IReadOnlyDictionary<DateOnly, int> perDay, DateOnly today)
    {
        this.perDay = perDay;
        Today = today;
        Count = perDay.Where(d => d.Key <= today).Sum(d => d.Value);
        IsEmpty = perDay.Count == 0;
        Earliest = perDay.Count == 0 ? today : DateOnly.FromDayNumber(Math.Min(perDay.Keys.Min().DayNumber, today.DayNumber));
    }

    /// <summary>No saved workouts.</summary>
    public static WorkoutHistory None { get; } = new(new Dictionary<DateOnly, int>(), DateOnly.MinValue);

    /// <summary>Today in the phone's time zone. Later days can't be picked.</summary>
    public DateOnly Today { get; }

    /// <summary>Day of the first saved workout, or <see cref="Today"/> when there are none. Earlier days can't be picked.</summary>
    public DateOnly Earliest { get; }

    /// <summary>Number of saved workouts up to <see cref="Today"/>, the most any span can show. A start after today, from a clock set wrong, is left out.</summary>
    public int Count { get; }

    /// <summary>True when no workout is saved.</summary>
    public bool IsEmpty { get; }

    /// <summary>True when a workout started on <paramref name="day"/>.</summary>
    public bool HasWorkoutOn(DateOnly day) => perDay.ContainsKey(day);

    /// <summary>Number of workouts started in the month.</summary>
    public int CountIn(int year, int month) => perDay.Where(d => d.Key.Year == year && d.Key.Month == month).Sum(d => d.Value);

    /// <summary>Number of workouts started in <paramref name="dates"/>.</summary>
    public int CountIn(DateSpan dates) => perDay.Where(d => dates.Contains(d.Key)).Sum(d => d.Value);

    /// <summary>Days <paramref name="range"/> covers: from its start, or <see cref="Earliest"/> for All, to <see cref="Today"/>.</summary>
    public DateSpan Span(HistoryRange range) => new(range.From(Today) ?? Earliest, Today);

    /// <summary>The range whose <see cref="Span"/> is <paramref name="dates"/>, or null for dates picked in the calendar.</summary>
    public HistoryRange? RangeOf(DateSpan dates) =>
        Enum.GetValues<HistoryRange>().Select(r => (HistoryRange?)r).FirstOrDefault(r => Span(r!.Value) == dates);
}
