namespace WorkoutLog.Core.Import.Checks;

/// <summary>What the checks know about the whole file.</summary>
/// <param name="StartCounts">How many workouts in the file have each valid start, malformed ones included.</param>
internal sealed record ImportContext(IReadOnlyDictionary<DateTimeOffset, int> StartCounts)
{
    /// <summary>Counts the valid starts of <paramref name="workouts"/>.</summary>
    public static ImportContext For(IEnumerable<ImportWorkout> workouts) =>
        new(workouts.Select(w => w.Start).OfType<DateTimeOffset>().CountBy(start => start).ToDictionary());
}
