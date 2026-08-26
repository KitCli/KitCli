# 0007. Aggregators

Every list command does the same four things: filter, project, sort, page.
`Aggregator<TSource, TAggregate>` is that pipeline as one reusable object.
Subclass it, implement `DoAggregation`, and a later "next page" command can
re-run the whole thing instead of rebuilding it.

```csharp
var aggregator = new TestAggregator(source)
    .BeforeAggregation(p => p.Where(ts => ts.Cost > 50))
    .AfterAggregation(a => a.OrderByDescending(ta => ta.TotalCost));
```

## Pipeline order

`Aggregate(pageSize, pageNumber)` runs: clone the source → every
`BeforeAggregation` function → `DoAggregation` → every `AfterAggregation`
function → `Skip`/`Take`.

So `BeforeAggregation` filters raw source, `AfterAggregation` sorts or
filters aggregated rows, and **paging always happens last**.

Remember an aggregator across commands with `ByAggregating(aggregator)`;
`AggregatorArtefactFactory` makes it queryable (see
[0008-artefacts.md](0008-artefacts.md)). To re-run one with different
paging, though, remember a `TableBuilder` instead — see
[0009-tables.md](0009-tables.md).

`AggregatorFilter` is a separate labelled record recording *which* filter a
command applied. `Aggregator` never reads it; nothing ties it back to the
function passed to `BeforeAggregation`.

## Gaps

The base class is abstract, so even "page a list, no real aggregation"
needs a subclass. Tracked as
[#50](https://github.com/KitCli/KitCli/issues/50).

## See also

[0009-tables.md](0009-tables.md) · [0006-outcomes.md](0006-outcomes.md) ·
[0008-artefacts.md](0008-artefacts.md)
