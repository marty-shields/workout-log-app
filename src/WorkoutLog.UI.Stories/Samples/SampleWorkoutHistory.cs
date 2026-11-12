using WorkoutLog.Core.Services;
using WorkoutLog.UI.Models.Workouts;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>
/// Two years of workouts for the Workouts stories and the UI tests. Today is Fri 6 Nov 2026. Workouts run from Mon 4 Nov 2024,
/// mostly on Mondays, Wednesdays and Fridays, with two on Wed 12 Mar 2025 and none from 9 to 24 Aug 2025 or from 23 to 31 December.
/// </summary>
public static class SampleWorkoutHistory
{
    /// <summary>Fri 6 Nov 2026.</summary>
    public static readonly DateOnly Today = new(2026, 11, 6);

    /// <summary>Wed 12 Mar 2025, with a morning and an evening workout.</summary>
    public static readonly DateOnly TwoWorkoutsDay = new(2025, 3, 12);

    /// <summary>What the Workouts screen's totals card shows for the 3M span.</summary>
    public static readonly WorkoutTotals Totals = new(38, 612, new TimeSpan(32, 30, 0), 2.9);

    /// <summary>Every sample workout, newest first.</summary>
    public static IReadOnlyList<WorkoutsRow> Rows { get; } = Build();

    /// <summary>The sample workouts counted per day.</summary>
    public static WorkoutHistory History { get; } = new(Rows.CountBy(r => DateOnly.FromDateTime(r.StartedAt)).ToDictionary(), Today);

    /// <summary>March 2025, picked in the calendar.</summary>
    public static readonly DateSpan March2025 = new(new(2025, 3, 1), new(2025, 3, 31));

    /// <summary>Days in the August 2025 break, with no workouts.</summary>
    public static readonly DateSpan NoWorkouts = new(new(2025, 8, 11), new(2025, 8, 22));

    /// <summary>Mon 11 Aug 2025 alone, in the August break.</summary>
    public static readonly DateSpan RestDay = new(new(2025, 8, 11), new(2025, 8, 11));

    /// <summary>The sample workouts in <paramref name="dates"/>, newest first, as the page's range query returns them.</summary>
    public static IReadOnlyList<WorkoutsRow> In(DateSpan dates) => [.. Rows.Where(r => dates.Contains(DateOnly.FromDateTime(r.StartedAt)))];

    private static List<WorkoutsRow> Build()
    {
        var starts = new List<DateTime>();
        for (var day = new DateOnly(2024, 11, 4); day < Today; day = day.AddDays(1))
        {
            var away = (day.Year == 2025 && day.Month == 8 && day.Day is >= 9 and <= 24) || (day.Month == 12 && day.Day >= 23);
            var trains = day.DayOfWeek is DayOfWeek.Monday or DayOfWeek.Wednesday or DayOfWeek.Friday
                || (day.DayOfWeek == DayOfWeek.Saturday && day.DayNumber % 3 == 0);
            if (day != TwoWorkoutsDay && (away || !trains || day.DayNumber % 11 == 0))
            {
                continue;
            }

            if (day == TwoWorkoutsDay)
            {
                starts.Add(day.ToDateTime(new TimeOnly(7, 15)));
            }

            var morning = day.DayNumber % 4 == 0 && day != TwoWorkoutsDay;
            starts.Add(day.ToDateTime(new TimeOnly(morning ? 7 : 17 + (day.DayNumber % 2), day.DayNumber * 7 % 12 * 5)));
        }

        return [.. starts.OrderDescending().Select((start, i) => new WorkoutsRow(starts.Count - i, start))];
    }
}
