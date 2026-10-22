# 13 — Product Backlog

Goal and Definition of Done: [12](12-product-goal.md).

## Conventions
- **Epics:** EP-01…EP-08, in milestone order. The milestone ([09](09-roadmap.md)) is in the epic heading.
- **Stories:** US-01… in one sequence. IDs never change; a new story takes the next number. Each story groups related requirements.
  - User stories: "As the user, I want…, so that…"
  - Technical stories: "As the developer, I want…, so that…"
- **Acceptance criteria:** the linked FR-xx in [02](02-requirements.md), plus any extra ACs listed on the story.
- **Size:** XS, S, M, L, XL. Relative effort only.
- **Status:** To do, In progress, Blocked, Done.

## EP-01 Project setup (M0)
| ID | Story | Links | Size | Status |
|---|---|---|---|---|
| [US-01](#us-01-review-the-draft-roadmap) | Review the draft roadmap | Open item 2 | XS | Done |
| [US-02](#us-02-confirm-package-versions) | Confirm package versions | Open item 3 | XS | Done |
| [US-03](#us-03-tooling-and-phone-setup) | Tooling and phone setup | [08](08-dev-setup-build.md) | S | To do |
| [US-04](#us-04-create-the-solution) | Create the solution | T06, T13, NFR-02 | M | To do |
| [US-28](#us-28-spike-back-button-on-android-16) | Spike: back button on Android 16 | FR-12, FR-23 | S | To do |

### US-01 Review the draft roadmap
As the developer, I want the draft roadmap reviewed, so that the milestones are agreed before coding starts.
- **Links:** README open item 2, [09](09-roadmap.md)
- **Extra ACs:**
  - The decision is recorded in [10](10-decisions-log.md).
  - [09](09-roadmap.md) is updated.
- **Size:** XS · **Status:** Done (D73)

### US-02 Confirm package versions
As the developer, I want the exact package versions confirmed, so that only approved dependencies are added.
- **Links:** README open item 3, [05](05-architecture.md)
- **Extra ACs:**
  - The versions are recorded in [05](05-architecture.md).
  - The decision is recorded in [10](10-decisions-log.md).
- **Size:** XS · **Status:** Done (D75)

### US-03 Tooling and phone setup
As the developer, I want the tooling installed and the phone set up for USB debugging, so that I can run builds on the phone.
- **Links:** [08](08-dev-setup-build.md)
- **Extra ACs:**
  - The prerequisites in [08](08-dev-setup-build.md) are installed.
  - `adb devices` lists the phone.
- **Size:** S · **Status:** To do

### US-04 Create the solution
As the developer, I want the solution created in local git, so that features have a place to go.
- **Links:** T06, T13, NFR-02, [05](05-architecture.md)
- **Extra ACs:**
  - The App, UI, Core and three test projects exist as in [05](05-architecture.md). `WorkoutLog.slnx`, `WorkoutLog.UI`, local git and `.gitignore` already exist ([US-29](#us-29-stories-project-setup)); `WorkoutLog.UI.Tests` already exists ([US-30](#us-30-wireframes)).
  - `.gitignore` matches [08](08-dev-setup-build.md).
  - A blank page shows on the phone.
  - Debug builds use the `.debug` package id and the name "Workout Log (Dev)" ([08](08-dev-setup-build.md)).
  - A test runs and passes with NUnit 5.0.0, NUnit3TestAdapter 6.3.1 and NUnit.Analyzers 4.15.0. If not, fall back to NUnit 4.6.1 and update [05](05-architecture.md) and D75.
- **Size:** M · **Status:** To do

### US-28 Spike: back button on Android 16
As the developer, I want to prove a Blazor page can intercept the Android back button on API 36, so that the Discard prompts (FR-12, FR-23) can be built.
- **Links:** FR-12, FR-23, [05](05-architecture.md#android-16-platform-notes)
- **Extra ACs:**
  - On the phone, back on a test page shows a prompt, and Cancel keeps the page open.
  - The approach is recorded in [05](05-architecture.md).
- **Size:** S · **Status:** To do

## EP-02 Exercises & data (M1)
| ID | Story | Links | Size | Status |
|---|---|---|---|---|
| [US-05](#us-05-review-the-exercise-list) | Review the exercise list | Open item 1 | XS | Done (D79–D94) |
| [US-06](#us-06-decide-the-pace-entry-format) | Decide the pace entry format | Open item 5 | XS | Done (D95) |
| [US-07](#us-07-exercise-definitions-and-seeding) | Exercise definitions and seeding | FR-40, FR-41, FR-42 | M | To do |
| [US-08](#us-08-sqlite-schema-and-repositories) | SQLite schema and repositories | NFR-05 | L | To do |
| [US-09](#us-09-workout-service) | Workout service | FR-09, FR-10, FR-11 | M | To do |

### US-05 Review the exercise list
As the user, I want to review the draft exercise list and metrics, so that the app ships with the exercises I use.
- **Links:** README open item 1, [04](04-exercise-list.md)
- **Extra ACs:**
  - [04](04-exercise-list.md) is updated and its DRAFT label is removed.
  - The decision is recorded in [10](10-decisions-log.md).
- **Size:** XS · **Status:** Done (D79–D94)

### US-06 Decide the pace entry format
As the user, I want to choose how pace (min/km, Outdoor Run) is entered (`5.5` or `5:30`), so that pace is always entered the same way.
- **Links:** README open item 5, [03](03-data-model.md)
- **Extra ACs:**
  - [03](03-data-model.md) is updated.
  - The decision is recorded in [10](10-decisions-log.md).
- **Size:** XS · **Status:** Done (D95)

### US-07 Exercise definitions and seeding
As the developer, I want the exercise definitions in code and seeded into SQLite, so that every screen uses the same fixed list.
- **Links:** FR-40, FR-41, FR-42, FR-43, [03](03-data-model.md), [04](04-exercise-list.md)
- **Extra ACs:**
  - [exercises.json](exercises.json) is moved into Core as an embedded file and loaded once at startup into the `IExerciseCatalog` singleton.
  - A unit test checks the file: unique Ids, names and aliases; valid enum values; strength exercises have muscle, equipment and mechanic, cardio exercises don't; cardio always requires Duration.
  - Seeding follows the rules in [03](03-data-model.md).
- **Size:** M · **Status:** To do

### US-08 SQLite schema and repositories
As the developer, I want the SQLite schema and repositories, so that workouts are stored on the phone.
- **Links:** NFR-05, [03](03-data-model.md)
- **Extra ACs:**
  - Tables match [03](03-data-model.md).
  - Deleting a workout removes its exercises, sets and entries.
  - A workout exercise has at most one cardio entry.
  - Times are stored in UTC (NFR-06).
  - The schema version is stored and upgrade steps run on startup ([03](03-data-model.md#schema-upgrades)).
- **Size:** L · **Status:** To do

### US-09 Workout service
As the developer, I want a workout service that validates and saves workouts and captures start and finish times, so that the screens hold no business rules.
- **Links:** FR-09, FR-10, FR-11
- **Extra ACs:**
  - Times come from `IClock`.
- **Size:** M · **Status:** To do

## EP-09 UX & design system (M1.5)
| ID | Story | Links | Size | Status |
|---|---|---|---|---|
| [US-29](#us-29-stories-project-setup) | Stories project setup | D99, T15 | S | Done |
| [US-30](#us-30-wireframes) | Wireframes | [06](06-screens-ux.md), D98 | M | Done |
| [US-31](#us-31-design-tokens) | Design tokens | D97, [14](14-design-system.md) | M | To do |
| [US-32](#us-32-components) | Components | [14](14-design-system.md) | L | In progress |
| [US-33](#us-33-final-screen-designs) | Final screen designs | [06](06-screens-ux.md), [14](14-design-system.md) | M | To do |

### US-29 Stories project setup
As the developer, I want a BlazingStory project for the UI components, so that screens can be designed and prototyped before M2.
- **Links:** D99, T15, [05](05-architecture.md), [08](08-dev-setup-build.md)
- **Extra ACs:**
  - `WorkoutLog.slnx` holds `WorkoutLog.UI` and `WorkoutLog.UI.Stories`; UI.Stories references UI.
  - `dotnet run --project src/WorkoutLog.UI.Stories` serves the stories.
  - Local git is initialised; `.gitignore` matches [08](08-dev-setup-build.md).
- **Size:** S · **Status:** Done

### US-30 Wireframes
As the user, I want greyscale wireframes of every screen and state, so that layout and flow are agreed before the screens are built.
- **Links:** [06](06-screens-ux.md), D98, D101–D107, T16
- **Extra ACs:**
  - One story per screen state: Dashboard (empty, with workouts); New Workout (empty, strength sets, cardio entry, save errors); Edit Workout; Exercise picker (no search text, results); History (empty, with workouts); Discard, Discard changes and Delete dialogs.
  - Stories render at phone size.
  - Content stays inside the safe-area insets.
  - The prototype has an auto-play story for each main user task (D101).
- **Size:** M · **Status:** Done

### US-31 Design tokens
As the user, I want the colours, type and spacing defined once, so that every screen looks consistent in light and dark.
- **Links:** D97, [14](14-design-system.md)
- **Extra ACs:**
  - Tokens are CSS custom properties in `WorkoutLog.UI/wwwroot/css/tokens.css`: colour, type, spacing, radius, touch-target size.
  - Dark values apply under `prefers-color-scheme: dark`.
  - Error text and error borders are red (D102).
  - A story shows every token in light and dark.
- **Size:** M · **Status:** To do

### US-32 Components
As the developer, I want the shared UI components built on the tokens, so that M2–M5 assemble screens from them.
- **Links:** [14](14-design-system.md)
- **Extra ACs:**
  - Each component is in `WorkoutLog.UI`, takes its data as parameters and has stories.
  - Components use tokens only; no hard-coded colours or sizes.
  - Interactive elements have `data-testid` ([07](07-testing.md)).
- **Size:** L · **Status:** In progress

### US-33 Final screen designs
As the user, I want the wireframes rebuilt with the design system, so that M2–M5 build screens to an agreed design.
- **Links:** [06](06-screens-ux.md), [14](14-design-system.md)
- **Extra ACs:**
  - Every US-30 story uses the tokens and components, in light and dark.
  - [06](06-screens-ux.md) and the Design System pages ([14](14-design-system.md)) are updated.
- **Size:** M · **Status:** To do

## EP-03 Workout logging (M2)
| ID | Story | Links | Size | Status |
|---|---|---|---|---|
| [US-10](#us-10-start-a-workout) | Start a workout | FR-01, FR-02, FR-14 | S | To do |
| [US-11](#us-11-add-and-remove-exercises) | Add and remove exercises | FR-03, FR-04, FR-05, FR-08 | S | To do |
| [US-12](#us-12-log-strength-sets) | Log strength sets | FR-06, FR-08, NFR-04 | M | To do |
| [US-13](#us-13-log-a-cardio-entry) | Log a cardio entry | FR-07 | M | To do |
| [US-14](#us-14-save-a-workout) | Save a workout | FR-09, FR-10, FR-13 | S | To do |
| [US-15](#us-15-discard-an-unsaved-workout) | Discard an unsaved workout | FR-12 | S | To do |

### US-10 Start a workout
As the user, I want a workout to start when I open New Workout, with a live timer, so that I don't have to note the time.
- **Links:** FR-01, FR-02, FR-14
- **Extra ACs:**
  - The header shows the date and day ([06](06-screens-ux.md)).
- **Size:** S · **Status:** To do

### US-11 Add and remove exercises
As the user, I want to add exercises from the list and remove them, so that the workout matches what I did.
- **Links:** FR-03, FR-04, FR-05, FR-08
- **Extra ACs:**
  - The picker is an overlay, grouped Strength / Cardio; back closes it ([06](06-screens-ux.md)).
  - Search matches names and nicknames as in [03](03-data-model.md#exercise-search), including ranking and word normalisation.
- **Size:** S · **Status:** To do

### US-12 Log strength sets
As the user, I want to add, fill in and remove sets for a strength exercise, so that I can record reps, weight, duration or distance.
- **Links:** FR-06, FR-08, NFR-04
- **Extra ACs:**
  - Weight accepts any decimal (D07).
  - Carries show distance in metres (D84); assisted machines label weight Assist (kg) (D85).
- **Size:** M · **Status:** To do

### US-13 Log a cardio entry
As the user, I want to fill in one cardio entry for a cardio exercise, so that I can record the machine's readout.
- **Links:** FR-07
- **Extra ACs:**
  - Duration is entered as h:mm:ss (D14).
  - Distance and rate show the exercise's units ([04](04-exercise-list.md)).
  - Level shows the exercise's label (Resistance, Damper, Speed level); Incline accepts decimals (D87).
  - Pace (min/km) is entered and shown as m:ss (D95).
- **Size:** M · **Status:** To do

### US-14 Save a workout
As the user, I want to save the workout, so that it is kept and the finish time is recorded.
- **Links:** FR-09, FR-10, FR-13
- **Size:** S · **Status:** To do

### US-15 Discard an unsaved workout
As the user, I want the back button to ask before dropping a new workout, so that I don't lose it by accident.
- **Links:** FR-12
- **Size:** S · **Status:** To do

## EP-04 History (M3)
| ID | Story | Links | Size | Status |
|---|---|---|---|---|
| [US-16](#us-16-view-history) | View history | FR-20 | S | To do |

### US-16 View history
As the user, I want a list of my saved workouts, newest first, so that I can look back.
- **Links:** FR-20
- **Extra ACs:**
  - Back goes to the Dashboard ([06](06-screens-ux.md)).
  - Rows are not tappable. Opening a workout (FR-21) comes in [US-17](#us-17-edit-a-saved-workout).
- **Size:** S · **Status:** To do

## EP-05 Edit workouts (M4)
| ID | Story | Links | Size | Status |
|---|---|---|---|---|
| [US-17](#us-17-edit-a-saved-workout) | Edit a saved workout | FR-21, FR-22, FR-11, FR-13 | M | To do |
| [US-18](#us-18-discard-unsaved-edits) | Discard unsaved edits | FR-23 | S | To do |
| [US-19](#us-19-delete-a-workout) | Delete a workout | FR-24 | S | To do |

### US-17 Edit a saved workout
As the user, I want to open a saved workout and change it, so that I can fix mistakes.
- **Links:** FR-21, FR-22, FR-11, FR-13
- **Extra ACs:**
  - Edit shows the total time instead of the live timer ([06](06-screens-ux.md)).
- **Size:** M · **Status:** To do

### US-18 Discard unsaved edits
As the user, I want the back button to ask before dropping my edits, so that I don't lose them by accident.
- **Links:** FR-23
- **Extra ACs:**
  - Discard changes returns to History ([06](06-screens-ux.md)).
- **Size:** S · **Status:** To do

### US-19 Delete a workout
As the user, I want to delete a workout after confirming, so that wrong entries don't stay in my history.
- **Links:** FR-24
- **Extra ACs:**
  - After delete, the app returns to History ([06](06-screens-ux.md)).
- **Size:** S · **Status:** To do

## EP-06 Dashboard (M5)
| ID | Story | Links | Size | Status |
|---|---|---|---|---|
| [US-20](#us-20-dashboard-home) | Dashboard home | FR-30, FR-31, FR-35 | S | To do |
| [US-21](#us-21-last-workout-summary) | Last workout summary | FR-32 | XS | To do |
| [US-22](#us-22-week-and-month-counts) | Week and month counts | FR-33, FR-34, NFR-06 | S | To do |

### US-20 Dashboard home
As the user, I want the app to open on a Dashboard with New Workout and History, so that I can start or look back in one tap.
- **Links:** FR-30, FR-31, FR-35
- **Extra ACs:**
  - Back leaves the app ([06](06-screens-ux.md)).
- **Size:** S · **Status:** To do

### US-21 Last workout summary
As the user, I want to see when my last workout was, so that I know when I last trained.
- **Links:** FR-32
- **Size:** XS · **Status:** To do

### US-22 Week and month counts
As the user, I want to see how many workouts I've done this week and this month, so that I can track how often I train.
- **Links:** FR-33, FR-34, NFR-06
- **Size:** S · **Status:** To do

## EP-07 Automated UI tests (M6)
| ID | Story | Links | Size | Status |
|---|---|---|---|---|
| [US-23](#us-23-appium-setup) | Appium setup | [07](07-testing.md) | M | To do |
| [US-24](#us-24-automate-the-v1-flows) | Automate the v1 flows | [07](07-testing.md) | L | To do |

### US-23 Appium setup
As the developer, I want Appium set up against the app's WebView, so that UI flows can be automated.
- **Links:** [07](07-testing.md)
- **Extra ACs:**
  - One test switches to the WebView context and finds an element by `data-testid`.
- **Size:** M · **Status:** To do

### US-24 Automate the v1 flows
As the developer, I want the v1 UI flows automated, so that regressions are caught before a release.
- **Links:** [07](07-testing.md)
- **Extra ACs:**
  - The 8 automated flows in [07](07-testing.md) pass.
- **Size:** L · **Status:** To do

## EP-08 Release (M6)
| ID | Story | Links | Size | Status |
|---|---|---|---|---|
| [US-25](#us-25-decide-the-keystore-backup-location) | Decide the keystore backup location | Open item 4 | XS | To do |
| [US-26](#us-26-signed-release-apk) | Signed release APK | [08](08-dev-setup-build.md) | M | To do |
| [US-27](#us-27-manual-phone-checklist) | Manual phone checklist | [07](07-testing.md) | S | To do |

### US-25 Decide the keystore backup location
As the developer, I want a backup location for the keystore, so that future APKs can still be signed if the original is lost.
- **Links:** README open item 4, [08](08-dev-setup-build.md)
- **Extra ACs:**
  - The location is recorded in [08](08-dev-setup-build.md).
  - The decision is recorded in [10](10-decisions-log.md).
- **Size:** XS · **Status:** To do

### US-26 Signed release APK
As the user, I want a signed release APK installed on my phone, so that I can use the app day to day.
- **Links:** [08](08-dev-setup-build.md)
- **Extra ACs:**
  - Signing passwords come from environment variables only.
  - A newer APK installs over the old one and the data survives.
  - The version code is higher than the previous APK's ([08](08-dev-setup-build.md)).
- **Size:** M · **Status:** To do

### US-27 Manual phone checklist
As the developer, I want the manual checklist run on the phone, so that v1 is checked end to end before I rely on it.
- **Links:** [07](07-testing.md)
- **Extra ACs:**
  - Every item in the manual checklist in [07](07-testing.md) is ticked.
- **Size:** S · **Status:** To do
