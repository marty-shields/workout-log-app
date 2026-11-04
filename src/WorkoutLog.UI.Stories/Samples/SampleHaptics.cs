using WorkoutLog.UI.Haptics;

namespace WorkoutLog.UI.Stories.Samples;

/// <summary>Haptics for the stories app: a browser has no touch feedback, so nothing plays.</summary>
public sealed class SampleHaptics : IHaptics
{
    /// <inheritdoc />
    public void Play(HapticCue cue)
    {
    }
}
