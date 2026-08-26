# 0014. Give the roadmap a page of its own, outside the five kinds

Status: Proposed
Date: 2026-08-26

## Context

Nothing on the site said where KitCli came from. The code is eleven months
older than its repository: it grew inside
[SpendfulnessCli](https://github.com/joshuaedwardcrowe/SpendfulnessCli) from
February 2025, was carved into `Cli.*` projects that October, and was copied
out as `KitCli.*` on 28 January 2026. That year explains most of the shapes a
reader meets — why a command is a marker record, why artefacts exist, why the
workflow reuses a run — and none of it is recoverable from this repository's
own history.

No existing kind holds it. `CHANGELOG.md` starts at 1.0.11 and records
released packages, not a year that predates the first package. A review is a
dated snapshot of what was wrong. A concept doc is present-tense by rule, and
history is the one thing it may not narrate. An ADR records one decision, not
a sequence of them.

## Decision

[`docs/roadmap.md`](../roadmap.md) is a single unnumbered page at the docs
root, in the top navigation, outside the five kinds and their numbering. It
renders with `layout: landing` as a vertical timeline; its styles sit with the
landing page's in `templates/kitcli/public/main.css`. Entries are written by
hand from both repositories' git histories — one per month, or per run of
months where nothing changed — and each names what changed rather than
listing commits.

It keeps the name **Roadmap** even though it looks backwards. That is the word
a reader scans a navigation bar for, and forward-looking entries belong on the
same timeline when there are any; milestones and the Ideas board carry the
plan until then.

## Alternatives considered

- **A sixth numbered kind, with its own `0000-template.md`** — a template and
  a sequence number for a folder that will only ever hold one page. Numbering
  says "more of these are coming"; none are.
- **A review** — the folder is dated snapshots of what was wrong, and its
  index page says "historical only". Close enough to attract the page, wrong
  enough to make both harder to describe afterwards.
- **Extending `CHANGELOG.md` backwards** — it is generated from squash-merge
  titles against released versions. The year that matters has neither.
- **Generating the page from `git log`** — the value is the judgement about
  which twenty commits out of a thousand mattered. A generator emits the
  thousand.
- **A plain markdown table**, which is what shipped first — accurate, and read
  as reference material nobody scrolls. The timeline is the argument that the
  framework grew rather than that it was designed.

## Consequences

- `CONTRIBUTING.md` said five kinds live in `docs/` and that everything is
  copied from a template and numbered. Both now carry the exception, updated
  in the same change as this ADR.
- The page is hand-written HTML, and the only page besides the landing page
  with bespoke CSS. Editing it means copying a card, not writing markdown.
- `layout: landing` drops the breadcrumb and the affix table of contents. The
  sticky year links replace them, and are the only navigation the page has.
- It goes stale by construction. An entry is owed when a release or a change
  of shape lands, not every month — a month with nothing in it is worth
  saying, but only once the timeline is past it.
- Half the timeline cites a separate personal repository. If SpendfulnessCli
  ever goes private, the pre-2026 entries become unverifiable claims rather
  than checkable ones.
