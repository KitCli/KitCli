---
title: Roadmap
layout: landing
---

<div class="landing">

<section class="hero tl-hero">
  <h1>The evolution of KitCli</h1>
  <p class="hero-sub">
    KitCli became its own repository in January 2026, but the code is nearly a
    year older. It grew inside a personal finance CLI,
    <a href="https://github.com/joshuaedwardcrowe/SpendfulnessCli">SpendfulnessCli</a>,
    as the machinery that app needed to get from a line the user typed to
    something that ran — and moved out once none of it was about budgets any
    more.
  </p>
</section>

<nav class="tl-years" aria-label="Jump to a year">
  <a href="#year-2025">2025</a>
  <a href="#year-2026">2026</a>
</nav>

<section class="timeline">

<h2 class="tl-year" id="year-2025">2025</h2>

<div class="tl-item tl-left">
  <div class="tl-card">
    <span class="tl-when">February 2025</span>
    <h3>A command handler, two days in</h3>
    <p>
      <code>BaseCommandHandler</code> lands on the 5th, two days after the
      budgeting app's first commit, and twelve minutes later sits beside
      <code>ICommand</code>, <code>ICommandGenerator</code>,
      <code>ICommandHandler</code> and a DI-driven console loop. On the 8th an
      instruction structure, typed argument builders and a parser turn a typed
      line into one of those commands. Four of
      <a href="README.md#the-seven-words">the seven words</a> exist by the end
      of the month.
    </p>
  </div>
</div>

<div class="tl-item tl-right">
  <div class="tl-card">
    <span class="tl-when">March – September 2025</span>
    <h3>Seven months of leaving it alone</h3>
    <p>
      The app grows aggregators, a database layer and a set of YNAB clients,
      and every one of them is built on the command machinery without changing
      it. Nothing was proven on purpose, but that is the proof the shape held.
    </p>
  </div>
</div>

<div class="tl-item tl-left">
  <div class="tl-card">
    <span class="tl-when">October 2025</span>
    <h3>Four projects come out of the app</h3>
    <p>
      "Wrote Cli Abstractions" on the 23rd introduces outcomes, the workflow
      and <code>CliApp</code>. The next day <code>Cli.Abstractions</code>,
      <code>Cli.Commands</code>, <code>Cli.ViewModel.Abstractions</code> and
      <code>Cli.Workflow</code> are projects of their own, types take the
      <code>Cli</code> prefix they still carry, and instruction parsing is
      rewritten around <a href="concepts/0005-instruction-parsing-pipeline.md">token
      indexing</a>.
    </p>
  </div>
</div>

<div class="tl-item tl-right">
  <div class="tl-card">
    <span class="tl-when">November 2025</span>
    <h3>The vocabulary settles</h3>
    <p>
      Generators become <a href="concepts/0001-command-registration.md">factories</a>,
      command properties become <a href="concepts/0008-artefacts.md">artefacts</a>,
      and reusable outcomes plus a <code>ReachedReusableOutcome</code> status
      let one run survive several asks — the
      <a href="concepts/0010-workflow-run-state-machine.md">state machine</a> in
      outline. Instruction validators, configurable prefixes and the first real
      test coverage arrive with them.
    </p>
  </div>
</div>

<div class="tl-item tl-left">
  <div class="tl-card">
    <span class="tl-when">December 2025</span>
    <h3>Paging</h3>
    <p>
      Page size and page number become
      <a href="concepts/0006-outcomes.md">outcomes</a> a later command can read
      back, rather than arguments every command has to ask for again. Artefacts
      get names.
    </p>
  </div>
</div>

<h2 class="tl-year" id="year-2026">2026</h2>

<div class="tl-item tl-right">
  <div class="tl-card">
    <span class="tl-when">January 2026</span>
    <h3>KitCli leaves home</h3>
    <p>
      The app moves to .NET 10, and on the 28th the <code>Cli.*</code> projects
      are copied into a new repository as <code>KitCli.*</code>. All nine
      packages publish at 1.0.0 the next day, and SpendfulnessCli deletes its
      copies to consume the package like anyone else would.
    </p>
  </div>
</div>

<div class="tl-item tl-left">
  <div class="tl-card">
    <span class="tl-when">February 2026</span>
    <h3>The first month as a dependency</h3>
    <p>
      Being referenced rather than edited shows up the wiring a consumer had
      been writing by hand: registries and same-assembly auto-registration
      remove it. Command reactions and automatic chaining arrive, outcome lists
      turn fluent, and artefacts become records.
    </p>
  </div>
</div>

<div class="tl-item tl-right">
  <div class="tl-card">
    <span class="tl-when">March – June 2026</span>
    <h3>Four quiet months</h3>
    <p>
      No commits. The packages sit at 1.0.10 and the app that spawned them
      carries on using them.
    </p>
  </div>
</div>

<div class="tl-item tl-left">
  <div class="tl-card">
    <span class="tl-when">July 2026</span>
    <h3>A paper trail</h3>
    <p>
      Process, not code: <code>CONTRIBUTING.md</code>, the first
      <a href="adr/0001-mediatr-for-command-dispatch.md">ADRs</a> and
      <a href="concepts/0001-command-registration.md">concept docs</a>, and an
      automated release workflow. A framework other people's code depends on
      needs to be able to answer "why is it like this".
    </p>
  </div>
</div>

<div class="tl-item tl-right">
  <div class="tl-card">
    <span class="tl-when">August 2026</span>
    <h3>Hosts, scopes and attributes</h3>
    <p>
      A <a href="adr/0013-merge-the-hosts-and-name-the-variant-headless.md">headless
      host</a> runs a command straight from process args, Ctrl+C becomes
      <a href="adr/0006-cooperative-cancellation.md">cooperative cancellation</a>,
      and every run gets <a href="adr/0002-di-scope-per-workflow-run.md">its own
      DI scope</a>. A command can now declare its extra names, what should
      follow it, and <a href="adr/0011-chain-to-a-command-by-type.md">the type
      it chains to</a>.
    </p>
  </div>
</div>

<div class="tl-item tl-left">
  <div class="tl-card">
    <span class="tl-when">August 2026</span>
    <h3>Three majors in five days</h3>
    <p>
      v1.0.11 on the 22nd through to v3.0.0 on the 26th, cut by a release tool
      that is itself a KitCli app — so publishing the packages uses them. These
      docs go up as a site in the same week.
    </p>
  </div>
</div>

</section>

</div>
