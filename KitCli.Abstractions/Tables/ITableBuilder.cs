using KitCli.Abstractions.Aggregators;

namespace KitCli.Abstractions.Tables;

public interface ITableBuilder<TAggregation> where TAggregation : notnull
{
    ITableBuilder<TAggregation> WithAggregator(CliListAggregator<TAggregation> aggregator);
    
    ITableBuilder<TAggregation> WithSortOrder(CliTableSortOrder viewModelSortOrder);

    ITableBuilder<TAggregation> WithRowCount(bool showRowCount);
    
    Table Build();
}