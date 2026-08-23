using KitCli.Abstractions.Exceptions;

namespace KitCli.Commands.Abstractions.Exceptions;

/// <summary>
/// Thrown when no <see cref="Factories.ICliCommandFactory"/> is available to construct a command for a
/// resolved instruction name — either none are keyed under that name, or none of the candidates keyed
/// under it return <see langword="true"/> from <c>CanCreateWhen()</c>.
/// </summary>
public class NoCommandGeneratorException : CliException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoCommandGeneratorException"/> class with an empty message.
    /// </summary>
    public NoCommandGeneratorException() : base(CliExceptionCode.NoCommandGenerator, string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoCommandGeneratorException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public NoCommandGeneratorException(string message) : base(CliExceptionCode.NoCommandGenerator, message)
    {
    }
}