using KitCli.Abstractions.Aggregators;
using KitCli.Commands.Abstractions;

namespace KitCli.Commands;

public abstract record PagedCliCommand<TSource, TAggregate>(
    Aggregator<TSource, TAggregate> Aggregator,
    int PageSize,
    int PageNumber) : CliCommand
{
    public static class ArgumentNames
    {
        public const string PageNumber = "pageNumber";
        public const string PageSize = "pageSize";
    }
     
    public static class ArtefactNames
    {
        public const string PageNumber = "pageNumber";
        public const string PageSize = "pageSize";
    }
}