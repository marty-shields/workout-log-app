# 08 — Dev Setup & Build

## App identity
- Display name: **Workout Log**
- Android package id: **`uk.martinshields.workoutlog`**. This can't change later without a reinstall, which wipes all data.
- Debug builds: package id **`uk.martinshields.workoutlog.debug`**, name **Workout Log (Dev)**. They install alongside the release app with their own data, so USB debugging never touches real workouts.
- Version: display version is SemVer (`1.0.0`, `1.1.0`…). The version code goes up by 1 on every release APK.

## Prerequisites (Windows)
- .NET 10 SDK
- MAUI workload: `dotnet workload install maui-android`
- Android SDK with platform API 36, plus build tools and platform tools (`adb`)
- JDK (the version required by the .NET 10 Android workload)
- VS Code with the **.NET MAUI** extension (brings C# Dev Kit)
- Optional: an Android emulator image for API 36 (needed for the Appium runs)
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

## Development: run on the phone over USB
- Pick the phone as the target device in VS Code and run/debug.
- Or from the CLI: `dotnet build -t:Run -f net10.0-android`

## Release: signed APK
1. Create a keystore once (`keytool -genkey ...`).
2. Set the signing properties in the app `.csproj` (`AndroidKeyStore`, `AndroidSigningKeyStore`, `AndroidSigningKeyAlias`). Passwords come from environment variables and are never stored in the repo.
3. Bump the version code (and the display version if needed).
4. `dotnet publish -f net10.0-android -c Release -p:AndroidPackageFormat=apk`
5. Copy the signed `.apk` to the phone and install it (allow "install unknown apps" for the file manager).

**The same keystore must sign every future APK.** If it's lost, updates can't be installed over the existing app. The only way back is to uninstall, which wipes all workout data.

## Keystore handling
- The keystore file lives outside the repo and is listed in `.gitignore`.
- Keep a backup copy. **The location is an open item and must be decided before the first release APK.**

## Source control
- `git init` locally. No remote.
- `.gitignore`: `bin/`, `obj/`, `.vs/`, `TestResults/`, `*.keystore`, `*.jks`
- Commit messages: Conventional Commits.
