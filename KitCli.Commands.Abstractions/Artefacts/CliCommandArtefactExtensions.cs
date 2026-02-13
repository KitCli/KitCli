using KitCli.Abstractions.Aggregators;
using KitCli.Commands.Abstractions.Artefacts.Aggregator;
using KitCli.Commands.Abstractions.Artefacts.CommandRan;
using LinqEnumerable = System.Linq.Enumerable;

namespace KitCli.Commands.Abstractions.Artefacts;

public static class CliCommandArtefactExtensions
{
    public static Artefact<TArtefactType>? OfType<TArtefactType>(
        this IEnumerable<AnonymousArtefact> artefacts, string artefactName)
        where TArtefactType : notnull
            => artefacts
                .Where(a => a.Name == artefactName)
                .OfType<TArtefactType>();

    public static Artefact<TArtefactType>? OfType<TArtefactType>(
        this IEnumerable<AnonymousArtefact> artefacts)
        where TArtefactType : notnull
            => LinqEnumerable
                .OfType<Artefact<TArtefactType>>(artefacts)
                .FirstOrDefault();

    public static Artefact<TArtefactType> OfRequiredType<TArtefactType>(
        this IEnumerable<AnonymousArtefact> artefacts) where TArtefactType : notnull
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

    public static ListAggregatorUnvaluedArtefact<TAggregate>? OfListAggregatorType<TAggregate>(
        this IEnumerable<AnonymousArtefact> artefacts) where TAggregate : notnull
    {
        var valuedCliCommandArtefact = OfType<CliListAggregator<TAggregate>>(artefacts);
        return valuedCliCommandArtefact as ListAggregatorUnvaluedArtefact<TAggregate>;
    } 
    
    public static bool LastCommandRanWas<TLastCliCommand>(
        this IEnumerable<AnonymousArtefact> artefacts) where TLastCliCommand : CliCommand
            => LinqEnumerable
                .OfType<RanCliCommandAnonymousArtefact>(artefacts)
                .LastOrDefault()
                ?.RanCommand is TLastCliCommand;
}