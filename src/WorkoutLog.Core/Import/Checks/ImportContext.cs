namespace WorkoutLog.Core.Import.Checks;

/// <summary>What the checks know about the whole file.</summary>
/// <param name="StartCounts">How many workouts in the file have each valid start, malformed ones included.</param>
/// <param name="Now">The time every workout in the file is judged against.</param>
internal sealed record ImportContext(IReadOnlyDictionary<DateTimeOffset, int> StartCounts, DateTimeOffset Now)
{
    /// <summary>Counts the valid starts of <paramref name="workouts"/>, at <paramref name="now"/>.</summary>
    public static ImportContext For(IEnumerable<ImportWorkout> workouts, DateTimeOffset now) =>
        new(workouts.Select(w => w.Start).OfType<DateTimeOffset>().CountBy(start => start).ToDictionary(), now);
}
