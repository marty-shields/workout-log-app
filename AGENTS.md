# AGENTS.md — Workout Log

Personal Android app (.NET 10 MAUI Blazor Hybrid). Setup, E2E runs and release: [docs/dev-setup-build.md](docs/dev-setup-build.md). Screen behaviour: the design system's [screen pages](src/WorkoutLog.UI.Stories/Stories/Screens/) (BlazingStory). Work items and their acceptance criteria: GitHub issues.

## Commands
- Build: `dotnet build WorkoutLog.slnx` (warnings fail the build)
- Test: `dotnet test tests/WorkoutLog.Core.Tests` and `dotnet test tests/WorkoutLog.UI.Tests`
- E2E (needs a device; a plain `dotnet test` skips them): build the App, then `dotnet test tests/WorkoutLog.E2E.Tests --filter "Category=smoke"`
- Design system: `dotnet run --project src/WorkoutLog.UI.Stories` → http://localhost:5056

## Rules
- Ask about any ambiguity.
- No new packages without approval.
- Never commit without review. After approval, push and open a DRAFT PR; never merge.
- PR titles are Conventional Commits with exactly one `semver:major|minor|patch` label (CI checks both).