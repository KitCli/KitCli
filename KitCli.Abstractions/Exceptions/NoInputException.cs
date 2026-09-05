namespace KitCli.Abstractions.Exceptions;

/// <summary>
/// Thrown when an ask is made of an I/O with nothing attached to its input, such as a headless
/// app's. Check <see cref="Io.ICliIo.CanAsk"/> before asking.
/// </summary>
public class NoInputException()
    : CliException(CliExceptionCode.NoInput, "Nothing is attached to this app's input, so there is nobody to answer. Check CanAsk before asking, and take what you need from the arguments instead.")
{
}
