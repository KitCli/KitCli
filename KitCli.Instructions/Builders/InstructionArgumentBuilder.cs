namespace KitCli.Instructions.Builders;

/// <summary>
/// Provides shared validation logic for instruction argument builders that require a non-null raw value.
/// </summary>
public abstract class InstructionArgumentBuilder
{
    /// <summary>
    /// Returns the supplied argument value, ensuring it is not <see langword="null"/>.
    /// </summary>
    /// <typeparam name="TValueType">The type of the argument value.</typeparam>
    /// <param name="argumentName">The name of the argument, used in the exception message if validation fails.</param>
    /// <param name="argumentValue">The argument value to validate.</param>
    /// <returns>The non-null argument value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argumentValue"/> is <see langword="null"/>.</exception>
    protected TValueType GetValidValue<TValueType>(string argumentName, TValueType? argumentValue) where TValueType : notnull
    {
        if (argumentValue == null)
        {
            throw new ArgumentNullException($"Argument {argumentName} cannot be null");
        }

        return argumentValue;
    }
}