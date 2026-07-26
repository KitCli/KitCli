# Aggregators

## Premise

Building a paged, filterable list for a table needs the same shape of
pipeline every time: take a source collection, optionally pre-filter
it, turn it into the aggregate rows a table will show, optionally
post-process those rows, then page. `Aggregator<TSource, TAggregate>`
(`KitCli.Abstractions/Aggregators/Aggregator.cs`) is that pipeline as a
reusable, composable object.

## Problem

Every paged/filtered list command doing its own ad-hoc "filter, then
project, then sort, then skip/take" inline would duplicate that logic
per command, and would give a later "next page" command nothing to
re-run against — it would have to redo the whole pipeline from scratch
(see [tables.md](tables.md) for the piece that actually remembers and
re-runs one).

## Solution

`Aggregator<TSource, TAggregate>` is abstract: a consumer subclasses it
and implements the one required method,
`DoAggregation(IEnumerable<TSource>) : IEnumerable<TAggregate>`.

Two fluent hooks queue functions to run around that:

```csharp
public Aggregator<TSource, TAggregate> BeforeAggregation(Func<IEnumerable<TSource>, IEnumerable<TSource>> operationFunction);
public Aggregator<TSource, TAggregate> AfterAggregation(Func<IEnumerable<TAggregate>, IEnumerable<TAggregate>> operationFunction);
```

`Aggregate(pageSize, pageNumber)` runs the whole pipeline in order:
clone the source, run every queued before-function, call
`DoAggregation`, run every queued after-function, then `Skip`/`Take` to
the requested page. A real example, from
`KitCli.Playground.Scenarios/TestTableBuilderCliCommand.cs`:

```csharp
var aggregator = new TestAggregator(source)
    .BeforeAggregation(p => p.Where(ts => ts.Cost > 50))
    .AfterAggregation(a => a.OrderByDescending(ta => ta.TotalCost));
```

`AggregatorFilter(FilterFieldName, FilterName, FilterValue)`
(`AggregatorFilter.cs`) is a separate, simpler record — just a labeled
filter value with a computed `FullName`
(`"{FilterFieldName}.{FilterName}-{FilterValue}"`). It isn't consumed
by `Aggregator` itself; it exists purely as something a command can
remember was applied, via `AggregatorFilterOutcome`/
`ByRememberingFilter` (see [outcomes.md](outcomes.md)).

An `Aggregator` is remembered across commands the same way as anything
else reusable: `ByAggregating(aggregator)` (`OutcomeList`) returns an
`AggregatorOutcome<TSource, TAggregate>`, which
`AggregatorArtefactFactory<TSource, TAggregate>` converts to a
queryable `AggregatorArtefact` (see [artefacts.md](artefacts.md)).

## Constraints & tradeoffs

**No built-in concrete `Aggregator`.** The base class is abstract —
even the common case of "just page/filter a list, no real aggregation"
requires subclassing and implementing `DoAggregation` yourself. Tracked
as [#50](https://github.com/KitCli/KitCli/issues/50).

**`AggregatorFilter` is advisory, not enforced.** Nothing ties a
remembered `AggregatorFilter` back to the actual function passed to
`BeforeAggregation` — it's just data a later command factory can read
to know what was applied, not something `Aggregator` itself checks
against.

## Questions & answers

**How do I re-run an `Aggregator` with different paging from a later command?**
You don't call `Aggregate` again directly from another command — you
remember the whole pipeline via a `TableBuilder` instead (see
[tables.md](tables.md)), and rebuild from that.

**What's the difference between `BeforeAggregation` and `AfterAggregation`?**
`BeforeAggregation` functions run on `IEnumerable<TSource>`, before
`DoAggregation` ever runs — good for filtering the raw source.
`AfterAggregation` functions run on `IEnumerable<TAggregate>`, after
aggregation — good for sorting or filtering the already-aggregated
rows. Paging (`Skip`/`Take`) always happens last, after both.

## Related concepts

- [tables.md](tables.md) — `TableBuilder.WithAggregator(...)` is the
  usual way an `Aggregator`'s output actually reaches the screen.
- [outcomes.md](outcomes.md) — `AggregatorOutcome`/`AggregatorFilterOutcome`
  are how an aggregator or filter gets remembered across commands.
- [artefacts.md](artefacts.md) — `AggregatorArtefactFactory`/
  `AggregatorFilterArtefactFactory` are what actually make them
  queryable later.
