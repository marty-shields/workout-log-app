# AGENTS.md — Workout Log

Personal Android app (.NET 10 MAUI Blazor Hybrid). Setup: [docs/dev-setup-build.md](docs/dev-setup-build.md) ([Windows](docs/dev-setup-windows.md), [Linux](docs/dev-setup-linux.md)). E2E runs: [docs/e2e-tests.md](docs/e2e-tests.md). Release: [docs/release.md](docs/release.md). Screen behaviour: the design system's [screen pages](src/WorkoutLog.UI.Stories/Stories/Screens/) (BlazingStory). Work items and their acceptance criteria: GitHub issues.

## Commands
- Build: `dotnet build WorkoutLog.slnx` (warnings fail the build)
- Test: `dotnet test tests/WorkoutLog.Core.Tests` and `dotnet test tests/WorkoutLog.UI.Tests`
- E2E (needs a device; a plain `dotnet test` skips them): build the App, then `dotnet test tests/WorkoutLog.E2E.Tests --filter "Category=smoke"`
- E2E without a device (Linux): `.github/scripts/e2e-local.sh "<filter for what you changed>"` boots an emulator, runs the filter, tears it down; exit 2 means skipped, so say the tests did not run ([details](docs/dev-setup-linux.md#throwaway-emulator-for-agents))
- Design system: `dotnet run --project src/WorkoutLog.UI.Stories` → http://localhost:5056

## Rules
- Ask about any ambiguity.
- No new packages without approval.
- Commit on a branch, push it and open a DRAFT PR when the work is done. Review happens on the draft PR, not before the commit. Merge only when the user says to.
- PR titles are Conventional Commits with exactly one `semver:major|minor|patch` label (CI checks both).