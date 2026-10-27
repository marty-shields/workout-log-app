namespace WorkoutLog.Core.Import.Checks;

/// <summary>One import rule.</summary>
internal interface IImportCheck
{
    /// <summary>Returns why <paramref name="workout"/> breaks the rule, or nothing. Only called for a workout that isn't malformed.</summary>
    IEnumerable<ImportSkipReason> Check(ImportWorkout workout, ImportContext context);
}
