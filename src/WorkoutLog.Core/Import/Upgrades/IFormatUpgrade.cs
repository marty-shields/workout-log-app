using System.Text.Json.Nodes;

namespace WorkoutLog.Core.Import.Upgrades;

/// <summary>A numbered export format upgrade step: changes a file of the previous format version into this one.</summary>
internal interface IFormatUpgrade
{
    /// <summary>Format version the step upgrades to. Steps start at 2.</summary>
    int Version { get; }

    /// <summary>Applies the step to the whole file, as left by the previous step.</summary>
    void Apply(JsonObject file);
}
