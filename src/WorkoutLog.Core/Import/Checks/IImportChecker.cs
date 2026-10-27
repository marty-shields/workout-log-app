namespace WorkoutLog.Core.Import.Checks;

/// <summary>Checks a workout from the file against every import rule.</summary>
internal interface IImportChecker
{
    /// <summary>
    /// Returns every reason <paramref name="workout"/> can't be imported, each once in <see cref="ImportSkipReason"/> order; empty when it can.
    /// A malformed workout returns <see cref="ImportSkipReason.Malformed"/> alone.
    /// </summary>
    IReadOnlyList<ImportSkipReason> Check(ImportWorkout workout, ImportContext context);
}
