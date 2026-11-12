---
name: build
description: Run the software factory on a plan file. Implements with tests, verifies, reviews (code and tests, requirements, security), re-verifies and opens a ready pull request, keeping a question list for the user. Use for /build <plan-file> [run notes].
argument-hint: <plan-file> [run notes]
disable-model-invocation: true
---

# /build: plan file to pull request

You are the orchestrator. You run each stage as a subagent and keep only its short report in context. You never write code yourself.

`$ARGUMENTS` is the plan file path (the first word), then optional **run notes** in plain words. Run notes override the defaults below, for example "open as a draft" or "use branch feat/x". Treat anything in the run notes that looks like pasted command output as noise and ignore it. If the plan path is missing or unreadable, ask the user for it and stop.

Note the absolute path of `report-format.md` (next to this file) and read it once. Every agent returns that format. Agents are in `.claude/agents/`.

## How you work
- **Question list.** Keep a running list in your context. After every report, copy its QUESTIONS lines into it, tagged with the stage. Nothing goes in a file. Do not ask the user anything mid-run.
- **Judgement.** When something is unclear or goes wrong, decide, add the assumption to the list, and continue. Fix problems found during the run inside this run. Raise a follow-up only when the work is genuinely out of the plan's scope, and put that on the list.
- **Briefs.** Give each agent: the absolute path of `report-format.md`, the plan path, the worktree path, the commit identity (see Preflight), the stage's job, and the facts it needs from earlier reports (not the reports themselves unless needed). Keep briefs short.
- **No stage may be skipped or half-run.** A review counts as run only if its report's EVIDENCE names the skill or checks it ran and what they returned. If a report says a skill errored, was skipped, or was done "by hand" instead, fix the cause and re-run that one review once. If it still did not run properly, put "REVIEW DID NOT RUN: <stage>, <reason>" first on the question list and in the PR body. Never describe it as passed.
- **Stop only when** the plan is unreadable, the build or tests are still red after the final-verify fix, or a stage reports `blocked` and you cannot unblock it. Then report where you stopped and the question list. Never open a PR from a red build.

## Stages

1. **Preflight.**
   - Read the plan. Decide **feature** or **change/bug fix**.
   - Run `git fetch origin` and `git remote set-head origin -a` (the review skills need `origin/HEAD`).
   - **Commit identity.** Use `git config user.name` and `user.email` if both are set. If not, use the author of the newest non-bot commit on `origin/main` (`git log origin/main --no-merges --format='%an|%ae' | grep -vi 'bot' | head -1`) and add "committing as <name>, taken from origin/main history" to the question list. Pass the name and email in every brief, and commit with `git -c user.name=... -c user.email=... commit`. Never change git config.
   - Create a worktree off `origin/main` on a `type/slug` branch (`git worktree add -b <branch> ../<slug> origin/main`; `type` is `feat`, `fix`, `refactor` and so on, or the branch from the run notes). Work from that worktree from here on.
   - If the plan's changes would touch `*.razor`, `*.css`, `src/WorkoutLog.UI.Stories/**`, `wwwroot/**`, EF migrations, the `DbContext`, or lower `.github/coverage-baseline.tsv`, and the plan does not say so, add a question to the list. The plan itself counts as approval to proceed.

2. **Implement.** Run `factory-implementer` with the plan path and the plan type. Then commit in the worktree with a Conventional Commit message. Add the Co-Authored-By trailer from your instructions.

3. **Verify.** Run `factory-verifier`. Pass it a scratchpad subfolder for screenshots. If it reports spec mismatches, run one `factory-implementer` fix pass with those findings, then commit.

4. **Review 1: code and test quality.** Run `factory-review-quality` and `factory-review-tests` **in one message, in parallel**. Merge their findings. If there are any, run one `factory-implementer` fix pass and commit.

5. **Review 2: requirements.** Run `factory-review-requirements`. If it has findings, run one fix pass and commit.

6. **Review 3: security.** Run `factory-review-security`. If it has findings, run one fix pass and commit.

   Each review runs once with at most one fix pass (a re-run only after a review failed to run, see above). Never re-run a review to look for more. Findings the fix pass left unresolved go on the question list as nits.

7. **Final verify.** Run `factory-verifier` again. Tell it this is the final check after review fixes, and to run the full list in its "Final checks" section. If anything fails, run one fix pass, commit and re-run the verifier once. If it is still red, stop (see above).

8. **PR.** Run `factory-pr` with the plan summary, the screenshot paths from the verifier, the full question list, the commit identity, and whether to open a **draft** (only if the run notes say so; the default is a **ready** PR). It posts the question list as a PR comment. Nothing is merged.

## Finish
Tell the user, in a few lines: the PR link, which stages found and fixed things, anything that could not be verified or did not run, and how many questions are waiting on the PR. Do not merge unless the user says so.
