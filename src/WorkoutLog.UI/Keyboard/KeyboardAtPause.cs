namespace WorkoutLog.UI.Keyboard;

/// <summary>What was true of the soft keyboard when the app went to the background.</summary>
/// <param name="KeyboardShown">The keyboard was on screen. Android back hides it and leaves the focus.</param>
/// <param name="TextInputFocused">A text input held the focus.</param>
public readonly record struct KeyboardAtPause(bool KeyboardShown, bool TextInputFocused)
{
    /// <summary>Whether the keyboard should come back when the app does. A hidden keyboard stays hidden.</summary>
    public bool ShowOnResume => KeyboardShown && TextInputFocused;
}
