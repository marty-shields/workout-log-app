namespace WorkoutLog.Core.Import.Checks;

/// <summary>The workout has a valid <c>finishedAt</c>, after its start.</summary>
internal sealed class FinishCheck : IImportCheck
{
    /// <inheritdoc />
    public IEnumerable<ImportSkipReason> Check(ImportWorkout workout, ImportContext context) =>
        (workout.Start, workout.Finish) switch
        {
            (_, null) => [ImportSkipReason.InvalidFinish],
            ({ } start, { } finish) when finish <= start => [ImportSkipReason.FinishNotAfterStart],
            _ => [],
        };
}
