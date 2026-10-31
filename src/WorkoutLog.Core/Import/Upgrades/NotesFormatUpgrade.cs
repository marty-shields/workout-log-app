using System.Text.Json.Nodes;

namespace WorkoutLog.Core.Import.Upgrades;

/// <summary>Format upgrade step 3: changes nothing. Version 3 adds an optional <c>note</c> to a workout and to each of its exercises.</summary>
internal sealed class NotesFormatUpgrade : IFormatUpgrade
{
    /// <inheritdoc />
    public int Version => 3;

    /// <inheritdoc />
    public void Apply(JsonObject file)
    {
    }
}
