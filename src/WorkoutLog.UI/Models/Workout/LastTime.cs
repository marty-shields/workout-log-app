namespace WorkoutLog.UI.Models.Workout;

/// <summary>
/// What an exercise's last saved workout had, as text, shown beside the sets or entry being logged. Display only: never copied into a value.
/// </summary>
/// <param name="Entries">
/// One text per set, or the one cardio entry, in order. Sets as the Exercise page shows them, e.g. 60×8, 1:30 or 32kg · 40m;
/// an entry's values joined by " · ", e.g. 5.2km · 30:00.
/// </param>
public sealed record LastTime(IReadOnlyList<string> Entries)
{
    /// <summary>The text of set (or entry) <paramref name="number"/>, from 1; null when last time had no such set.</summary>
    public string? EntryOrNull(int number) => number >= 1 && number <= Entries.Count ? Entries[number - 1] : null;

    /// <summary>The text of set (or entry) <paramref name="number"/>, from 1; — when last time had no such set, to keep a Last column aligned.</summary>
    public string Entry(int number) => EntryOrNull(number) ?? "—";
}
