# Aggregators

## Premise

Building a paged, filterable list for a table takes the same pipeline
every time: take a source collection, pre-filter it, turn it into the
rows a table will show, post-process those rows, then page.
`Aggregator<TSource, TAggregate>`
(`KitCli.Abstractions/Aggregators/Aggregator.cs`) is that pipeline as a
reusable, composable object.

## Problem

Written inline, "filter, then project, then sort, then skip/take"
repeats in every list command. Worse, it leaves a later "next page"
command nothing to re-run — that command must rebuild the whole pipeline
from scratch. See [tables.md](tables.md) for the piece that remembers and
re-runs one.

## Solution

`Aggregator<TSource, TAggregate>` is abstract. Subclass it and implement
the one required method,
`DoAggregation(IEnumerable<TSource>) : IEnumerable<TAggregate>`.

Two fluent hooks queue functions to run around that:

```csharp
public Aggregator<TSource, TAggregate> BeforeAggregation(Func<IEnumerable<TSource>, IEnumerable<TSource>> operationFunction);
public Aggregator<TSource, TAggregate> AfterAggregation(Func<IEnumerable<TAggregate>, IEnumerable<TAggregate>> operationFunction);
```

`Aggregate(pageSize, pageNumber)` runs the pipeline in order: clone the
source, run every queued before-function, call `DoAggregation`, run every
queued after-function, then `Skip`/`Take` to the requested page. From
`KitCli.Playground.Scenarios/TestTableBuilderCliCommand.cs`:

```csharp
var aggregator = new TestAggregator(source)
    .BeforeAggregation(p => p.Where(ts => ts.Cost > 50))
    .AfterAggregation(a => a.OrderByDescending(ta => ta.TotalCost));
```

`AggregatorFilter(FilterFieldName, FilterName, FilterValue)`
(`AggregatorFilter.cs`) is a separate, simpler record: a labeled filter
value with a computed `FullName`
(`"{FilterFieldName}.{FilterName}-{FilterValue}"`). `Aggregator` never
reads it. It exists so a command can record which filter it applied, via
`ByRememberingFilter` and `AggregatorFilterOutcome` (see
[outcomes.md](outcomes.md)). That outcome is `Anonymous`-kind — recording
a filter leaves the run's state alone — yet it still becomes a queryable
`AggregatorFilterArtefact`, named after the filter's `FullName`.

Commands remember an `Aggregator` the way they remember anything else:
`ByAggregating(aggregator)` (`OutcomeList`) returns an
`AggregatorOutcome<TSource, TAggregate>`, which
`AggregatorArtefactFactory<TSource, TAggregate>` converts to a queryable
`AggregatorArtefact` (see [artefacts.md](artefacts.md)).

## Constraints & tradeoffs

**No built-in concrete `Aggregator`.** The base class is abstract, so
even "page and filter a list, no real aggregation" needs a subclass
implementing `DoAggregation`. Tracked as
[#50](https://github.com/KitCli/KitCli/issues/50).

**`AggregatorFilter` is advisory.** Nothing ties a remembered filter back
to the function passed to `BeforeAggregation`. It is data a later command
factory can read, not a constraint `Aggregator` enforces.

## Questions & answers

**How do I re-run an `Aggregator` with different paging from a later command?**
Don't call `Aggregate` again from another command. Remember the whole
pipeline as a `TableBuilder` instead (see [tables.md](tables.md)), and
rebuild from that.

**What's the difference between `BeforeAggregation` and `AfterAggregation`?**
`BeforeAggregation` functions run on `IEnumerable<TSource>`, ahead of
`DoAggregation` — use them to filter the raw source. `AfterAggregation`
functions run on `IEnumerable<TAggregate>` — use them to sort or filter
aggregated rows. Paging (`Skip`/`Take`) always comes last.

## Related concepts

- [tables.md](tables.md) — `TableBuilder.WithAggregator(...)` is the
  usual route from an `Aggregator`'s output to the screen.
- [outcomes.md](outcomes.md) — `AggregatorOutcome` and
  `AggregatorFilterOutcome` remember an aggregator or filter across
  commands.
- [artefacts.md](artefacts.md) — `AggregatorArtefactFactory` and
  `AggregatorFilterArtefactFactory` make them queryable later.
