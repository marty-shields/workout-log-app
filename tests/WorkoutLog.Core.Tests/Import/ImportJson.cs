using System.Globalization;
using System.Text;

namespace WorkoutLog.Core.Tests.Import;

/// <summary>Builds export file JSON for import tests.</summary>
internal static class ImportJson
{
    /// <summary>A Barbell Bench Press (8) with one set of 8 × 60 kg.</summary>
    public const string Bench = """{"exercise":{"id":8,"name":"Barbell Bench Press","type":"Strength"},"sets":[{"reps":8,"weightKg":60}]}""";

    /// <summary>An Outdoor Run (311): 30 minutes, 5.2 km, 5:46 per km.</summary>
    public const string Run =
        """{"exercise":{"id":311,"name":"Outdoor Run","type":"Cardio","distanceUnit":"Km","paceUnit":"PerKm"},"entry":{"duration":"00:30:00","distance":5.2,"pace":"00:05:46"}}""";

    /// <summary>Tuesday 21 Oct 2026, 17:05:00.1234567 UTC.</summary>
    public static readonly DateTimeOffset Start = new DateTimeOffset(2026, 10, 21, 17, 5, 0, TimeSpan.Zero).AddTicks(1234567);

    /// <summary>A file with the version 1 header and <paramref name="workouts"/>.</summary>
    public static string File(params string[] workouts) =>
        $$"""{"formatVersion":1,"exportedAt":"2026-10-26T18:05:12Z","appVersion":"1.4.0","workouts":[{{string.Join(",", workouts)}}]}""";

    /// <summary>A workout starting <paramref name="hours"/> after <see cref="Start"/> and finishing an hour later, with <paramref name="exercises"/>.</summary>
    public static string Workout(int hours, params string[] exercises) =>
        Workout(Time(Start.AddHours(hours)), Time(Start.AddHours(hours + 1)), exercises);

    /// <summary>A workout with the given time texts and <paramref name="exercises"/>.</summary>
    public static string Workout(string startedAt, string finishedAt, params string[] exercises) =>
        $$"""{"startedAt":"{{startedAt}}","finishedAt":"{{finishedAt}}","exercises":[{{string.Join(",", exercises)}}]}""";

    /// <summary>The export's text for <paramref name="time"/>.</summary>
    public static string Time(DateTimeOffset time) =>
        time.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture);

    /// <summary>A UTF-8 stream of <paramref name="json"/>.</summary>
    public static MemoryStream Stream(string json) => new(Encoding.UTF8.GetBytes(json));
}
