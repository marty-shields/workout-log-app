namespace WorkoutLog.UI.Haptics;

/// <summary>Plays a <see cref="HapticCue"/> as the phone's touch feedback. Implemented by the host; never throws.</summary>
public interface IHaptics
{
    /// <summary>Plays <paramref name="cue"/>, or nothing when the phone's touch feedback is off.</summary>
    void Play(HapticCue cue);
}
