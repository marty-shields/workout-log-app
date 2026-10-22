# 10 — Decisions Log

All decisions were made on 2026-10-21 and have status **Accepted** unless noted.

## Product
| # | Decision | Choice | Alternatives offered |
|---|---|---|---|
| D01 | Audience | Just me, sideloaded APK | Play Store internal track; public Play Store |
| D02 | Data storage | On-device only; backup later | Local + export; local + cloud sync; cloud-first |
| D03 | Workout structure | Flexible: any number of exercises and sets | Mirror notebook limits; notebook as default |
| D04 | Exercise source | Hard-coded list shipped with the app; no add/delete in the app | Own editable list; free text; preloaded + own |
| D05 | Initial exercise list | Martin gives the must-haves; Claude builds the rest. Revised 2026-10-21 (D79) | Claude drafts, Martin reviews; Martin provides |
| D06 | Weight unit | kg only | lbs; setting; per entry |
| D07 | Weight precision | Any decimal | 0.25 kg; 0.5 kg; whole kg |
| D08 | "Type of exercise" field | Dropped | Own list; free text |
| D09 | Body fat / body weight | Dropped | Per workout; separate log |
| D10 | Notes and rating | Dropped (workout notes, rating, per-exercise notes) | Keep any of them |
| D11 | Exercise types | Strength or Cardio; the fields come from the exercise's metric definition (see D33) | — |
| D12 | Bodyweight exercises | Weight optional; extra weight can be added | Weight required (0); third type |
| D13 | Cardio fields | Duration required; all other cardio metrics optional | All required; at least one |
| D14 | Duration format | h:mm:ss | mm:ss; whole minutes |
| D15 | Distance | Decimal, unit set per exercise | Whole numbers; km; miles; metres |
| D16 | Level | Whole number | Decimal |
| D17 | Start/finish time | Start = New Workout opens; finish = first save | Buttons; manual entry |
| D18 | Time edits | Fixed after first save; no override | Manually editable; re-save updates finish |
| D19 | Unsaved workout | Back asks **Discard / Cancel** (saving only via the Save button); lost if the app is killed. Was Save/Discard; revised 2026-10-21 | Save/Discard/Cancel; auto-keep in progress; discard silently |
| D20 | Save rule | At least one exercise; every exercise has a set or entry; every set/entry has its required metrics. Otherwise save is blocked (FR-09). Revised 2026-10-21 | Anything, even empty; drop empty/incomplete items silently; ask before dropping |
| D21 | Reorder | No; order added | Exercises only; both |
| D22 | Duplicate exercise in a workout | Allowed | Not allowed |
| D23 | Remove items | Sets and exercises can be removed | Sets only; neither |
| D24 | Delete workout | Confirm dialog, permanent | Undo; no confirm |
| D25 | Workouts per day | More than one allowed | One per day |
| D26 | Home screen | Dashboard | History list; straight into new workout |
| D27 | Dashboard content | New Workout, last workout, week/month counts, History link | — |
| D28 | Week start | Monday | Sunday |
| D29 | History row | Date + day of week + start time. Revised 2026-10-21 (rows were identical with two workouts on one day) | Date + day only; + total time; exercise count; names |
| D30 | v1 scope | Logging + history/edit/delete + dashboard | Last-time numbers; progress; templates (→ future) |
| D31 | Rest timer, export | Future ideas | v1 |
| D32 | Dark mode / design system | Later UX phase; not covered now. **Superseded by D96** (2026-10-22) | v1 |
| D33 | Exercise data model | Hybrid per type: separate StrengthSet and CardioEntry tables, each with a nullable column per metric; each exercise declares the metrics it shows and requires | Two fixed types; more fixed types; generic metric/value rows; one shared hybrid table |
| D34 | Strength metrics | Reps, WeightKg, Duration, Distance. Revised 2026-10-21 (D84) | — |
| D35 | Cardio metrics | Duration, Distance, Floors, Level, Incline, Calories, Rate. Revised 2026-10-21 (D87) | — |
| D36 | Cardio entries per exercise | One | Several (intervals) |
| D37 | Plank | Strength; duration required, weight optional | Drop; cardio; separate Timed type |
| D38 | Stair Climber | Duration, floors, level, calories, rate (steps/min), based on typical gym console readouts | Distance as floors/steps; drop it |
| D39 | Rate | Generic Rate column; unit declared per exercise; entered from the machine | Derive from duration/distance; both |
| D40 | Units storage | Nullable enums on the exercise definition (distance unit, rate unit); no lookup table | Lookup table + FK |
| D41 | Invalid save | Stay on the page, show an error, nothing saved | — |
| D42 | Leaving unsaved edits | Prompt: Discard changes / Cancel. Amended by D121 | Drop silently |
| D43 | Live timer | Elapsed h:mm:ss shown on New Workout | No timer |
| D44 | After save | Always return to the Dashboard | New → Dashboard, Edit → History; stay on the workout |
| D45 | App name | Workout Log | Gym Log |
| D46 | Package id | `uk.martinshields.workoutlog` | `com.martinshields.workoutlog` |
| D47 | Product goal | "Replace my paper notebook: log every gym session on my phone, and look back at past workouts." | Quick logging; notebook + habit |
| D48 | Goal done-when | APK installed; every session for 4 weeks logged in the app; v1 FRs met | 2 weeks; 10 sessions; goal sentence only |
| D49 | Backlog location | Markdown: [12](12-product-goal.md) goal + DoD, [13](13-backlog.md) backlog | One file; GitHub Issues; Jira; Claude Docs; personal tool |
| D50 | Epics | 8 feature epics EP-01…EP-08 in milestone order; milestone shown on the epic. Revised 2026-10-21 (D73) | 1 epic per milestone; 5 or 6 epics; milestone per story |
| D51 | Stories | Grouped by feature; IDs US-01…; "As the user" / "As the developer" for technical stories | One per FR; M2-01; WL-001; tasks; fold tech into stories |
| D52 | Story ACs | Link FR IDs; add only ACs not already in 02 | Link only; copy ACs in |
| D53 | Sizing | XS–XL, relative effort, drafted by Claude for review | S/M/L; rough time; Martin sizes |
| D54 | Status values | To do / In progress / Blocked / Done | Three states |
| D55 | Open items | Each becomes a backlog story | README only |
| D56 | Tests in the backlog | Unit/bUnit via DoD; Appium and the manual checklist as stories | DoD only; test items per epic |
| D57 | Definition of Done | Code, Tests, Verified on phone (where the story changes the app), Docs + commit ([12](12-product-goal.md)) | — |
| D58 | Backlog layout | Summary table per epic, then a card per story | Table only; cards only |
| D59 | When open items close | In the milestone of their backlog story; no "before M0" rule | All before M0 |
| D60 | Debug build identity | Package `uk.martinshields.workoutlog.debug`, name "Workout Log (Dev)"; installs alongside the release app | Emulator only; sign debug with the release key |
| D61 | Draft lost if Android kills the backgrounded app | Accepted; D19 stands | Persist the draft; decide after testing |
| D62 | Android 16 predictive back | Spike in M0 (US-28) | AC on US-04; handle in M2 |
| D63 | Schema upgrades | Stored schema version; sqlite-net adds tables/columns; other changes are numbered upgrade steps | Additive only; decide later |
| D64 | Time storage | UTC, shown in the phone's current time zone | UTC + offset; local time |
| D65 | Date and count basis | Start time | Finish time |
| D66 | Exercise picker | Overlay on the workout page; back closes it | Own page |
| D67 | Back on Dashboard / History | History → Dashboard; Dashboard leaves the app | Confirm exit |
| D68 | Versioning | SemVer display version; version code +1 per release APK | Date-based; counter only |
| D69 | Logging a past session | Out of scope (future idea) | Manual start/finish |
| D70 | Metric limits | Level, Calories, Rate ≥ 0; no maximums | Level ≥ 1, Rate > 0; all > 0; sanity caps |
| D71 | History empty state, total time | "No workouts yet"; total as h:mm:ss | + New Workout button; "1h 2m" |
| D72 | Retiring exercises | Never; exercises stay in the picker | Hidden flag; decide later |
| D73 | Roadmap (US-01) | Accepted, with History and editing split: M3 History (list only, rows not tappable), M4 Edit workouts (edit, Discard changes, delete; wording amended by D121), then M5 Dashboard and M6 Tests + release | One History milestone; delete in M3; read-only view in M3 |
| D74 | Backlog | Accepted as drafted, including story sizes | — |
| D75 | Package versions (US-02) | Latest stable as listed in [05](05-architecture.md#dependencies); NUnit 5.0.0 with 4.6.1 as fallback; NUnit.Analyzers added; `SQLitePCLRaw.bundle_green` dropped | NUnit 4.6.1; only versions ≥ 2 weeks old; no analyzers |
| D76 | Removing a cardio entry | Not removable on its own; removed only with its exercise (FR-08) | Add entry button after removal |
| D77 | New exercise contents | Starts with one empty set (strength) or its empty entry (cardio) (FR-03) | Starts empty |
| D78 | Back on Edit with no unsaved changes | History, no prompt (FR-23) | Dashboard |
| D79 | Must-have exercises (US-05) | 44, all in [exercises.json](../src/WorkoutLog.Core/Domain/Exercises/exercises.json): Machine Chest Press; Machine Shoulder Press; Cable Lat Pulldown; Cable Close-Grip Lat Pulldown; Cable Underhand Lat Pulldown; Cable V-Bar Lat Pulldown; Cable Seated Row; Machine Row; Barbell Bent-Over Row; Dumbbell One-Arm Row; Machine Leg Extension; Machine Lying Leg Curl; Machine Seated Leg Curl; Barbell Back Squat; Machine Hack Squat; Machine Pendulum Squat; Barbell Curl; EZ-Bar Curl; Dumbbell Curl; Dumbbell Hammer Curl; Cable Curl; Cable Rope Hammer Curl; Cable Rope Overhead Tricep Extension; Dumbbell Overhead Tricep Extension; Barbell Skull Crusher; EZ-Bar Skull Crusher; Machine Tricep Extension; Cable Tricep Pushdown; Cable V-Bar Tricep Pushdown; Cable Rope Tricep Pushdown; Dumbbell Incline Press; Dumbbell Shoulder Press; Machine Chest Fly; Machine Rear Delt Fly; Machine Leg Press; Machine Seated Leg Press; Machine Bicep Curl; Dip; Machine Assisted Dip; Pull-up; Wide-Grip Pull-up; Chin-up; Machine Assisted Pull-up; Machine Assisted Chin-up | — |
| D80 | Exercise source | free-exercise-db (public domain) plus hand-added exercises; muscleandstrength.com names used only to find gaps | Hand-built only |
| D81 | Exercise scope | Strength + powerlifting, plus 6 carries and 16 cardio: 315 in total. No stretching, plyometrics, Olympic lifts or other strongman | Add plyometrics, Olympic, stretching; restore rarely used variants; pad to 1000 |
| D82 | Variant rules | Alternating, one-arm, seated/standing and stance variants fold into the base exercise; equipment, bench angle, grip and attachment stay separate ([04](04-exercise-list.md#selection-rules)) | Keep one-arm; keep every variant; separate stances |
| D83 | Exercise names | Equipment first; no prefix for bodyweight moves, carries and Deadlift | Source names |
| D84 | Carries | One entry each (Farmer's Walk, Suitcase Carry, Overhead Carry, Sled Push, Sled Pull, Yoke Walk). New StrengthSet.Distance, required, in metres, ≥ 0 | Weight + duration only; one per equipment; leave out |
| D85 | Assisted machines | Separate exercises; assistance stored in WeightKg, labelled Assist (kg) | Negative weight; not tracked |
| D86 | Cardio list and units | 16 exercises with units as in [04](04-exercise-list.md#cardio). Treadmill covers walking; Stair Climber covers StepMill; VersaClimber in feet and feet/min; Miles unit dropped | Just the 6 drafted; separate Treadmill Walk and StepMill |
| D87 | Level and Incline | Level stays a whole number with a per-exercise label (Resistance, Damper, Speed level). New Incline metric (decimal %) for Treadmill, Cross Trainer and Arc Trainer | One decimal Level with a label; unlabelled Level |
| D88 | Field labels | Optional label per metric in the definition (Claude's choice) | LevelLabel field + WeightIsAssistance flag |
| D89 | Exercise Ids | One sequence 1…N for all types; new exercises take the next number | Ranges per type |
| D90 | Exercise details | Primary muscle, equipment and mechanic stored per strength exercise; not shown or filtered in v1 | Filter by muscle/equipment/mechanic |
| D91 | Picker search | Search box; matches every word in any order against name and nicknames; hyphens as spaces; plurals and joined words (triceps, flies, pullup) normalised. Empty search shows the full list A–Z, grouped Strength / Cardio | Exact phrase; name only; nothing until you type |
| D92 | Search engine | In memory: definitions loaded once at startup into `IExerciseCatalog`; ranked by name prefix, then name, then nickname | SQLite FTS5 |
| D93 | Dumbbell weight | Per dumbbell | Total; undefined |
| D94 | Exercise list file | [exercises.json](../src/WorkoutLog.Core/Domain/Exercises/exercises.json) until the code repo exists; it becomes Core's embedded file in US-07 | CSV; Excel; tables in 04 |
| D95 | Pace entry format (US-06) | Entry follows the unit: min/km as `m:ss` (`5:30`), stored as decimal minutes; other rates as decimals ([03](03-data-model.md#rate-entry-format)) | Decimal (`5.5`) |
| D96 | Design system timing (2026-10-22) | Wireframes and design system before M2, as M1.5 ([EP-09](13-backlog.md#ep-09-ux--design-system-m15)). Supersedes D32 | Wireframes now, visual design after v1; explore only |
| D97 | Themes (2026-10-22) | Light and dark; the app follows the phone's setting | Dark only; light only for now |
| D98 | Number entry (2026-10-22) | Phone's numeric keyboard (`inputmode` numeric/decimal). Duration as three boxes: h, mm, ss | Keyboard + steppers; custom in-app keypad |
| D99 | Prototyping tool (2026-10-22) | BlazingStory 1.0.0-preview.96, WebAssembly, in `WorkoutLog.UI.Stories`; stories render the real `WorkoutLog.UI` components | Static HTML/CSS prototype; Storybook (HTML); Penpot; Figma (no access); BlazingStory Server |
| D100 | Design system docs (2026-10-22) | Markdown pages in the stories site (MD2RazorGenerator 1.2.4); [14](14-design-system.md) points to them | Markdown in `docs/` only |
| D101 | Prototype scenarios (2026-10-22) | One auto-play story per main user task: the player outlines each step's target, then taps it or types into it | Numbered steps beside or above the phone, clicked through by hand |
| D102 | Error colour (2026-10-22) | Error text and error borders are red, from US-31. The wireframes stay greyscale | Red now in the wireframe tokens, for errors only or for errors and danger buttons |
| D103 | Validation display (2026-10-22) | Each faulty field gets an error border; one message per problem under its set or entry, in field order | One combined message per set or entry; field border plus one combined message |
| D104 | Set rows with more than two fields (2026-10-22) | Duration moves to its own labelled line under the set, so its boxes fit the text and the touch target. Superseded by D105 | Squeeze into one row; two-column grid per set; shorter placeholders (h : m : s) |
| D105 | Sets with more than two fields, one a duration (2026-10-22) | One open set with every field labelled, duration full width; other sets show as a summary line and open on tap, one at a time. New workout opens the last set; edit starts all closed; removing the open set leaves all closed. Errors on a closed set show under its summary | Shaded set blocks; dividers between sets; duration in the column header. Collapse for all strength exercises. Several open sets. Errors open their sets |
| D106 | Set validation before moving on (2026-10-22) | Add set, and opening another set, are blocked while the current set (open set, or last row) is empty or has errors; its errors show on it. All strength exercises. Remove, Add exercise, Back and Save are not blocked. The screen supplies the rules | Check without blocking (errors on the summary line); validate on Save only; block Add set only; let empty sets close; collapsing sets only |
| D107 | Partly filled duration and pace (2026-10-22) | When focus leaves the field, empty boxes become 0 and minutes and seconds are padded to two digits (1:: → 1:00:00, ::22 → 0:00:22, :30 → 0:30). An empty field stays empty | Fill in when the set closes or Save runs; on Save only; no padding; duration only |
| D108 | When errors clear (2026-10-22) | A set or entry showing errors is re-checked on each change: fixed errors go, new ones show. Add set clears the exercise error; adding an exercise clears the workout error | Clear a field's error when it is edited; leave errors until the next Save |
| D109 | Overlay focus (2026-10-22) | When it opens, a confirm dialog focuses Cancel and the Exercise picker focuses its search box | Focus the dialog or picker container; leave focus where it was |
| D110 | Palette and colour roles (2026-10-22) | Coolors palette 77878b, 305252, 373e40, 488286, b7d5d4, plus derived background, card border, muted text and red. Light: dark-teal top bar and primary buttons, aqua secondary buttons and open set, white cards on #f4f8f8, #488286 for focus and stat numbers. Dark: the same roles swapped to aqua on teal, #373e40 cards on #1e2324. Input borders #77878b light, #9aaaad dark. Placeholders use muted text. Text and controls meet WCAG AA in both | Palette colours only, without derived shades; change the mapping first |
| D111 | Theme switching (2026-10-22) | Most colour tokens are `light-dark()` pairs; `color-scheme` follows the phone (D97). `data-theme="light"` or `"dark"` forces a theme on an element; the stories phone frame has a System / Light / Dark switch | Follow the OS setting only, with no switch in the stories |
| D112 | Danger buttons (2026-10-22) | Filled red, the same red as errors (#a8231c light, #f2b8b5 dark) | Red outline; same as primary |
| D113 | Type and shape (2026-10-22) | System font. Body 16px; number and duration inputs 20px. Radius 8 / 12 / 20px (inputs / buttons and open set / cards and dialogs). Spacing stays 4–32px | Bundled font; current sizes or everything one step up; radius 4 / 8 / 16 or pill buttons; more generous or tighter spacing |
| D114 | Screen stories (2026-10-22) | One stories section per screen: Dashboard, New Workout, Edit Workout, Exercise Picker, History. Dialogs are states of the screen they belong to | One Workout section for New, Edit, picker and dialogs; a separate Dialogs section |
| D115 | Design System pages (2026-10-22) | One page per component group and one per screen, under Design System | One Components page and one Screens page; Introduction only |
| D116 | Component story themes (2026-10-22) | Component stories without the phone frame use `ThemeLayout`: the System / Light / Dark switch above the story on the app background | Light and dark side by side; screens only |
| D117 | Design-system checks (2026-10-22) | bUnit/NUnit tests fail on: literal colours (hex, colour functions, named colours) or sizes (units, percentages other than 100%) and unknown tokens in UI CSS; token pairs below WCAG AA; buttons and inputs under the touch target; interactive elements without `data-testid`; screen stories that differ from the tests' list of screen states; components and screens without a Design System page. Story names are read from the `.stories.razor` files, as BlazingStory can't render stories outside its host | Review by hand; render stories through BlazingStory internals; a shared state catalogue rendered by stories and tests |
| D118 | Closed set with errors (2026-10-22) | The set's summary line gets the red error outline, as well as its messages underneath | Messages only |
| D119 | Confirm dialogs (2026-10-22) | Anchored to the bottom, inside the safe area, with Cancel (secondary) and the confirm button full width side by side | Centred, with a text Cancel |
| D120 | Remove exercise (2026-10-22) | Small muted text, so the exercise name leads the card header | Same weight as the name |
| D121 | Leaving unsaved edits: wording (2026-10-22) | Title "Discard changes?", buttons **Discard / Cancel**. Amends D42; FR-23 updated | Keep "Discard your changes?" / **Discard changes** |
| D122 | Exercise search timing (2026-10-22) | `IExerciseCatalog` exposes the list only in US-07; search is added in US-11 | Build search in US-07 |
| D123 | Seeding renamed or retyped exercises (2026-10-22) | Seeding updates Name and Type by Id ([03](03-data-model.md#seeding-the-exercise-list)) | Name only |
| D124 | Exercise file checks (2026-10-22) | As well as the US-07 checks: Ids 1…N without gaps; 315 / 299 / 16; metrics fit the type; a distance unit only with Distance and a rate unit only with Rate | US-07 checks only |
| D125 | Exercise file location (2026-10-22) | Moved to [src/WorkoutLog.Core/Domain/Exercises/exercises.json](../src/WorkoutLog.Core/Domain/Exercises/exercises.json); docs link to it. Completes D94 | `Domain/` or `Resources/`; keep a copy in docs; embed from docs |

## Technical
| # | Decision | Choice | Alternatives offered |
|---|---|---|---|
| T01 | Framework | .NET MAUI | Kotlin + Compose; Flutter |
| T02 | .NET version | .NET 10 LTS | .NET 11 STS |
| T03 | UI model | MAUI Blazor Hybrid | MAUI XAML + CommunityToolkit.Mvvm |
| T04 | Database | SQLite via sqlite-net-pcl | EF Core; Microsoft.Data.Sqlite |
| T05 | Min Android | API 36 (Android 16) only | MAUI default minimum |
| T06 | Solution structure | App + Core library + Tests (UI project added by T13) | Single project + Tests |
| T07 | Testing | Unit (NUnit/Moq/AutoFixture), bUnit, Appium, manual checklist | — |
| T08 | IDE | VS Code + .NET MAUI extension | Visual Studio 2026; Rider |
| T09 | Source control | Local git only | Personal GitHub; decide later |
| T10 | Install | USB debugging for dev + signed APK for real use | One or the other |
| T11 | Docs | 15 markdown files in the repo's `docs/` folder (14 until 2026-10-22) | Fewer, larger docs |
| T12 | Roadmap | Milestones, drafted for review | v1 vs future only |
| T13 | Component location | Razor Class Library `WorkoutLog.UI` (net10.0) so bUnit can test it; App → UI → Core | Keep in App and drop bUnit |
| T14 | Appium test project | `WorkoutLog.E2E.Tests`, NUnit | `WorkoutLog.UiTests`; `WorkoutLog.Appium.Tests`; xUnit |
| T15 | Solution file (2026-10-22) | `WorkoutLog.slnx` | `WorkoutLog.sln` |
| T16 | UI test project (2026-10-22) | `tests/WorkoutLog.UI.Tests` (bUnit, NUnit) is created in US-30, with the components it tests | Create it in US-04 with the other projects |
| T17 | Core projects (2026-10-22) | `WorkoutLog.Core` and `WorkoutLog.Core.Tests` are created in US-07; US-04 adds the App and E2E.Tests | Do US-04 first; a separate story |
| T18 | Exercise table and seeder (2026-10-22) | Created in US-07 with sqlite-net-pcl on an `SQLiteAsyncConnection`; `IExerciseSeeder.SeedAsync()`. US-08 adds the other tables | Seeder behind a repository interface; seeding in US-08; `SQLiteConnection`; seeder without an interface |
| T19 | Core registration (2026-10-22) | `AddCore(databasePath)` registers the connection, catalog and seeder as singletons. Adds Microsoft.Extensions.DependencyInjection.Abstractions 10.0.13 | Register everything in `MauiProgram`; connection registered by the App |
| T20 | Moq and AutoFixture in Core.Tests (2026-10-22) | Added with the first test that uses them | Add them in US-07 |
| T21 | Seeding order (2026-10-22) | Exercise seeding runs after the schema upgrade steps | Seeding before the upgrade steps |

## Pending
See [README open items](README.md#open-items).
