# AGENTS.md — Workout Log

Personal Android app (.NET 11 MAUI Blazor Hybrid). Setup: [docs/dev-setup-build.md](docs/dev-setup-build.md) ([Windows](docs/dev-setup-windows.md), [Linux](docs/dev-setup-linux.md)). E2E runs: [docs/e2e-tests.md](docs/e2e-tests.md). Release: [docs/release.md](docs/release.md). Screen behaviour: the design system's [screen pages](src/WorkoutLog.UI.Stories/Stories/Screens/) (BlazingStory). Work items and their acceptance criteria: GitHub issues.

## Commands
- Build: `dotnet build WorkoutLog.slnx` (warnings fail the build)
- Code style: `dotnet format WorkoutLog.slnx --verify-no-changes` (CI runs it; drop `--verify-no-changes` to apply the fixes)
- Test: `dotnet test tests/WorkoutLog.Core.Tests` and `dotnet test tests/WorkoutLog.UI.Tests`
- E2E (needs a device; a plain `dotnet test` skips them): build the App, then `dotnet test tests/WorkoutLog.E2E.Tests --filter "Category=smoke"`
- E2E without a device (Linux): `.github/scripts/e2e-local.sh "<filter for what you changed>"` boots an emulator, runs the filter, tears it down; exit 2 means skipped, so say the tests did not run ([details](docs/dev-setup-linux.md#throwaway-emulator-for-agents)). The full suite is CI-only (8 shards); one emulator takes an hour. The script refuses a broad filter, more than 15 scenarios or a run over 10 minutes; `E2E_ALLOW_BROAD=1` is the deliberate opt-in.
- Red emulator shard: read the job summary's `infra` or `test` verdict (`classify-shard-failure.sh`). Rerun the failed job once, and only on `infra`. On `test`, or when the rerun fails too, fix the cause instead ([rule](docs/e2e-tests.md#a-red-emulator-shard)).
- Story layout (needs Chrome and the stories running): `node .github/scripts/check-story-layout.mjs http://localhost:5056 <chrome>` fails on clipped, overflowing or wrapping story content at the narrowest card; CI runs it as `story-layout`, and `data-wrap-ok` marks text allowed to wrap.
- README screenshots (needs Chrome and the stories running): `node .github/scripts/readme-screenshots.mjs http://localhost:5056 <chrome>` retakes every screenshot in the [README table](docs/dev-setup-build.md#readme-screenshots), light and dark; `--check` fails on any that no longer match their story. CI runs the check in `story-layout` and uploads retaken images as the `readme-screenshots` artifact when it fails.
- CI lint (needs `shellcheck`): `.github/scripts/lint-ci.sh` runs shellcheck over the scripts and actionlint over the workflows; CI runs it in `dev / build-and-test`.
- Docs: `python3 -I .github/scripts/check-docs.py` fails on a relative link or `#anchor` in a tracked `*.md` that doesn't resolve, or a README screenshot and its table row that don't match; CI runs it in `dev / build-and-test`.
- Design system: `dotnet run --project src/WorkoutLog.UI.Stories` → http://localhost:5056

## How the code fits
- Projects: App → UI → Core; Stories → UI. `WorkoutLog.UI.Tests` reads the App's files because it can't reference it.
- Screens: a `*Page` calls services, a `*View` takes parameters. `StoryCoverageTests` requires a screen story for every `*View`.
- Services return a status (`ExportResult`), not exceptions.
- Every database call goes through `DatabaseGate` (`DatabaseGateTests`).
- Event ids: `CoreEvent` 1000s, `UiEvent` 2000s, `AppEvent` 5000s. `CoreEventTests` and `EventIdTests` fix the values and ranges.
- Styling: tokens only, no literal colours or sizes (`TokensOnlyCssTests`).
- Logs never hold workout contents (`NotesNotLoggedTests`, `Logging.feature`).
- Docs: `ReadmeDocsTests` checks the README's screenshot table and Project layout.

## Rules
- Ask about any ambiguity.
- No new packages without approval.
- XML comments only when they add something the name and type don't (a unit, a range, what null means, an order, a why). Component parameters are the exception. Never restate the name. Build warnings no longer ask for them, and CI's `check-filler-comments.py` fails a `<summary>` or `<param>` that only restates the name (a real exception gets `/// <!-- filler-ok: why -->`).
- Interfaces only where something else implements them, such as a test fake or the stories site. No interface for a single implementation nothing swaps.
- Commit on a branch, push it and open a ready (non-draft) PR when the work is done, so all CI runs (a draft skips the heavy jobs). Merge only when the user says to.
- PR titles are Conventional Commits with exactly one `semver:major|minor|patch` label (CI checks both).
- A PR that changes the UI includes screenshots of the changed screens, posted as a PR comment or in the body, before it is marked ready.
- A PR that changes how a screen in the README looks (its markup, CSS, tokens, or the sample data its story shows) retakes the README screenshots with `readme-screenshots.mjs` and commits them in the same PR. Run `--check` before marking it ready; never leave them stale for later.

## Autonomous runs
Applies when an agent or tool, such as the `/factory:build` plugin, takes a plan to a pull request without stopping for the user. It opens a ready PR and never merges unasked.
1. **Track what you create** in a `Run resources` `<details>` list in the PR body, updated as you go: worktrees (including any outside the repo), branches pushed (the story branch, `pr-screenshots/*`, `assets/*`), background tasks and processes or ports started, and temp dirs.
2. **Build** in a new worktree off `origin/main`, under the rules above.
3. **Gate.** These changes need the user's explicit approval. Either the plan says the change is intended, or the PR's question list asks the user to decide before it is merged:
   - the diff touches `*.razor`, `*.css`, `src/WorkoutLog.UI.Stories/**` or `wwwroot/**`
   - the diff touches EF migrations, the `DbContext` or model configuration
   - the diff lowers or removes a value in `.github/coverage-baseline.tsv` (a PR could otherwise refresh its own floor with no human stop)
   - the verifier's verdict isn't clean, or the change departs from the plan's acceptance criteria

   For a UI change, retake the README screenshots (`readme-screenshots.mjs`, see Commands) and commit them. Then screenshot every changed story and every screen that uses a changed component, before (`origin/main`) and after, in light and dark, at phone width. Look at each one. Push them to a `pr-screenshots/<slug>` branch (tracked in step 1) and put them in the PR body as a table: story, before, after.
4. **Clean up** once the user has merged or closed the PR: stop the tasks and processes on the `Run resources` list, `git worktree remove --force` each worktree (delete a leftover folder, then `git worktree prune`), delete each branch locally and on `origin`, and delete the temp dirs and the session scratchpad. Never touch `main`, anything not on the list, or anything with uncommitted work; report those instead.
