using KitCli.Abstractions.Exceptions;

namespace KitCli.Commands.Abstractions.Exceptions;

/// <summary>
/// Thrown when no <see cref="Factories.ICliCommandReactionFactory"/> is available to construct a reaction
/// for a specified reaction type — either none are keyed under that type, or none of the candidates keyed
/// under it return <see langword="true"/> from <c>CanCreateWhen()</c>.
/// </summary>
public class NoReactionFactoryException : CliException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoReactionFactoryException"/> class with an empty message.
    /// </summary>
    public NoReactionFactoryException() : base(CliExceptionCode.NoReactionFactory, string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoReactionFactoryException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public NoReactionFactoryException(string message) : base(CliExceptionCode.NoReactionFactory, message)
    {
    }
}
