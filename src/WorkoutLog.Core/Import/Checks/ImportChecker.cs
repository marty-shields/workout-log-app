namespace WorkoutLog.Core.Import.Checks;

/// <summary>Runs every registered <see cref="IImportCheck"/>.</summary>
internal sealed class ImportChecker(IEnumerable<IImportCheck> checks) : IImportChecker
{
    private readonly IReadOnlyList<IImportCheck> checks = checks.ToList();

    /// <inheritdoc />
    public IReadOnlyList<ImportSkipReason> Check(ImportWorkout workout, ImportContext context) =>
        workout.Malformed
            ? [ImportSkipReason.Malformed]
            : checks.SelectMany(c => c.Check(workout, context)).Distinct().Order().ToList();
}
