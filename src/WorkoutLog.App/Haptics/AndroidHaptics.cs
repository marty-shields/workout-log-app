using Android.Views;
using Microsoft.Extensions.Logging;
using WorkoutLog.App.Logging;
using WorkoutLog.UI.Haptics;

namespace WorkoutLog.App.Haptics;

/// <summary>
/// Plays each cue as the current activity's window performing Android's semantic touch feedback, which needs no VIBRATE permission
/// and does nothing when the user has turned touch feedback off. A failure is logged at Debug and otherwise ignored: haptics are
/// never worth an error.
/// </summary>
internal sealed partial class AndroidHaptics(ILogger<AndroidHaptics> logger) : IHaptics
{
    /// <inheritdoc />
    public void Play(HapticCue cue) => MainThread.BeginInvokeOnMainThread(() =>
    {
        try
        {
            Platform.CurrentActivity?.Window?.DecorView.PerformHapticFeedback(Feedback(cue));
        }
        catch (Exception exception)
        {
            Log.HapticFailed(logger, exception, cue);
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

    private static partial class Log
    {
        [LoggerMessage((int)AppEvent.HapticFailed, LogLevel.Debug, "Haptic feedback {Cue} failed")]
        public static partial void HapticFailed(ILogger logger, Exception exception, HapticCue cue);
    }
}
