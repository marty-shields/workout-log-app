using WorkoutLog.Core.Domain.Workouts.Consistency;

namespace WorkoutLog.Core.Domain.Workouts.Periods;

/// <summary>A workout is in the week of its local start date, and a week is in the year its Monday is in.</summary>
internal sealed class LongestRunRule
{
    /// <summary>
    /// The longest run of consecutive weeks with a workout among the weeks whose Monday is in <paramref name="year"/>; a run is cut at the
    /// year's ends. Of equal runs the later one counts.
    /// </summary>
    public LongestRun Longest(IEnumerable<DateTimeOffset> starts, int year, DateTimeOffset now, TimeZoneInfo zone)
    {
        var mondays = starts.Select(start => LocalWeek.Monday(start, zone)).Where(monday => monday.Year == year).Distinct().Order().ToList();
        var thisMonday = LocalWeek.Monday(now, zone);
        var best = new LongestRun(0, false);
        var length = 0;
        for (var i = 0; i < mondays.Count; i++)
        {
            length = i > 0 && mondays[i].DayNumber - mondays[i - 1].DayNumber == 7 ? length + 1 : 1;
            if (length >= best.Weeks)
            {
                best = new LongestRun(length, mondays[i] >= thisMonday.AddDays(-7));
            }
        }

        return best;
    }
}
