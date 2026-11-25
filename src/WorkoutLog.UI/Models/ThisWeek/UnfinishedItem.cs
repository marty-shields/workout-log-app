using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Models.ThisWeek;

/// <summary>The unfinished workout as the banner and its dialogs show it.</summary>
/// <param name="Started">When it started, e.g. "Started yesterday, 6:12 pm".</param>
/// <param name="Exercises">Number of exercises.</param>
/// <param name="Sets">Number of strength sets.</param>
public sealed record UnfinishedItem(string Started, int Exercises, int Sets)
{
    /// <summary>What discarding it loses, e.g. "4 exercises, 12 sets will be lost".</summary>
    public string Lost => $"{DisplayFormat.Exercises(Exercises)}, {DisplayFormat.Sets(Sets)} will be lost";

    /// <summary>The summary with its start on the phone's clock in <paramref name="zone"/>, as of <paramref name="now"/>.</summary>
    public static UnfinishedItem From(UnfinishedSummary summary, TimeZoneInfo zone, DateTimeOffset now) =>
        new(DisplayFormat.Started(TimeZoneInfo.ConvertTime(summary.StartedAt, zone).DateTime, TimeZoneInfo.ConvertTime(now, zone).DateTime), summary.Exercises, summary.Sets);
}
