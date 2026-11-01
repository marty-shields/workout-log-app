namespace WorkoutLog.Core.Import.Checks;

/// <summary>The workout has already happened: neither its start nor its finish is after now.</summary>
internal sealed class FutureCheck(TimeProvider time) : IImportCheck
{
    /// <inheritdoc />
    public IEnumerable<ImportSkipReason> Check(ImportWorkout workout, ImportContext context)
    {
        var now = time.GetUtcNow();
        return workout.Start > now || workout.Finish > now ? [ImportSkipReason.InFuture] : [];
    }
}
