# 08 — Decisions Log

Made from 2026-10-21 to 2026-10-26. Every decision is **Accepted** unless its Choice says Superseded. IDs never change; a new decision takes the next free ID. Format: T110.

## Topics
| Topic | Decisions |
|---|---|
| Product and scope | D01–D10, D26–D31, D45–D48, D69 |
| Workouts, metrics and validation | D11–D25, D33–D44, D61, D63–D65, D70, D76–D78, D84–D93, D103–D108, D126–D130, D157–D179, D188, D236; T22–T34, T55 |
| Exercise list and search | D04, D05, D72, D79–D83, D89–D92, D94, D122–D125, D146–D156 |
| Screens and design system | D66, D67, D96–D121, D131, D133–D134, D138, D140, D143–D145, D180–D199, D209–D214 |
| Architecture and logging | T01–T06, T13, T15, T19, T21, T25–T27, T35–T36, T38–T74, T99, T112; D139–D142 |
| Testing | D200–D208, D215; T07, T14, T16–T18, T20, T75–T88, T94, T108–T109 |
| Build, CI and release | D60, D68, D135–D136, D220, D223–D229; T08–T10, T89–T98, T101–T107, T111, T113–T114 |
| Ways of working and docs | D49–D59, D73–D75, D193, D216–D219, D221–D222, D230–D235; T11–T12, T100, T110 |

## Product
| # | Decision | Choice | Alternatives |
|---|---|---|---|
| D01 | Audience | Just me, sideloaded APK | Play Store internal track; public Play Store |
| D02 | Data storage | On-device only | Local + export; local + cloud sync; cloud-first |
| D03 | Workout structure | Flexible: any number of exercises and sets | Mirror notebook limits; notebook as default |
| D04 | Exercise source | Hard-coded list shipped with the app; no add/delete in the app | Own editable list; free text; preloaded + own |
| D05 | Initial exercise list | The reviewer gives the must-haves; Claude builds the rest. Amended by D79 | Claude drafts, the reviewer reviews; the reviewer provides |
| D06 | Weight unit | kg only | lbs; setting; per entry |
| D07 | Weight precision | Any decimal. Amended by D158 (up to 2 places) | 0.25 kg; 0.5 kg; whole kg |
| D08 | "Type of exercise" field | Dropped | Own list; free text |
| D09 | Body fat / body weight | Dropped | Per workout; separate log |
| D10 | Notes and rating | Dropped (workout notes, rating, per-exercise notes) | Keep any of them |
| D11 | Exercise types | Strength or Cardio; the fields come from the exercise's metric definition (D33) | |
| D12 | Bodyweight exercises | Weight optional; extra weight can be added | Weight required (0); third type |
| D13 | Cardio fields | Duration required; all other cardio metrics optional | All required; at least one |
| D14 | Duration format | h:mm:ss | mm:ss; whole minutes |
| D15 | Distance | Decimal, unit set per exercise | Whole numbers; km; miles; metres |
| D16 | Level | Whole number | Decimal |
| D17 | Start/finish time | Start = New Workout opens; finish = first save | Buttons; manual entry |
| D18 | Time edits | Fixed after first save; no override | Manually editable; re-save updates finish |
| D19 | Unsaved workout | Back asks **Discard / Cancel** (saving only via the Save button); lost if the app is killed | Save/Discard/Cancel; auto-keep in progress; discard silently |
| D20 | Save rule | At least one exercise; every exercise has a set or entry; every set/entry has its required metrics. Otherwise save is blocked (FR-09) | Anything, even empty; drop empty items silently; ask before dropping |
| D21 | Reorder | No; order added. Amended by D236 | Exercises only; both |
| D22 | Duplicate exercise in a workout | Superseded by D154 | |
| D23 | Remove items | Sets and exercises can be removed | Sets only; neither |
| D24 | Delete workout | Confirm dialog, permanent | Undo; no confirm |
| D25 | Workouts per day | More than one allowed | One per day |
| D26 | Home screen | Dashboard | History list; straight into new workout |
| D27 | Dashboard content | New Workout, last workout, week/month counts, History link | |
| D28 | Week start | Monday | Sunday |
| D29 | History row | Date + day of week + start time | Date + day only; + total time; exercise count; names |
| D30 | Scope | Logging + history/edit/delete + dashboard | Last-time numbers; progress; templates |
| D31 | Rest timer, export | Not in the app | Include them |
| D32 | Dark mode / design system | Superseded by D96 | |
| D33 | Exercise data model | Hybrid per type: separate StrengthSet and CardioEntry tables, each with a nullable column per metric; each exercise declares the metrics it shows and requires | Two fixed types; more fixed types; metric/value rows; one shared table |
| D34 | Strength metrics | Reps, WeightKg, Duration, Distance. Amended by D84 | |
| D35 | Cardio metrics | Duration, Distance, Floors, Level, Incline, Calories, Rate. Amended by D87, D168 | |
| D36 | Cardio entries per exercise | One | Several (intervals) |
| D37 | Plank | Strength; duration required, weight optional | Drop; cardio; Timed type |
| D38 | Stair Climber | Duration, floors, level, calories, rate (steps/min), as on gym consoles | Distance as floors/steps; drop it |
| D39 | Rate | Generic Rate column; unit declared per exercise; entered from the machine | Derive from duration/distance; both |
| D40 | Units storage | Nullable enums on the exercise definition; no lookup table | Lookup table + FK |
| D41 | Invalid save | Stay on the page, show an error, nothing saved | |
| D42 | Leaving unsaved edits | Prompt: Discard changes / Cancel. Amended by D121 | Drop silently |
| D43 | Live timer | Elapsed h:mm:ss shown on New Workout | No timer |
| D44 | After save | Always return to the Dashboard | New → Dashboard, Edit → History; stay on the workout |
| D45 | App name | Workout Log | Gym Log |
| D46 | Package id | `uk.martinshields.workoutlog` | `com.martinshields.workoutlog` |
| D47 | Product goal | "Replace my paper notebook: log every gym session on my phone, and look back at past workouts." | Quick logging; notebook + habit |
| D48 | Goal done-when | APK installed; every session for 4 weeks logged in the app; the FRs met | 2 weeks; 10 sessions; goal sentence only |
| D49 | Backlog location | Superseded by D216 | |
| D50 | Epics | Feature epics EP-01… in milestone order. Amended by D73 | 1 epic per milestone; milestone per story |
| D51 | Stories | Grouped by feature; IDs US-01…; "As the user" / "As the developer" for technical stories | One per FR; tasks; fold tech into stories |
| D52 | Story ACs | Link FR IDs; add only ACs not already in 02 | Link only; copy ACs in |
| D53 | Sizing | XS–XL, relative effort, drafted by Claude for review | S/M/L; rough time; the reviewer sizes |
| D54 | Status values | To do / In progress / Blocked / Done | Three states |
| D55 | Open items | Superseded by D216 | |
| D56 | Tests in the backlog | Unit/bUnit via DoD; Appium and the manual checklist as stories. Amended by D230 | DoD only; test items per epic |
| D57 | Definition of Done | Code, Tests, Verified on phone (where the story changes the app), Docs + commit ([09](09-product-goal.md)). Amended by D141, D217 | |
| D58 | Backlog layout | Superseded by D216 | |
| D59 | When open items close | In the milestone of their story | All before M0 |
| D60 | Debug build identity | Package `uk.martinshields.workoutlog.debug`, name "Workout Log (Dev)"; installs alongside the release app | Emulator only; sign debug with the release key |
| D61 | Draft lost if Android kills the backgrounded app | Accepted; D19 stands | Persist the draft; decide after testing |
| D62 | Android 16 predictive back | Superseded by D143 | |
| D63 | Schema upgrades | Stored schema version; the ORM adds tables and columns; other changes are numbered upgrade steps ([02](02-data-model.md#schema-upgrades)) | Additive only; decide later |
| D64 | Time storage | UTC, shown in the phone's current time zone | UTC + offset; local time |
| D65 | Date and count basis | Start time | Finish time |
| D66 | Exercise picker | Overlay on the workout page; back closes it | Own page |
| D67 | Back on Dashboard / History | History → Dashboard; Dashboard leaves the app | Confirm exit |
| D68 | Versioning | SemVer display version; version code +1 per release. Amended by D225, D226 | Date-based; counter only |
| D69 | Logging a past session | Not in the app | Manual start/finish |
| D70 | Metric limits | Level, Calories, Rate ≥ 0; no maximums. Amended by D130 | Level ≥ 1, Rate > 0; all > 0; sanity caps |
| D71 | History empty state, total time | "No workouts yet"; total as h:mm:ss | + New Workout button; "1h 2m" |
| D72 | Retiring exercises | Never; exercises stay in the picker | Hidden flag; decide later |
| D73 | Roadmap (US-01) | History and editing split: M3 History, M4 Edit workouts, M5 Dashboard, M6 Tests + release. Amends D50 | One History milestone; delete in M3 |
| D74 | Backlog | Accepted as drafted, with story sizes | |
| D75 | Packages (US-02) | Latest stable versions; NUnit analyzers added. Amended by D218 | Older versions; no analyzers |
| D76 | Removing a cardio entry | Not removable on its own; removed only with its exercise (FR-08) | Add entry button after removal |
| D77 | New exercise contents | Starts with one empty set (strength) or its empty entry (cardio) | Starts empty |
| D78 | Back on Edit with no unsaved changes | History, no prompt (FR-23) | Dashboard |
| D79 | Must-have exercises (US-05) | The reviewer's 44, all in [exercises.json](../src/WorkoutLog.Core/Domain/Exercises/exercises.json) under agreed names. Amends D05 | |
| D80 | Exercise source | free-exercise-db (public domain) plus hand-added exercises; muscleandstrength.com names used only to find gaps | Hand-built only |
| D81 | Exercise scope | Strength + powerlifting, plus carries and cardio ([03](03-exercise-list.md)). No stretching, plyometrics, Olympic lifts or other strongman | Add plyometrics, Olympic, stretching; pad to 1000 |
| D82 | Variant rules | Alternating, one-arm, seated/standing and stance variants fold into the base exercise; equipment, bench angle, grip and attachment stay separate ([03](03-exercise-list.md#selection-rules)) | Keep one-arm; keep every variant; separate stances |
| D83 | Exercise names | Equipment first; no prefix for bodyweight moves, carries and Deadlift | Source names |
| D84 | Carries | Six carries; new StrengthSet.Distance, required, in metres ([03](03-exercise-list.md#strength)). Amends D34 | Weight + duration only; one per equipment; leave out |
| D85 | Assisted machines | Separate exercises; assistance stored in WeightKg, labelled Assist (kg) | Negative weight; not tracked |
| D86 | Cardio list and units | 16 exercises with units as in [03](03-exercise-list.md#cardio); no miles | Just the 6 drafted; separate Treadmill Walk and StepMill |
| D87 | Level and Incline | Level stays a whole number with a per-exercise label; new Incline metric (decimal %). Amends D35 | One decimal Level with a label; unlabelled Level |
| D88 | Field labels | Optional label per metric in the definition | LevelLabel field + WeightIsAssistance flag |
| D89 | Exercise Ids | One sequence 1…N for all types; new exercises take the next number | Ranges per type |
| D90 | Exercise details | Primary muscle, equipment and mechanic stored per strength exercise; not shown or filtered | Filter by muscle/equipment/mechanic |
| D91 | Picker search | Matches every word in any order against name and nicknames, with variants normalised; empty search shows the full list ([02](02-data-model.md#exercise-search)). Amended by D146, D147, D155 | Exact phrase; name only; nothing until you type |
| D92 | Search engine | In memory in `IExerciseCatalog`; ranked by name prefix, then name, then nickname. Amended by D148, D156 | SQLite FTS5 |
| D93 | Dumbbell weight | Per dumbbell | Total; undefined |
| D94 | Exercise list file | [exercises.json](../src/WorkoutLog.Core/Domain/Exercises/exercises.json), Core's embedded file from US-07. Completed by D125 | CSV; Excel; tables in 04 |
| D95 | Pace entry format | Superseded by D168 | |
| D96 | Design system timing | Wireframes and design system before M2, as M1.5 ([EP-09](https://github.com/marty-shields/workout-log-app/issues/9)). Supersedes D32 | Visual design after v1; explore only |
| D97 | Themes | Light and dark; the app follows the phone's setting | Dark only; light only |
| D98 | Number entry | Phone's numeric keyboard; duration as three boxes: h, mm, ss | Steppers; in-app keypad |
| D99 | Prototyping tool | BlazingStory (WebAssembly) in `WorkoutLog.UI.Stories`; stories render the real `WorkoutLog.UI` components | Static HTML; Storybook; Penpot; Figma; BlazingStory Server |
| D100 | Design system docs | Markdown pages in the stories site; [10](10-design-system.md) points to them | Markdown in `docs/` only |
| D101 | Prototype scenarios | One auto-play story per main user task, outlining and tapping each step's target | Numbered steps clicked by hand |
| D102 | Error colour | Error text and borders are red, from US-31 | Red in the wireframe tokens |
| D103 | Validation display | Each faulty field gets an error border; one message per problem under its set or entry, in field order | One combined message per set or entry |
| D104 | Set rows with more than two fields | Superseded by D105 | |
| D105 | Sets with more than two fields, one a duration | One open set with every field labelled; the others show as summary lines and open on tap ([05](05-screens-ux.md#new--edit-workout)). Supersedes D104; amended by D118 | Shaded blocks; dividers; collapse all strength; several open sets |
| D106 | Set validation before moving on | Add set, and opening another set, wait while the current set is empty or has errors; its errors show on it. Amended by T29 (the rules come from Core), D162 | Check without blocking; Save only; block Add set only |
| D107 | Partly filled duration and pace | When focus leaves the field, empty boxes become 0 and minutes and seconds are padded to two digits; an empty field stays empty | On close or Save; no padding; duration only |
| D108 | When errors clear | A set or entry showing errors is re-checked on each change. Add set clears the exercise error; adding an exercise clears the workout error. Amended by D174 | Clear on edit; leave until the next Save |
| D109 | Overlay focus | A confirm dialog focuses Cancel; the Exercise picker focuses its search box | Focus the container; leave focus |
| D110 | Palette and colour roles | Teal palette with derived shades, roles for light and dark, all meeting WCAG AA ([Tokens story](10-design-system.md)) | Palette colours only; change the mapping first |
| D111 | Theme switching | Colour tokens are `light-dark()` pairs; `data-theme` forces a theme on an element; the stories have a System / Light / Dark switch | OS setting only |
| D112 | Danger buttons | Filled red, the same red as errors | Red outline; same as primary |
| D113 | Type and shape | System font; body 16px, number inputs 20px; radius and spacing from the tokens | Bundled font; other sizes and radii |
| D114 | Screen stories | One stories section per screen; dialogs are states of their screen | One Workout section; a Dialogs section |
| D115 | Design System pages | One page per component group and one per screen | One Components and one Screens page |
| D116 | Component story themes | Component stories outside the phone frame use `ThemeLayout`'s System / Light / Dark switch | Light and dark side by side; screens only |
| D117 | Design-system checks | UI.Tests fail on literal colours or sizes, unknown tokens, token pairs below WCAG AA, small touch targets, missing `data-testid`, screen stories that differ from the tests' screen states, and a missing Design System page | Review by hand; BlazingStory internals; a shared state catalogue |
| D118 | Closed set with errors | The summary line gets the red error outline as well as its messages. Amends D105 | Messages only |
| D119 | Confirm dialogs | Anchored to the bottom, inside the safe area, with Cancel and the confirm button full width side by side | Centred, with a text Cancel |
| D120 | Remove exercise | Small muted text, so the exercise name leads the card header | Same weight as the name |
| D121 | Leaving unsaved edits: wording | "Discard changes?" with **Discard / Cancel**. Amends D42 | "Discard your changes?" / **Discard changes** |
| D122 | Exercise search timing | Search added in US-11, after the list in US-07 | Search in US-07 |
| D123 | Seeding renamed or retyped exercises | Seeding updates Name and Type by Id ([02](02-data-model.md#seeding-the-exercise-list)) | Name only |
| D124 | Exercise file checks | Ids 1…N without gaps, the documented counts, metrics fit the type, units only with their metric. Amended by D168 | US-07 checks only |
| D125 | Exercise file location | [src/WorkoutLog.Core/Domain/Exercises/exercises.json](../src/WorkoutLog.Core/Domain/Exercises/exercises.json); docs link to it. Completes D94 | `Resources/`; a copy in docs |
| D126 | Negative values | Weight, distance, floors, level, incline, calories and rate below 0 are an error: "{Label} must be 0 or more". Amended by D161 | Throw; no check |
| D127 | Zero values | A required distance must be more than 0; optional metrics at 0 are valid and stored as 0 | Required means filled only; 0 saved as empty |
| D128 | Input the screen should never send | Unknown exercise, wrong type and a value for a metric the exercise doesn't show are validation errors, not exceptions; nulls throw (T34). Amended by D163, D170 | Throw; strip; show on the workout |
| D129 | Clock earlier than the start on first save | The finish is set to the start, so the total is 0:00:00 and the save succeeds; logged | Save the clock's time; block the save |
| D130 | Maximum values | Every metric has a maximum, held in `MetricLimits`; above it is `TooLarge` ([02](02-data-model.md#metrics)). Amends D70; amended by D168 | Decimals only; 1e15; limits in exercises.json |
| D131 | Front-end process | New or changed screens and components are built and approved in the stories site before the App uses them ([10](10-design-system.md#process)). Amended by D193 | Build in the App, review on the phone |
| D132 | Dev tooling | Visual Studio or VS Code with .NET MAUI; wireless debugging when USB isn't possible ([07](07-dev-setup-build.md)). Amends T08 | VS Code only; emulator |
| D133 | Startup error | "Couldn't open the workout database." under the top bar; no details | Show the exception; crash |
| D134 | Startup loading | `Spinner` with "Loading…" under the top bar | Blank; text only |
| D135 | Android Auto Backup | Cloud backup off (`allowBackup="false"`); phone-to-phone transfer still copies the data | On; block transfer too |
| D136 | Network permissions | None in the manifest (NFR-01); Debug builds get INTERNET from the SDK | Keep the template's |
| D137 | App icon | Superseded by D209 | |
| D138 | Reduced motion | The `Spinner` stands still under `prefers-reduced-motion` | Slow it down; ignore the setting |
| D139 | Log levels | Debug builds from Debug, Release from Information; `Microsoft.*` from Warning | Release from Warning; Information in both |
| D140 | Rendering error | An Error screen: "Something went wrong." and **Restart**, to the Dashboard; an unsaved workout is lost | Message only; reuse the Startup screen |
| D141 | Logging in new work | Every story logs its errors and unexpected events as in [04](04-architecture.md#logging), checked by the Definition of Done. Amends D57 | Rely on the DoD only |
| D142 | Log content | Ids, counts, versions, durations and exception details only; never set or entry values ([04](04-architecture.md#logging)) | Ids-only exception messages |
| D143 | Back routing (US-28) | US-28 builds the real back routing; back on the Dashboard isn't taken, so the app closes ([04](04-architecture.md#navigation)). Supersedes D62 | A spike; prove it in US-11; `Finish()` the activity |
| D144 | Back with a dialog open | Back cancels the dialog and stays on the page | Ignore back; run the screen's back again |
| D145 | New Workout in US-10 | Scope of US-10. Kept: while the clock is before the start, the timer shows 0:00:00 and nothing is logged. Back amended by D179 | Log a warning once; show the negative time |
| D146 | Search word variants | A fixed table of variants; apostrophes removed; hyphens as spaces ([02](02-data-model.md#exercise-search)). Amended by D155. Amends D91 | Strip a trailing "s" from every word |
| D147 | Search matching | Every query word must start a word of the name or nicknames. Amends D91 | Found anywhere in the text |
| D148 | Search ranking | "Name starts with the query" compares the split name and query words. Amended by D156. Amends D92 | First words only |
| D149 | Field labels and units | From the exercise definition, as in [03](03-exercise-list.md#field-labels-and-units); the assisted machines' label is `Assist` and weight always shows kg | Strip " (kg)" in code; `Assist (kg)` with no unit |
| D150 | Remove exercise | Removes at once, with no confirm | A confirm dialog |
| D151 | After a pick | The picker closes and focus moves to the new exercise's first field, scrolling it into view. Amended by D236 | No scroll; scroll without focus |
| D152 | Picker search on open | Empty each time the picker opens | Keep the last search |
| D153 | US-11 scope | Scope of US-11. Amended by D165, D179 | |
| D154 | Picking an exercise already in the workout | Goes to its card instead of adding a second: strength adds a set as Add set does, cardio focuses its entry. Supersedes D22; amended by D236 | A second card; tag or block added exercises |
| D155 | Search words, after review | Any character that isn't a letter or digit separates words; more plural variants; partly typed variants split; queries cut to 50 characters ([02](02-data-model.md#exercise-search)). Amends D91, D146 | Plurals only; leave as is; 100 characters |
| D156 | Search ranking, after review | The first group is names where each query word, in order, starts the name's word in the same place. Amends D92, D148 | Only the last query word may be partial |
| D157 | Number entry format | `.` is the only decimal separator, whatever the phone's language; the accepted forms are in [02](02-data-model.md#entry-formats-ui) | `.` and `,`; the phone's culture; accept `5.`, spaces or `+` |
| D158 | Decimal places | Every decimal metric takes at most 2 places, counting typed digits. Amends D07 | No limit; 3 places; weight only |
| D159 | Whole numbers | Any `.` in a whole-number field is "{Label} must be a whole number" | Accept `8.0` as 8 |
| D160 | Duration text | An empty box counts as 0 and all empty is empty; non-digits and minutes or seconds over 59 are format errors ([02](02-data-model.md#entry-formats-ui)) | Missing until every box is filled; a bad box counts as 0 |
| D161 | Minus sign | A format error, so the UI never sends a negative; Core's `Negative` check stays. Amends D126 | Read as negative |
| D162 | Focus on a blocked set | Focus moves to the blocking set's first field at fault. Amends D106 | Focus stays where it was |
| D163 | US-12 scope and messages | TooLarge messages have no unit; `NotShown`, which the UI can't produce, has no message. Amended by D164, D170. Amends D128 | Units in TooLarge messages; word `NotShown` |
| D164 | Logging in set checks, after review | A Core code that gets no message logs a Warning with ids, code and metric only; a failed focus on a blocked set logs a Warning and the workout stays open. Amends D163; amended by D170, D175 | Nothing logged; the error boundary |
| D165 | US-13 scope | Cardio checks, the Pace metric, whole seconds and Save checks on New Workout in US-13 (XL); the save itself in US-14. Amends D153 | A separate story for the Save checks |
| D166 | When a cardio entry is checked | On Save, and re-checked on each change after that (D174); a re-pick doesn't check it | Re-pick checks it; check on leaving the entry |
| D167 | Pace text | m:ss; an empty box counts as 0; non-digits and seconds over 59 are format errors ([02](02-data-model.md#entry-formats-ui)) | Both boxes needed; max 59:59 |
| D168 | Pace metric | A `Pace` metric (TimeSpan, cardio, optional, per km, max 60:00) replaces Rate on Outdoor Run and Outdoor Walk. Supersedes D95; amends D35, D124, D130 | Rate in seconds; decimal minutes; Pace plus Rate |
| D169 | Whole seconds | A duration or pace with part of a second is `NotWholeSeconds`; one code per metric, in a fixed order ([02](02-data-model.md#validation)) | No check; pace only; log-only |
| D170 | Save wording | Save's rules come from `IWorkoutService.Validate` and the UI words every code; exercise errors show on Save only and are logged once; NotShown stays log-only. Amends D128, D163, D164; amended by D175 | UI rules; a NotShown message |
| D171 | Focus after a failed Save | The first error in screen order ([05](05-screens-ux.md#new--edit-workout)); a closed set stays closed. Amended by D176 | Scroll only; open the closed set |
| D172 | Mistake scenarios | "Workout mistakes", "Strength mistakes" and "Cardio mistakes" prototype scenarios | One per activity type or machine |
| D173 | Error stories | "Save errors" and "Save with no exercises" come from the real check; ExerciseCard gains "Cardio with errors" | Save errors only |
| D174 | Re-check while typing, after review | Once a set or entry fails a check, every change re-checks it, even after it becomes valid, until it passes Add set, opening another set or Save. Amends D108 | Keep D108; check every change from the start |
| D175 | Review fixes (US-13) | Every validator gets D174; Save's exercise errors and the set check log separate Warnings; a cardio exercise with no entry reads as an empty entry. Amends D164, D170 | Leave the logging; log unmapped codes |
| D176 | SaveFailed on New Workout | "Couldn't save the workout. Try again." as the workout error; focus moves to Save; the workout stays. Amends D171 | Focus Add exercise; no focus move |
| D177 | Save while saving | Taps on Save while a save runs, or after one succeeds, are ignored with no visual change. Amended by D179 | Disable Save while saving; no guard |
| D178 | Save failed story | New Workout gains a "Save failed" story; the sample service validates and stores nothing. Amended by D187 | No story |
| D179 | Discard on New Workout | Back always asks "Discard this workout?"; ignored while a save runs and after one succeeds; Discard runs once ([05](05-screens-ux.md#new--edit-workout)). Amends D145, D153, D177 | No prompt when empty; accept a double tap; inert background |
| D180 | History load failure | "Couldn't load your history." and a **Try again** that takes focus and reloads | The Error screen; a Primary button |
| D181 | History loading | `Spinner` "Loading…", shown at once; back works | Blank; spinner after a delay |
| D182 | History order | Start time newest first, then Id newest first | Id descending; finish time |
| D183 | History paging | None: every workout loads at once (FR-20) | `Virtualize`; Show more; infinite scroll |
| D184 | Edit Workout scope | Edit Workout saves as New Workout does and returns to the Dashboard (FR-13). Back amended by D189; Delete by D191 | Return to History after a save |
| D185 | Edit Workout loading | Title "Workout" until it loads; Loading, Load failed with **Try again**, and Not found; back goes to History in these states | Blank; Try again on Not found; the Error screen |
| D186 | History rows open Edit | Tapping a row opens `/workout/{id}`; rows are always tappable | Rows not tappable without `OnOpen` |
| D187 | Edit Workout stories | Edit Workout gains its load, save and Live page stories, on a sample service that stores nothing. Amends D178 | Static states only |
| D188 | Values Edit can't show | Accepted: Edit shows only the metrics in the exercise's current definition and decimals to 2 places; a definition change is made with its data change | Load failed for such a value; keep hidden values |
| D189 | Discard changes on Edit Workout | A change is a value typed, a set added or removed, or an exercise added or removed, even if later undone. Back with changes asks "Discard changes?", without goes to History, and is guarded as D179 ([05](05-screens-ux.md#new--edit-workout)). Amends D184; amended by D236 | Compare with the loaded text or values |
| D190 | Back and discard story | Edit Workout gains a story that moves between the live page, History and the Dashboard as the App does. Amended by D192 | Live page screenshots only; live `HistoryPage` |
| D191 | Delete on Edit Workout | "Delete this workout?" / Delete / Cancel, the same with or without unsaved edits; Delete goes to History; a failed delete keeps the workout and focuses Delete; guarded as D189 ([05](05-screens-ux.md#new--edit-workout)). Amends D184 | The Error screen; a missing id as deleted; visibly disabled buttons |
| D192 | Delete stories | Edit Workout gains "Delete failed"; "Back and discard" becomes "Back, discard and delete". Amends D190 | A separate Delete story |
| D193 | Stories review | The stories site is left running and each new or changed story and page is opened in the reviewer's browser, then stopped once approved ([10](10-design-system.md#process)). Amends D131 | Screenshots only |
| D194 | Dashboard scope | US-20 covers the whole Dashboard in one load; US-21 and US-22 are Done in it and keep their IDs | Separate loads per section; remove US-21 and US-22 |
| D195 | Dashboard loading and errors | A spinner, or "Couldn't load the Dashboard." and **Try again** (focused), replace the last workout and counts; History and New Workout still work | Spinner or error for all content; no Try again |
| D196 | Dashboard counts | In the phone's time zone: this week from Monday 00:00 and this month from the 1st at 00:00, to now; the last workout is the latest start, even after now; a skipped or repeated midnight starts at its first local time. Amends FR-34 | Count to the period end; ignore clock changes |
| D197 | Dashboard reload | Loads each time it's shown and when the activity starts while it's shown, with the spinner; a load while one runs is ignored | Shown only; activity resume; keep the values |
| D198 | Dashboard stories | Dashboard gains Loading, Load failed and Live page | Loading and Load failed only |
| D199 | Load failed component | One `LoadFailed` component for History, Edit Workout and the Dashboard; they look and behave as before | A copy per screen; a component with the spinner |
| D200 | E2E tests | Typical user flows end to end on the Debug build on a device, chosen by the `DeviceUdid` run setting; one at a time; the Debug app's data cleared per test; a locked device fails the test and no PIN is stored ([06](06-testing.md#e2e-tests-appium)) | Emulator only; unlock with a stored PIN; no lock check |
| D201 | US-23 scope | US-23 builds the whole E2E harness | The original AC only; a new story |
| D202 | E2E acceptance criteria | Each Done story with app behaviour gains an **E2E (US-24)** AC; US-24 is Done when the scenarios pass. Amended by D207; the checklist part superseded by D230 | ACs on US-24 only; reopen the stories |
| D203 | E2E scope | Behaviour only, no visual checks; failures a user can't cause stay in UI.Tests ([06](06-testing.md#in-uitests-not-e2e)) | Screenshot checks; failures forced in Debug |
| D204 | E2E tags | `@smoke` on the core journeys; features and scenarios tagged with their stories, FRs and decisions; the full run before each release. Amended by D208 | Smoke only; full run only |
| D205 | E2E clock | Steps work out expected dates from the device's time and time zone; seeded times sit 2 hours from a day boundary; a `@today` scenario fails at its start near midnight | Set the device clock; fixed dates |
| D206 | Back with the keyboard open | With the soft keyboard open, Android back only hides it; the back arrows act at once | |
| D207 | E2E acceptance criteria form | Each **E2E (US-24)** AC is "the scenarios tagged `@US-xx` pass"; the feature files hold the titles. Amends D202; amended by D233 | Feature files and titles in each AC |
| D208 | E2E tag placement | A tag goes on the feature only when every scenario shares it, so `Category=US-xx` runs only that story's scenarios. Amends D204 | Every story tag on the feature |
| D209 | App icon and splash design | A pixel-art notebook with a dumbbell header, ruled lines and spiral binding, white on dark teal; the splash is the same art in both themes. Supersedes D137 | Dumbbell, kettlebell, arm, plate and barbell concepts; themed splash |
| D210 | App icon colours | The five palette colours and white only, checked in UI.Tests against `tokens.css` | Any token colour; by eye only |
| D211 | App icon geometry | A 108-unit adaptive-icon canvas with whole device pixels per sprite pixel, inside the safe circle; the splash uses the same canvas | A larger sprite that masks crop |
| D212 | Themed icon | The foreground is also the monochrome layer; details are cut-outs | A separate monochrome SVG |
| D213 | Debug icon | Debug and Release share the icon and splash; the "(Dev)" label tells them apart | "DEV" on the page; another background |
| D214 | Icon source | SVGs in `src/WorkoutLog.App/Resources/`, embedded by the stories for the App Icon page | Copies in the stories; the UI's `wwwroot` |
| D215 | US-34 checks | No E2E for launcher or splash pixels. The checklist part superseded by D230 | An E2E smoke tag |
| D216 | Backlog on GitHub | The [Workout Log project](https://github.com/users/marty-shields/projects/2) over the repo's issues: epics and stories are issues, stories sub-issues of their epic, milestones on GitHub, Size and Status as project fields ([ways of working](README.md#ways-of-working)). Supersedes D49, D55, D58, T12; amended by D219 | docs as the source; epics as labels |
| D217 | Story workflow | A branch per story; local review before the commit; a DRAFT PR that closes the story, squash-merged by the reviewer ([07](07-dev-setup-build.md#source-control)). Amends D57 | Review on the PR; merge or rebase merges |
| D218 | Package versions | Versions live in `Directory.Packages.props`, `package.json` and the workflows; Dependabot proposes updates and merging one approves it. Amends D75; amended by T97, D232 | Update the docs on each Dependabot PR |
| D219 | Roadmap and backlog docs removed | The GitHub milestones and epics hold the roadmap and backlog; the [README](README.md#ways-of-working) has the ways of working; the docs after them were renumbered. Amends D216, T11, T12 | Keep both docs; keep the old numbers |
| D220 | Keystore backup | One backup of the keystore file, passwords and alias, kept by the owner in a private location the repo doesn't record; the `release` environment's secrets aren't the backup ([07](07-dev-setup-build.md#keystore-handling)) | Secrets as the only copy; a second copy |
| D221 | Root README | A root `README.md` is the repo's front page: features, light and dark screenshots of the screen stories and the stack. Amended by D229, D234 | docs/README.md as the front page; phone screenshots |
| D222 | Keeping the README current | The Definition of Done checks `README.md` still matches the app; a changed screen retakes its README screenshots ([10](10-design-system.md#readme-screenshots)) | A recapture script; memory only |
| D223 | Release cert pin | The release cert's SHA-256 is in [07](07-dev-setup-build.md#keystore-handling) and `CERT_SHA256`; the release fails unless the APK has exactly one signer with it; the fingerprint heads the release notes | Check by hand; no check |
| D224 | Release approval | Only the sign-and-publish job uses the `release` environment, after the other jobs pass; the owner approves it and admins can't bypass | Repo-level secrets; approval before the tests |
| D225 | Version bump | `v1.0.0` first; then the highest `semver:*` label among the PRs since the last tag; a PR without exactly one label fails the run. Amends D68 | Version typed in; Conventional Commit type; unlabelled as patch |
| D226 | Version code | The number of `v*` tags + 1; tags can't be deleted or moved; the workflow passes both versions and the `.csproj` sets neither. Amends D68 | Run number; derived from the version; bumped by a PR |
| D227 | Release contents | `WorkoutLog-X.Y.Z.apk` (arm64), its `.sha256`, and notes from the merged PRs under the cert's fingerprint | arm64 and x64; an AAB; hand-written notes |
| D228 | US-26 and US-27 delivery | US-26 and US-27 close after `v1.0.0` is checked on the phone; the results are a comment on #36; `v1.0.1` is installed over `v1.0.0` to check the data survives | "Closes #35"; a results doc |
| D229 | README install | The root README has an Install section and a latest-release badge. Amends D221 | Install steps in 08 only |
| D230 | Manual checklist removed (US-27) | The manual on-phone checklist leaves the Testing doc, with nothing in its place: the release workflow's E2E and release smoke are the checks. The v1.0.0 results are on #36. Supersedes the checklist parts of D202 and D215; amends D56, T07 | Keep it for each release |
| D231 | Ideas doc removed (US-27) | The Future ideas doc is deleted; each idea is an `enhancement` issue in the project, To do, with no milestone. Supersedes T11; amended by D235 | Keep the doc; one issue for all ideas |
| D232 | No packages or code mirrors in docs (US-27) | Docs name no packages or versions, and don't repeat what code or config holds (layout trees, column lists, config lists, file lists); the approved packages are the ones in `Directory.Packages.props`, `package.json` and the workflows. Amends D218 | Keep 05's package list |
| D233 | Features in the Testing doc (US-27) | The Testing doc lists no features; each feature file describes what it covers. Amends D207 | Keep the Features table |
| D234 | README links (US-27) | The root README has no Documentation section; the docs are in `docs/`. Amends D221 | Keep the links to the docs, decisions, backlog and ideas |
| D235 | Docs describe the app as it is (US-27) | The docs describe only what the released app has: no scope, plans or ideas; a change updates them as it lands. The Vision & scope doc is deleted and the docs renumbered 01–10 (Requirements 01 … Design system 10). Amends D231 | Keep Vision & scope; keep the old numbers |
| D236 | Newest exercise first (#64) | Exercise cards show newest first; sets stay in the order added. A picked exercise goes on top and scrolls into view; picking one already in the workout moves its card to the top and is a change. Saved positions are the order added or last picked, from 0, so saved workouts need no migration ([05](05-screens-ux.md#new--edit-workout)). Amends D21, D151, D154, D189 | Bottom (as before); display only, keeping saved order; moving only when not on top |

## Technical
| # | Decision | Choice | Alternatives |
|---|---|---|---|
| T01 | Framework | .NET MAUI | Kotlin + Compose; Flutter |
| T02 | .NET version | .NET 10 LTS | .NET 11 STS |
| T03 | UI model | MAUI Blazor Hybrid | MAUI XAML + MVVM |
| T04 | Database | SQLite through a lightweight ORM | EF Core; raw ADO.NET |
| T05 | Min Android | API 36 (Android 16) only | MAUI default minimum |
| T06 | Solution structure | App + Core library + Tests. Amended by T13 | Single project + Tests |
| T07 | Testing | Unit, component and E2E tests ([06](06-testing.md)). Amended by D230 | |
| T08 | IDE | VS Code + .NET MAUI extension. Amended by D132 | Visual Studio; Rider |
| T09 | Source control | Superseded by T89 | |
| T10 | Install | USB debugging for dev + signed APK for real use | One or the other |
| T11 | Docs | Superseded by D219, D231 | |
| T12 | Roadmap | Superseded by D216, D219 | |
| T13 | Component location | Razor Class Library `WorkoutLog.UI` so bUnit can test it; App → UI → Core. Amends T06 | Keep in App and drop bUnit |
| T14 | Appium test project | `WorkoutLog.E2E.Tests`, NUnit | Other names; xUnit |
| T15 | Solution file | `WorkoutLog.slnx` | `WorkoutLog.sln` |
| T16 | UI test project | `tests/WorkoutLog.UI.Tests` created in US-30 with the components it tests | Create it in US-04 |
| T17 | Core projects | `WorkoutLog.Core` and `WorkoutLog.Core.Tests` created in US-07 | Do US-04 first; a separate story |
| T18 | Exercise table and seeder | Created in US-07 on an async connection, behind `IExerciseSeeder` | Seeder behind a repository; seeding in US-08 |
| T19 | Core registration | `AddCore(databasePath)` registers Core's types. Amended by T25, T99 | Register everything in `MauiProgram` |
| T20 | Mocking in Core.Tests | Moq added with the first test that uses it | Add it in US-07 |
| T21 | Seeding order | Exercise seeding runs after the schema upgrade steps | Seeding first |
| T22 | Workout repository | One `IWorkoutRepository` over the whole workout, taking and returning immutable domain records; the row classes are internal; position is list order | A repository per table; generic repository; expose the rows |
| T23 | Saving an edited workout | `AddAsync` returns the new Id; `ReplaceExercisesAsync` replaces all children in one transaction and can't change the times | Update children by Id; `UpdateAsync` ignoring the times |
| T24 | Time storage type | `DateTimeOffset`, stored as UTC ticks | `DateTime` |
| T25 | Database startup | `IDatabaseInitializer.InitializeAsync()` checks the version, runs the upgrade steps and creates the tables, then seeds ([02](02-data-model.md#schema-upgrades)). Amends T19; amended by T28, T36 | Upgrader and seeder in `MauiProgram`; carry on with a newer database |
| T26 | Repository checks and reads | No checks beyond the schema's unique and plain indexes ([02](02-data-model.md#tables)); a missing Id throws `KeyNotFoundException`. Amended by T61 | Check exercise Ids; no-op on a missing Id |
| T27 | One database call at a time | An internal `DatabaseGate` singleton; gated decorators run every repository and initializer call through it. Amended by T99 | A connection owner class; a semaphore per class; accept overlaps |
| T28 | Schema upgrade order | Steps run before the ORM creates tables and columns, so a step sees the old schema; every startup checks the steps. Amends T25 | Tables before steps |
| T29 | Workout service | `IWorkoutService` singleton: `StartNew()` returns a draft the page keeps; save, save edits and validate without saving. Amends D106; amended by T68 | Service holds the draft; new workouts only |
| T30 | Validation | `WorkoutValidator` checks the domain records against the catalog and returns codes the UI words; format checks stay in the UI ([02](02-data-model.md#validation)). Amended by T33 | The service parses text; Core builds the messages |
| T31 | Time source | .NET `TimeProvider`, faked in tests. Amended by T47 | Own `IClock`; mocking `TimeProvider` |
| T32 | Save outcome | `SaveResult`; nothing is written when invalid; any repository exception becomes `SaveFailed` | Throw; catch in the page; catch SQLite errors only |
| T33 | Core visibility | Only the services and their records are public; the rest is internal, visible to the mocking library and the stories. Amends T30; amended by T48, T60, T99 | All public; repository only |
| T34 | Null input | A null list, exercise, set, entry or draft throws `ArgumentNullException` at the service. Exception to D128 | Validation errors |
| T35 | Stale XML docs in the stories | The stories fetch the unfingerprinted `.xml` doc files past the browser cache (`loadBootResource`) | Hard reload after each run; no XML docs |
| T36 | Startup gate | `StartupGate` awaits `IDatabaseInitializer.InitializeAsync` and shows the Startup screen until it completes or throws. Amends T25 | Block in `CreateMauiApp`; lazy init in Core |
| T37 | E2E project | Superseded by T77 | |
| T38 | App packages | MAUI pinned in the App project; the template's debug logging package dropped | The workload's version; keep the package |
| T39 | .gitignore | MAUI Android entries from GitHub's VisualStudio.gitignore plus signing files. Amended by T90 | The full VisualStudio.gitignore |
| T40 | NuGet sources | `nuget.config` maps every package to nuget.org only | Machine sources; lock files |
| T41 | Logging seam | Core and UI log through `ILogger<T>`; each host registers its providers; the App writes to logcat with its own `AndroidLogProvider` ([04](04-architecture.md#logging)) | The console provider; own interface |
| T42 | Log file | `FileLogProvider` in Core writes a 1 MB rolling log in the app's external files folder, readable with adb on both builds ([04](04-architecture.md#logging)) | A logging library; internal storage; 5 MB |
| T43 | Logging code | `[LoggerMessage]` methods with fixed EventIds; tests check them with a fake logger. Amended by T66 | `LogX` calls; mocking `ILogger` |
| T44 | Android hooks | Lifecycle and memory events through `ConfigureLifecycleEvents`; unhandled and unobserved exceptions logged; the WebView renderer's end and memory kills read from the next start's exit reason ([04](04-architecture.md#logging)) | `MainActivity` overrides; replace Blazor's WebView client |
| T45 | Exercise catalog load | The catalog reads exercises.json on first use, inside `StartupGate`, so a bad file shows the D133 error | Load in the constructor |
| T46 | Back button | A stack of back handlers in UI, the newest runs; an AndroidX callback in the App, on only while a handler exists; navigation keeps no WebView history ([04](04-architecture.md#navigation)). Amended by T74 | WebView history with `NavigationLock`; a handler per page; `OnBackPressed` |
| T47 | New Workout page | `NewWorkoutPage` in UI starts the draft and runs the timer from `TimeProvider`; the App's page only hosts it. Amends T31; amended by T112 | Logic in the App; log in UI |
| T48 | Stories sample service | `SampleWorkoutService` implements `IWorkoutService` for the stories; the unused browser SQLite native file is removed from the stories build. Amends T33 | A public draft constructor; accept the warning |
| T49 | Adding exercises | The catalog splits search words once; `ExerciseFields` maps definitions to fields; `WorkoutView.Focus` focuses or adds a set; a failed focus is logged and the workout stays open | Add the id in US-14; scroll by JS |
| T50 | Stories on the real catalog | The stories and UI.Tests use the real `IExerciseCatalog`; the stand-in search is removed | Keep `SampleData.Search` |
| T51 | Set checks | `ISetChecker` reads each field's text, calls Core's `ValidateSet` and words the codes; `AddUi()` registers UI's services | A static helper; register inline |
| T52 | Stories on the real set checks | The sample service runs Core's validator; the stand-in rules are removed. Amended by T54 | Fakes in the tests only |
| T53 | Focus after a blocked tap, after review | Add set and opening another set set a flag; `ExerciseCard` focuses the set at fault after its next render, logging a failure | Await the focus in the handler; fire and forget |
| T54 | Workout checker | `IWorkoutChecker` composes `ISetChecker` and `IWorkoutService`: it checks every set and entry, then the workout, and words the codes; the stories use it. Amends T52 | The workout check on `ISetChecker` |
| T55 | Pace storage | `Pace` is a nullable TimeSpan column the ORM adds at startup, so no upgrade step | A schema upgrade step |
| T56 | Focus on the first error | `WorkoutView` takes an `ErrorFocus` request and focuses the first error; a failure is logged and the workout stays open. Amended by T69 | Not logged; the error boundary |
| T57 | Scenario tests type as the player does | `ScenarioTests` types one character at a time and checks each error step's messages | The whole value at once |
| T58 | Saving a new workout | `NewWorkoutPage` saves through `SaveNewAsync` and raises `OnSaved`; the App goes to the Dashboard; `IWorkoutChecker` holds the save-failed wording. Amended by T69 | The page navigates itself |
| T59 | Discarding a new workout | `NewWorkoutPage` shows the Discard prompt, logs the discard with its exercise count and raises `OnDiscarded` | Discard logged in Core; keep `OnBack` |
| T60 | History service | A public `IHistoryService` returns the workouts' ids and starts, or `Failed` with the error logged (as T32). Amends T33; amended by T70 | `IWorkoutService.ListAsync`; catch in the page |
| T61 | StartedAt index | `StartedAt` is indexed, added at startup with no upgrade step; it serves the History order. Amends T26 | No index |
| T62 | History page | `HistoryPage` in UI loads through `IHistoryService` and shows the load states; the App's page only hosts it. Amended by T65, T73 | Load in the App page |
| T63 | Opening a workout | `IWorkoutService.GetAsync` returns Loaded, NotFound or Failed, each logged | `Workout?` and catch in the page |
| T64 | Saved values as text | `ExerciseFields.Saved` turns a saved exercise into the text the user would type; a kind that doesn't match gives Load failed | Map in the page; throw on a mismatch |
| T65 | Edit Workout page | `EditWorkoutPage` in UI loads through `GetAsync`, checks each exercise against the catalog, and shows `WorkoutView` in Edit mode or the load states; the App's page only hosts it. Amends T62; amended by T73, T112 | One page for New and Edit |
| T66 | EventId names | Each project has an internal enum of its EventIds in fixed ranges, checked by tests ([04](04-architecture.md#logging)). Amends T43 | Numbers in place; one shared enum |
| T67 | Discarding edits | `EditWorkoutPage` keeps a changed flag, shows the Discard changes prompt and logs the discard | `OnDiscarded` as T59 |
| T68 | Deleting a workout | `IWorkoutService.DeleteAsync` returns Deleted or Failed, each logged. Amends T29 | `Task<bool>`; a separate service; log in the UI |
| T69 | Delete on the page | `EditWorkoutPage` gains `OnDeleted`; `ErrorFocus` takes a target (first error, Save or Delete). Amends T56, T58 | Reuse `OnBack`; a `focusDelete` flag |
| T70 | Dashboard service | `ListAsync(from, to, limit)` on the repository; a public `IDashboardService` reads the last start and counts the week and month in memory, or returns `Failed` with the error logged. Amends T60 | A repository aggregate query |
| T71 | Dashboard page | `DashboardPage` in UI loads through `IDashboardService` and reloads on app start through `IAppStart` | Load in the App page |
| T72 | App start signal | One `AppLifecycle` in UI: pages register app start handlers through `IAppStart`; the host runs them from the activity's start; a failing handler is logged and the others still run. Amended by T74 | Activity resume; one interface for both |
| T73 | LoadFailed | `LoadFailed` takes a message, `OnRetry` and a test id prefix, and focuses Try again; the screens' own focus events are retired. Amends T62, T65 | A logger per screen |
| T74 | Handlers per activity | Back and app start handlers belong to the current activity: `Attach` on create drops earlier ones, `Detach` on destroy clears them only if that activity is current. Amends T46, T72 | Leave the race; track the activity in the App only |
| T75 | E2E tool | Appium with the UiAutomator2 driver, from NUnit | Selenium; Playwright Android; Maestro; MAUI DevFlow; Espresso-Web |
| T76 | Appium install | Appium and its driver as exact devDependencies in the E2E project's `package.json`, installed with `npm ci --ignore-scripts`; Chromedriver downloads itself locally. Accepted for a local setup: an unchecked Chromedriver download and an unauthenticated Appium on 127.0.0.1. Amended by T106 | Global npm install; a pinned Chromedriver |
| T77 | E2E session | `AppiumSetup` checks the setup, starts Appium on a free port, installs the APK and opens one session for the run; at the end it closes the app and removes Appium's helpers ([06](06-testing.md#e2e-tests-appium)). Supersedes T37; amended by T80, T94 | A session per test; uninstall the Debug app; a server started by hand |
| T78 | Debug APK embeds assemblies | `EmbedAssembliesIntoApk` in Debug, as a Fast Deployment APK can't run when Appium installs it | Embed only for E2E builds |
| T79 | E2E results | One folder per run; a failed test saves its screen, page source, app log and Appium log slice and attaches them ([07](07-dev-setup-build.md#results)) | Save for every test; logcat dumps |
| T80 | E2E BDD | Gherkin features run by Reqnroll on NUnit; `AppHooks` replaces `AppTest`; declarative steps over screen classes. Amends T77 | SpecFlow; NUnit with comments |
| T81 | E2E seeding | The E2E project references Core and builds a seeded database on the PC with a clock set to each workout's times, then copies it into the app with `run-as` | SQL by hand; seed through the UI; an import in the app |
| T82 | E2E typing | Typing taps the field, clears it and types a key at a time; with the keyboard open, focus moves by script; a tap that doesn't focus the field fails the step | The whole text at once; exercise ids in steps |
| T83 | E2E log checks | Steps pull the app's log and check key events and that typed values and validation messages never appear (D142) | logcat |
| T84 | E2E back, focus and keyboard | "Android back" is the back key, which reaches the same callback as the back gesture (T46). Given-steps end with the keyboard hidden; When-steps leave it as a user would (D206). Amended by T85 | An edge swipe; both swipe and key |
| T85 | E2E button taps | Taps on the bottom bar and card buttons hide the soft keyboard first, as a driver tap lands below its target with it open (T82). Amends T84 | Tap with the keyboard open |
| T86 | E2E screen rotation | The run holds the screen in portrait and puts the rotation settings back at the end | Leave rotation to the tester |
| T87 | E2E device commands | Device commands run from the WebView context, as switching to native for each cost most of a run; the keyboard is hidden with the back key, only when an input has focus | Native context per command; Appium's `hideKeyboard` |
| T88 | E2E example rows | Scenario outlines keep one row per device path; other values of a rule are tested in UI.Tests or Core.Tests | Every row on the device; one row per outline |
| T89 | Source control | Public GitHub repo [marty-shields/workout-log-app](https://github.com/marty-shields/workout-log-app), default branch `main`, protected by a ruleset; git and `gh` use the personal account in this repo only ([07](07-dev-setup-build.md#source-control)). Supersedes T09 | Private repo; switch the `gh` account per task |
| T90 | Local settings ignored | `.vscode/` and `.claude/settings.local.json` are gitignored, as they hold machine-specific settings. Amends T39 | Shared VS Code settings in the repo |
| T91 | PR build | `pr-build.yml` builds and runs the Core and UI tests on PRs to `main`, on `ubuntu-latest`, with the SDK and workload set pinned; actions pinned by SHA with least permissions; its check is required ([07](07-dev-setup-build.md#ci)). Amended by T96, T111, T113, T114 | Windows runner; floating SDK; one `ci.yml` |
| T92 | Dependabot | Weekly updates for NuGet, the E2E npm packages, the SDK and the Actions; minor and patch grouped per ecosystem. Amended by T103 | One PR per package; NuGet only |
| T93 | E2E npm overrides | `overrides` lift packages Appium pins to vulnerable versions; the driver's bundled copies can't be overridden and stay tracked in [US-38](https://github.com/marty-shields/workout-log-app/issues/47) | `npm audit fix --force`; hide the bundled entries |
| T94 | Appium on loopback | Appium binds to 127.0.0.1. Amends T77 | The library default |
| T95 | Test results in the PR build | Test steps report failed tests as annotations and a results table in the job summary | TRX plus a script; a third-party action; parallel test jobs |
| T96 | Warnings are errors | `Directory.Build.props` makes warnings errors for every project, so every build fails on one. Amends T91 | `-warnaserror` in the PR build only |
| T97 | Central package versions | `Directory.Packages.props` holds every package version; project files reference packages without versions. Amends D218 | Versions in each project file |
| T98 | Shared project settings | `Directory.Build.props` sets the target framework, nullable and implicit usings for every project, and the test packages for `*.Tests` projects | Settings in each project file |
| T99 | Decorator registration | `AddCore` registers only the gated interfaces, built around inner instances it creates with one connection; nothing outside the gate can reach the database. Amends T19, T27, T33 | Concrete types registered too; a third-party decorator library; keyed services |
| T100 | Ticket review skill | `.agents/skills/ticket-review/SKILL.md` runs three read-only reviews (requirements, code and tests, security) one at a time in fresh contexts, only on request; the agent fixes the dead certs and asks about the rest before the local review | Parallel reviews; inline reviews; re-review after fixes |
| T101 | Release signing | Keystore PKCS12, RSA 4096, 10000 days; the file, password and alias are `release` environment secrets; signing properties only on the release job's command line, passwords as `env:` references; restore before the keystore is written; the keystore deleted after the build | Signing in the `.csproj`; `apksigner` after an unsigned build |
| T102 | PR checks | `pr-checks.yml` requires a Conventional Commit title and exactly one `semver:*` label ([07](07-dev-setup-build.md#ci)) | A step in the PR build; the label from the type |
| T103 | Dependabot labels | Dependabot PRs get `semver:patch` and the `chore(deps)` prefix. Amends T92 | A label per update type |
| T104 | Release workflow | `release.yml`, run by hand on `main`, one run at a time; jobs `version`, then `build`, `e2e` and `release-smoke`, then `release`; its shell in `.github/scripts/` ([07](07-dev-setup-build.md#release-signed-apk)). Amended by T113 | A release per merge; a tag push trigger; inline shell; third-party actions |
| T105 | E2E on emulators | A matrix of jobs with one emulator each, set up by scripts with no third-party actions, on the AOSP image; scenarios split by whole feature; results uploaded for 1 day; the tests step times out at 30 minutes ([07](07-dev-setup-build.md#qa-pipeline)). Amended by T113 | One emulator; the `google_apis` image; smaller emulators; split by scenario |
| T106 | Chromedriver in CI | `release.yml` pins Chromedriver by version and SHA-256 and fails when the emulator's WebView major differs; local runs still download their own. Amends T76; amended by T113 | Autodownload in CI |
| T107 | Release smoke | The Release APK of the same commit, x64 with the debug key, must start cleanly on an emulator: the start line, the exercise list seeded and startup finished, with no error or crash | Launch the signed arm64 APK; no check |
| T108 | E2E harness fixes | `WebView.Text` reads inside its wait; failure folder names drop characters Windows and artifacts don't allow | |
| T109 | E2E retries in CI | `e2e-run.sh` re-runs only the failed scenarios, up to 3 tries; a pass on a retry gets a warning. Local runs don't retry | NUnit `[Retry]`; a testing-platform extension; no retries |
| T110 | Decisions log format (US-27) | Choice is the decision in 1–2 sentences, linking to the doc that holds the detail; Alternatives are option names; superseded rows keep only their title and successor; changed rows point both ways ("Amends" / "Amended by"); dates are in the header | Keep the full rows |
| T111 | Exact SDK in CI (US-27) | `global.json` uses `rollForward: patch`, so CI installs exactly its SDK version; a new patch comes as a Dependabot PR. Locally a newer patch still works when that one isn't installed. Amends T91 | `latestPatch` (CI took 10.0.403, whose download failed); a version per workflow |
| T112 | Shared workout editing (#62) | `WorkoutEditor` (UI), a transient each workout page injects, holds the exercises, picker, changes and the check and save for New Workout, Edit Workout and the stories prototype; each page keeps its own load, timer, back, discard and delete ([04](04-architecture.md#dependency-injection)). Amends T47, T65 | A shared Razor component; a base page class; one page for New and Edit |
| T113 | E2E on PRs (#62) | `e2e.yml` runs the E2E suite on every PR to `main` and in the release; its `emulator-e2e-tests` job is a required check; the emulator's AVD and image are set in `emulator-start.sh` ([07](07-dev-setup-build.md#qa-pipeline)). Amends T91, T104, T105, T106. Amended by T114 | Release only; skip docs-only PRs; required per shard |
| T114 | E2E after the dev pipeline (#64) | The workflows are renamed: `pr-build.yml` is the PR caller, `dev-pipeline.yml` builds and runs the Core and UI tests, `qa-pipeline.yml` runs the E2E suite. On PRs to `main` the QA pipeline runs only if the dev pipeline passes; the required checks are `dev / build-and-test` and `qa / emulator-e2e-tests`; the release still runs both side by side ([07](07-dev-setup-build.md#ci)). Amends T91, T113 | Build and E2E side by side; one workflow for both |
