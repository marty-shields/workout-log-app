namespace WorkoutLog.Core.Domain.Workouts;

/// <summary>The stored form of a workout or exercise note.</summary>
internal static class NoteText
{
    /// <summary>The note trimmed; null when it is null, empty or whitespace.</summary>
    public static string? Normalize(string? note) => string.IsNullOrWhiteSpace(note) ? null : note.Trim();
}
