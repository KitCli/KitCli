---
title: KitCli
layout: landing
---

<div class="landing">

<section class="hero">
  <img class="hero-mark" src="docs/images/logo.png" alt="" width="112" height="86">
  <h1>Create an extensible CLI in minutes.</h1>
  <p class="hero-sub">
    A .NET framework for terminal apps: DI-driven command dispatch, with
    Commands, Outcomes, Artefacts and Workflow layered on top for state that
    carries across a session — page size, filters, "next page" — without each
    command hand-rolling it.
  </p>
  <p class="hero-actions">
    <a class="kit-btn kit-btn-solid" href="docs/user-guides/0001-writing-a-basic-command.md">Write your first command</a>
    <a class="kit-btn kit-btn-quiet" href="api/index.md">API reference</a>
  </p>
  <p class="hero-install"><code>dotnet add package KitCli</code></p>
</section>

<section class="showcase">
  <div class="showcase-head">
    <h2>A command is two types</h2>
    <p>No registration code, no attributes, no name to declare. <code>HelloCliCommand</code> answers to <code>/hello</code> — and to <code>/h</code> — because of what it is called.</p>
  </div>
  <div class="showcase-code">

<pre><code class="lang-csharp">// A command — just a marker type.
public record HelloCliCommand : CliCommand;

// Does the actual work.
public class HelloCliCommandHandler
    : CliCommandHandler&lt;HelloCliCommand&gt;
{
    public override Task&lt;Outcome[]&gt; HandleCommand(
        HelloCliCommand command, CancellationToken ct)
        =&gt; FinishThisCommand()
            .ByFinallySaying("Hello, World!")
            .EndAsync();
}</code></pre>

<pre><code class="lang-csharp">// Program.cs
var app = new CliAppBuilder()
    .WithBasicApp()
    .WithRegistry&lt;HelloRegistry&gt;();

await app.Run();</code></pre>

  </div>
</section>

<section class="cards">
  <h2>Find your way around</h2>
  <div class="card-grid">
    <a class="kit-card" href="docs/user-guides/0001-writing-a-basic-command.md">
      <h3>User guides</h3>
      <p class="kit-card-q">How do I do X?</p>
      <p>You are building an app with KitCli.</p>
    </a>
    <a class="kit-card" href="docs/concepts/0001-command-registration.md">
      <h3>Concepts</h3>
      <p class="kit-card-q">How does X work?</p>
      <p>A guide left you wondering why.</p>
    </a>
    <a class="kit-card" href="docs/adr/0001-mediatr-for-command-dispatch.md">
      <h3>ADRs</h3>
      <p class="kit-card-q">Why is X like this?</p>
      <p>You want to change something and need the reason it is that way.</p>
    </a>
    <a class="kit-card" href="docs/investigations/0002-which-extension-points-can-use-a-consumers-lifetimes.md">
      <h3>Investigations</h3>
      <p class="kit-card-q">What did we find out?</p>
      <p>You are picking up work a spike scoped.</p>
    </a>
    <a class="kit-card" href="docs/technology/microsoft-dependency-injection.md">
      <h3>Technology</h3>
      <p class="kit-card-q">What can I do with KitCli's dependency X?</p>
      <p>You are asking "does the container support…".</p>
    </a>
    <a class="kit-card" href="docs/reviews/0001-architectural-review.md">
      <h3>Reviews</h3>
      <p class="kit-card-q">What was wrong on a given date?</p>
      <p>Historical only. Never current state.</p>
    </a>
  </div>
  <p class="cards-footer">
    New here? <a href="docs/README.md">The seven words</a> names every stage
    between a user typing something and something happening. Every page uses
    them and no page redefines them.
  </p>
</section>

</div>
