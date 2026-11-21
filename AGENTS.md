# AGENTS.md — Workout Log

Personal Android app (.NET 11 MAUI Blazor Hybrid). Setup: [docs/dev-setup-build.md](docs/dev-setup-build.md) ([Windows](docs/dev-setup-windows.md), [Linux](docs/dev-setup-linux.md)). E2E runs: [docs/e2e-tests.md](docs/e2e-tests.md). Release: [docs/release.md](docs/release.md). Screen behaviour: the design system's [screen pages](src/WorkoutLog.UI.Stories/Stories/Screens/) (BlazingStory). Work items and their acceptance criteria: GitHub issues.

## Commands
- Build: `dotnet build WorkoutLog.slnx` (warnings fail the build)
- Try a UI kit change locally: `dotnet build WorkoutLog.slnx -p:UseKitSource=true -p:KitPath=<kit clone>` builds every kit reference (`Shields.UI`, `Shields.UI.Stories`, `Shields.UI.Testing`) from the kit source instead of its package (`KitPath` defaults to `../app-ui-kit`; a relative `KitPath` is relative to the repo root, the folder holding `Directory.Build.props`, so pass an absolute one from a worktree under `.claude/worktrees`; the committed lock files are left alone then)
- Code style: `dotnet format WorkoutLog.slnx --verify-no-changes` (CI runs it; drop `--verify-no-changes` to apply the fixes)
- Test: `dotnet test tests/WorkoutLog.Core.Tests` and `dotnet test tests/WorkoutLog.UI.Tests`
- E2E (needs a device; a plain `dotnet test` skips them): build the App, then `dotnet test tests/WorkoutLog.E2E.Tests --filter "Category=smoke"`
- E2E without a device (Linux): `.github/scripts/e2e-local.sh "<filter for what you changed>"` boots an emulator, runs the filter, tears it down; exit 2 means skipped, so say the tests did not run ([details](docs/dev-setup-linux.md#throwaway-emulator-for-agents)). The full suite is CI-only (8 shards); one emulator takes an hour. The script refuses a broad filter, more than 15 scenarios or a run over 10 minutes; `E2E_ALLOW_BROAD=1` is the deliberate opt-in.
- Red emulator shard: read the job summary's `infra` or `test` verdict (`classify-shard-failure.sh`). Rerun the failed job once, and only on `infra`. On `test`, or when the rerun fails too, fix the cause instead ([rule](docs/e2e-tests.md#a-red-emulator-shard)).
- Story layout (needs Chrome and the stories running): `node .github/scripts/check-story-layout.mjs http://localhost:5056 <chrome>` fails on clipped, overflowing or wrapping story content at the narrowest card; CI runs it as `story-layout`, and `data-wrap-ok` marks text allowed to wrap.
- README screenshots (needs Chrome and the stories running): `node .github/scripts/readme-screenshots.mjs http://localhost:5056 <chrome>` retakes every screenshot in the [README table](docs/dev-setup-build.md#readme-screenshots), light and dark; `--check` fails on any that no longer match their story: more than 0.01% of pixels (about 40 of 398,000) whose colour moved by more than 24 (summed RGBA difference), so a one-word text change fails (252 pixels measured) while an unchanged screen differs by 0 locally; CI's Chrome may differ, so the PR's `story-layout` run is the cross-build check (measurements in the same section). CI runs the check in `story-layout` and uploads retaken images as the `readme-screenshots` artifact when it fails.
- CI lint (needs `shellcheck`): `.github/scripts/lint-ci.sh` runs shellcheck over the scripts and actionlint over the workflows; CI runs it in `dev / build-and-test`.
- Docs: `python3 -I .github/scripts/check-docs.py` fails on a relative link or `#anchor` in a tracked `*.md` that doesn't resolve, or a README screenshot and its table row that don't match; CI runs it in `dev / build-and-test`.
- Design system: `dotnet run --project src/WorkoutLog.UI.Stories.Host` → http://localhost:5056. Every story and page title starts with `StoryTitles.Prefix` ("Workout Log"), so story ids do too, e.g. `workout-log-screens-this-week--mid-week` (`StoryTitleTests`).

## How the code fits
- Projects: App → UI → Core; Stories.Host → Stories → UI. `WorkoutLog.UI.Tests` reads the App's files because it can't reference it.
- **This repo holds no custom UI elements.** Every general component, style and token comes from the shared kit ([app-ui-kit](https://github.com/marty-shields/app-ui-kit): `Shields.UI`, `Shields.UI.Stories`, `Shields.UI.Testing`, one pinned version in `Directory.Packages.props`); the app holds screens and the few app-only components on the allow-list. A missing component, style or token is requested in the kit (open an issue there), not built here. Guards: `AppComponentAllowListTests` fails on a `*.razor` file or `ComponentBase` class not on its list (each with a one-line reason); `LayoutOnlyCssTests` allows app CSS to lay out only (no colour, background, border, shadow, outline, font); `NoAppTokensTests` fails on an app `tokens.css` or any `--x:` declaration; `KitDuplicateTests` fails on a component sharing a name with a kit one. Try a kit change before it is released with `UseKitSource` (see Commands).
- Screens: a `*Page` calls services, a `*View` takes parameters. `StoryCoverageTests` requires a screen story for every `*View`.
- Services return a status (`ExportResult`), not exceptions.
- Every database call goes through `DatabaseGate` (`DatabaseGateTests`).
- Event ids: `CoreEvent` 1000s, `UiEvent` 2000s, `AppEvent` 5000s. `CoreEventTests` and `EventIdTests` fix the values and ranges.
- Styling: kit tokens only, no literal colours or sizes, no custom properties of our own (`TokensOnlyCssTests`, `ContrastTests` and `TouchTargetTests` run the kit's `Shields.UI.Testing` checks over this app's CSS and markup).
- Logs never hold workout contents (`NotesNotLoggedTests`, `Logging.feature`).
- Docs: `ReadmeDocsTests` checks the README's screenshot table and Project layout.

## Rules
- Ask about any ambiguity.
- No new packages without approval.
- XML comments only when they add something the name and type don't (a unit, a range, what null means, an order, a why). Component parameters are the exception. Never restate the name. Build warnings no longer ask for them, and CI's `check-filler-comments.py` fails a `<summary>` or `<param>` that only restates the name (a real exception gets `/// <!-- filler-ok: why -->`).
- Interfaces only where something else implements them, such as a test fake or the stories site. No interface for a single implementation nothing swaps.
- Commit on a branch, push it and open a ready (non-draft) PR when the work is done, so all CI runs (a draft skips the heavy jobs). Merge only when the user says to.
- Epics: a task of an epic issue gets a `task/<epic>-<id>` branch and a PR into the epic's `epic/<name>` branch, not `main`. You may squash-merge that PR yourself once its checks are green, a verifier other than the author has run every acceptance criterion on the issue and every review finding is fixed. Put open questions in the PR, then add them to the epic PR description when you merge. Never merge an epic PR ([how](docs/epics.md)).
- PR titles are Conventional Commits with exactly one `semver:major|minor|patch` label (CI checks both).
- A PR that changes the UI includes screenshots of the changed screens, posted as a PR comment or in the body, before it is marked ready.
- A PR that changes how a screen in the README looks (its markup, CSS, tokens, or the sample data its story shows) retakes the README screenshots with `readme-screenshots.mjs` and commits them in the same PR. Run `--check` before marking it ready; never leave them stale for later.
