namespace WorkoutLog.Core.Import.Checks;

/// <summary>Runs every registered <see cref="IImportCheck"/>.</summary>
internal sealed class ImportChecker(IEnumerable<IImportCheck> checks)
{
    private readonly IReadOnlyList<IImportCheck> checks = checks.ToList();

    /// <summary>
    /// Returns every reason <paramref name="workout"/> can't be imported, each once in <see cref="ImportSkipReason"/> order; empty when it can.
    /// A malformed workout returns <see cref="ImportSkipReason.Malformed"/> alone.
    /// </summary>
    public IReadOnlyList<ImportSkipReason> Check(ImportWorkout workout, ImportContext context) =>
        workout.Malformed
            ? [ImportSkipReason.Malformed]
            : checks.SelectMany(c => c.Check(workout, context)).Distinct().Order().ToList();
}
