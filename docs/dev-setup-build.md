# Dev Setup & Build
Also: [Windows setup](dev-setup-windows.md), [Linux setup](dev-setup-linux.md), [E2E tests](e2e-tests.md) and [Release](release.md) (signed APK, keystore).

## App identity
- Release: **Workout Log**, package id **`uk.martinshields.workoutlog`**. The id can't change without a reinstall, which wipes all data.
- Debug: **Workout Log (Dev)**, package id **`uk.martinshields.workoutlog.debug`**. It installs alongside the release app with its own data, so USB debugging never touches real workouts.
- Version: SemVer display version; the version code is the number of `v*` tags + 1. The release workflow sets both; local builds are `1.0.0` (1).

## Prerequisites
- .NET 10 SDK: the version in `global.json` or a later patch.
- Git and the GitHub CLI (`gh`).
- The rest depends on your OS: [Windows](dev-setup-windows.md#prerequisites) or [Linux](dev-setup-linux.md#setup).

The stories need only the .NET SDK.

## Stories (design system)
`dotnet run --project src/WorkoutLog.UI.Stories`, then open http://localhost:5056. Story files are `*.stories.razor` in `src/WorkoutLog.UI.Stories/Stories/`. `main` is published at https://marty-shields.github.io/workout-log-app/.

### README screenshots
`README.md` shows screen stories, light and dark, from `docs/images/readme/`. To retake one, run the stories, then with Playwright set the colour scheme and open `http://localhost:5056/iframe.html?id=<story id>&viewMode=story`. Screenshot the `.phone` element with a transparent background to `<file>-light.png` and `<file>-dark.png`.

| File | Story id |
|---|---|
| `dashboard` | `screens-dashboard--with-workouts` |
| `workout-summary` | `screens-workout-summary--records-and-changes` |
| `new-workout` | `screens-new-workout--strength` |
| `cardio` | `screens-new-workout--cardio` |
| `last-time` | `screens-new-workout--last-time` |
| `notes` | `screens-new-workout--notes` |
| `add-exercise` | `screens-add-exercise--results` |
| `workouts` | `screens-workouts--with-workouts` |
| `settings` | `screens-settings--with-workouts` |
| `exercise-lift` | `screens-exercise--lift-top-set` |
| `exercise-cardio` | `screens-exercise--cardio-distance` |

`icon.svg` is the app icon's foreground on its background with rounded corners; rebuild it when the icon changes.

## Phone setup (Android 16)
1. Settings → About phone → tap **Build number** 7 times to enable Developer options.
2. Developer options → turn on **USB debugging**, connect by USB and accept the RSA prompt. `adb devices` should list the phone.

Without USB, use wireless debugging (same Wi-Fi): turn on **Wireless debugging**, **Pair device with pairing code**, run `adb pair <ip>:<pairing port> <code>`, then `adb connect <ip>:<port>` with the port on the main Wireless debugging screen. The port changes when wireless debugging restarts.

## Development: run on the phone
Pick the phone as the target in Visual Studio or VS Code, or run `dotnet build src/WorkoutLog.App -t:Run -f net10.0-android`. Debug builds embed their assemblies, so the APK runs on its own.

## Reading the logs
- **Logcat:** `adb logcat -s WorkoutLog` (add `-d` to print and stop).
- **File:** `adb pull /sdcard/Android/data/<package id>/files/logs/`. It holds `workoutlog.log` and, past 1 MB, `workoutlog.1.log`. Without external storage it's in the app data folder; only the Debug app's can be read: `adb shell run-as uk.martinshields.workoutlog.debug cat files/logs/workoutlog.log`.

## CI
Every PR to `main` runs these; the four checks must pass before it can merge:
- **PR build** (`pr-build.yml`): the dev pipeline (`dev-pipeline.yml`) checks the E2E step bindings, builds, runs the Core and UI tests with coverage, tests the CI scripts, lints them and the workflows, and checks `allowBackup` and `dataExtractionRules` in the Debug and Release APKs and that the arm64 Release APK holds arm64-v8a only (`check-abi.sh`, as the release does) (`dev / build-and-test`, `dev / backup-manifest`). `dev / build-and-test` also keeps the Debug APK and the E2E test output as the `e2e-build` artifact. Then the QA pipeline (`qa-pipeline.yml`) runs the E2E tests on emulators in 8 shards, each downloading `e2e-build` and restoring the E2E project instead of building, retrying failed scenarios up to 3 tries (`qa / emulator-e2e-tests`). The release workflow runs the same two pipelines, the QA pipeline after the dev pipeline.
- **PR checks** (`pr-checks.yml`): a Conventional Commit title and exactly one `semver:major`, `semver:minor` or `semver:patch` label (`pr-checks`).

Two more run on a PR only when it changes the paths they test, and aren't required checks. The release workflow runs both on every release ([E2E tests](e2e-tests.md#other-builds)):
- **Release E2E** (`release-e2e.yml`, for `tests/WorkoutLog.E2E.Tests`, `src/WorkoutLog.Core/Export`, `src/WorkoutLog.Core/Import` and `src/WorkoutLog.Core/Data`): `build` builds the x64 Release APK once and keeps it with its debug keystore as the `release-apk` artifact. Then two jobs run on emulators. `release-e2e` checks that it starts cleanly (`release-smoke.sh`), then runs the `@release` and `@backup` scenarios on it. `release-upgrade-e2e` builds the latest `v*` tag's x64 Release APK with that keystore and runs the upgrade check from it.
- **Upgrade E2E** (`upgrade-e2e.yml`, for `tests/WorkoutLog.E2E.Tests` and `src/WorkoutLog.Core/Data`): builds the Debug APK of the latest `v*` tag (`previous-apk.sh`) and of this commit in one job, so the same debug key signs both, then runs the upgrade check (`upgrade-e2e.sh`): the tag's own E2E project saves workouts in its build, and this commit's `@upgrade` scenario checks them after installing over it ([The upgrade check](e2e-tests.md#the-upgrade-check)).

Each emulator job (the QA shards, Release E2E and Upgrade E2E) starts the emulator first, so it boots while the job builds or downloads what it tests, then ends with the `emulator-e2e` action (`.github/actions/emulator-e2e`): Appium, the emulator wait, the pinned Chromedriver, the tests under a timeout, the emulator log and the results artifact. The log and the results upload after a failure or a timeout too.

Keep in step:
- NuGet restores are locked: each project commits a `packages.lock.json`, and CI (`CI` set) fails a restore that would change one (`RestoreLockedMode`). After changing a package or its version, run `dotnet restore WorkoutLog.slnx --force-evaluate` and commit the lock files. Dependabot's NuGet PRs rewrite them. Restore the App without `-r` and publish with `-r ... --no-restore`; a restore with `-r` doesn't match the App's lock file, which lists `android-arm64` and `android-x64` (`RuntimeIdentifiers` in the App's project file). A publish with `-r` packs that one ABI only, because the project file narrows `RuntimeIdentifiers` to `RuntimeIdentifier`; without `-r` the APK holds both.
- When the SDK in `global.json` changes, set the workload set `--version` to match (`dotnet workload search version`) in the same PR, in `.github/actions/setup-maui/action.yml`. Every job that builds the App or the stories installs the SDK and the workload through that action. The workload set decides the WebAssembly SDK pack version the Stories lock file records, so a job without it fails the locked restore.
- Chromedriver is pinned in `qa-pipeline.yml` (`CHROMEDRIVER_VERSION`, `CHROMEDRIVER_SHA256`, from [Chrome for Testing](https://googlechromelabs.github.io/chrome-for-testing/)); `chromedriver.sh` reads the pin for every E2E job and `e2e-local.sh`. When the emulator's WebView major version changes, the jobs fail: pin the matching version and its SHA-256.

On a Linux machine set up for `e2e-local.sh`, see [Keeping it current](dev-setup-linux.md#keeping-it-current).

Coverage and script tests:
- `dev / build-and-test` collects Cobertura coverage for the Core and UI tests. The job summary lists line and branch % of `WorkoutLog.Core` (from the Core tests) and `WorkoutLog.UI` (from the UI tests) (`coverage-summary.py`) and the `coverage` artifact holds the XML (kept 14 days). The step fails when either line % is more than 1 point below `.github/coverage-baseline.tsv`. Refresh the baseline when coverage moves on purpose, from the XML of both runs: `python3 -I .github/scripts/coverage-summary.py --tsv WorkoutLog.Core=<Core xml> WorkoutLog.UI=<UI xml> > .github/coverage-baseline.tsv` (the `coverage` artifact of a green run has them). Locally: `dotnet test tests/WorkoutLog.Core.Tests --collect "Code Coverage;Format=cobertura" --results-directory <folder>`, then `python3 -I .github/scripts/coverage-summary.py [--baseline .github/coverage-baseline.tsv] WorkoutLog.Core=<Core xml>` (one `assembly=file` per run).
- `.github/scripts/tests/run.sh` tests `next-version.sh` (a stub `gh` answers with canned PR JSON, in a throwaway git repo), the shard grouping in `e2e-shard-plan.sh` (the part of `e2e-shard.sh` after the `dotnet test --list-tests` call) and `failed_tests` / `filter_for` in `e2e-trx.sh` (sourced by `e2e-run.sh`), against the sample files in `.github/scripts/tests/fixtures` (the trx sample is named `.xml` because `.gitignore` excludes `*.trx`). It also tests `check-filler-comments.py` and the gate of `coverage-summary.py` on small sample files. It also tests which commit's E2E project `upgrade-seed-ref.sh` picks to save the upgrade check's workouts, in a throwaway git repo. It also tests `check-abi.sh` on zips with different native library folders. It needs bash, git, jq, awk and python3, and runs as a step of `dev / build-and-test`. Add a `*-test.sh` there for another script.

Doc comments:
- `.github/scripts/check-filler-comments.py` fails on a `<summary>` or `<param>` in `src` or `tests` whose words are all in the name of the member, parameter or type, such as "Exercise id." on `ExerciseState.Id`. Delete the comment or say something the name doesn't. A component `[Parameter]` is skipped (the stories site shows its summary), and a real exception takes `/// <!-- filler-ok: why -->`. `Directory.Build.props` turns off CS1591 and CS1573, so a public member needs no comment. Run it with `python3 -I .github/scripts/check-filler-comments.py [paths]`; its tests are in `.github/scripts/tests`.
- `.github/scripts/lint-ci.sh` runs `shellcheck` over `.github/scripts/*.sh` and `tests/*.sh` and actionlint over `.github/workflows` (it shellchecks the `run` steps too). It needs `shellcheck` on the PATH (`apt install shellcheck`; the runner image has it). It downloads actionlint at the version pinned in the script, after checking its SHA-256; to bump it, take the new version's `linux_amd64.tar.gz` digest from the release's checksums file. A real exception takes `# shellcheck disable=SCxxxx # why` above the line; `.github/scripts/.shellcheckrc` lets it follow `source`.

E2E shards and step bindings:
- `e2e-shard.sh` gives each shard scenarios balanced by time, leaving out `@upgrade` and `@backup`: the longest first, each to the shard with the least time so far. It reads the median seconds of each scenario method (a Scenario Outline's rows summed, so row numbers in their names do not matter) from `.github/e2e-durations.tsv`; a row of a method missing from it counts as 15 s. The rows of a Scenario Outline stay in one shard, as do scenarios whose name starts with another scenario's name (the filter matches by name prefix). The shard log shows how many scenarios of its shard are not in the table.
- Refresh the table when scenarios are added or renamed, or their times change. Download the `e2e-results-*` artifacts of recent successful PR build runs (kept 1 day), then make the table from their trx files:
  ```
  gh run list --workflow "PR build" --status success --limit 6
  gh run download <run-id> --pattern "e2e-results-*" --dir <folder>/<run-id>   # for each run
  .github/scripts/e2e-durations.sh $(find <folder> -name "*.trx") > .github/e2e-durations.tsv
  ```
- `.github/scripts/e2e-step-check.py` matches every step in the feature files to the step definitions, as Reqnroll does when a scenario runs, and fails on a step with no definition or more than one. Run it locally with `python3 -I .github/scripts/e2e-step-check.py tests/WorkoutLog.E2E.Tests` (`py -I` on Windows).

Also: Dependabot opens weekly update PRs (`chore(deps): …`, `semver:patch`); every push to `main` publishes the stories to GitHub Pages (`stories-pages.yml`). Actions are GitHub-owned only, pinned by commit SHA.

## Source control
- GitHub: [marty-shields/workout-log-app](https://github.com/marty-shields/workout-log-app), public, default branch `main`.
- `main` is protected by the `Protect main` ruleset: no direct pushes, force pushes or deletion; changes go through a PR with the [CI](#ci) checks passing. PRs are opened as drafts; the reviewer marks them ready and squash-merges. Merged branches are deleted.
- Branches: `type/<issue>-slug` (e.g. `ci/123-faster-e2e-shards`, `fix/118-skip-future-workouts`); work with no issue leaves out the number (e.g. `chore/update-readme`).
