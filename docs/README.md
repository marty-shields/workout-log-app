# Workout Log — Docs

| Doc | Purpose |
|---|---|
| [01 Requirements](01-requirements.md) | Functional and non-functional requirements with acceptance criteria |
| [02 Data model](02-data-model.md) | Entities, metrics, validation rules |
| [03 Exercise list](03-exercise-list.md) | Metrics per activity type. Full list: [exercises.json](../src/WorkoutLog.Core/Domain/Exercises/exercises.json) |
| [04 Architecture](04-architecture.md) | Projects, layers, logging, error handling |
| [05 Screens & UX](05-screens-ux.md) | Screen inventory and navigation |
| [06 Testing](06-testing.md) | Test layers and E2E tests |
| [07 Dev setup & build](07-dev-setup-build.md) | Tooling, running on the phone, release APK, source control, CI |
| [08 Decisions log](08-decisions-log.md) | Every decision made so far |
| [09 Product goal](09-product-goal.md) | Product goal and Definition of Done |
| [10 Design system](10-design-system.md) | Where the design system lives (the stories site) |

## Ways of working
- **Backlog:** the [Workout Log project](https://github.com/users/marty-shields/projects/2) over the repo's [issues](https://github.com/marty-shields/workout-log-app/issues) (D216).
- **Milestones:** M0…M6 are [GitHub milestones](https://github.com/marty-shields/workout-log-app/milestones); each holds its scope and exit criteria. A milestone is closed when all its epics are Done.
- **Epics:** issues titled `EP-xx Name`, in their milestone. An epic's stories are its sub-issues.
- **Stories:** issues titled `US-xx Title`, in their epic's milestone. IDs never change; a new story takes the next number. "As the user, I want…, so that…", or "As the developer…" for technical stories.
- **Acceptance criteria:** the linked FR-xx in [01](01-requirements.md), plus any extra ACs in the issue.
- **Size and Status:** project fields. Size is XS, S, M, L or XL (relative effort); Status is To do, In progress, Blocked or Done.
- **Ticket review:** on request, before the local review ([ticket-review](../.agents/skills/ticket-review/SKILL.md), T100).
- **Delivery:** [07 Source control](07-dev-setup-build.md#source-control) and the [Definition of Done](09-product-goal.md).
