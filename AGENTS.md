# AGENTS.md — Workout Log

A personal Android app (.NET 10 MAUI Blazor Hybrid) that replaces a paper workout notebook. One user, sideloaded, offline.

## Source of truth
`docs/` holds the requirements, decisions and backlog. Read [docs/README.md](docs/README.md) first.

| Need | Doc |
|---|---|
| Requirements (FR-xx, NFR-xx) | [02](docs/02-requirements.md) |
| Data model, validation | [03](docs/03-data-model.md) |
| Exercise list and metrics | [04](docs/04-exercise-list.md), [exercises.json](src/WorkoutLog.Core/Domain/Exercises/exercises.json) |
| Solution layout, packages | [05](docs/05-architecture.md) |
| Screens and navigation | [06](docs/06-screens-ux.md) |
| Tests | [07](docs/07-testing.md) |
| Setup, build, run | [08](docs/08-dev-setup-build.md) |
| Decisions (D-xx, T-xx) | [10](docs/10-decisions-log.md) |
| Definition of Done | [12](docs/12-product-goal.md) |
| Backlog (EP-xx, US-xx) | [13](docs/13-backlog.md) |
| Design system | [14](docs/14-design-system.md) |

## Layout
| Path | What |
|---|---|
| `WorkoutLog.slnx` | Solution |
| `src/WorkoutLog.App/` | MAUI Blazor Hybrid host (Android): DI, startup, routing |
| `src/WorkoutLog.UI/` | Razor class library: pages, components, design tokens |
| `src/WorkoutLog.UI.Stories/` | BlazingStory app: stories for UI components |
| `src/WorkoutLog.Core/` | Class library: exercise definitions (embedded `exercises.json`), SQLite data, `AddCore` registration |
| `tests/WorkoutLog.UI.Tests/` | bUnit tests for UI components and screens; plays the prototype scenarios |
| `tests/WorkoutLog.Core.Tests/` | NUnit tests for Core |
| `tests/WorkoutLog.E2E.Tests/` | NUnit + Appium tests on a device; `[Explicit]` until M6 |

## Commands
| Task | Command |
|---|---|
| Build | `dotnet build WorkoutLog.slnx` |
| Run the UI tests | `dotnet test tests/WorkoutLog.UI.Tests` |
| Run the Core tests | `dotnet test tests/WorkoutLog.Core.Tests` |
| Run the stories | `dotnet run --project src/WorkoutLog.UI.Stories` → http://localhost:5056 |
| Run on the phone | `dotnet build src/WorkoutLog.App -t:Run -f net10.0-android` ([08](docs/08-dev-setup-build.md)) |

## Rules
- Ask about any ambiguity. Don't pick a default.
- No new packages without approval. Approved packages and versions are in [05](docs/05-architecture.md#dependencies).
- Every decision goes in [10](docs/10-decisions-log.md) with the next free ID. Story and decision IDs never change.
- A story is done when it meets the Definition of Done in [12](docs/12-product-goal.md).
- UI components follow the Design System pages in the stories site ([14](docs/14-design-system.md)): tokens only, data in through parameters, a story for each component, `data-testid` on interactive elements.
- Front-end work is stories first: build each new or changed screen or component in the stories site, get Martin's approval there, then use it in the App ([14](docs/14-design-system.md#process)).
- The UI never touches the database; it calls services only.
- Local git only, no remote. Commits use Conventional Commits.
- Never commit without review. When a story's work is ready, stop, summarise the changes and ask Martin to review. Commit only after he approves.
- Docs are short and factual.
- Ignore `bin/`, `obj/`, `.vs/` and `TestResults/`.
