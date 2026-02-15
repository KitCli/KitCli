using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;

namespace KitCli.Commands.Abstractions.Factories;

public abstract class CliCommandFactory<TCliCommand> : ICliCommandFactory where TCliCommand : CliCommand
{
    protected CliInstruction? Instruction;
    protected List<AnonymousArtefact>? Artefacts;
    
    public ICliCommandFactory Attach(CliInstruction instruction, List<AnonymousArtefact> artefacts)
    {
        Instruction = instruction;
        Artefacts = artefacts;

        return this;
    }

    public abstract bool CanCreateWhen();

    public abstract CliCommand Create();

    protected bool SubCommandIs(string subCommandName)
    {
        ValidateAttached();
        
        return Instruction!.SubInstructionName == subCommandName;
    }
    
    protected ValuedCliInstructionArgument<TArgumentType>? GetArgument<TArgumentType>(string? argumentName) where TArgumentType : notnull
    {
        var typedArguments = GetArguments<TArgumentType>();
        
        return argumentName == null
            ? typedArguments.SingleOrDefault() 
            : typedArguments.SingleOrDefault(argument => argument.Name == argumentName);
    }

    protected ValuedCliInstructionArgument<TArgumentType> GetRequiredArgument<TArgumentType>(string? argumentName)
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

    protected Artefact<TArtefactType>? GetArtefact<TArtefactType>(string? artefactName) where TArtefactType : notnull
    {
        var typedArtefacts = GetArtefacts<TArtefactType>();
        
        return artefactName == null
            ? typedArtefacts.SingleOrDefault() 
            : typedArtefacts.SingleOrDefault(artefact => artefact.Name == artefactName);
    }
    
    protected Artefact<TArtefactType> GetRequiredArtefact<TArtefactType>(string? artefactName) where TArtefactType : notnull
    {
        var artefact = GetArtefact<TArtefactType>(artefactName);
        
        if (artefact == null)
        {
            // TODO: Handle further upstream in future.
            throw new Exception($"Required artefact '{artefactName}' of type '{typeof(TArtefactType).Name}' not found.");
        }

        return artefact;
    }

    protected IEnumerable<ValuedCliInstructionArgument<TArgumentType>> GetArguments<TArgumentType>()
        where TArgumentType : notnull
    {
        ValidateAttached();

        return Instruction!.Arguments.OfType<ValuedCliInstructionArgument<TArgumentType>>();
    } 
    
    protected IEnumerable<Artefact<TArtefactType>> GetArtefacts<TArtefactType>() where TArtefactType : notnull
    {
        ValidateAttached();

        return Artefacts!.OfType<Artefact<TArtefactType>>();
    }

    private void ValidateAttached()
    {
        if (Instruction == null || Artefacts == null)
        {
            throw new Exception("Factory not registered, automatic attaching.");
        }
    }
}