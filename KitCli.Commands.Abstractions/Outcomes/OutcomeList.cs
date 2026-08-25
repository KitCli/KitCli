using KitCli.Abstractions.Aggregators;
using KitCli.Abstractions.Tables;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;
using KitCli.Commands.Abstractions.Arguments;

namespace KitCli.Commands.Abstractions.Outcomes;

// TODO: Duplication handling.
// For example, if two tables are added, should they be merged into one table outcome
// with both tables, or should they be kept separate? If they are kept separate,
// how should they be ordered in the list of outcomes?
/// <summary>
/// A fluent builder for the array of <see cref="Outcome"/>s a <c>CliCommandHandler{T}</c> returns. Start
/// with an empty list (e.g. <c>FinishThisCommand()</c>), chain one <c>By...</c> call per outcome to append,
/// then materialize with <see cref="End"/> or <see cref="EndAsync"/>.
/// </summary>
public class OutcomeList : List<Outcome>
{
    /// <summary>
    /// Appends the given outcome.
    /// </summary>
    /// <param name="outcome">The outcome to append.</param>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByResultingIn(Outcome outcome)
    {
        Add(outcome);
        return this;
    }

    /// <summary>
    /// Appends the given outcomes.
    /// </summary>
    /// <param name="outcomes">The outcomes to append.</param>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByResultingIn(params Outcome[] outcomes)
    {
        AddRange(outcomes);
        return this;
    }

    /// <summary>
    /// Appends a <see cref="SayOutcome"/> carrying the given message.
    /// </summary>
    /// <param name="message">The message to say.</param>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList BySaying(string message)
        => ByResultingIn(new SayOutcome(message));

    /// <summary>
    /// Appends one <see cref="SayOutcome"/> per given message.
    /// </summary>
    /// <param name="messages">The messages to say.</param>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList BySaying(params string[] messages)
        => ByResultingIn(messages.
            Select(message => new SayOutcome(message))
            .ToArray<Outcome>());

    /// <summary>
    /// Appends a <see cref="TableOutcome"/> carrying the given table.
    /// </summary>
    /// <param name="table">The table to show.</param>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByShowingTable(Table table)
        => ByResultingIn(new TableOutcome(table));

    /// <summary>
    /// Appends an <see cref="AggregatorOutcome{TSource,TAggregate}"/> remembering the given aggregator.
    /// </summary>
    /// <typeparam name="TSource">The type of the aggregator's source elements.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregator's aggregated elements.</typeparam>
    /// <param name="aggregator">The aggregator to remember.</param>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByAggregating<TSource, TAggregate>(Aggregator<TSource, TAggregate> aggregator)
        => ByResultingIn(new AggregatorOutcome<TSource, TAggregate>(aggregator));

    /// <summary>
    /// Appends an <see cref="AggregatorFilterOutcome"/> remembering the given filter.
    /// </summary>
    /// <param name="filter">The filter to remember.</param>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByRememberingFilter(AggregatorFilter filter)
        => ByResultingIn(new AggregatorFilterOutcome(filter));

    /// <summary>
    /// Appends a <see cref="PageSizeOutcome"/> remembering the given page size.
    /// </summary>
    /// <param name="pageSize">The page size to remember.</param>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByRememberingPageSize(int pageSize)
        => ByResultingIn(new PageSizeOutcome(pageSize));

    /// <summary>
    /// Appends a <see cref="PageNumberOutcome"/> remembering the given page number.
    /// </summary>
    /// <param name="pageNumber">The page number to remember.</param>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByRememberingPageNumber(int pageNumber)
        => ByResultingIn(new PageNumberOutcome(pageNumber));

    /// <summary>
    /// Appends a <see cref="TableBuilderOutcome{TSource,TAggregate}"/> remembering the given table builder,
    /// so a later "next page" command can rebuild the table without re-supplying its aggregator or map.
    /// </summary>
    /// <typeparam name="TSource">The type of the table's source elements.</typeparam>
    /// <typeparam name="TAggregate">The type of the table's aggregated elements.</typeparam>
    /// <param name="tableBuilder">The table builder to remember.</param>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByRememberingHowToBuildTable<TSource, TAggregate>(TableBuilder<TSource, TAggregate> tableBuilder)
        => ByResultingIn(new TableBuilderOutcome<TSource, TAggregate>(tableBuilder));

    /// <summary>
    /// Appends a <see cref="SpecifiedNextCliCommandOutcome"/> naming the given command type as the next
    /// one to run. The command is not built here: the run resolves it through its
    /// <c>ICliCommandFactory</c> when it gets there, so the factory sees the run's accumulated
    /// artefacts — the same construction path an instruction-resolved command takes.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to move to.</typeparam>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByMovingToCommand<TCommand>()
        where TCommand : CliCommand
        => ByResultingIn(new SpecifiedNextCliCommandOutcome(typeof(TCommand)));

    /// <summary>
    /// Appends a <see cref="SpecifiedNextCliCommandOutcome"/> naming the given command type as the next
    /// one to run, along with arguments for that command's factory to read. Use this for what the calling
    /// handler decides; the run's own artefacts reach the factory either way.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to move to.</typeparam>
    /// <param name="arguments">The arguments the next command's factory should see.</param>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByMovingToCommand<TCommand>(params AnonymousNextCliCommandArgument[] arguments)
        where TCommand : CliCommand
        => ByResultingIn(new SpecifiedNextCliCommandOutcome(typeof(TCommand), [..arguments]));

    /// <summary>
    /// Appends a <see cref="ProvidedNextCliCommandOutcome"/> remembering the given command as the next one to run.
    /// The command is built here, by the calling handler, so its factory never runs and never sees the
    /// run's artefacts. Prefer <see cref="ByMovingToCommand{TCommand}()"/> unless the next command takes
    /// its data by constructor.
    /// </summary>
    /// <param name="nextCommand">The command to move to.</param>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByMovingToCommand(CliCommand nextCommand)
        => ByResultingIn(new ProvidedNextCliCommandOutcome(nextCommand));

    /// <summary>
    /// Appends a <see cref="ReactionOutcome"/> carrying the given reaction, published as a side effect.
    /// </summary>
    /// <param name="reaction">The reaction to publish.</param>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByReacting(CliCommandReaction reaction)
        => ByResultingIn(new ReactionOutcome(reaction));

    /// <summary>
    /// Appends a <see cref="NothingOutcome"/>, ending the run without displaying anything further.
    /// </summary>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByFinallyDoingNothing()
        => ByResultingIn(new NothingOutcome());

    /// <summary>
    /// Appends a <see cref="FinalSayOutcome"/> carrying the given message, ending the run.
    /// </summary>
    /// <param name="message">The final message to say.</param>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByFinallySaying(string message)
        => ByResultingIn(new FinalSayOutcome(message));

    /// <summary>
    /// Appends a <see cref="CliCommandNotFoundOutcome"/>, ending the run because no command was found.
    /// </summary>
    /// <returns>This list, for chaining.</returns>
    public OutcomeList ByFinallyNotFindingCommand()
        => ByResultingIn(new CliCommandNotFoundOutcome());

    /// <summary>
    /// Materializes this list to an outcome array.
    /// </summary>
    /// <returns>The outcomes appended to this list, as an array.</returns>
    public Outcome[] End() => ToArray();

    /// <summary>
    /// Materializes this list to a completed task of an outcome array.
    /// </summary>
    /// <returns>A completed task carrying the outcomes appended to this list, as an array.</returns>
    public Task<Outcome[]> EndAsync() => Task.FromResult(ToArray());
}