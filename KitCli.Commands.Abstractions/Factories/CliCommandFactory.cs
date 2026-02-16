using KitCli.Abstractions.Aggregators;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;

namespace KitCli.Commands.Abstractions.Factories;

/// <summary>
/// A custom factory for creating a specific <see cref="CliCommand"/>.
/// This is useful when the command creation logic is complex and cannot be easily handled by a simple constructor or when it requires access to the instruction and artefacts for decision-making.
/// If you need don't need to use Arguments or Artefacts, you do not need to create this factory: basic commands are automatically created.
/// </summary>
/// <typeparam name="TCliCommand">A custom created Command.</typeparam>
public abstract class CliCommandFactory<TCliCommand> : ICliCommandFactory where TCliCommand : CliCommand
{
    protected Instruction Instruction => _instruction ?? Instruction.Empty;
    protected List<AnonymousArtefact> Artefacts => _artefacts ?? [];
    
    private Instruction? _instruction;
    private List<AnonymousArtefact>? _artefacts;
    
    public abstract bool CanCreateWhen();

    public abstract CliCommand Create();
    
    public ICliCommandFactory Attach(Instruction instruction, List<AnonymousArtefact> artefacts)
    {
        _instruction = instruction;
        _artefacts = artefacts;

        return this;
    }

    protected bool SubCommandIs(string subCommandName)
    {
        ValidateAttached();
        
        return _instruction!.SubInstructionName == subCommandName;
    }
    
    protected bool AnyArgument<TArgumentType>(string? argumentName) where TArgumentType : notnull
    {
        var typedArguments = GetArguments<TArgumentType>();
        
        return argumentName == null
            ? typedArguments.Any() 
            : typedArguments.Any(argument => argument.Name == argumentName);
    }
    
    protected InstructionArgument<TArgumentType>? GetArgument<TArgumentType>(string? argumentName) where TArgumentType : notnull
    {
        var typedArguments = GetArguments<TArgumentType>();
        
        return argumentName == null
            ? typedArguments.LastOrDefault() 
            : typedArguments.LastOrDefault(argument => argument.Name == argumentName);
    }

    protected InstructionArgument<TArgumentType> GetRequiredArgument<TArgumentType>(string? argumentName)
        where TArgumentType : notnull
    {
        var argument = GetArgument<TArgumentType>(argumentName);

        if (argument == null)
        {
            // TODO: Handle better upstream.
            throw new Exception($"Required argument '{argumentName}' of type '{typeof(TArgumentType).Name}' not found.");
        }

        return argument;
    }

    protected bool AnyArtefact<TArtefactType>(string? artefactName) where TArtefactType : notnull
    {
        var typedArtefacts = GetArtefacts<TArtefactType>();
        
        return artefactName == null
            ? typedArtefacts.Any() 
            : typedArtefacts.Any(artefact => artefact.Name == artefactName);
    }
    
    protected bool LastCommandWas<TRanCliCommand>() where TRanCliCommand : CliCommand
    {
        var artefact = GetArtefact<CliCommand>(typeof(TRanCliCommand).Name);

        return artefact != null;
    }
    
    protected Artefact<TArtefactType>? GetArtefact<TArtefactType>(string? artefactName = null) where TArtefactType : notnull
    {
        var typedArtefacts = GetArtefacts<TArtefactType>();
        
        return artefactName == null
            ? typedArtefacts.LastOrDefault() 
            : typedArtefacts.LastOrDefault(artefact => artefact.Name == artefactName);
    }
    
    protected Artefact<Aggregator<TSource, TAggregate>>? GetAggregatorArtefact<TSource, TAggregate>(string? artefactName = null)
    {
        var typedArtefacts = GetArtefacts<Aggregator<TSource, TAggregate>>();
        
        return artefactName == null
            ? typedArtefacts.LastOrDefault() 
            : typedArtefacts.LastOrDefault(artefact => artefact.Name == artefactName);
    }
    
    protected Artefact<TArtefactType> GetRequiredArtefact<TArtefactType>(string? artefactName = null) where TArtefactType : notnull
    {
        var artefact = GetArtefact<TArtefactType>(artefactName);
        
        if (artefact == null)
        {
            // TODO: Handle further upstream in future.
            throw new Exception($"Required artefact '{artefactName}' of type '{typeof(TArtefactType).Name}' not found.");
        }

        return artefact;
    }
    
    protected Artefact<Aggregator<TSource, TAggregate>> GetRequiredAggregatorArtefact<TSource, TAggregate>(string? artefactName = null)
    {
        var artefact = GetAggregatorArtefact<TSource, TAggregate>(artefactName);
        
        if (artefact == null)
        {
            // TODO: Handle further upstream in future.
            throw new Exception($"Required artefact '{artefactName}' of type '{typeof(Aggregator<TSource, TAggregate>).Name}' not found.");
        }

        return artefact;
    }

    protected IEnumerable<InstructionArgument<TArgumentType>> GetArguments<TArgumentType>()
        where TArgumentType : notnull
    {
        ValidateAttached();

        return _instruction!.Arguments.OfType<InstructionArgument<TArgumentType>>();
    } 
    
    protected IEnumerable<Artefact<TArtefactType>> GetArtefacts<TArtefactType>() where TArtefactType : notnull
    {
        ValidateAttached();

        return _artefacts!.OfType<Artefact<TArtefactType>>();
    }

    private void ValidateAttached()
    {
        if (_instruction == null || _artefacts == null)
        {
            throw new Exception("Factory not registered, automatic attaching.");
        }
    }
}