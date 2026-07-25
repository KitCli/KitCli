# Operating model rollout — status

Branch: `docs/operating-model` (uncommitted, local only as of 2026-07-24)

## Done — full reduced scope complete
- `CONTRIBUTING.md` — full operating model doc.
- `docs/adr/0000-template.md`, `docs/adr/0001-mediatr-for-command-dispatch.md`.
- `.github/workflows/ci.yml`.
- `.github/ISSUE_TEMPLATE/bug_report.yml`, `feature_request.yml`,
  `tech_debt.yml`, `config.yml`.
- `.github/PULL_REQUEST_TEMPLATE.md`.
- `CODEOWNERS` (root).
- `CHANGELOG.md` (seeded, empty `[Unreleased]`).

All still local/uncommitted on `docs/operating-model`.

## Cut from this pass (not started — fine to add anytime, nothing depends on them)
- `docs/adr/0002-...` (Abstractions/concrete project split), `0003-...`
  (outcome taxonomy), `0004-...` (instruction argument type inference).
- Moving `ADR/Notes - *.md` → `docs/concepts/` — not started. Note when
  doing this: "Notes - Outcomes.md" says outcome kinds are
  Reusable/Skippable/Final — the real enum is Anonymous/Reusable/Final, fix
  the wording when moving it.

## After that (not yet done, don't do without asking)
- Review file contents with the user before committing.
- `git add` + commit (only when asked — see repo's global git-commit
  policy).
- Push branch + open PR (visible/shared action — confirm first).
- Branch protection + required-review settings on GitHub — repo settings
  change, confirm first, needs `gh` CLI or the GitHub UI.
- GitHub label creation (`type:*`, `area:*`, `sev:*`) — needed before the
  issue templates are fully useful; not yet run.

## To resume cold
Read this file, `CONTRIBUTING.md`, and the two existing ADRs for the
agreed shape, then continue down "Still to do" in order.
