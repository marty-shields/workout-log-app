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
│  │   ├─ wwwroot/             index.html, CSS
│  │   └─ MauiProgram.cs       DI registration
│  ├─ WorkoutLog.UI/           Razor Class Library (net10.0): pages and components
│  │   └─ wwwroot/css/         Design tokens and shared CSS ([14](14-design-system.md))
│  ├─ WorkoutLog.UI.Stories/   BlazingStory WebAssembly app (net10.0): stories for UI components
│  └─ WorkoutLog.Core/         Class library (net10.0)
│      ├─ Domain/              Records/entities, metrics, exercise definitions, validation
│      ├─ Data/                sqlite-net repositories, schema, seeding
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
    S --> C[IClock<br/>Core]
    S --> X[Exercise definitions<br/>Core]
```
- **App** hosts the BlazorWebView, registers DI and handles the Android back button.
- **UI** renders and calls services only. It never touches the database. Form fields come from the exercise's metric definition.
- **Services** hold the rules: save validation, start/finish capture, dashboard counts.
- **Repositories** handle sqlite-net access. They sit behind interfaces so services can be unit-tested with Moq.
- **IClock** supplies the current time, so start/finish, the live timer and week/month counts can be tested.
- **Exercise definitions** are an embedded JSON file in Core, loaded once at startup into the `IExerciseCatalog` singleton. The catalog also runs the picker search in memory (see [03](03-data-model.md#exercise-search), [04](04-exercise-list.md)).

## Dependency injection
Everything is registered in `MauiProgram.CreateMauiApp()` using the built-in `IServiceCollection`. Core registers its own types through `AddCore(databasePath)` (T19). The database connection is an `SQLiteAsyncConnection` singleton. Services and repositories are registered by interface. At startup the App awaits `IDatabaseInitializer.InitializeAsync()`, which upgrades and creates the schema, then seeds the exercise list (T25, T28). The initializer and repository are decorators that run each call through one `DatabaseGate`, so database calls run one at a time (T27).

## Navigation
Blazor routing (`@page`). The routes are proposed:
| Route | Screen |
|---|---|
| `/` | Dashboard |
| `/workout/new` | New workout |
| `/workout/{id}` | Edit workout |
| `/history` | History |

The Exercise picker is an overlay on the workout page, not a route.

The Android back button is passed to the current page:
- Workout pages: close the picker if it's open; otherwise show the Discard / Cancel prompt (FR-12, FR-23).
- History: go to the Dashboard.
- Dashboard: leave the app.

## Start / finish capture
- The New Workout page asks the service for a draft workout. The service stamps `StartedAt` (UTC) from `IClock`.
- The page shows a live elapsed timer based on `StartedAt`.
- The draft stays in memory only (FR-12: lost if the app is killed). Android may kill the app while the phone is locked; this is accepted (D61).
- On the first save, the service stamps `FinishedAt`, validates (FR-09) and writes everything in one transaction.
- Later saves update the exercises, sets and entries only. They never change the times.

## Error handling
- Validation failures are shown inline on the page. Nothing is written.
- Database errors are shown as a simple error message. The draft stays on screen.

## Dependencies
Confirmed in US-02 (D75).

| Package | Version | Project | Purpose |
|---|---|---|---|
| sqlite-net-pcl | 1.11.285 | Core | ORM. Brings in SQLitePCLRaw 3.0.3 and the native SQLite library (SourceGear.sqlite3 3.53.3) |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.13 | Core | `IServiceCollection` for `AddCore` (T19) |
| NUnit | 5.0.0 | tests | Test framework. Fallback: 4.6.1 (see [US-04](13-backlog.md#us-04-create-the-solution)) |
| NUnit3TestAdapter | 6.3.1 | tests | Test adapter |
| NUnit.Analyzers | 4.15.0 | tests | Build-time NUnit checks |
| Microsoft.NET.Test.Sdk | 18.11.0 | tests | Test host |
| Moq | 4.21.0 | tests | Mocks |
| AutoFixture, AutoFixture.AutoMoq | 4.18.1 | tests | Test data |
| bunit | 2.12.5 | UI.Tests | Component tests |
| Appium.WebDriver | 9.0.0 | E2E.Tests | UI automation (Selenium 4.46). Check against the Appium server version in M6 |

| BlazingStory | 1.0.0-preview.96 | UI.Stories | Component stories and prototyping (D99). Prerelease only; no stable version exists |
| MD2RazorGenerator | 1.2.4 | UI.Stories | Build-time only. Turns the design-system Markdown pages into BlazingStory pages (D100) |

MAUI packages (`Microsoft.Maui.Controls`, `Microsoft.AspNetCore.Components.WebView.Maui`, 10.0.120) come from the MAUI workload and template.

Blazor packages (`Microsoft.AspNetCore.Components.Web` in UI; `Microsoft.AspNetCore.Components.WebAssembly` and `.DevServer` in UI.Stories; 10.0.13) come from the project templates.

Checks:
- `SQLitePCLRaw.bundle_green` is not used. It is on the 2.x line and conflicts with the 3.x packages sqlite-net-pcl brings in.
- 16 KB memory pages: the Android native libraries in SourceGear.sqlite3 3.53.3 (arm, arm64, x64, x86) are 16 KB aligned.

## Android 16 platform notes
- **Edge-to-edge:** the app draws under the status and navigation bars, with no opt-out on API 36. The layout must keep content inside the safe-area insets.
- **Predictive back:** the old back-press callbacks aren't called on API 36. Intercepting back for the Discard prompts is proven in a spike ([US-28](13-backlog.md#us-28-spike-back-button-on-android-16)).
