using KitCli.Instructions.Abstractions;

namespace KitCli.Instructions.Builders;

/// <summary>
/// Builds a strongly-typed <see cref="AnonymousInstructionArgument"/> from a raw argument value,
/// and determines whether it can handle a given raw value.
/// </summary>
public interface IInstructionArgumentBuilder
{
    /// <summary>
    /// Determines whether this builder can parse the supplied raw argument value.
    /// </summary>
    /// <param name="argumentValue">The raw argument value from terminal input, or <see langword="null"/> if none was supplied.</param>
    /// <returns><see langword="true"/> if this builder can handle the value; otherwise, <see langword="false"/>.</returns>
    bool For(string? argumentValue);

    /// <summary>
    /// Creates a strongly-typed instruction argument from the raw argument name and value.
    /// </summary>
    /// <param name="argumentName">The name of the argument.</param>
    /// <param name="argumentValue">The raw argument value from terminal input, or <see langword="null"/> if none was supplied.</param>
    /// <returns>The parsed instruction argument.</returns>
    AnonymousInstructionArgument Create(string argumentName, string? argumentValue);
}
