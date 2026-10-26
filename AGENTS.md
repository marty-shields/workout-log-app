# AGENTS.md — Workout Log

A personal Android app (.NET 10 MAUI Blazor Hybrid) that replaces a paper workout notebook. One user, sideloaded, offline.

## Source of truth
`docs/` holds the requirements and decisions; the backlog is on GitHub. Read [docs/README.md](docs/README.md) first.

| Need | Doc |
|---|---|
| Requirements (FR-xx, NFR-xx) | [01](docs/01-requirements.md) |
| Data model, validation | [02](docs/02-data-model.md) |
| Exercise list and metrics | [03](docs/03-exercise-list.md), [exercises.json](src/WorkoutLog.Core/Domain/Exercises/exercises.json) |
| Architecture, logging | [04](docs/04-architecture.md) |
| Screens and navigation | [Screens](src/WorkoutLog.UI.Stories/Stories/Screens/), [Navigation](src/WorkoutLog.UI.Stories/Stories/Foundations/Navigation.md) |
| Tests | [05](docs/05-testing.md) |
| Setup, build, run | [06](docs/06-dev-setup-build.md) |
| Decisions (D-xx) | [07](docs/07-decisions-log.md) |
| Definition of Done | [08](docs/08-product-goal.md) |
| Design system | [09](docs/09-design-system.md) |
| Backlog (EP-xx, US-xx), milestones | [Issues](https://github.com/marty-shields/workout-log-app/issues); [ways of working](docs/README.md#ways-of-working) |

## Layout
| Path | What |
|---|---|
| `README.md` | The repo's front page: what the app is, features, screenshots (`docs/images/readme/`) (D181) |
| `WorkoutLog.slnx` | Solution |
| `Directory.Build.props` | Settings for every project: target framework, nullable, implicit usings, warnings are errors; NUnit for `*.Tests` projects (D163) |
| `Directory.Packages.props` | NuGet package versions for every project (D164) |
| `src/WorkoutLog.App/` | MAUI Blazor Hybrid host (Android): DI, startup, routing |
| `src/WorkoutLog.UI/` | Razor class library: pages, components, design tokens |
| `src/WorkoutLog.UI.Stories/` | BlazingStory app: the design system (tokens, components, screens, their behaviour and navigation), published to [GitHub Pages](https://marty-shields.github.io/workout-log-app/) |
| `src/WorkoutLog.Core/` | Class library: exercise definitions (embedded `exercises.json`), SQLite data, file logging, `AddCore` registration |
| `tests/WorkoutLog.UI.Tests/` | bUnit tests for UI components and screens; design-system and story coverage checks |
| `tests/WorkoutLog.Core.Tests/` | NUnit tests for Core |
| `tests/WorkoutLog.E2E.Tests/` | Gherkin features run by Reqnroll + NUnit + Appium on a device; `[Explicit]`, so a plain `dotnet test` skips them ([05](docs/05-testing.md#e2e-tests-appium)) |
| `.github/` | PR build, dev pipeline, QA pipeline, PR checks and stories site (GitHub Pages) workflows, Dependabot config ([06](docs/06-dev-setup-build.md#ci)); release workflow and its scripts in `scripts/` ([06](docs/06-dev-setup-build.md#release-signed-apk)) |
| `.agents/skills/` | Agent skills in the open Agent Skills format; `ticket-review` (D184) |

## Commands
| Task | Command |
|---|---|
| Build | `dotnet build WorkoutLog.slnx` (warnings fail the build) |
| Run the UI tests | `dotnet test tests/WorkoutLog.UI.Tests` |
| Run the Core tests | `dotnet test tests/WorkoutLog.Core.Tests` |
| Run the E2E tests | Build the App, then `dotnet test tests/WorkoutLog.E2E.Tests --filter "FullyQualifiedName~WorkoutLog.E2E.Tests"` with a device connected and unlocked ([06](docs/06-dev-setup-build.md#e2e-tests)) |
| Run the E2E smoke tests | As above, with `--filter "Category=smoke"` |
| Run the stories | `dotnet run --project src/WorkoutLog.UI.Stories` → http://localhost:5056 |
| Run on the phone | `dotnet build src/WorkoutLog.App -t:Run -f net10.0-android` ([06](docs/06-dev-setup-build.md)) |
| Release | Actions → Release → Run workflow on `main`, then approve the `release` environment ([06](docs/06-dev-setup-build.md#release-signed-apk)) |

## Rules
- Ask about any ambiguity. Don't pick a default.
- No new packages without approval. The approved packages and their versions are the ones in `Directory.Packages.props` (NuGet), `package.json` and the workflows (D182). Docs name no packages or versions. A Dependabot PR is a request to update one; merging it approves it.
- Every decision goes in [07](docs/07-decisions-log.md), stating how things work now: a change edits its decision; a new subject takes the next free ID. Decision IDs aren't reused; story IDs never change.
- A story is done when it meets the Definition of Done in [08](docs/08-product-goal.md).
- UI components follow the design system in the stories site ([09](docs/09-design-system.md)): tokens only, data in through parameters, one stories file per component with a story per state, `data-testid` on interactive elements.
- Front-end work is stories first: build each new or changed screen or component in the stories site, get the reviewer's approval there, then use it in the App ([09](docs/09-design-system.md#process)).
- The UI never touches the database; it calls services only.
- New code logs its errors and unexpected events as in [04 Logging](docs/04-architecture.md#logging), with tests. No workout contents in logs.
- GitHub repo [marty-shields/workout-log-app](https://github.com/marty-shields/workout-log-app). `main` is protected: changes reach it only through a squash-merged PR ([06](docs/06-dev-setup-build.md#source-control)). Commits and PR titles use Conventional Commits, and each PR has exactly one `semver:major`, `semver:minor` or `semver:patch` label ([06](docs/06-dev-setup-build.md#ci)).
- One branch per story, `type/us-xx-slug` (e.g. `feat/us-26-signed-release-apk`), from an up-to-date `main`.
- When the user asks for a ticket review or a code review of a story, follow [.agents/skills/ticket-review/SKILL.md](.agents/skills/ticket-review/SKILL.md). Never start it otherwise.
- Never commit without review. When a story's work is ready, stop, summarise the changes and ask the reviewer to review locally. Commit only after the reviewer approves, then push the branch and open a DRAFT PR that closes the story's issue (`Closes #n`). The reviewer marks it ready and squash-merges it; never merge.
- GitHub work uses `gh`, which runs as the repo owner here ([06](docs/06-dev-setup-build.md#source-control)). Never use the GitHub MCP tools in this repo: they are signed in to another account.
- Docs are short and factual, and describe only what the app has (D182).
- Ignore `bin/`, `obj/`, `.vs/` and `TestResults/`.
