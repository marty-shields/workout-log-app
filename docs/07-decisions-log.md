# 07 — Decisions Log

Each decision states how the app and the repo work now, one row per subject. A change edits its decision; a new subject takes the next free ID. IDs aren't reused. Next free ID: D192.

## Product and scope
| # | Decision | Choice |
|---|---|---|
| D01 | Audience | Just me, sideloaded APK |
| D02 | Product goal | "Replace my paper notebook: log every gym session on my phone, and look back at past workouts." |
| D03 | Goal done-when | APK installed; every session for 4 weeks logged in the app; the FRs met |
| D04 | App name | Workout Log |
| D05 | Data storage and backup | On the device in SQLite, with no cloud sync. The Release app has `allowBackup="true"`: Android Auto Backup takes its default contents (the database and the logs) for cloud backup and phone-to-phone transfer, with Android's default encryption. The Debug app has `false`. The manifest's `${allowBackup}` placeholder is set per configuration in `WorkoutLog.App.csproj`; the dev pipeline checks the Debug and Release APKs, and the release checks the signed APK ([06](06-dev-setup-build.md#ci)) |
| D06 | Workout structure | Any number of exercises and sets |
| D07 | Weight unit | kg only |
| D08 | Workouts per day | More than one allowed |

## Workouts, metrics and validation
| # | Decision | Choice |
|---|---|---|
| D09 | Exercise types | Strength or Cardio; the fields come from the exercise's metric definition (D10) |
| D10 | Exercise data model | Hybrid per type: separate StrengthSet and CardioEntry tables, each with a nullable column per metric; each exercise declares the metrics it shows and requires |
| D11 | Strength metrics | Reps, WeightKg, Duration, Distance. Distance is in metres and required on the six carries ([03](03-exercise-list.md#strength)) |
| D12 | Cardio metrics | Duration, Distance, Floors, Level, Incline, Calories, Rate, Pace |
| D13 | Bodyweight exercises | Weight optional; extra weight can be added |
| D14 | Cardio fields | Duration required; all other cardio metrics optional |
| D15 | Cardio entries per exercise | One |
| D16 | Plank | Strength; duration required, weight optional |
| D17 | Stair Climber | Duration, floors, level, calories, rate (steps/min), as on gym consoles |
| D18 | Rate | A generic Rate column; its unit is declared per exercise; entered from the machine |
| D19 | Pace | A `Pace` metric (TimeSpan, cardio, optional, per km, max 60:00) on Outdoor Run and Outdoor Walk, in place of Rate. A nullable column the ORM adds at startup, with no upgrade step |
| D20 | Level and Incline | Level is a whole number with a per-exercise label; Incline is a decimal % |
| D21 | Distance | Decimal; unit set per exercise |
| D22 | Units storage | Nullable enums on the exercise definition; no lookup table |
| D23 | Assisted machines | Separate exercises; assistance stored in WeightKg, labelled Assist. A blank assist is saved as 0 kg, and import counts a blank and a 0 assist as the same ([02](02-data-model.md#metrics)) |
| D24 | Dumbbell weight | Per dumbbell |
| D25 | Field labels and units | An optional label per metric in the definition; labels and units as in [03](03-exercise-list.md#field-labels-and-units). The assisted machines' label is `Assist` and weight always shows kg |
| D26 | Workout times | Start is when New Workout opens; finish is the first save; both are fixed after the first save. New Workout shows the elapsed time as h:mm:ss; while the clock is before the start it shows 0:00:00 and nothing is logged. If the clock is before the start at the first save, the finish is set to the start (total 0:00:00), the save succeeds and it is logged |
| D27 | Time storage | `DateTimeOffset` stored as UTC ticks, shown in the phone's current time zone. A workout's date and counts come from its start time |
| D28 | Number entry format | `.` is the only decimal separator, whatever the phone's language; the accepted forms are in [02](02-data-model.md#entry-formats-ui) |
| D29 | Decimal places | Every decimal metric, weight included, takes at most 2 places, counting typed digits. Shown, edited and exported values are rounded to 2 places, halves away from zero, by one Core helper, `Decimals.Round2` ([02](02-data-model.md#metrics)) |
| D30 | Whole numbers | Any `.` in a whole-number field is "{Label} must be a whole number" |
| D31 | Duration and pace text | Duration is h:mm:ss in three boxes, pace m:ss in two. An empty box counts as 0 and all empty is empty; non-digits and minutes or seconds over 59 are format errors. When focus leaves the field, empty boxes become 0 and minutes and seconds are padded to two digits ([02](02-data-model.md#entry-formats-ui)) |
| D32 | Whole seconds | A duration or pace with part of a second is `NotWholeSeconds`; one code per metric, in a fixed order ([02](02-data-model.md#validation)) |
| D33 | Metric limits | Every metric has a maximum in `MetricLimits`; above it is `TooLarge` ([02](02-data-model.md#metrics)). Weight, distance, floors, level, incline, calories and rate below 0 are "{Label} must be 0 or more"; a minus sign is a format error in the UI, and Core's `Negative` check stays. A required distance must be more than 0; optional metrics at 0 are valid and stored as 0 |
| D34 | Save rule | At least one exercise; every exercise has a set or entry; every set or entry has its required metrics (FR-09). Otherwise nothing is saved and the page stays, showing the errors |
| D35 | Save checks | Save's rules come from `IWorkoutService.Validate` and the UI words every code; `TooLarge` messages have no unit. An unknown exercise, a wrong type and a value for a metric the exercise doesn't show are validation errors; a null list, exercise, set, entry or draft throws `ArgumentNullException` at the service. `NotShown` has no message and is only logged. Exercise errors show on Save only and are logged once. A code with no message logs a Warning with ids, code and metric only; Save's exercise errors and the set check log separate Warnings. A cardio exercise with no entry reads as an empty entry |
| D36 | Validation display | Each faulty field gets an error border; one message per problem under its set or entry, in field order |
| D37 | Set checks before moving on | Add set, and opening another set, wait while the current set is empty or has errors: its errors show on it and focus moves to its first field at fault. A failed focus logs a Warning and the workout stays open. The rules come from Core |
| D38 | When errors clear | Once a set or entry fails a check, every change re-checks it, even after it becomes valid, until it passes Add set, opening another set or Save. Add set clears the exercise error; adding an exercise clears the workout error |
| D39 | When a cardio entry is checked | On Save, then on each change after that (D38); a re-pick doesn't check it |
| D40 | Focus after a failed Save | The first error in screen order takes focus; a closed set stays closed. On `SaveFailed`, "Couldn't save the workout. Try again" shows as the workout error, focus moves to Save and the workout stays ([New Workout](../src/WorkoutLog.UI.Stories/Stories/Screens/NewWorkout.md)) |
| D41 | Save while saving | Taps on Save while a save runs, or after one succeeds, are ignored with no visual change |
| D42 | Exercise order | Exercise cards show newest first; sets stay in the order added; there is no manual reorder. After a pick the add-exercise list closes, the exercise goes on top, scrolls into view and its first field takes focus. Picking an exercise already in the workout moves its card to the top instead of adding a second: strength adds a set as Add set does, cardio focuses its entry. Saved positions are the order added or last picked, from 0 ([New Workout](../src/WorkoutLog.UI.Stories/Stories/Screens/NewWorkout.md)) |
| D43 | Removing items | Sets and exercises can be removed; an exercise is removed at once, with no confirm. A cardio entry is removed only with its exercise (FR-08) |
| D44 | New exercise contents | Starts with one empty set (strength) or its empty entry (cardio) |
| D45 | Leaving New Workout | Back always asks "Discard this workout?" with **Discard / Cancel**; saving is only through Save. Back is ignored while a save runs and after one succeeds; Discard runs once. The draft is lost if Android kills the app ([New Workout](../src/WorkoutLog.UI.Stories/Stories/Screens/NewWorkout.md)) |
| D46 | Delete workout | Edit Workout's Delete asks "Delete this workout?" (**Delete / Cancel**), the same with or without unsaved edits. Deleting is permanent and goes to Workouts; a failed delete keeps the workout and focuses Delete; guarded as D45 ([Edit Workout](../src/WorkoutLog.UI.Stories/Stories/Screens/EditWorkout.md)) |
| D47 | Values Edit can't show | Edit shows only the metrics in the exercise's current definition, and decimals rounded to 2 places by `Decimals.Round2` (D29); a definition change is made with its data change |
| D48 | Last and PB weight | New Workout strength cards show the best weight of the last saved workout with the exercise (latest start) and of all saved workouts, from `IWeightHistoryService`. Best is the heaviest, or the lowest on an exercise whose weight is labelled `Assist`; 0 kg counts. Loaded once when the card is added; a failed load hides the line and Core logs Error 1501 ([New Workout](../src/WorkoutLog.UI.Stories/Stories/Screens/NewWorkout.md)) |
| D49 | Units in text | A value written with its unit has no space: *60kg*, *40m*, *3.2km*, in the last and PB line and set summaries; a count keeps its word, *8 reps* ([New Workout](../src/WorkoutLog.UI.Stories/Stories/Screens/NewWorkout.md)) |

## Exercise list and search
| # | Decision | Choice |
|---|---|---|
| D50 | Exercise source | A hard-coded list shipped with the app; no adding or deleting in the app. Built from free-exercise-db (public domain) plus hand-added exercises; muscleandstrength.com names were used only to find gaps |
| D51 | Must-have exercises | The reviewer's 44 are all in [exercises.json](../src/WorkoutLog.Core/Domain/Exercises/exercises.json) under agreed names |
| D52 | Exercise scope | Strength and powerlifting, plus carries and cardio ([03](03-exercise-list.md)). No stretching, plyometrics, Olympic lifts or other strongman |
| D53 | Variant rules | Alternating, one-arm, seated/standing and stance variants fold into the base exercise; equipment, bench angle, grip and attachment stay separate ([03](03-exercise-list.md#selection-rules)) |
| D54 | Exercise names | Equipment first; no prefix for bodyweight moves, carries and Deadlift |
| D55 | Cardio list and units | 16 exercises with units as in [03](03-exercise-list.md#cardio); no miles |
| D56 | Retiring exercises | Never; exercises stay in the add-exercise list |
| D57 | Exercise Ids | One sequence 1…N for all types; a new exercise takes the next number |
| D58 | Exercise details | Primary muscle, equipment (None for no equipment) and mechanic stored per strength exercise, and a category per exercise ([03](03-exercise-list.md#fields-per-exercise)); not filtered. The Exercise screen shows muscle · equipment, the muscle only when equipment is None |
| D59 | Exercise list file | [src/WorkoutLog.Core/Domain/Exercises/exercises.json](../src/WorkoutLog.Core/Domain/Exercises/exercises.json), embedded in Core; docs link to it |
| D60 | Exercise file checks | Ids 1…N without gaps, the documented counts, metrics fit the type, the category fits the type and metrics, units only with their metric |
| D61 | Seeding renamed or retyped exercises | Seeding updates Name and Type by Id ([02](02-data-model.md#seeding-the-exercise-list)) |
| D62 | Exercise search | Every query word, in any order, must start a word of the name or nicknames. Variants come from a fixed table and partly typed variants split; apostrophes are removed and any other character that isn't a letter or digit separates words; queries are cut to 50 characters. An empty search shows the whole list in its own order ([02](02-data-model.md#exercise-search)) |
| D63 | Search engine and ranking | In memory in `IExerciseCatalog`. Ranked in three groups: names where each query word, in order, starts the name's word in the same place; then other name matches; then nickname matches |
| D64 | Add-exercise list on open | No search text and the All chip each time it opens |

## Screens and design system
| # | Decision | Choice |
|---|---|---|
| D65 | Home screen | The Dashboard: New Workout, the last workout, week and month counts, and Workouts and Exercises links |
| D66 | Dashboard counts | In the phone's time zone: this week from Monday 00:00 and this month from the 1st at 00:00, to now. The last workout is the latest start, even after now. A skipped or repeated midnight starts at its first local time |
| D67 | Dashboard loading and errors | Loads each time it's shown, and when the activity starts while it's shown; a load while one runs is ignored. A spinner, or "Couldn't load the Dashboard" and **Try again** (focused), replace the last workout and counts; Workouts, Exercises and New Workout still work |
| D68 | Workouts row | Date, day of week and start time; total time as h:mm:ss; "No workouts yet" when empty |
| D69 | Workouts loading and errors | `Spinner` "Loading…", shown at once; back works. "Couldn't load your workouts" and **Try again**, which takes focus and reloads |
| D70 | Workouts order | Start time newest first; every workout loads at once (FR-20) |
| D71 | Workouts rows open Edit | Tapping a row opens `/workout/{id}`; rows are always tappable |
| D72 | Edit Workout | Saves as New Workout does. After any save the app returns to the Dashboard. Title "Workout" until it loads; Loading, Load failed with **Try again**, and Not found, where back goes to Workouts |
| D73 | Leaving Edit Workout | Back with changes asks "Discard changes?" with **Discard / Cancel**; without changes it goes to Workouts (FR-23). A change is a value typed, a set added or removed, or an exercise added, removed or moved to the top, even if later undone. Guarded as D45 ([Edit Workout](../src/WorkoutLog.UI.Stories/Stories/Screens/EditWorkout.md)) |
| D74 | Load failed component | One `LoadFailed` component for Workouts, Edit Workout and the Dashboard |
| D75 | Startup screen | `Spinner` with "Loading…" under the top bar; on failure "Couldn't open the workout database" with no details |
| D76 | Rendering error | An Error screen: "Something went wrong" and **Restart**, to the Dashboard; an unsaved workout is lost |
| D77 | Add-exercise list | The Exercises list layout as an overlay on the workout page (`AddExerciseView`), listing the whole catalog; back closes it ([Add Exercise](../src/WorkoutLog.UI.Stories/Stories/Screens/AddExercise.md)) |
| D78 | Back | Workouts goes to the Dashboard; back on the Dashboard isn't taken, so the app closes ([04](04-architecture.md#navigation)). Back with a dialog open cancels it and stays on the page. With the soft keyboard open, Android back only hides it; the back arrows act at once |
| D79 | Number entry | The phone's numeric keyboard; duration in three boxes (h, mm, ss) |
| D80 | Sets with more than two fields | One open set with every field labelled; the others show as summary lines and open on tap. A closed set with errors gets the red error outline as well as its messages ([New Workout](../src/WorkoutLog.UI.Stories/Stories/Screens/NewWorkout.md)) |
| D81 | Overlay focus | A confirm dialog focuses Cancel; the add-exercise list focuses its search box once the list shows |
| D82 | Confirm dialogs | Anchored to the bottom, inside the safe area, with Cancel and the confirm button full width side by side |
| D83 | Remove exercise | A muted trash icon (`IconButton` Delete), so the exercise name leads the card header |
| D84 | Themes | Light and dark, following the phone's setting. Colour tokens are `light-dark()` pairs; `data-theme` forces a theme on an element; the stories have a System / Light / Dark switch |
| D85 | Palette and colour roles | A teal palette with derived shades and roles for light and dark, all meeting WCAG AA ([Tokens story](09-design-system.md)). Error text and borders are red; danger buttons are filled with the same red |
| D86 | Type and shape | System font; body 16px, number inputs 20px; radius and spacing from the tokens |
| D87 | Reduced motion | The `Spinner` stands still under `prefers-reduced-motion` |
| D88 | App icon and splash design | A pixel-art notebook with a dumbbell header, ruled lines and spiral binding, white on dark teal; the splash is the same art in both themes |
| D89 | App icon colours | The five palette colours and white only, checked in UI.Tests against `tokens.css` |
| D90 | App icon geometry | A 108-unit adaptive-icon canvas with whole device pixels per sprite pixel, inside the safe circle; the splash uses the same canvas |
| D91 | Themed icon | The foreground is also the monochrome layer; details are cut-outs |
| D92 | Debug icon | Debug and Release share the icon and splash; the "(Dev)" label tells them apart |
| D93 | Icon source | SVGs in `src/WorkoutLog.App/Resources/`, embedded by the stories for the App Icon page |
| D94 | Stories site | BlazingStory (WebAssembly) in `WorkoutLog.UI.Stories`; the stories render the real `WorkoutLog.UI` components. The design system docs are Markdown pages in the stories site; [09](09-design-system.md) points to them |
| D95 | Stories structure | Three sections: Foundations (Introduction, Tokens, Navigation, App Icon); Components, one folder per group with an Overview page and one stories file per component; Screens, one folder per screen the user sees, with an Overview page holding its parts, behaviour and `data-testid`s. Dialogs are states of their screen. The transitions and back rules are on Foundations / Navigation ([09](09-design-system.md)) |
| D96 | Story content | Screen stories show static states only, one story per state, named by the state with the same name for the same state on every screen. Component stories are one story per state, with values in the template and no `Arguments`; outside the phone frame they use `ThemeLayout`'s System / Light / Dark switch |
| D97 | Design-system checks | UI.Tests fail on literal colours or sizes, unknown tokens, token pairs below WCAG AA, small touch targets, missing `data-testid`, and screen stories that differ from the tests' screen states. They also fail unless each component has one stories file in its group, each `*View` is shown by a screen story, and stories show nothing else. A view may instead be shown in another screen's overlay, as `AddExerciseView` over `WorkoutView` |
| D98 | Front-end process | New or changed screens and components are built in the stories site and approved there before the App uses them. Every screenshot is checked, light and dark, for the look and position of what changed, in every story that shows it. The stories site is left running and each new or changed story and page is opened in the reviewer's browser, then stopped once approved ([09](09-design-system.md#process)) |

## Architecture and logging
| # | Decision | Choice |
|---|---|---|
| D99 | Framework | .NET MAUI |
| D100 | .NET version | .NET 10 LTS |
| D101 | UI model | MAUI Blazor Hybrid |
| D102 | Database | SQLite through a lightweight ORM |
| D103 | Min Android | API 36 (Android 16) only |
| D104 | Solution structure | `WorkoutLog.slnx`: the App host, `WorkoutLog.UI` (a Razor class library, tested with bUnit) and `WorkoutLog.Core`, with their test projects; App → UI → Core |
| D105 | Core registration | `AddCore(databasePath)` registers the gated interfaces, built around inner instances it creates with one connection, and the schema upgrade steps (`ISchemaUpgrade`); nothing outside the gate can reach the database |
| D106 | One database call at a time | An internal `DatabaseGate` singleton; gated decorators run every repository and initializer call through it |
| D107 | Core visibility | Only the services and their records are public; the rest is internal, visible to the mocking library and the stories |
| D108 | Database startup | The schema version is stored in SQLite `user_version`. `StartupGate` awaits `IDatabaseInitializer.InitializeAsync()` and shows the Startup screen until it completes or throws. It throws on a version newer than the app's, runs the numbered upgrade steps (checked on every startup) before the ORM creates missing tables and columns, then seeds the exercise list ([02](02-data-model.md#schema-upgrades)) |
| D109 | Exercise catalog load | The catalog reads exercises.json on first use, inside `StartupGate`, so a bad file shows the Startup error (D75) |
| D110 | Workout repository | One `IWorkoutRepository` over the whole workout, taking and returning immutable domain records; the row classes are internal; position is list order. `AddAsync` returns the new Id; `ReplaceExercisesAsync` replaces all children in one transaction and can't change the times |
| D111 | Repository checks and indexes | No checks beyond the schema's unique and plain indexes, including the unique `StartedAt` index that serves the Workouts order ([02](02-data-model.md#tables)); a missing Id throws `KeyNotFoundException` |
| D112 | Workout service | `IWorkoutService` singleton: `StartNew()` returns a draft the page keeps; it saves new workouts and edits, and validates without saving. `GetAsync` returns Loaded, NotFound or Failed; `DeleteAsync` returns Deleted or Failed; each is logged |
| D113 | Validation | `WorkoutValidator` checks the domain records against the catalog and returns codes the UI words; format checks stay in the UI ([02](02-data-model.md#validation)) |
| D114 | Time source | .NET `TimeProvider`, faked in tests |
| D115 | Save outcome | `SaveResult`; nothing is written when invalid; any repository exception becomes `SaveFailed` |
| D116 | Read services | `IWorkoutsService` returns the workouts' ids and starts. `IDashboardService` reads the last start and counts the week and month in memory, from the repository's `ListAsync(from, to, limit)`. Each returns `Failed` with the error logged |
| D117 | Workout pages | `NewWorkoutPage` and `EditWorkoutPage` live in UI; the App's pages only host them. `WorkoutEditor`, a transient each page injects, holds the exercises, the add-exercise list (search, chip and last sessions), changes, check and save; each page keeps its own load, timer, back, discard and delete ([04](04-architecture.md#dependency-injection)). New Workout raises `OnSaved` and the App goes to the Dashboard; it logs a discard with its exercise count and raises `OnDiscarded`. Edit Workout loads through `GetAsync`, checks each exercise against the catalog, logs a discard and raises `OnDeleted`. `IWorkoutChecker` holds the save-failed wording |
| D118 | List pages | `WorkoutsPage`, `ExercisesPage` and `DashboardPage` live in UI, load through their services and show the load states; the App's pages only host them. `DashboardPage` reloads on app start through `IAppStart`. `LoadFailed` takes a message, `OnRetry` and a test id prefix, and focuses Try again |
| D119 | Back and app start handlers | A stack of back handlers in UI, where the newest runs; an AndroidX callback in the App, on only while a handler exists; navigation keeps no WebView back stack ([04](04-architecture.md#navigation)). One `AppLifecycle` in UI: pages register app start handlers through `IAppStart`, the host runs them from `MainActivity`'s start (MAUI's own activities, such as the file picker's, are ignored), and a failing handler is logged while the others still run. Handlers belong to the current activity: `Attach` on create drops earlier ones; `Detach` on destroy clears them only if that activity is current |
| D120 | Stories services | The stories use the real `IExerciseCatalog` and register only `SampleWorkoutService`, which runs Core's validator and stores nothing. `SampleWeightHistoryService` is in UI.Tests. The browser SQLite native file is removed from the stories build |
| D121 | XML docs in the stories | The stories fetch the unfingerprinted `.xml` doc files past the browser cache (`loadBootResource`) |
| D122 | Adding exercises | The catalog splits search words once; `ExerciseFields` maps definitions to fields; `WorkoutView.Focus` focuses or adds a set; a failed focus is logged and the workout stays open |
| D123 | Set checks | `ISetChecker` reads each field's text, calls Core's `ValidateSet` and words the codes; `AddUi()` registers UI's services |
| D124 | Focus after a blocked tap | Add set and opening another set set a flag; `ExerciseCard` focuses the set at fault after its next render, logging a failure |
| D125 | Workout checker | `IWorkoutChecker` composes `ISetChecker` and `IWorkoutService`: it checks every set and entry, then the workout, and words the codes |
| D126 | Focus on errors | `WorkoutView` takes an `ErrorFocus` request with a target (first error, Save or Delete); a failure is logged and the workout stays open |
| D127 | Saved values as text | `ExerciseFields.Saved` turns a saved exercise into the text the user would type; a kind that doesn't match gives Load failed |
| D128 | App packages | MAUI pinned in the App project; no debug logging package |
| D129 | NuGet sources | `nuget.config` maps every package to nuget.org only |
| D130 | Logging seam | Core and UI log through `ILogger<T>`; each host registers its providers; the App writes to logcat with its own `AndroidLogProvider` ([04](04-architecture.md#logging)) |
| D131 | Log file | `FileLogProvider` in Core writes a 1 MB rolling log in the app's external files folder, readable with adb on both builds ([04](04-architecture.md#logging)) |
| D132 | Logging code | `[LoggerMessage]` methods with fixed EventIds; each project has an internal enum of its EventIds in fixed ranges, checked by tests; tests check the calls with a fake logger ([04](04-architecture.md#logging)) |
| D133 | Android hooks | Lifecycle and memory events through `ConfigureLifecycleEvents`; unhandled and unobserved exceptions logged; the WebView renderer's end and memory kills read from the next start's exit reason ([04](04-architecture.md#logging)) |
| D134 | Log levels | Debug builds from Debug, Release from Information; `Microsoft.*` from Warning |
| D135 | Log content | Ids, counts, versions, durations and exception details only; never set or entry values ([04](04-architecture.md#logging)) |

## Testing
| # | Decision | Choice |
|---|---|---|
| D136 | Testing | Unit, component and E2E tests ([05](05-testing.md)). There is no manual on-phone checklist: the release workflow's E2E and release smoke are the checks |
| D137 | E2E tests | Typical user flows end to end on the Debug build on a device, chosen by the `DeviceUdid` run setting; one at a time; the Debug app's data cleared per test; a locked device fails the test and no PIN is stored ([05](05-testing.md#e2e-tests-appium)) |
| D138 | E2E scope | Behaviour only, no visual checks; failures a user can't cause stay in UI.Tests ([05](05-testing.md#in-uitests-not-e2e)) |
| D139 | E2E tool | Appium with the UiAutomator2 driver, from NUnit, in `WorkoutLog.E2E.Tests` |
| D140 | Appium install | Appium and its driver are exact devDependencies in the E2E project's `package.json`, installed with `npm ci --ignore-scripts`. Locally Chromedriver downloads itself and Appium runs unauthenticated on 127.0.0.1; CI pins Chromedriver (D167) |
| D141 | E2E session | `AppiumSetup` checks the setup, starts Appium on a free port on 127.0.0.1, installs the APK and opens one session for the run; at the end it closes the app and removes Appium's helpers. Gherkin features run by Reqnroll on NUnit, with `AppHooks` and declarative steps over screen classes ([05](05-testing.md#e2e-tests-appium)) |
| D142 | One app launch per E2E scenario | `AppHooks` clears the app before each scenario and launches it before the first step, unless that step launches it itself (seeding steps, after copying the database in, and "Given I am on") ([05](05-testing.md#e2e-tests-appium)) |
| D143 | Debug APK embeds assemblies | `EmbedAssembliesIntoApk` in Debug, so the APK Appium installs can run |
| D144 | E2E results | One folder per run; a failed test saves its screen, page source, app log and Appium log slice and attaches them ([06](06-dev-setup-build.md#results)) |
| D145 | E2E seeding | The E2E project references Core and builds a seeded database on the PC with a clock set to each workout's times, then copies it into the app with `run-as` |
| D146 | E2E typing | Typing taps the field, clears it and types a key at a time; with the keyboard open, focus moves by script; a tap that doesn't focus the field fails the step |
| D147 | E2E log checks | Steps pull the app's log and check key events and that typed values and validation messages never appear (D135) |
| D148 | E2E back, focus and keyboard | "Android back" is the back key, which reaches the same callback as the back gesture (D119). Given-steps end with the keyboard hidden; When-steps leave it as a user would (D78). Taps on the bottom bar and card buttons hide the soft keyboard first |
| D149 | E2E screen rotation | The run holds the screen in portrait and puts the rotation settings back at the end |
| D150 | E2E device commands | Device commands run from the WebView context; the keyboard is hidden with the back key, only when an input has focus |
| D151 | E2E example rows | Scenario outlines keep one row per device path; other values of a rule are tested in UI.Tests or Core.Tests |
| D152 | E2E harness details | `WebView.Text` reads inside its wait; failure folder names drop characters Windows and artifacts don't allow |
| D153 | E2E clock | Steps work out expected dates from the device's time and time zone; seeded times sit 2 hours from a day boundary; a `@today` scenario fails at its start near midnight |
| D154 | E2E tags | `@smoke` on the core journeys; features and scenarios tagged with their stories, FRs and decisions. A tag goes on the feature only when every scenario shares it, so `Category=US-xx` runs only that story's scenarios. The full run comes before each release |
| D155 | E2E retries in CI | `e2e-run.sh` re-runs only the failed scenarios, up to 3 tries; a pass on a retry gets a warning. Local runs don't retry |

## Build, CI and release
| # | Decision | Choice |
|---|---|---|
| D156 | Package ids | Release `uk.martinshields.workoutlog`. Debug `uk.martinshields.workoutlog.debug`, named "Workout Log (Dev)", installs alongside the release app |
| D157 | Dev tooling | Visual Studio or VS Code with .NET MAUI; wireless debugging when USB isn't possible ([06](06-dev-setup-build.md)) |
| D158 | Install | USB debugging for development; the signed APK from a release for real use |
| D159 | Network permissions | None in the manifest (NFR-01); Debug builds get INTERNET from the SDK |
| D160 | Versioning | SemVer display version: `v1.0.0` first, then the highest `semver:*` label among the PRs since the last tag; a PR without exactly one label fails the run. Version code: the number of `v*` tags + 1; tags can't be deleted or moved. The workflow passes both versions and the `.csproj` sets neither |
| D161 | Source control | Public GitHub repo [marty-shields/workout-log-app](https://github.com/marty-shields/workout-log-app), default branch `main`, protected by a ruleset; git and `gh` use the personal account in this repo only ([06](06-dev-setup-build.md#source-control)) |
| D162 | .gitignore | MAUI Android entries from GitHub's VisualStudio.gitignore, signing files, `.vscode/` and `.claude/settings.local.json` |
| D163 | Shared project settings | `Directory.Build.props` sets the target framework, nullable, implicit usings and warnings as errors for every project, and the test packages for `*.Tests` projects |
| D164 | Package versions | `Directory.Packages.props` holds every NuGet version and project files reference packages without versions; the E2E npm packages are in `package.json` and the actions in the workflows. These are the approved packages: Dependabot proposes updates and merging one approves it. Docs name no packages or versions |
| D165 | PR build | `pr-build.yml` runs on PRs to `main`: `dev-pipeline.yml` builds, runs the Core and UI tests and checks `allowBackup` (D05), then `qa-pipeline.yml` runs if it passes. `ubuntu-latest`; `global.json` (`rollForward: patch`) pins the exact SDK and the workflow pins the workload set; actions pinned by SHA with least permissions. Failed tests show as annotations and a results table in the job summary. The required checks are `dev / build-and-test`, `dev / backup-manifest`, `qa / emulator-e2e-tests` and `pr-checks` ([06](06-dev-setup-build.md#ci)) |
| D166 | PR checks | `pr-checks.yml` requires a Conventional Commit title and exactly one `semver:*` label ([06](06-dev-setup-build.md#ci)) |
| D167 | E2E on emulators | `qa-pipeline.yml` runs the E2E suite on PRs to `main` and in the release: one job per shard, each with one API 36 AOSP emulator set up by scripts with no third-party actions (AVD and image in `emulator-start.sh`); scenarios split by whole feature; results kept for 1 day; the tests step times out at 30 minutes. Chromedriver is pinned by version and SHA-256, and the job fails when the emulator's WebView major differs. Its `emulator-e2e-tests` job passes only when every shard passes ([06](06-dev-setup-build.md#qa-pipeline)) |
| D168 | Dependabot | Weekly updates for NuGet, the E2E npm packages, the SDK and the Actions; minor and patch grouped per ecosystem; each PR labelled `semver:patch` and titled `chore(deps): …` |
| D169 | E2E npm overrides | `overrides` lift packages Appium pins to vulnerable versions; the driver's bundled copies can't be overridden |
| D170 | Release workflow | `release.yml`, run by hand on `main`, one run at a time: `version`, then `build` (the dev pipeline), `e2e` (the QA pipeline) and `release-smoke`, then `release`; its shell is in `.github/scripts/` ([06](06-dev-setup-build.md#release-signed-apk)) |
| D171 | Release signing | Keystore PKCS12, RSA 4096, 10000 days; the file, password and alias are `release` environment secrets; signing properties only on the release job's command line, passwords as `env:` references; restore before the keystore is written; the keystore deleted after the build |
| D172 | Release approval | Only the sign-and-publish job uses the `release` environment, after the other jobs pass; the owner approves it and admins can't bypass |
| D173 | Release cert pin | The release cert's SHA-256 is in [06](06-dev-setup-build.md#keystore-handling) and `CERT_SHA256`; the release fails unless the APK has exactly one signer with it; the fingerprint heads the release notes |
| D174 | Release contents | `WorkoutLog-X.Y.Z.apk` (arm64), its `.sha256`, and notes from the merged PRs under the cert's fingerprint |
| D175 | Release smoke | The Release APK of the same commit, x64 with the debug key, must start cleanly on an emulator: the start line, the exercise list seeded and startup finished, with no error or crash |
| D176 | Keystore backup | One backup of the keystore file, passwords and alias, kept by the owner in a private location the repo doesn't record; the `release` environment's secrets aren't the backup ([06](06-dev-setup-build.md#keystore-handling)) |
| D177 | Stories on GitHub Pages | Every push to `main` publishes the stories site to [GitHub Pages](https://marty-shields.github.io/workout-log-app/) with GitHub's Pages actions: `stories-pages.yml` publishes the stories in Release with no compressed copies and sets `<base href>` in `index.html` and `iframe.html` to the Pages base path; the stories pages link relatively (`./?path=…`). The README links to it ([06](06-dev-setup-build.md#ci)) |

## Ways of working and docs
| # | Decision | Choice |
|---|---|---|
| D178 | Backlog | The repo's GitHub issues: epics and stories (US-xx) are issues, each story a sub-issue of its epic, with milestones on GitHub; each idea is an `enhancement` issue with no milestone ([ways of working](README.md#ways-of-working)) |
| D179 | Definition of Done | Code, Tests, Verified (where the story changes the app: E2E scenarios where possible, else by hand on the phone), Docs and commit; new work logs its errors and unexpected events as in [04](04-architecture.md#logging) ([08](08-product-goal.md)) |
| D180 | Story workflow | A branch per story; local review before the commit; a DRAFT PR that closes the story, squash-merged by the reviewer ([06](06-dev-setup-build.md#source-control)) |
| D181 | Root README | `README.md` is the repo's front page: features, light and dark screenshots of the screen stories, an Install section, the PR build, latest-release, stories site and licence badges, the stack and a License section; no Documentation section. The Definition of Done checks it still matches the app; a changed screen retakes its README screenshots ([09](09-design-system.md#readme-screenshots)) |
| D182 | Docs | The docs (01–09) describe only what the app has: no scope, plans or ideas; a change updates them as it lands. They name no packages or versions and don't repeat what code or config holds. The Testing doc lists no features; each feature file describes what it covers. Screen behaviour lives in the stories site (D95), which may also show approved designs the App doesn't use yet; their Overview page says so |
| D183 | Decisions log | One row per subject: its Choice states how things work now in 1–2 sentences, linking to the doc that holds the detail. A change edits its row; a new subject takes the next free ID; IDs aren't reused |
| D184 | Ticket review skill | `.agents/skills/ticket-review/SKILL.md` runs three read-only reviews (requirements, code and tests, security) one at a time in fresh contexts, only on request; the agent fixes the dead certs and asks about the rest before the local review |
| D185 | Licence | MIT, in `LICENSE` at the repo root; the README has its badge and a License section. Third-party notices (Material Symbols, Apache-2.0) are in `NOTICE` at the repo root, linked from the License section |
| D186 | Workout identity | A workout is identified across phones by its `StartedAt` (UTC, to the tick), unique in the database; the int Id is the primary key, used only inside one database ([02](02-data-model.md#tables)) |
| D187 | Settings | The Dashboard top bar has a Settings gear that opens Settings (`/settings`); back returns to the Dashboard. Icons are Material Symbols Outlined (weight 400) SVG paths copied into the `Icon` component, which `IconButton` uses: settings, arrow_back (the back arrows), close (remove a set), delete (Delete workout, remove an exercise), chevron_right (the end of a tappable row) and search (the Exercises search box) |
| D188 | Export | Settings shows the workout count and exports every workout to one JSON file in the cache folder, then opens the share sheet; nothing shows afterwards. A file over 5 MB isn't written. Core's `IExportService` writes it; the App supplies the version and folder (`IExportEnvironment`) and the share sheet (`IFileShare`) ([02](02-data-model.md#export-file), [Settings](../src/WorkoutLog.UI.Stories/Stories/Screens/Settings.md)) |
| D189 | Message punctuation | No message in the app ends with a full stop; a two-sentence message keeps its inner one, e.g. "Couldn't export your workouts. Try again" ([09](09-design-system.md#messages)) |
| D190 | Import | Settings imports a file made by Export through Android's file picker; Import Review shows the groups and the choices, and nothing is written until Import, in one transaction. One `IImportCheck` class per rule, run by `IImportChecker`; Save's rules are reused. Format upgrades are numbered `IFormatUpgrade` steps. The file under review is held in memory only, as a New Workout draft is (D45) ([02](02-data-model.md#import), [Import Review](../src/WorkoutLog.UI.Stories/Stories/Screens/ImportReview.md)) |
| D191 | Exercises list | The Dashboard's Exercises link opens `/exercises`: each exercise with a saved workout, latest start first, exercises from the same workout A–Z. Each row shows its kind at the right and `Last <date> · <best>` of its last session; the date is in the phone's time zone, with the year only when it isn't this year. One `IBestRule` per exercise category (`category` in exercises.json, [03](03-exercise-list.md#fields-per-exercise)) picks the best: lift, top set (heaviest; tie → most reps); bodyweight, most reps, or the top set when a set has more than 0 kg; assisted, least assist (blank = 0 kg; tie → most reps); hold, longest; carry, heaviest with its distance (tie → longest); cardio, distance when the exercise has it, else duration. A lift or carry with no weight, or cardio with no distance logged, shows the date only. `ILoggedExercisesService` reads it through one repository query; it loads each time the list opens, not on app start. An id not in the catalog is left out and logged as a Warning; a failed read logs a Warning and shows `LoadFailed`, or, in the add-exercise list, the catalog with no last sessions ([Exercises](../src/WorkoutLog.UI.Stories/Stories/Screens/Exercises.md)) |
| D192 | Exercise page: lifts | A lift row on the Exercises list opens `/exercises/{id}` (FR-61); back returns to the Exercises list, which loads again with no search or chip. A lift is `category` Lift in exercises.json. `IExerciseProgressService` reads every session of the lift in one repository query when the page opens, and works out each tab and range in memory: one point per session, by start time. Top set: the heaviest set, tie → most reps, labelled ×reps. e1RM: the best Epley value, weight × (1 + reps / 30), of the sets with a weight and 1–12 reps, a 1-rep set being its weight; shown to up to 1 decimal place (e.g. 82.3kg, 80kg), halves away from zero, compared unrounded. A session with no value for a tab has no point. Ranges are calendar months in the phone's time zone: a session is in range when its local start date is on or after today minus 3, 6 or 12 months; All has no limit. Records in the range: heaviest weight (weight, then reps) and best e1RM; a tie takes the earliest session. A workout holding the lift twice is one session, its sets in workout order. The range filter, top set, e1RM and records are separate rules in Core. An id not in the catalog, or not a lift, logs a Warning and shows `LoadFailed` ([Exercise](../src/WorkoutLog.UI.Stories/Stories/Screens/Exercise.md)) |
