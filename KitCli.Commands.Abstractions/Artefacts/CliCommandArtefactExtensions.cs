using KitCli.Abstractions.Aggregators;
using KitCli.Commands.Abstractions.Artefacts.Aggregator;
using KitCli.Commands.Abstractions.Artefacts.CommandRan;
using LinqEnumerable = System.Linq.Enumerable;

namespace KitCli.Commands.Abstractions.Artefacts;

public static class CliCommandArtefactExtensions
{
    public static ValuedCliCommandArtefact<TArtefactType>? OfType<TArtefactType>(
        this IEnumerable<CliCommandArtefact> artefacts, string artefactName)
        where TArtefactType : notnull
            => artefacts
                .Where(a => a.ArtefactName == artefactName)
                .OfType<TArtefactType>();

    public static ValuedCliCommandArtefact<TArtefactType>? OfType<TArtefactType>(
        this IEnumerable<CliCommandArtefact> artefacts)
        where TArtefactType : notnull
            => LinqEnumerable
                .OfType<ValuedCliCommandArtefact<TArtefactType>>(artefacts)
                .FirstOrDefault();

    public static ValuedCliCommandArtefact<TArtefactType> OfRequiredType<TArtefactType>(
        this IEnumerable<CliCommandArtefact> artefacts) where TArtefactType : notnull
    {
        var artefact = OfType<TArtefactType>(artefacts);

        if (artefact == null)
        {
            // TODO: Make a real exception.
            throw new Exception(
                $"Artefact of type '{typeof(TArtefactType).Name}' is required for this command.");
        }
        
        return artefact;
    }

    public static ListAggregatorCliCommandArtefact<TAggregate>? OfListAggregatorType<TAggregate>(
        this IEnumerable<CliCommandArtefact> artefacts) where TAggregate : notnull
    {
        var valuedCliCommandArtefact = OfType<CliListAggregator<TAggregate>>(artefacts);
        return valuedCliCommandArtefact as ListAggregatorCliCommandArtefact<TAggregate>;
    } 
    
    public static bool LastCommandRanWas<TLastCliCommand>(
        this IEnumerable<CliCommandArtefact> artefacts) where TLastCliCommand : CliCommand
            => LinqEnumerable
                .OfType<RanCliCommandArtefact>(artefacts)
                .LastOrDefault()
                ?.RanCommand is TLastCliCommand;
}