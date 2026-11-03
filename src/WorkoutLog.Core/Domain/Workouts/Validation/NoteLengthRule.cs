namespace WorkoutLog.Core.Domain.Workouts.Validation;

/// <summary>The length rule for a workout or exercise note, in UTF-16 code units after trimming.</summary>
public sealed class NoteLengthRule
{
    /// <summary>
    /// <see cref="NoteError.TooLong"/> when the trimmed note has more than <see cref="WorkoutLimits.MaxNoteLength"/> characters;
    /// otherwise null. A null, empty or whitespace note is no note and passes.
    /// </summary>
    public NoteError? Check(string? note) =>
        NoteText.Normalize(note) is { Length: > WorkoutLimits.MaxNoteLength } ? NoteError.TooLong : null;
}
