using KitCli.Abstractions.Tables;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Outcomes;

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
    
    public OutcomeList ByMovingToCommand(CliCommand nextCommand)
        => ByResultingIn(new NextCliCommandOutcome(nextCommand));
    
    public OutcomeList ByReacting(CliCommandReaction reaction)
        => ByResultingIn(new ReactionOutcome(reaction));

    public OutcomeList ByFinallySaying(string message)
        => ByResultingIn(new FinalSayOutcome(message));
    
    public OutcomeList ByFinallyDoingNothing()
        => ByResultingIn(new NothingOutcome());
    
    public Outcome[] End() => ToArray();
    
    public Task<Outcome[]> EndAsync() => Task.FromResult(ToArray());
}