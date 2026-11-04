using WorkoutLog.UI.Haptics;

namespace WorkoutLog.UI.Tests;

/// <summary>Records each cue played, in order.</summary>
public sealed class FakeHaptics : IHaptics
{
    public List<HapticCue> Played { get; } = [];

    public void Play(HapticCue cue) => Played.Add(cue);
}
