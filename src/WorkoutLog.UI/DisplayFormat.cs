using System.Globalization;
using WorkoutLog.Core.Domain.Exercises;
using WorkoutLog.Core.Domain.Workouts.Periods;

namespace WorkoutLog.UI;

/// <summary>Text formats for dates, times and weights.</summary>
internal static class DisplayFormat
{
    /// <summary>Day and date, e.g. Tue 20 Oct 2026.</summary>
    public static string Date(DateTime value) => value.ToString("ddd d MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>Day and date, with the year only when it isn't <paramref name="today"/>'s, e.g. Tue 20 Oct or Wed 15 Oct 2025.</summary>
    public static string ShortDate(DateTime value, DateTime today) =>
        value.ToString(value.Year == today.Year ? "ddd d MMM" : "ddd d MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>Date, with the year only when it isn't <paramref name="today"/>'s, e.g. 20 Oct or 15 Oct 2025.</summary>
    public static string DayMonth(DateTime value, DateTime today) =>
        value.ToString(value.Year == today.Year ? "d MMM" : "d MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>Time as m:ss, or h:mm:ss from an hour, e.g. 1:45 or 1:02:30.</summary>
    public static string Clock(TimeSpan value) =>
        value.TotalHours >= 1 ? Duration(value) : $"{(int)value.TotalMinutes}:{value.Seconds:00}";

    /// <summary>Day, date and time, e.g. Tue 20 Oct 2026 · 18:05.</summary>
    public static string DateAndTime(DateTime value) => value.ToString("ddd d MMM yyyy · HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Day, date without the year, and time, e.g. Thu 5 Nov · 18:05.</summary>
    public static string DayAndTime(DateTime value) => value.ToString("ddd d MMM · HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Day and date without the year, e.g. Thu 12 Mar.</summary>
    public static string Day(DateOnly value) => value.ToString("ddd d MMM", CultureInfo.InvariantCulture);

    /// <summary>Day and date with a two-digit year, e.g. Thu 12 Mar 25.</summary>
    public static string DayShortYear(DateOnly value) => value.ToString("ddd d MMM yy", CultureInfo.InvariantCulture);

    /// <summary>Month and year, e.g. November 2026.</summary>
    public static string Month(int year, int month) => new DateOnly(year, month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>A number of workouts, e.g. 1 workout or 38 workouts.</summary>
    public static string Workouts(int count) => count == 1 ? "1 workout" : $"{count} workouts";

    /// <summary>A number of exercises, e.g. 1 exercise or 4 exercises.</summary>
    public static string Exercises(int count) => count == 1 ? "1 exercise" : $"{count} exercises";

    /// <summary>When an unfinished workout started, e.g. "Started yesterday, 6:12 pm" or "Started Wed 18 Nov, 6:12 pm".</summary>
    public static string Started(DateTime value, DateTime today) =>
        $"Started {(value.Date == today.Date ? "today" : value.Date == today.Date.AddDays(-1) ? "yesterday" : value.ToString("ddd d MMM", CultureInfo.InvariantCulture))}, "
        + value.ToString("h:mm tt", CultureInfo.InvariantCulture).ToLowerInvariant();

    /// <summary>A number of sets, e.g. 1 set or 12 sets.</summary>
    public static string Sets(int count) => count == 1 ? "1 set" : $"{count} sets";

    /// <summary>Calendar days since a date: Today, Yesterday, or e.g. 3 days ago.</summary>
    public static string DaysAgo(int days) => days switch
    {
        0 => "Today",
        1 => "Yesterday",
        _ => $"{days} days ago",
    };

    /// <summary>Elapsed time as h:mm:ss.</summary>
    public static string Duration(TimeSpan value) => $"{(int)value.TotalHours}:{value.Minutes:00}:{value.Seconds:00}";

    /// <summary>Whole minutes as e.g. 3h 05m, or 25m under an hour.</summary>
    public static string Minutes(int minutes) => minutes >= 60 ? $"{minutes / 60}h {minutes % 60:00}m" : $"{minutes}m";

    /// <summary>Elapsed time as h:mm, e.g. 0:58 or 1:02.</summary>
    public static string HoursMinutes(TimeSpan value) => $"{(int)value.TotalHours}:{value.Minutes:00}";

    /// <summary>What this week so far is compared with: Against Monday last week on a Monday, otherwise Against Monday to Wednesday last week.</summary>
    public static string AgainstLastWeek(DayOfWeek today) => today == DayOfWeek.Monday
        ? "Against Monday last week"
        : $"Against Monday to {today} last week";

    /// <summary>
    /// What this month so far is compared with, up to the same day of the month before: Against October to the 18th,
    /// with the year when it is not <paramref name="year"/>: Against December 2025 to the 18th.
    /// </summary>
    public static string AgainstLastMonth(DateOnly samePoint, int year) =>
        $"Against {samePoint.ToString(samePoint.Year == year ? "MMMM" : "MMMM yyyy", CultureInfo.InvariantCulture)} to the {Ordinal(samePoint.Day)}";

    /// <summary>What this year so far is compared with, up to the same date a year before: Against 2025 to 18 Nov.</summary>
    public static string AgainstLastYear(DateOnly samePoint) =>
        $"Against {samePoint.Year} to {samePoint.ToString("d MMM", CultureInfo.InvariantCulture)}";

    /// <summary>What a finished month is compared with: Against September, with the year when it is not <paramref name="year"/>.</summary>
    public static string AgainstMonthBefore(DateOnly month, int year) =>
        $"Against {month.ToString(month.Year == year ? "MMMM" : "MMMM yyyy", CultureInfo.InvariantCulture)}";

    /// <summary>A day of the month with its ordinal suffix, e.g. 1st, 12th or 23rd.</summary>
    public static string Ordinal(int day) => day + (day % 100 is >= 11 and <= 13 ? "th" : (day % 10) switch
    {
        1 => "st",
        2 => "nd",
        3 => "rd",
        _ => "th",
    });

    /// <summary>A Monday to Sunday week, without the year: 16 – 22 Nov, or 28 Sep – 4 Oct across months.</summary>
    public static string WeekDates(DateOnly monday)
    {
        var sunday = monday.AddDays(6);
        var from = monday.ToString(monday.Month == sunday.Month ? "%d" : "d MMM", CultureInfo.InvariantCulture);
        return $"{from} – {sunday.ToString("d MMM", CultureInfo.InvariantCulture)}";
    }

    /// <summary>A week of a month cut to the month, without the year: 2 – 8 Nov, or just 1 Nov when it is one day.</summary>
    public static string MonthWeekDates(DateOnly start, DateOnly last) => last == start
        ? start.ToString("d MMM", CultureInfo.InvariantCulture)
        : $"{start.Day} – {last.ToString("d MMM", CultureInfo.InvariantCulture)}";

    /// <summary>A year as its number, 2026, whatever the culture.</summary>
    public static string Year(int year) => year.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Weight rounded by <see cref="Decimals.Round2"/>, no trailing zeros and no space before the unit, e.g. 22.5kg; — when there is none.
    /// </summary>
    public static string Weight(decimal? kg) => kg is { } value ? $"{Decimals.Round2(value).ToString("0.##", CultureInfo.InvariantCulture)}kg" : "—";

    /// <summary>
    /// Volume for a week or month as whole kilograms with a comma between thousands, e.g. 9,840 kg; for a year as tonnes, e.g. 548 t.
    /// Both round halves away from zero (kg to the nearest kg, tonnes to the nearest tonne, so 548,500 kg is 549 t).
    /// </summary>
    public static string Volume(decimal kg, PeriodKind kind) =>
        kind == PeriodKind.Year
            ? $"{(kg / 1000m).ToString("N0", CultureInfo.InvariantCulture)} t"
            : $"{kg.ToString("N0", CultureInfo.InvariantCulture)} kg";

    /// <summary>
    /// Change from <paramref name="earlier"/> to <paramref name="current"/> as a percentage of <paramref name="earlier"/>, rounded to a whole
    /// percent with halves away from zero and signed, e.g. +6% or -12%; — when they are equal or <paramref name="earlier"/> is zero.
    /// </summary>
    public static string PercentChange(decimal current, decimal earlier)
    {
        if (current == earlier || earlier == 0)
        {
            return "—";
        }

        var percent = Math.Round((current - earlier) / earlier * 100m, MidpointRounding.AwayFromZero);
        return percent > 0 ? $"+{percent.ToString("0", CultureInfo.InvariantCulture)}%" : $"{percent.ToString("0", CultureInfo.InvariantCulture)}%";
    }
}
