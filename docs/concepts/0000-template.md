<!--
Style, from real review feedback on earlier concept docs:
- One idea per paragraph. If a paragraph does two things, split it.
- Prefer a list or table over a dense sentence chaining multiple
  branches/clauses with arrows or semicolons.
- Don't cram more than 2-3 inline-code names into one sentence — pick
  the ones that matter and drop or defer the rest.
- Define or avoid jargon on first use; if a reader would ask "what?",
  rephrase instead of assuming the term lands.
- In a Q&A entry, check the answer actually resolves the literal
  question asked, not just adjacent context.
- If you mention a known bug/gap, say plainly whether it's tracked or
  already fixed — don't let it read as narrative color.
-->

# Title

## Premise

What this subsystem is and why it exists — the context a reader needs
before "Problem" makes sense.

## Problem

The specific challenge this part of KitCli solves. Concrete, not abstract.

## Solution

Progressive disclosure of how it actually works — real class/method names,
real signatures, worked examples using code that exists in the repo today.
Verify every name and signature against source before writing it down;
don't describe intended or aspirational behavior as if it's current.

## Constraints & tradeoffs

Design decisions and the alternatives that lost, in brief — this is the
one place a concept doc overlaps with an ADR. If a specific decision here
already has (or deserves) its own ADR, link it instead of re-explaining it.

## Questions & answers

Common "how do I..." and "why does it..." questions a consumer would
actually ask, answered directly.

## Related concepts

A concept doc covers **one** concept — don't let a subsystem's
neighbors creep in just because they're used together. Instead, list
the other concept docs this one connects to, one line each: which doc,
and why a reader might need it. Link out rather than re-explaining;
this is what keeps splitting concepts into single-topic docs
sustainable instead of just moving the sprawl around.
