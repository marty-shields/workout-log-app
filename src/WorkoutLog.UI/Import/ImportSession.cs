using WorkoutLog.Core.Services;

namespace WorkoutLog.UI.Import;

/// <summary>Import state shared by Settings and Import Review. Held in memory only.</summary>
public sealed class ImportSession
{
    /// <summary>The file being reviewed, or null.</summary>
    public ImportReview? Review { get; set; }

    /// <summary>The finished import Settings shows next, or null.</summary>
    public ImportResult? Done { get; set; }

    /// <summary>Returns <see cref="Done"/> and clears it.</summary>
    public ImportResult? TakeDone()
    {
        var done = Done;
        Done = null;
        return done;
    }
}
