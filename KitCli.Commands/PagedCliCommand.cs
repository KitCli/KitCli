using KitCli.Abstractions.Aggregators;
using KitCli.Commands.Abstractions;

namespace KitCli.Commands;

/// <summary>
/// Base command for handlers that return results in pages, aggregating a source into an aggregate
/// and tracking the requested page size and page number.
/// </summary>
/// <typeparam name="TSource">The type of the individual items being aggregated.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregated result produced from <typeparamref name="TSource"/> items.</typeparam>
/// <param name="Aggregator">The aggregator used to combine source items into the aggregate result.</param>
/// <param name="PageSize">The number of items to include in a single page.</param>
/// <param name="PageNumber">The one-based number of the page to return.</param>
public abstract record PagedCliCommand<TSource, TAggregate>(
    Aggregator<TSource, TAggregate> Aggregator,
    int PageSize,
    int PageNumber) : CliCommand
{
    /// <summary>
    /// Names of the command-line arguments used to supply paging values.
    /// </summary>
    public static class ArgumentNames
    {
        /// <summary>The argument name for the requested page number.</summary>
        public const string PageNumber = "pageNumber";

        /// <summary>The argument name for the requested page size.</summary>
        public const string PageSize = "pageSize";
    }

    /// <summary>
    /// Names of the artefacts used to carry paging values between commands.
    /// </summary>
    public static class ArtefactNames
    {
        /// <summary>The artefact name for the requested page number.</summary>
        public const string PageNumber = "pageNumber";

        /// <summary>The artefact name for the requested page size.</summary>
        public const string PageSize = "pageSize";
    }
}