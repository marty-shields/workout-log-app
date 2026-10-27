using System.Text.Json.Serialization;

namespace WorkoutLog.Core.Export;

/// <summary>The export file.</summary>
/// <param name="FormatVersion">Format version.</param>
/// <param name="ExportedAt">When the file was made, UTC to the second.</param>
/// <param name="AppVersion">The app's display version.</param>
/// <param name="Workouts">Every saved workout, by start time oldest first.</param>
internal sealed record ExportFile(
    int FormatVersion,
    [property: JsonConverter(typeof(UtcSecondsConverter))] DateTimeOffset ExportedAt,
    string AppVersion,
    IReadOnlyList<ExportWorkout> Workouts);
