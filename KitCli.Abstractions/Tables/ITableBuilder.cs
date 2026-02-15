namespace KitCli.Abstractions.Tables;

// TODO: Needs revisiting - should be a clever way to build/sort/page tables.
// public interface ITableBuilder<TAggregation> where TAggregation : notnull
// {
//     ITableBuilder<TAggregation> WithAggregator(Aggregator<TAggregation> aggregator);
//     
//     ITableBuilder<TAggregation> WithSortOrder(CliTableSortOrder viewModelSortOrder);
//
//     ITableBuilder<TAggregation> WithRowCount(bool showRowCount);
//     
//     Table Build();
// }