namespace WorkoutLog.Core.Domain.Workouts.Validation;

/// <summary>Why a workout or exercise note can't be saved.</summary>
public enum NoteError
{
    /// <summary>More than <see cref="WorkoutLimits.MaxNoteLength"/> characters after trimming.</summary>
    TooLong,
}
