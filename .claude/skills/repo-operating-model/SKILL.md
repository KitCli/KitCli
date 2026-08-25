---
name: repo-operating-model
description: Run an architectural review of this repo (or an area of it), file findings as labeled GitHub issues, write/update ADRs or concept docs, and respond to PR review comments — following the process and conventions this repo already uses. Use when asked to review the codebase, audit an area, write a concept doc, file review findings, or when starting review of a new area that hasn't been through this process yet.
---

# Repo operating model

This encodes the review → docs → issues → PR-review-response loop this
repo already runs on. `CLAUDE.md` holds the standing facts (labels,
commit format, PR triggers); this skill holds the *procedure*.

## 1. Check whether a full review is actually due

Full architectural reviews aren't run on a calendar — they're gated on
real change volume, since calendar time alone doesn't mean anything
architectural actually moved.

**Last full architectural review: 2026-07-25** (write-up:
[`docs/reviews/0001-architectural-review.md`](../../docs/reviews/0001-architectural-review.md)),
at commit `1290121160171b232b3d52bb12a5ac14bcd80a98`.

Before starting a new *full* review pass (not before every invocation of
this skill — only when actually considering a full sweep), check lines
of source changed since that commit:

```
git log --since="2026-07-25" --pretty=tformat: --numstat -- '*.cs' \
  | awk '{add+=$1; del+=$2} END {print add+del}'
```

Only recommend or run a full review if **both** are true:
- At least 1,000 lines of `.cs` source changed since that commit — not
  docs, not generated files. This repo's docs work alone can produce
  1,000+ lines without a single line of code changing, so don't count
  the raw repo diff.
- At least 6 months elapsed since that date.

If asked to "review the codebase" and neither threshold is met, say so
and scope the work to whatever's actually being asked (a specific area,
a specific concern) instead of running a full sweep.

After completing a full review, write it up as
`docs/reviews/<date>-architectural-review.md` (see the existing one for
the shape: summary counts, methodology, a findings table grouped by
severity linking each GitHub issue, and any skipped/already-resolved
findings), then update the date, write-up link, and commit hash above to
match.

## 2. Architectural review

- For a review spanning multiple independent dimensions (e.g. DI/composition,
  error handling, test coverage, naming/dead code), split it across
  parallel review passes — one dimension per pass — rather than one
  linear read-through. Use background `Agent` calls for this when the
  scope genuinely spans multiple unrelated areas; for a single focused
  area, just read the source directly.
- **Never trust a high-severity finding without personally re-verifying
  it against real source** (exact file, exact line, exact current
  behavior) before it goes in a report or an issue. A finding that
  turns out to be wrong once published is worse than one skipped.
- Deduplicate findings across passes before reporting — the same root
  cause often surfaces from more than one angle.
- If producing a written report (not just issues), match the visual
  language already established for this repo's reviews: severity-tiered
  findings, one finding per card/section, file:line references.

## 3. Findings → GitHub issues

- One issue per finding. Title: a plain-language statement of the
  problem, not a category label.
- Every issue gets all three label axes from `CLAUDE.md` — type, area,
  severity. Use the matching issue template in `.github/ISSUE_TEMPLATE/`.
- **Skip filing a finding that's already resolved** — e.g. a missing-CI
  finding when a CI workflow was added in the same session. Note the
  skip rather than silently dropping it.
- If a finding needs more explanation than fits the issue body (e.g. two
  related bugs in the same class), a follow-up comment on the issue is
  fine — don't cram everything into the initial body.

## 4. ADRs and concept docs

- **ADR** (`docs/adr/`, copy `0000-template.md`): a decision + its
  alternatives + its consequences. Write one only for a new
  cross-cutting pattern, a project/package boundary change, a breaking
  public API change, or reversing a prior ADR. Skip it for bug fixes and
  internal refactors.
- **Concept doc** (`docs/concepts/`, copy `0000-template.md`): how a
  subsystem works *today*, for onboarding. Write one for any subsystem a
  new contributor would need explained that isn't already covered.
- **Before writing either**: read the actual current source for every
  class/method/signature you're about to describe. Never describe
  intended or aspirational behavior as current.
- **Style** (learned from real review feedback, encoded in
  `docs/concepts/0000-template.md`'s header comment — read it before
  writing): one idea per paragraph; prefer a list or table over a dense
  sentence chaining branches with arrows/semicolons; don't cram more
  than 2-3 inline-code names into one sentence; define or avoid jargon
  on first use; make sure a Q&A entry actually answers the literal
  question asked; state plainly whether a mentioned bug is tracked or
  already fixed, so it doesn't read as narrative color.
- Open a PR for the new/updated doc. **Do not merge it** — see
  `CLAUDE.md`'s standing rules.

## 5. Responding to PR review comments

- Fetch the full comment list with `gh api --paginate
  repos/{owner}/{repo}/pulls/{pr}/comments` — the default page size
  truncates at 30 and will make real comments look answered when they
  aren't (or vice versa).
- If a review shows as `PENDING` with no visible comments, the reviewer
  hasn't submitted it yet — pending review comments aren't visible via
  the API. Ask them to submit before trying to read it.
- Reply to each comment **on its own thread**
  (`.../pulls/{pr}/comments/{comment_id}/replies`), prefixed with
  `🤖 **Claude:**`. Never post one combined summary comment instead —
  see `CLAUDE.md` for why.
- Make the actual fix, then reply stating what changed — don't reply
  first and fix later, and don't fix without confirming in the reply
  what was done.
- If a comment's complaint is a general pattern (e.g. "there are lots of
  these in here"), fix it everywhere that pattern appears in the
  changed files, not just at the flagged line — then say so in the
  reply.
- If a comment raises an idea that's out of scope for this PR, offer to
  file it as its own issue; once confirmed, file it (labels per
  `CLAUDE.md`) and link it back on that thread.
- After a round of fixes, re-fetch comments before assuming you're
  done — new comments (including replies asking follow-up questions on
  your replies) can land after your last check.

## 6. Applying this to a repo that doesn't have this operating model yet

If asked to bootstrap this process on a repo without `CONTRIBUTING.md`,
`docs/adr/`, `docs/concepts/`, issue/PR templates, CI, `CODEOWNERS`, or
`CHANGELOG.md`: scaffold those first, adapted to the repo's actual
package/area structure — don't copy KitCli's or YnabSharp's area names
verbatim. Open one PR for the scaffold, get it merged (ask first), then
proceed with the review.
