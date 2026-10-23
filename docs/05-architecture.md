# 05 — Architecture

## Stack
| Area | Choice |
|---|---|
| Runtime | .NET 10 (LTS) |
| App | .NET MAUI Blazor Hybrid (Razor components in `BlazorWebView`) |
| Platform | Android only, minSdk = targetSdk = API 36 (Android 16) |
| Database | SQLite via sqlite-net-pcl |
| State | Blazor components + injected services. No MVVM toolkit. |
| Tests | NUnit, Moq, AutoFixture, bUnit, Appium (see [07](07-testing.md)) |

## App identity
| Item | Value |
|---|---|
| Display name | Workout Log (Debug builds: Workout Log (Dev)) |
| Android package id | `uk.martinshields.workoutlog` (Debug builds: `uk.martinshields.workoutlog.debug`) |

## Solution layout
```
WorkoutLog.slnx
├─ src/
│  ├─ WorkoutLog.App/          MAUI Blazor Hybrid host (net10.0-android)
│  │   ├─ wwwroot/             index.html
│  │   └─ MauiProgram.cs       DI registration
│  ├─ WorkoutLog.UI/           Razor Class Library (net10.0): pages and components
│  │   └─ wwwroot/css/         Design tokens and shared CSS ([14](14-design-system.md))
│  ├─ WorkoutLog.UI.Stories/   BlazingStory WebAssembly app (net10.0): stories for UI components
│  └─ WorkoutLog.Core/         Class library (net10.0)
│      ├─ Domain/              Records/entities, metrics, exercise definitions, validation
│      ├─ Data/                sqlite-net repositories, schema, seeding
│      ├─ Logging/             File log provider, shared log line format
│      └─ Services/            Workout, history, dashboard services
└─ tests/
   ├─ WorkoutLog.Core.Tests/       NUnit + Moq + AutoFixture
   ├─ WorkoutLog.UI.Tests/         bUnit
   └─ WorkoutLog.E2E.Tests/        NUnit + Appium
```
References: App → UI → Core. UI.Stories → UI. UI.Tests → UI, UI.Stories (to play the prototype scenarios). The App never references UI.Stories.

## Layers
```mermaid
flowchart LR
    A[App host<br/>WorkoutLog.App] --> UI[Razor components<br/>WorkoutLog.UI]
    UI --> S[Services<br/>Core]
    S --> R[Repositories<br/>Core]
    R --> DB[(SQLite file<br/>app data dir)]
    S --> C[TimeProvider<br/>.NET]
    UI --> C
    S --> X[Exercise definitions<br/>Core]
```
- **App** hosts the BlazorWebView, registers DI and the logging providers, and handles the Android back button.
- **UI** renders and calls services only. It never touches the database. Form fields come from the exercise's metric definition.
- **Services** hold the rules: save validation, start/finish capture, dashboard counts.
- **Repositories** handle sqlite-net access. They sit behind interfaces so services can be unit-tested with Moq.
- **TimeProvider** (.NET) supplies the current time, so start/finish, the live timer and week/month counts can be tested with `FakeTimeProvider` (T31). The UI uses it for the live timer (T47).
- **Exercise definitions** are an embedded JSON file in Core, read into the `IExerciseCatalog` singleton on first use: the seeder, at startup (T45). The catalog also runs the picker search in memory (see [03](03-data-model.md#exercise-search), [04](04-exercise-list.md)).

## Dependency injection
Everything is registered in `MauiProgram.CreateMauiApp()` using the built-in `IServiceCollection`. Core registers its own types through `AddCore(databasePath)` (T19); the App passes `workoutlog.db3` in `FileSystem.AppDataDirectory`. Of the services, only `IWorkoutService` is public; the repository and validator are internal (T33). The database connection is an `SQLiteAsyncConnection` singleton. Services and repositories are registered by interface. At startup the App's `Routes` component wraps the router in `StartupGate`, which awaits `IDatabaseInitializer.InitializeAsync()` and shows the Startup screen until it completes, or its error if it throws (T36, D133). The initializer upgrades and creates the schema, then seeds the exercise list (T25, T28). The initializer and repository are decorators that run each call through one `DatabaseGate`, so database calls run one at a time (T27). Logging providers are registered in `MauiProgram` too ([Logging](#logging)). So are the `IBackButton` and `AndroidBackButton` singletons ([Navigation](#navigation)); the stories app registers `IBackButton` as well. UI page components such as `NewWorkoutPage` inject `IWorkoutService`, `IExerciseCatalog` and `TimeProvider`; the stories app registers `TimeProvider.System`, `SampleWorkoutService` and `ExerciseCatalog` for them (T48, T50).

## Navigation
Blazor routing (`@page`). `/workout/{id}` is proposed; the others are in use:
| Route | Screen |
|---|---|
| `/` | Dashboard |
| `/workout/new` | New workout (T47) |
| `/workout/{id}` | Edit workout |
| `/history` | History |

The Exercise picker is an overlay on the workout page, not a route.

The Android back button is passed to the current page:
- Workout pages: close the picker if it's open; otherwise show the Discard / Cancel prompt (FR-12, FR-23).
- History: go to the Dashboard.
- Dashboard: leave the app.

How (T46):
- `BackHandler` (UI) registers a handler with `IBackButton` while its `OnBack` is set. `AppScreen` uses it for `OnBack`, `ExercisePicker` for `OnClose` and `ConfirmDialog` for `OnCancel`, so Android back does what the on-screen back or Cancel does (D144). The newest registration runs, so the picker or a dialog takes back over its page.
- A handler that throws shows the Error screen, as a click would (D140).
- `AndroidBackButton` (App) adds an AndroidX `OnBackPressedCallback` when the activity is created. It is on only while a handler is registered, so Android knows before the gesture starts whether the app takes back. When the activity is destroyed it clears every handler, as the pages in it are not disposed.
- With no handler (the Dashboard has no `OnBack`), the callback is off and the default runs: the app closes and the activity is destroyed (D143).
- The BlazorWebView takes back while the WebView can go back in its history, so the App navigates with `GoTo` (`replace: true`), has no links, and never builds a history.

## Start / finish capture
- `NewWorkoutPage` (UI) asks `IWorkoutService.StartNew()` for a `WorkoutDraft`. The service stamps `StartedAt` (UTC) from `TimeProvider` (T29) and logs the start.
- The page shows the date in `TimeProvider.LocalTimeZone` and an elapsed timer that ticks every second from `TimeProvider`. While the clock is before `StartedAt` it shows 0:00:00 (T47, D145).
- The draft stays in memory only (FR-12: lost if the app is killed). Android may kill the app while the phone is locked; this is accepted (D61).
- On the first save, the service validates (FR-09), stamps `FinishedAt` and writes everything in one transaction. If the clock is earlier than `StartedAt`, `FinishedAt` is set to `StartedAt` (D129).
- Later saves update the exercises, sets and entries only. They never change the times.

## Error handling
- Validation failures are shown inline on the page. Nothing is written. The service returns error codes; the page words them (T30).
- Database errors are shown as a simple error message ("Couldn't save the workout. Try again."). The service turns any repository exception into `WorkoutError.SaveFailed` (T32) and logs it. The draft stays on screen.
- An unhandled error in a screen shows the Error screen: "Something went wrong." and **Restart**, which goes to the Dashboard. `AppErrorBoundary` in the App's `Routes` shows it and logs the error (D140).
- Unhandled .NET exceptions are logged, then the app crashes. Unobserved task exceptions are logged only (T44).

## Logging
Every story logs its errors and unexpected events (D141). The Definition of Done checks this ([12](12-product-goal.md)).

**How**
- Core and UI inject `ILogger<T>` and never know where logs go. Each host registers its own `ILoggerProvider`s and levels in its startup (T41).
- Each logging type has a nested `static partial class Log` of `[LoggerMessage]` methods, each with a fixed EventId and level (T43). EventIds: Core 1000+, UI 2000+, App 3000+, a block of 100 per type. In a Razor component the `Log` class goes in `<Component>.razor.cs`.
- Tests check each event with `FakeLogger` (Microsoft.Extensions.Diagnostics.Testing): EventId, level, message and exception ([07](07-testing.md)).

**What to log**
| Level | When |
|---|---|
| Critical | The app is about to crash |
| Error | An operation failed: an exception caught and turned into an error code or error screen. Pass the exception |
| Warning | Something unexpected that the app works around (e.g. the clock before the start, D129) |
| Information | A milestone: startup steps, a workout saved or deleted, lifecycle events |
| Debug | Detail for a debugging session |

**Never log** set values, entry values or notes, and never put them in an exception message. Ids, counts, versions, durations and exception details only. Validation errors (FR-09) are not logged (D142). So that exception messages stay clean:
- Parse user input with `TryParse`, never `Parse`: a failed `Parse` puts the input in its `FormatException` message.
- Never put a domain record (set, entry, workout) in a message: its `ToString()` prints every value.

**Android App**
| Provider | Where | Read with |
|---|---|---|
| `AndroidLogProvider` (App) | logcat, tag `WorkoutLog` | `adb logcat -s WorkoutLog` ([08](08-dev-setup-build.md#reading-the-logs)) |
| `FileLogProvider` (Core) | `logs/workoutlog.log` in the app-specific external files folder, or the app data folder if there is none; becomes `workoutlog.1.log` at 1 MB (T42) | `adb pull` ([08](08-dev-setup-build.md#reading-the-logs)) |

- Levels: Debug builds from Debug, Release from Information, `Microsoft.*` from Warning (D139).
- `AndroidDiagnostics` logs the start (version, version code, build, API level), the previous exit reason, activity created/stopped/destroyed and trim-memory requests. `MauiProgram` registers its handlers with `ConfigureLifecycleEvents`; on start it subscribes to unhandled and unobserved-task exceptions (T44).
- On API 36 Android never calls `OnLowMemory`, and trim memory only reports `UiHidden` or `Background`, so it is logged at Information. Memory kills show as the previous exit reason (`LowMemory`) on the next start (T44).
- The WebView render process ending can't be logged; the next start logs the exit reason (T44).
- The log file shows when each workout was saved (ids and counts only). adb or a USB-connected PC can read it on both builds (T42).

## Dependencies
Confirmed in US-02 (D75).

| Package | Version | Project | Purpose |
|---|---|---|---|
| sqlite-net-pcl | 1.11.285 | Core | ORM. Brings in SQLitePCLRaw 3.0.3 and the native SQLite library (SourceGear.sqlite3 3.53.3) |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.13 | Core | `IServiceCollection` for `AddCore` (T19) |
| Microsoft.Extensions.Logging.Abstractions | 10.0.13 | Core | `ILogger<T>` and `[LoggerMessage]` (T41, T43) |
| NUnit | 5.0.0 | tests | Test framework. Fallback: 4.6.1 (see [US-04](13-backlog.md#us-04-create-the-solution)) |
| NUnit3TestAdapter | 6.3.1 | tests | Test adapter |
| NUnit.Analyzers | 4.15.0 | tests | Build-time NUnit checks |
| Microsoft.NET.Test.Sdk | 18.11.0 | tests | Test host |
| Moq | 4.21.0 | tests | Mocks |
| AutoFixture, AutoFixture.AutoMoq | 4.18.1 | tests | Test data |
| Microsoft.Extensions.TimeProvider.Testing | 10.11.0 | Core.Tests, UI.Tests | `FakeTimeProvider` (T31) |
| Microsoft.Extensions.Diagnostics.Testing | 10.11.0 | Core.Tests, UI.Tests | `FakeLogger` (T43) |
| bunit | 2.12.5 | UI.Tests | Component tests |
| Appium.WebDriver | 9.0.0 | E2E.Tests | UI automation (Selenium 4.46). Check against the Appium server version in M6 |
| BlazingStory | 1.0.0-preview.96 | UI.Stories | Component stories and prototyping (D99). Prerelease only; no stable version exists |
| MD2RazorGenerator | 1.2.4 | UI.Stories | Build-time only. Turns the design-system Markdown pages into BlazingStory pages (D100) |

MAUI packages (`Microsoft.Maui.Controls`, `Microsoft.AspNetCore.Components.WebView.Maui`) come from the template, pinned to 10.0.120 with `MauiVersion` in the App project (T38).

Blazor packages (`Microsoft.AspNetCore.Components.Web` in UI; `Microsoft.AspNetCore.Components.WebAssembly` and `.DevServer` in UI.Stories; 10.0.13) come from the project templates.

`ILogger<T>` reaches UI through `Microsoft.AspNetCore.Components.Web`, and the App gets Microsoft.Extensions.Logging through MAUI. No logging provider packages are used (T38, T41).

Checks:
- `SQLitePCLRaw.bundle_green` is not used. It is on the 2.x line and conflicts with the 3.x packages sqlite-net-pcl brings in.
- 16 KB memory pages: the Android native libraries in SourceGear.sqlite3 3.53.3 (arm, arm64, x64, x86) are 16 KB aligned.

## Android 16 platform notes
- **Edge-to-edge:** the app draws under the status and navigation bars, with no opt-out on API 36. The layout must keep content inside the safe-area insets.
- **Predictive back:** the old back-press callbacks aren't called on API 36. Back goes through an AndroidX `OnBackPressedCallback` instead ([Navigation](#navigation), [US-28](13-backlog.md#us-28-back-button-on-android-16)).
