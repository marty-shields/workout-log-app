# AGENTS.md — Workout Log

Personal Android app (.NET 10 MAUI Blazor Hybrid). Setup: [docs/dev-setup-build.md](docs/dev-setup-build.md) ([Windows](docs/dev-setup-windows.md), [Linux](docs/dev-setup-linux.md)). E2E runs: [docs/e2e-tests.md](docs/e2e-tests.md). Release: [docs/release.md](docs/release.md). Screen behaviour: the design system's [screen pages](src/WorkoutLog.UI.Stories/Stories/Screens/) (BlazingStory). Work items and their acceptance criteria: GitHub issues.

## Commands
- Build: `dotnet build WorkoutLog.slnx` (warnings fail the build)
- Test: `dotnet test tests/WorkoutLog.Core.Tests` and `dotnet test tests/WorkoutLog.UI.Tests`
- E2E (needs a device; a plain `dotnet test` skips them): build the App, then `dotnet test tests/WorkoutLog.E2E.Tests --filter "Category=smoke"`
- E2E without a device (Linux): `.github/scripts/e2e-local.sh "<filter for what you changed>"` boots an emulator, runs the filter, tears it down; exit 2 means skipped, so say the tests did not run ([details](docs/dev-setup-linux.md#throwaway-emulator-for-agents)). The full suite is CI-only (8 shards); one emulator takes an hour. The script refuses a broad filter, more than 15 scenarios or a run over 10 minutes; `E2E_ALLOW_BROAD=1` is the deliberate opt-in.
- Design system: `dotnet run --project src/WorkoutLog.UI.Stories` → http://localhost:5056

## Rules
- Ask about any ambiguity.
- No new packages without approval.
- Commit on a branch, push it and open a DRAFT PR when the work is done. Review happens on the draft PR, not before the commit. Merge only when the user says to, or in an autopilot run (below).
- PR titles are Conventional Commits with exactly one `semver:major|minor|patch` label (CI checks both).
- A PR that changes the UI includes screenshots of the changed screens, posted as a PR comment or in the body, before it is marked ready.

## Autopilot
Applies to the issues the user hands to `/pstack:poteto-mode autopilot-full`. Those issues go from branch to merged PR without stopping, except at the gate.
1. **Track what you create** in a `Run resources` `<details>` list in the PR body, updated as you go: worktrees (including any outside the repo), branches pushed (the story branch, `pr-screenshots/*`, `assets/*`), background tasks and processes or ports started, and temp dirs.
2. **Build** in a new worktree off `origin/main`, under the rules above.
3. **Gate.** Before merging, stop for the user's explicit approval in the session when:
   - the diff touches `*.razor`, `*.css`, `src/WorkoutLog.UI.Stories/**` or `wwwroot/**`
   - the diff touches EF migrations, the `DbContext` or model configuration
   - the verifier's verdict isn't clean, or the change departs from the issue's acceptance criteria

   For a UI change, first screenshot every changed story and every screen that uses a changed component, before (`origin/main`) and after, in light and dark, at phone width. Look at each one. Push them to a `pr-screenshots/<issue>-<slug>` branch (tracked in step 1) and put them in the PR body as a table: story, before, after. Then send a push notification and stop with the PR link, the reasons and the table. Requested changes go back to step 2, and the gate runs again with new screenshots.
4. **Merge** once CI is green, the verdict is clean and the gate passed or was approved: `gh pr ready`, then `gh pr merge --squash --delete-branch`.
5. **Clean up** everything on the `Run resources` list: stop the tasks and processes, `git worktree remove --force` each worktree (delete a leftover folder, then `git worktree prune`), delete each branch locally and on `origin`, and delete the temp dirs and the session scratchpad. Never touch `main`, anything not on the list, or anything with uncommitted work; report those instead. Finish with what merged, what was removed and what was kept.
