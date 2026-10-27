namespace WorkoutLog.Core.Import.Checks;

/// <summary>The workout has a valid <c>startedAt</c>.</summary>
internal sealed class StartCheck : IImportCheck
{
    /// <inheritdoc />
    public IEnumerable<ImportSkipReason> Check(ImportWorkout workout, ImportContext context) =>
        workout.Start is null ? [ImportSkipReason.InvalidStart] : [];
}
