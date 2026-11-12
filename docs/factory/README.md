# The /build factory

A small pipeline that turns a plan file into a ready pull request. You write the plan with Claude, then run `/build <plan-file>` and come back to a PR with a list of questions on it.

## Use it
1. Plan with Claude until the plan is good. Use [plan-template.md](plan-template.md) as the skeleton.
2. Save the plan as a markdown file, for example `plans/my-change.md`.
3. Run `/build plans/my-change.md` in a Claude Code session started from `main/`.
4. When it finishes, open the PR, read the question-list comment, and answer or fix.

The builder never merges. It opens a ready (not draft) PR so all CI runs.

## Parts
| File | Job | Model |
|---|---|---|
| `.claude/skills/build/SKILL.md` | The builder: runs the stages, holds the question list | your session |
| `.claude/skills/build/report-format.md` | The short report every agent returns | n/a |
| `.claude/agents/factory-implementer.md` | Code and tests; also applies review fixes | sonnet |
| `.claude/agents/factory-verifier.md` | Build, tests, emulator run, screenshots against the spec | sonnet |
| `.claude/agents/factory-review-quality.md` | Code quality, via `/code-review` and the AGENTS.md rules | opus |
| `.claude/agents/factory-review-tests.md` | Test quality | opus |
| `.claude/agents/factory-review-requirements.md` | Diff against the plan's requirements | opus |
| `.claude/agents/factory-review-security.md` | Security, via `/security-review` | opus |
| `.claude/agents/factory-pr.md` | Push, open the PR, post the questions | sonnet |

Each agent ends with a report of at most about 40 lines. The builder keeps the reports and the question list in context only, so no files and no gitignore entries are needed.

## Flow
Preflight (worktree off `origin/main`) → implement → verify → review 1 (code quality and test quality in parallel) → review 2 (requirements) → review 3 (security) → final verify → PR.

Each review runs once. If it finds something, one fix pass runs and the builder moves on. Anything still open goes on the question list.

A feature gets code and tests together. A change or bug fix gets a failing test first, then the fix.

## Tuning
- **Change a model:** edit `model:` in the agent's frontmatter.
- **Drop a stage:** delete its step in `SKILL.md` and its agent file.
- **Add a stage:** write a new agent that follows `report-format.md`, then add a step to `SKILL.md`.
- **Change what a reviewer checks:** edit that agent's checklist; the others are not affected.

## Verification notes
- Screenshots come from the Android emulator through `.github/scripts/e2e-local.sh` and `adb exec-out screencap -p`, because Chrome is not installed here. If the emulator cannot start, the verifier says so and the PR question list records it.
- The README screenshots (`readme-screenshots.mjs`) need Chrome, so the factory does not regenerate them. When a change affects a README screen, the question list says so.
- Subagents call `/code-review` and `/security-review` through the Skill tool. If that ever stops working, the reviewer agent still carries its own checklist.
- AGENTS.md used to say "draft PR". The factory opens ready PRs, and AGENTS.md now says the same.
