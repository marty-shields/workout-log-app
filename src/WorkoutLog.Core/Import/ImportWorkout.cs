using System.Globalization;
using WorkoutLog.Core.Domain.Workouts;

namespace WorkoutLog.Core.Import;

/// <summary>A workout read from the file.</summary>
/// <param name="Position">Position in the file, from 1.</param>
/// <param name="StartedAt">The <c>startedAt</c> text, or null when it isn't a string.</param>
/// <param name="FinishedAt">The <c>finishedAt</c> text, or null when it isn't a string.</param>
/// <param name="Exercises">The exercises, or null when the workout couldn't be read.</param>
/// <param name="Note">The workout's note as in the file.</param>
internal sealed record ImportWorkout(int Position, string? StartedAt, string? FinishedAt, IReadOnlyList<ImportExercise>? Exercises, string? Note = null)
{
    private const string TimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'";

    /// <summary>True when the workout couldn't be read.</summary>
    public bool Malformed => Exercises is null;

    /// <summary>The start, or null when missing or not in the export's time format.</summary>
    public DateTimeOffset? Start => Parse(StartedAt);

    /// <summary>The finish, or null when missing or not in the export's time format.</summary>
    public DateTimeOffset? Finish => Parse(FinishedAt);

    /// <summary>The domain workout, with notes normalized by <see cref="NoteText"/>. Only for a workout that passed the import checks.</summary>
    public NewWorkout ToNewWorkout() => new(
        Start ?? throw new InvalidOperationException("The workout has no valid start"),
        Finish ?? throw new InvalidOperationException("The workout has no valid finish"),
        (Exercises ?? throw new InvalidOperationException("The workout is malformed")).Select(e => e.ToDomain()).ToList(),
        NoteText.Normalize(Note));

    private static DateTimeOffset? Parse(string? text) =>
        DateTimeOffset.TryParseExact(
            text, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var time)
            ? time
            : null;
}
