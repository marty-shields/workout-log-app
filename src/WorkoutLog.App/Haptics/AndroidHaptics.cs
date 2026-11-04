using Android.Views;
using WorkoutLog.UI.Haptics;

namespace WorkoutLog.App.Haptics;

/// <summary>
/// Plays each cue as the current activity's window performing Android's semantic touch feedback, which needs no VIBRATE permission
/// and does nothing when the user has turned touch feedback off. A failure is ignored: haptics are never worth an error.
/// </summary>
internal sealed class AndroidHaptics : IHaptics
{
    /// <inheritdoc />
    public void Play(HapticCue cue) => MainThread.BeginInvokeOnMainThread(() =>
    {
        try
        {
            Platform.CurrentActivity?.Window?.DecorView.PerformHapticFeedback(Feedback(cue));
        }
        catch (Exception)
        {
        }
    });

    /// <summary>
    /// SetLogged is the most frequent cue, so it is the light tick; Done and Save complete something, so they confirm;
    /// a blocked action is the distinct reject pattern.
    /// </summary>
    private static FeedbackConstants Feedback(HapticCue cue) => cue switch
    {
        HapticCue.SetLogged => FeedbackConstants.ContextClick,
        HapticCue.ExerciseDone => FeedbackConstants.Confirm,
        HapticCue.WorkoutSaved => FeedbackConstants.Confirm,
        HapticCue.Rejected => FeedbackConstants.Reject,
        _ => FeedbackConstants.NoHaptics,
    };
}
