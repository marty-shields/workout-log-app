namespace WorkoutLog.Core.Import.Checks;

/// <summary>Why a workout in the file can't be imported.</summary>
internal enum ImportSkipReason
{
    /// <summary>The workout couldn't be read.</summary>
    Malformed,

    /// <summary><c>startedAt</c> is missing or not a valid time.</summary>
    InvalidStart,

    /// <summary>Another workout in the file has the same start.</summary>
    RepeatedStart,

    /// <summary><c>finishedAt</c> is missing or not a valid time.</summary>
    InvalidFinish,

    /// <summary>The finish is before the start.</summary>
    FinishBeforeStart,

    /// <summary>An exercise id is not in the exercise list.</summary>
    UnknownExercise,

    /// <summary>An exercise's type or units differ from the exercise list.</summary>
    ExerciseMismatch,

    /// <summary>The workout fails Save's checks (FR-09).</summary>
    FailsSaveRules,
}
