namespace WorkoutLog.Core.Import.Checks;

/// <summary>No other workout in the file has the same start.</summary>
internal sealed class RepeatedStartCheck : IImportCheck
{
    /// <inheritdoc />
    public IEnumerable<ImportSkipReason> Check(ImportWorkout workout, ImportContext context) =>
        workout.Start is { } start && context.StartCounts.GetValueOrDefault(start) > 1 ? [ImportSkipReason.RepeatedStart] : [];
}
