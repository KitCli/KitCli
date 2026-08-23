namespace KitCli.Commands.Abstractions;

/// <summary>
/// Declares an instruction name that the workflow run should suggest once this <see cref="CliCommand"/>
/// has reached a reusable checkpoint and the next typed input doesn't resolve to any command. Apply more
/// than once to suggest more than one next command.
/// </summary>
/// <param name="name">The instruction name to suggest.</param>
/// <param name="description">A short description of what that instruction does, shown alongside it.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class CliNextCommandIsAttribute(string name, string description) : Attribute
{
    /// <summary>
    /// The instruction name to suggest.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// A short description of what that instruction does, shown alongside it.
    /// </summary>
    public string Description { get; } = description;
}
