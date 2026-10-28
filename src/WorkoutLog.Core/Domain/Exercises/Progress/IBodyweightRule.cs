namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Works out a bodyweight session's chart values.</summary>
internal interface IBodyweightRule
{
    /// <summary>The chart values of <paramref name="session"/>.</summary>
    BodyweightSessionProgress Progress(StrengthSession session);
}
