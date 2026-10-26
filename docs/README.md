# Workout Log — Docs

| Doc | Purpose |
|---|---|
| [01 Requirements](01-requirements.md) | Functional and non-functional requirements with acceptance criteria |
| [02 Data model](02-data-model.md) | Entities, metrics, validation rules |
| [03 Exercise list](03-exercise-list.md) | Metrics per activity type. Full list: [exercises.json](../src/WorkoutLog.Core/Domain/Exercises/exercises.json) |
| [04 Architecture](04-architecture.md) | Projects, layers, logging, error handling |
| [05 Testing](05-testing.md) | Test layers and E2E tests |
| [06 Dev setup & build](06-dev-setup-build.md) | Tooling, running on the phone, release APK, source control, CI |
| [07 Decisions log](07-decisions-log.md) | How the app and repo work, one decision per subject |
| [08 Product goal](08-product-goal.md) | Product goal and Definition of Done |
| [09 Design system](09-design-system.md) | The stories site: tokens, components, screens and navigation |

## Ways of working
- **Backlog:** the repo's [issues](https://github.com/marty-shields/workout-log-app/issues) (D178).
- **Milestones:** M0…M6 are [GitHub milestones](https://github.com/marty-shields/workout-log-app/milestones); each holds its scope and exit criteria. A milestone is closed when all its epics are closed.
- **Epics:** issues titled `EP-xx Name`, in their milestone. An epic's stories are its sub-issues.
- **Stories:** issues titled `US-xx Title`, in their epic's milestone. IDs never change; a new story takes the next number. "As the user, I want…, so that…", or "As the developer…" for technical stories.
- **Acceptance criteria:** the linked FR-xx in [01](01-requirements.md), plus any extra ACs in the issue.
- **Ticket review:** on request, before the local review ([ticket-review](../.agents/skills/ticket-review/SKILL.md), D184).
- **Delivery:** [06 Source control](06-dev-setup-build.md#source-control) and the [Definition of Done](08-product-goal.md).
