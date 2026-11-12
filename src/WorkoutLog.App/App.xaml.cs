namespace WorkoutLog.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Every screen's top bar sits under the status bar and is dark in both themes, so the status bar keeps light icons.
    /// MAUI 11's default follows the phone's theme instead, which draws dark icons on the bar when the phone is light.
    /// </summary>
    protected override Window CreateWindow(IActivationState? activationState) => new(new MainPage()) { StatusBarTheme = StatusBarTheme.Dark };
}
