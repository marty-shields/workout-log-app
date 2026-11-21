using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity;
using Shields.UI.Lifecycle;
using Shields.UI.Navigation;

namespace WorkoutLog.App;

/// <summary>
/// Passes the activity's create, start and destroy to the kit's bridges, which hand them to the BlazorWebView's back button and app lifecycle once <c>HostBridges</c> connects them.
/// Without a page back handler the back callback is off and Android handles back: the app closes.
/// </summary>
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private BackButtonBridge backBridge = null!;
    private AppLifecycleBridge lifecycleBridge = null!;
    private BackCallback backCallback = null!;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        var services = IPlatformApplication.Current!.Services;
        backBridge = services.GetRequiredService<BackButtonBridge>();
        lifecycleBridge = services.GetRequiredService<AppLifecycleBridge>();
        backBridge.Attach(this);
        lifecycleBridge.Attach(this);

        backCallback = new BackCallback(this, backBridge);
        OnBackPressedDispatcher.AddCallback(this, backCallback);
        backBridge.EnabledChanged += SyncBackCallback;
        SyncBackCallback();
    }

    protected override void OnStart()
    {
        base.OnStart();
        _ = lifecycleBridge.StartedAsync();
    }

    protected override void OnDestroy()
    {
        backBridge.EnabledChanged -= SyncBackCallback;
        backBridge.Detach(this);
        lifecycleBridge.Detach(this);
        base.OnDestroy();
    }

    private void SyncBackCallback() => RunOnUiThread(() => backCallback.Enabled = backBridge.Enabled);

    // HandleAsync is null when no page handles back: switch the callback off and let Android handle it.
    private sealed class BackCallback(ComponentActivity activity, BackButtonBridge bridge) : OnBackPressedCallback(false)
    {
        public override void HandleOnBackPressed()
        {
            if (bridge.HandleAsync() is null)
            {
                Enabled = false;
                activity.OnBackPressedDispatcher.OnBackPressed();
            }
        }
    }
}
