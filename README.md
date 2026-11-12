<div align="center">

<img src="docs/images/readme/icon.svg" alt="Workout Log app icon" width="112" />

# Workout Log

**A paper gym notebook, rebuilt as a fast, offline Android app.**

[![Latest release](https://img.shields.io/github/v/release/marty-shields/workout-log-app?label=release)](https://github.com/marty-shields/workout-log-app/releases/latest)
![Android 16](https://img.shields.io/badge/Android-16_(API_36)-34A853?logo=android&logoColor=white)
![.NET 11](https://img.shields.io/badge/.NET-11-512BD4?logo=dotnet&logoColor=white)
![MAUI Blazor Hybrid](https://img.shields.io/badge/MAUI-Blazor_Hybrid-512BD4?logo=blazor&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-on_device-003B57?logo=sqlite&logoColor=white)
[![Stories site](https://img.shields.io/badge/stories-GitHub_Pages-305252?logo=storybook&logoColor=white)](https://marty-shields.github.io/workout-log-app/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

[Features](#features) · [Install](#install) · [Built with](#built-with) · [Getting started](#getting-started) · [License](#license)

</div>

---

## Why

Every gym session used to go in a paper notebook: date, start and finish times, exercises, sets, weights and reps. Workout Log keeps that simple flow and drops the paper. Open the app, add exercises, log sets as you go, then look back at past sessions.

It's built for one person and one phone: no accounts, no sign-up.

## Features

### Logging

- Add exercises and log sets. Each exercise shows the fields it needs: weight and reps, duration, or distance.
- 315 built-in strength and cardio exercises, with search.
- Cardio entries record duration plus the machine's metrics: distance, incline, floors, level, calories and rate.
- Each set shows what you did in it last time, so you know what to beat. It's display only: you still type every value.
- Start and finish times are recorded for you, with a timer while you log.
- Saving a new workout opens a summary: how long it took, your exercises and sets, any records you set, and how each exercise compares with last time.
- Saving checks every set and points to the field that's wrong.
- A note on the workout and on each exercise, up to 500 characters each. Adding an exercise shows the note it had last time.

<table align="center">
  <tr>
    <td align="center">
      <picture>
        <source media="(prefers-color-scheme: dark)" srcset="docs/images/readme/new-workout-dark.png" />
        <img src="docs/images/readme/new-workout-light.png" alt="Logging strength sets" width="150" />
      </picture>
    </td>
    <td align="center">
      <picture>
        <source media="(prefers-color-scheme: dark)" srcset="docs/images/readme/cardio-dark.png" />
        <img src="docs/images/readme/cardio-light.png" alt="Logging cardio entries" width="150" />
      </picture>
    </td>
    <td align="center">
      <picture>
        <source media="(prefers-color-scheme: dark)" srcset="docs/images/readme/add-exercise-dark.png" />
        <img src="docs/images/readme/add-exercise-light.png" alt="Adding an exercise" width="150" />
      </picture>
    </td>
    <td align="center">
      <picture>
        <source media="(prefers-color-scheme: dark)" srcset="docs/images/readme/last-time-dark.png" />
        <img src="docs/images/readme/last-time-light.png" alt="Last time's values beside each set" width="150" />
      </picture>
    </td>
    <td align="center">
      <picture>
        <source media="(prefers-color-scheme: dark)" srcset="docs/images/readme/notes-dark.png" />
        <img src="docs/images/readme/notes-light.png" alt="A workout note and exercise notes, with last time's note" width="150" />
      </picture>
    </td>
  </tr>
  <tr>
    <td align="center"><sub><b>Strength sets</b></sub></td>
    <td align="center"><sub><b>Cardio</b></sub></td>
    <td align="center"><sub><b>Add exercise</b></sub></td>
    <td align="center"><sub><b>Last time</b></sub></td>
    <td align="center"><sub><b>Notes</b></sub></td>
  </tr>
</table>

### Progress

- Workouts opens on the last 3 months, grouped by month. 3M, 6M, 1Y and All change the range in one tap, and the date field picks any day or span of days from a calendar. Open a workout to edit or delete it.
- Exercises lists every exercise you've logged, with its last session and best.
- Each exercise has a chart, records and its sessions over 3 months, 6 months, a year or all time:
  - lifts: top set and estimated one-rep max
  - bodyweight moves: best set, total reps and top set
  - assisted machines: least assist
  - holds: duration and weight
  - carries: weight and distance
  - cardio: distance, duration, pace or speed, floors and calories

<table align="center">
  <tr>
    <td align="center">
      <picture>
        <source media="(prefers-color-scheme: dark)" srcset="docs/images/readme/workouts-dark.png" />
        <img src="docs/images/readme/workouts-light.png" alt="Workouts" width="230" />
      </picture>
    </td>
    <td align="center">
      <picture>
        <source media="(prefers-color-scheme: dark)" srcset="docs/images/readme/exercise-lift-dark.png" />
        <img src="docs/images/readme/exercise-lift-light.png" alt="A lift's top set chart, records and sessions" width="230" />
      </picture>
    </td>
    <td align="center">
      <picture>
        <source media="(prefers-color-scheme: dark)" srcset="docs/images/readme/exercise-cardio-dark.png" />
        <img src="docs/images/readme/exercise-cardio-light.png" alt="A cardio exercise's distance chart, records and sessions" width="230" />
      </picture>
    </td>
  </tr>
  <tr>
    <td align="center"><sub><b>Workouts</b></sub></td>
    <td align="center"><sub><b>Lift progress</b></sub></td>
    <td align="center"><sub><b>Cardio progress</b></sub></td>
  </tr>
</table>

### Dashboard and data

- The Dashboard shows your last workout, this week so far against last week up to the end of the same day, a bar per week for the last 12 weeks and your weekly streak.
- Recent records on the Dashboard shows the exercises you've beaten your best on in the last 30 days; tap one to open its progress.
- Sets per muscle group shows each group's strength sets in the last 7 days and which you haven't trained.
- Export saves every workout, with its notes, to a JSON file you can share.
- Import reads an exported file, shows what's new, already there or different, and adds or replaces them.
- Android Auto Backup and a phone-to-phone transfer restore your workouts on a new phone. Only the database is included; the logs and exports are not.
- Light and dark themes follow the phone.

<table align="center">
  <tr>
    <td align="center">
      <picture>
        <source media="(prefers-color-scheme: dark)" srcset="docs/images/readme/dashboard-dark.png" />
        <img src="docs/images/readme/dashboard-light.png" alt="Dashboard" width="230" />
      </picture>
    </td>
    <td align="center">
      <picture>
        <source media="(prefers-color-scheme: dark)" srcset="docs/images/readme/workout-summary-dark.png" />
        <img src="docs/images/readme/workout-summary-light.png" alt="Workout summary after saving" width="230" />
      </picture>
    </td>
    <td align="center">
      <picture>
        <source media="(prefers-color-scheme: dark)" srcset="docs/images/readme/settings-dark.png" />
        <img src="docs/images/readme/settings-light.png" alt="Settings with Export and Import" width="230" />
      </picture>
    </td>
  </tr>
  <tr>
    <td align="center"><sub><b>Dashboard</b></sub></td>
    <td align="center"><sub><b>Workout summary</b></sub></td>
    <td align="center"><sub><b>Export and import</b></sub></td>
  </tr>
</table>

## Install

Workout Log is sideloaded: it isn't on Google Play.

1. Download `WorkoutLog-X.Y.Z.apk` from the [latest release](https://github.com/marty-shields/workout-log-app/releases/latest).
2. Check it's signed with the release key: `apksigner verify --print-certs WorkoutLog-X.Y.Z.apk` (Android SDK build tools) must show the SHA-256 digest at the top of the release notes.
3. Open it on the phone (allow your file manager to install apps), or run `adb install -r WorkoutLog-X.Y.Z.apk`. A newer version installs over the old one and keeps your workouts.

## Your data

- Your workouts are in a SQLite database on the phone. Nothing is sent to a server.
- Android Auto Backup copies that database, and only that, to your Google account, and a phone-to-phone transfer carries it too. The Dev build is never backed up.
- Export writes its JSON file to the app's cache, and the share sheet hands it on. Android may clear the cache at any time.
- Logs are in `Android/data/<package id>/files/logs`, readable over USB or `adb pull`. They hold events, ids and counts, never what you typed ([`Logging.feature`](tests/WorkoutLog.E2E.Tests/Features/Logging.feature) checks it).

## Built with

| Area | Choice |
|---|---|
| App | [.NET MAUI Blazor Hybrid](https://learn.microsoft.com/dotnet/maui/user-interface/controls/blazorwebview) on .NET 11: Razor components in a native Android shell |
| Platform | Android 16 (API 36), sideloaded |
| Data | SQLite via sqlite-net-pcl, on the device |
| Design system | Design tokens, components and screens, documented in a [BlazingStory](https://github.com/jsakamoto/BlazingStory) stories site, [published on GitHub Pages](https://marty-shields.github.io/workout-log-app/) |
| Tests | NUnit unit tests, bUnit component tests, and end-to-end Gherkin features run by Reqnroll and Appium on a phone or emulator |
| CI | GitHub Actions builds and tests every pull request, end-to-end tests on emulators included; the release workflow runs them again, then signs and publishes the APK; Dependabot keeps packages current |

<details>
<summary><b>Project layout</b></summary>

```
src/
  WorkoutLog.App/          MAUI Blazor Hybrid host (Android): startup, DI, routing
  WorkoutLog.UI/           Razor class library: screens, components, design tokens
  WorkoutLog.UI.Stories/   BlazingStory stories site for the UI
  WorkoutLog.Core/         Domain rules, SQLite data, services, import and export
tests/
  WorkoutLog.Core.Tests/   NUnit tests for Core
  WorkoutLog.UI.Tests/     bUnit tests for components and screens
  WorkoutLog.E2E.Tests/    Gherkin features on a device (Reqnroll + Appium)
```

</details>

## Getting started

You need the .NET 11 SDK with the `maui-android` workload. The full setup, including the Android SDK and the phone, is in [Dev setup & build](docs/dev-setup-build.md).

```powershell
# Build (warnings fail the build)
dotnet build WorkoutLog.slnx

# Run the unit and component tests
dotnet test tests/WorkoutLog.Core.Tests
dotnet test tests/WorkoutLog.UI.Tests

# Browse the stories site at http://localhost:5056 (main is at https://marty-shields.github.io/workout-log-app/)
dotnet run --project src/WorkoutLog.UI.Stories

# Run on a connected Android phone
dotnet build src/WorkoutLog.App -t:Run -f net11.0-android
```

The end-to-end tests need a connected device or an emulator: see [E2E tests](docs/e2e-tests.md).

## License

[MIT](LICENSE). 
Third-party notices are in [NOTICE](NOTICE).