namespace WorkoutLog.Core.Import.Checks;

/// <summary>The workout has already happened: neither its start nor its finish is after now.</summary>
internal sealed class FutureCheck : IImportCheck
{
    /// <inheritdoc />
    public IEnumerable<ImportSkipReason> Check(ImportWorkout workout, ImportContext context) =>
        workout.Start > context.Now || workout.Finish > context.Now ? [ImportSkipReason.InFuture] : [];
}
