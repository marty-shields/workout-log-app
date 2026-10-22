# 08 — Dev Setup & Build

## App identity
- Display name: **Workout Log**
- Android package id: **`uk.martinshields.workoutlog`**. This can't change later without a reinstall, which wipes all data.
- Debug builds: package id **`uk.martinshields.workoutlog.debug`**, name **Workout Log (Dev)**. They install alongside the release app with their own data, so USB debugging never touches real workouts.
- Version: display version is SemVer (`1.0.0`, `1.1.0`…). The version code goes up by 1 on every release APK.

## Prerequisites (Windows)
- .NET 10 SDK
- One of (D132):
  - **Visual Studio** with the **.NET Multi-platform App UI development** workload. It installs the MAUI workloads, the Android SDK (`C:\Program Files (x86)\Android\android-sdk`) and OpenJDK.
  - **VS Code** with the **.NET MAUI** extension (brings C# Dev Kit), plus `dotnet workload install maui-android`, the Android SDK (platform API 36, build tools, platform tools) and the JDK the .NET 10 Android workload needs.
- Optional: an Android emulator image for API 36 (needed for the Appium runs). Creating one needs admin rights when the SDK is in Program Files.
- For Appium (M6): Node.js, the Appium server, the UiAutomator2 driver, and a Chromedriver that matches the WebView version
- Git (local repo only)
- Optional: BlazingStory templates, to add story files with `dotnet new bstories`: `dotnet new install BlazingStory.ProjectTemplates::1.0.0-preview.96`

The stories need only the .NET 10 SDK. They don't need the MAUI workload, Android SDK or phone.

## Stories (design system and prototypes)
- `dotnet run --project src/WorkoutLog.UI.Stories`, then open http://localhost:5056
- Story files are `*.stories.razor` in `src/WorkoutLog.UI.Stories/Stories/`.
- Component tests: `dotnet test tests/WorkoutLog.UI.Tests`

## Phone setup (Android 16)
1. Settings → About phone → tap **Build number** 7 times to enable Developer options.
2. Developer options → turn on **USB debugging**.
3. Connect the phone by USB and accept the RSA prompt.
4. `adb devices` should list the phone.

Without USB, use wireless debugging (phone and PC on the same Wi-Fi):
1. Developer options → turn on **Wireless debugging**.
2. **Pair device with pairing code**, then on the PC: `adb pair <ip>:<pairing port> <code>`
3. `adb connect <ip>:<port>`, using the port on the main Wireless debugging screen. The port changes when wireless debugging restarts.

## Development: run on the phone
- Pick the phone as the target device in Visual Studio or VS Code and run/debug.
- Or from the CLI: `dotnet build src/WorkoutLog.App -t:Run -f net10.0-android`
- Debug builds install as "Workout Log (Dev)".

## Release: signed APK
1. Create a keystore once (`keytool -genkey ...`).
2. Set the signing properties in the app `.csproj` (`AndroidKeyStore`, `AndroidSigningKeyStore`, `AndroidSigningKeyAlias`). Passwords come from environment variables and are never stored in the repo.
3. Bump the version code (and the display version if needed).
4. `dotnet publish src/WorkoutLog.App -f net10.0-android -c Release -p:AndroidPackageFormat=apk`
5. Copy the signed `.apk` to the phone and install it (allow "install unknown apps" for the file manager).

**The same keystore must sign every future APK.** If it's lost, updates can't be installed over the existing app. The only way back is to uninstall, which wipes all workout data.

## Keystore handling
- The keystore file lives outside the repo and is listed in `.gitignore`.
- Keep a backup copy. **The location is an open item and must be decided before the first release APK.**

## Source control
- `git init` locally. No remote.
- `.gitignore` (T39): build output and logs, test results, IDE and per-user files (`*.user`), APKs, signing keys and secrets (`*.apk`, `*.aab`, `*.keystore`, `*.jks`, `*.pfx`, `*.p12`, `*.pem`, `*.key`, `.env`), tool output and OS files.
- `nuget.config` restores from nuget.org only (T40).
- Commit messages: Conventional Commits.
