namespace WorkoutLog.Core.Domain.Exercises.Progress;

/// <summary>Decides whether a session is in a <see cref="ChartRange"/>.</summary>
internal interface IRangeFilter
{
    /// <summary>True when the session starting at <paramref name="startedAt"/> is in <paramref name="range"/>.</summary>
    bool Includes(DateTimeOffset startedAt, ChartRange range);
}
