namespace KitCli.Abstractions.Io;

/// <summary>
/// One item in a list the person picks from, as shown by <see cref="ICliIo.ChooseAsync"/> and
/// <see cref="ICliIo.ChooseManyAsync"/>.
/// </summary>
/// <param name="Label">The text the person sees.</param>
/// <param name="Value">Anything the caller wants handed back with the picked choice, such as the object the label stands for.</param>
public record CliChoice(string Label, object? Value = null);
