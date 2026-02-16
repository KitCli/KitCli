using KitCli.Abstractions.Aggregators;
using KitCli.Abstractions.Tables;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Outcomes;

// TODO: Duplication handling.
// For example, if two tables are added, should they be merged into one table outcome
// with both tables, or should they be kept separate? If they are kept separate,
// how should they be ordered in the list of outcomes?
public class OutcomeList : List<Outcome>
{
    public OutcomeList ByResultingIn(Outcome outcome)
    {
        Add(outcome);
        return this;
    }
    
    public OutcomeList ByResultingIn(params Outcome[] outcomes)
    {
        AddRange(outcomes);
        return this;
    }
    
    public OutcomeList BySaying(string message)
        => ByResultingIn(new SayOutcome(message));

    public OutcomeList BySaying(params string[] messages)
        => ByResultingIn(messages.
            Select(message => new SayOutcome(message))
            .ToArray<Outcome>());
    
    public OutcomeList ByShowingTable(Table table)
        => ByResultingIn(new TableOutcome(table));

    public OutcomeList ByAggregating<TSource, TAggregate>(Aggregator<TSource, TAggregate> aggregator)
        => ByResultingIn(new AggregatorOutcome<TSource, TAggregate>(aggregator));
    
    public OutcomeList ByRememberingFilter(AggregatorFilter filter)
        => ByResultingIn(new AggregatorFilterOutcome(filter));
    
    public OutcomeList ByRememberingPageSize(int pageSize)
        => ByResultingIn(new PageSizeOutcome(pageSize));
    
    public OutcomeList ByRememberingPageNumber(int pageNumber)
        => ByResultingIn(new PageNumberOutcome(pageNumber));
    
    public OutcomeList ByMovingToCommand(CliCommand nextCommand)
        => ByResultingIn(new NextCliCommandOutcome(nextCommand));
    
    public OutcomeList ByReacting(CliCommandReaction reaction)
        => ByResultingIn(new ReactionOutcome(reaction));

    public OutcomeList ByFinallyDoingNothing()
        => ByResultingIn(new NothingOutcome());
    
    public OutcomeList ByFinallySaying(string message)
        => ByResultingIn(new FinalSayOutcome(message));
    
    public OutcomeList ByFinallyNotFindingCommand()
        => ByResultingIn(new CliCommandNotFoundOutcome());
    
    public Outcome[] End() => ToArray();
    
    public Task<Outcome[]> EndAsync() => Task.FromResult(ToArray());
}