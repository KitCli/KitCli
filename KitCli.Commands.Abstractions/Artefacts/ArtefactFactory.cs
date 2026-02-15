using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Commands.Abstractions.Artefacts;

public abstract class ArtefactFactory<TOutcome> : IArtefactFactory where TOutcome : Outcome
{
    public bool For(Outcome outcome) => outcome is TOutcome;

    public AnonymousArtefact Create(Outcome outcome)
    {
        if (outcome is TOutcome typedOutcome)
        {
            return CreateArtefact(typedOutcome);
        }
        
        // TODO: Custom exception to be handled somewhere else.
        throw new InvalidOperationException(
            $"Cannot create artefact from outcome of type {outcome.GetType().Name} using factory for {typeof(TOutcome).Name}");
    }

    protected abstract AnonymousArtefact CreateArtefact(TOutcome outcome);
}