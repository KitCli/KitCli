---
paths:
  - "CHANGELOG.md"
---

# Writing a CHANGELOG entry

`CONTRIBUTING.md` says a behaviour change "gets a line". Hold to that here:

- **One bullet per change, two lines maximum.** If it needs more, the detail
  belongs in the ADR the bullet links to.
- **Write for someone installing the package**, not someone who read the diff —
  CI builds the GitHub Release body from the `[Unreleased]` notes verbatim.
- Say what a consumer can now do, in plain language. Name a type only when the
  reader has to type it.
- A breaking change is prefixed **Breaking:** and gives the one-line edit a
  consumer makes.
