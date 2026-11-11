using WorkoutLog.UI.Keyboard;

namespace WorkoutLog.UI.Tests.Keyboard;

public class KeyboardAtPauseTests
{
    [TestCase(true, true, true)]
    [TestCase(true, false, false)]
    [TestCase(false, true, false)]
    [TestCase(false, false, false)]
    public void ShowOnResume_IsTrueOnlyForAShownKeyboardOverAFocusedInput(bool shown, bool focused, bool expected) =>
        Assert.That(new KeyboardAtPause(shown, focused).ShowOnResume, Is.EqualTo(expected));
}
