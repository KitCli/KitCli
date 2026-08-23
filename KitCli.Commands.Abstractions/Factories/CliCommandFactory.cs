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
    /// <summary>
    /// The instruction currently attached to this factory, or <see cref="Instruction.Empty"/> if none has been attached.
    /// </summary>
    protected Instruction Instruction => _instruction ?? Instruction.Empty;

    /// <summary>
    /// The artefacts currently attached to this factory, or an empty list if none have been attached.
    /// </summary>
    protected List<AnonymousArtefact> Artefacts => _artefacts ?? [];

    private Instruction? _instruction;
    private List<AnonymousArtefact>? _artefacts;

    /// <inheritdoc/>
    public abstract bool CanCreateWhen();

    /// <inheritdoc/>
    public abstract CliCommand Create();

    /// <inheritdoc/>
    public ICliCommandFactory Attach(Instruction instruction, List<AnonymousArtefact> artefacts)
    {
        _instruction = instruction;
        _artefacts = artefacts;

        return this;
    }

    /// <summary>
    /// Determines whether the attached instruction's sub-instruction name equals the given value.
    /// </summary>
    /// <param name="subCommandName">The sub-instruction name to compare against.</param>
    /// <returns><see langword="true"/> if the attached instruction's sub-instruction name matches; otherwise <see langword="false"/>.</returns>
    /// <exception cref="Exception">Thrown when no instruction and artefacts have been attached via <see cref="Attach"/>.</exception>
    protected bool SubCommandIs(string subCommandName)
    {
        ValidateAttached();

        return _instruction!.SubInstructionName == subCommandName;
    }

    /// <summary>
    /// Determines whether the attached instruction has any argument of the given type, optionally filtered by name.
    /// </summary>
    /// <typeparam name="TArgumentType">The argument value type to look for.</typeparam>
    /// <param name="argumentName">The argument name to filter by, or <see langword="null"/> to match any name.</param>
    /// <returns><see langword="true"/> if a matching argument exists; otherwise <see langword="false"/>.</returns>
    protected bool AnyArgument<TArgumentType>(string? argumentName) where TArgumentType : notnull
    {
        var typedArguments = GetArguments<TArgumentType>();

        return argumentName == null
            ? typedArguments.Any()
            : typedArguments.Any(argument => argument.Name == argumentName);
    }

    /// <summary>
    /// Gets the last argument of the given type from the attached instruction, optionally filtered by name.
    /// </summary>
    /// <typeparam name="TArgumentType">The argument value type to look for.</typeparam>
    /// <param name="argumentName">The argument name to filter by, or <see langword="null"/> to match any name.</param>
    /// <returns>The last matching argument, or <see langword="null"/> if none match.</returns>
    protected InstructionArgument<TArgumentType>? GetArgument<TArgumentType>(string? argumentName) where TArgumentType : notnull
    {
        var typedArguments = GetArguments<TArgumentType>();

        return argumentName == null
            ? typedArguments.LastOrDefault()
            : typedArguments.LastOrDefault(argument => argument.Name == argumentName);
    }

    /// <summary>
    /// Gets the last argument of the given type from the attached instruction, optionally filtered by name.
    /// </summary>
    /// <typeparam name="TArgumentType">The argument value type to look for.</typeparam>
    /// <param name="argumentName">The argument name to filter by, or <see langword="null"/> to match any name.</param>
    /// <returns>The last matching argument.</returns>
    /// <exception cref="Exception">Thrown when no matching argument is found.</exception>
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

    /// <summary>
    /// Determines whether the attached artefacts contain any artefact of the given type, optionally filtered by name.
    /// </summary>
    /// <typeparam name="TArtefactType">The artefact value type to look for.</typeparam>
    /// <param name="artefactName">The artefact name to filter by, or <see langword="null"/> to match any name.</param>
    /// <returns><see langword="true"/> if a matching artefact exists; otherwise <see langword="false"/>.</returns>
    protected bool AnyArtefact<TArtefactType>(string? artefactName) where TArtefactType : notnull
    {
        var typedArtefacts = GetArtefacts<TArtefactType>();

        return artefactName == null
            ? typedArtefacts.Any()
            : typedArtefacts.Any(artefact => artefact.Name == artefactName);
    }

    /// <summary>
    /// Determines whether the given command type is the most recently run command, based on whether a
    /// matching <c>RanCliCommandArtefact</c> exists in the attached artefacts.
    /// </summary>
    /// <typeparam name="TRanCliCommand">The command type to check for.</typeparam>
    /// <returns><see langword="true"/> if <typeparamref name="TRanCliCommand"/> ran previously; otherwise <see langword="false"/>.</returns>
    protected bool LastCommandWas<TRanCliCommand>() where TRanCliCommand : CliCommand
    {
        var artefact = GetArtefact<CliCommand>(typeof(TRanCliCommand).Name);

        return artefact != null;
    }

    /// <summary>
    /// Gets the last artefact of the given type from the attached artefacts, optionally filtered by name.
    /// </summary>
    /// <typeparam name="TArtefactType">The artefact value type to look for.</typeparam>
    /// <param name="artefactName">The artefact name to filter by, or <see langword="null"/> to match any name.</param>
    /// <returns>The last matching artefact, or <see langword="null"/> if none match.</returns>
    protected Artefact<TArtefactType>? GetArtefact<TArtefactType>(string? artefactName = null) where TArtefactType : notnull
    {
        var typedArtefacts = GetArtefacts<TArtefactType>();

        return artefactName == null
            ? typedArtefacts.LastOrDefault()
            : typedArtefacts.LastOrDefault(artefact => artefact.Name == artefactName);
    }

    /// <summary>
    /// Gets the last aggregator artefact for the given source/aggregate type pair, optionally filtered by name.
    /// </summary>
    /// <typeparam name="TSource">The type of the aggregator's source elements.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregator's aggregated elements.</typeparam>
    /// <param name="artefactName">The artefact name to filter by, or <see langword="null"/> to match any name.</param>
    /// <returns>The last matching aggregator artefact, or <see langword="null"/> if none match.</returns>
    protected Artefact<Aggregator<TSource, TAggregate>>? GetAggregatorArtefact<TSource, TAggregate>(string? artefactName = null)
    {
        var typedArtefacts = GetArtefacts<Aggregator<TSource, TAggregate>>();

        return artefactName == null
            ? typedArtefacts.LastOrDefault()
            : typedArtefacts.LastOrDefault(artefact => artefact.Name == artefactName);
    }

    /// <summary>
    /// Gets the last artefact of the given type from the attached artefacts, optionally filtered by name.
    /// </summary>
    /// <typeparam name="TArtefactType">The artefact value type to look for.</typeparam>
    /// <param name="artefactName">The artefact name to filter by, or <see langword="null"/> to match any name.</param>
    /// <returns>The last matching artefact.</returns>
    /// <exception cref="Exception">Thrown when no matching artefact is found.</exception>
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

    /// <summary>
    /// Gets the last aggregator artefact for the given source/aggregate type pair, optionally filtered by name.
    /// </summary>
    /// <typeparam name="TSource">The type of the aggregator's source elements.</typeparam>
    /// <typeparam name="TAggregate">The type of the aggregator's aggregated elements.</typeparam>
    /// <param name="artefactName">The artefact name to filter by, or <see langword="null"/> to match any name.</param>
    /// <returns>The last matching aggregator artefact.</returns>
    /// <exception cref="Exception">Thrown when no matching aggregator artefact is found.</exception>
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

    /// <summary>
    /// Gets every argument of the given type from the attached instruction.
    /// </summary>
    /// <typeparam name="TArgumentType">The argument value type to look for.</typeparam>
    /// <returns>The matching arguments, in the instruction's order.</returns>
    /// <exception cref="Exception">Thrown when no instruction and artefacts have been attached via <see cref="Attach"/>.</exception>
    protected IEnumerable<InstructionArgument<TArgumentType>> GetArguments<TArgumentType>()
        where TArgumentType : notnull
    {
        ValidateAttached();

        return _instruction!.Arguments.OfType<InstructionArgument<TArgumentType>>();
    }

    /// <summary>
    /// Gets every artefact of the given type from the attached artefacts.
    /// </summary>
    /// <typeparam name="TArtefactType">The artefact value type to look for.</typeparam>
    /// <returns>The matching artefacts, in the run's history order.</returns>
    /// <exception cref="Exception">Thrown when no instruction and artefacts have been attached via <see cref="Attach"/>.</exception>
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