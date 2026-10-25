# AGENTS.md — Workout Log

A personal Android app (.NET 10 MAUI Blazor Hybrid) that replaces a paper workout notebook. One user, sideloaded, offline.

## Source of truth
`docs/` holds the requirements and decisions; the backlog is on GitHub. Read [docs/README.md](docs/README.md) first.

| Need | Doc |
|---|---|
| Requirements (FR-xx, NFR-xx) | [02](docs/02-requirements.md) |
| Data model, validation | [03](docs/03-data-model.md) |
| Exercise list and metrics | [04](docs/04-exercise-list.md), [exercises.json](src/WorkoutLog.Core/Domain/Exercises/exercises.json) |
| Solution layout, packages | [05](docs/05-architecture.md) |
| Screens and navigation | [06](docs/06-screens-ux.md) |
| Tests | [07](docs/07-testing.md) |
| Setup, build, run | [08](docs/08-dev-setup-build.md) |
| Decisions (D-xx, T-xx) | [09](docs/09-decisions-log.md) |
| Definition of Done | [11](docs/11-product-goal.md) |
| Design system | [12](docs/12-design-system.md) |
| Backlog (EP-xx, US-xx), milestones | [Workout Log project](https://github.com/users/marty-shields/projects/2) and [issues](https://github.com/marty-shields/workout-log-app/issues); [ways of working](docs/README.md#ways-of-working) |

## Layout
| Path | What |
|---|---|
| `WorkoutLog.slnx` | Solution |
| `src/WorkoutLog.App/` | MAUI Blazor Hybrid host (Android): DI, startup, routing |
| `src/WorkoutLog.UI/` | Razor class library: pages, components, design tokens |
| `src/WorkoutLog.UI.Stories/` | BlazingStory app: stories for UI components |
| `src/WorkoutLog.Core/` | Class library: exercise definitions (embedded `exercises.json`), SQLite data, file logging, `AddCore` registration |
| `tests/WorkoutLog.UI.Tests/` | bUnit tests for UI components and screens; plays the prototype scenarios |
| `tests/WorkoutLog.Core.Tests/` | NUnit tests for Core |
| `tests/WorkoutLog.E2E.Tests/` | Gherkin features run by Reqnroll + NUnit + Appium on a device; `[Explicit]`, so a plain `dotnet test` skips them ([07](docs/07-testing.md#e2e-tests-appium)) |
| `.github/` | PR build workflow and Dependabot config ([08](docs/08-dev-setup-build.md#ci)) |

## Commands
| Task | Command |
|---|---|
| Build | `dotnet build WorkoutLog.slnx -warnaserror` (as the PR build) |
| Run the UI tests | `dotnet test tests/WorkoutLog.UI.Tests` |
| Run the Core tests | `dotnet test tests/WorkoutLog.Core.Tests` |
| Run the E2E tests | Build the App, then `dotnet test tests/WorkoutLog.E2E.Tests --filter "FullyQualifiedName~WorkoutLog.E2E.Tests"` with a device connected and unlocked ([08](docs/08-dev-setup-build.md#e2e-tests)) |
| Run the E2E smoke tests | As above, with `--filter "Category=smoke"` |
| Run the stories | `dotnet run --project src/WorkoutLog.UI.Stories` → http://localhost:5056 |
| Run on the phone | `dotnet build src/WorkoutLog.App -t:Run -f net10.0-android` ([08](docs/08-dev-setup-build.md)) |

## Rules
- Ask about any ambiguity. Don't pick a default.
- No new packages without approval. Approved packages are in [05](docs/05-architecture.md#dependencies); their versions are in the project files, `package.json` and the workflows. A Dependabot PR is a request to update one; merging it approves it.
- Every decision goes in [09](docs/09-decisions-log.md) with the next free ID. Story and decision IDs never change.
- A story is done when it meets the Definition of Done in [11](docs/11-product-goal.md).
- UI components follow the Design System pages in the stories site ([12](docs/12-design-system.md)): tokens only, data in through parameters, a story for each component, `data-testid` on interactive elements.
- Front-end work is stories first: build each new or changed screen or component in the stories site, get the reviewer's approval there, then use it in the App ([12](docs/12-design-system.md#process)).
- The UI never touches the database; it calls services only.
- New code logs its errors and unexpected events as in [05 Logging](docs/05-architecture.md#logging), with tests. No workout contents in logs.
- GitHub repo [marty-shields/workout-log-app](https://github.com/marty-shields/workout-log-app). `main` is protected: changes reach it only through a squash-merged PR ([08](docs/08-dev-setup-build.md#source-control)). Commits and PR titles use Conventional Commits.
- One branch per story, `type/us-xx-slug` (e.g. `feat/us-26-signed-release-apk`), from an up-to-date `main`. Set the story's Status to In progress when work starts.
- Never commit without review. When a story's work is ready, stop, summarise the changes and ask the reviewer to review locally. Commit only after the reviewer approves, then push the branch and open a DRAFT PR that closes the story's issue (`Closes #n`). The reviewer marks it ready and squash-merges it; never merge.
- GitHub work uses `gh`, which runs as the repo owner here ([08](docs/08-dev-setup-build.md#source-control)). Never use the GitHub MCP tools in this repo: they are signed in to another account.
- Docs are short and factual.
- Ignore `bin/`, `obj/`, `.vs/` and `TestResults/`.
