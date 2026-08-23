namespace KitCli.Commands.Abstractions;

/// <summary>
/// Declares an additional instruction name that a <see cref="CliCommand"/> responds to, alongside its
/// mechanically-derived full and shorthand names (see <see cref="CliCommand.GetInstructionName()"/>).
/// Apply more than once to register more than one alias.
/// </summary>
/// <param name="name">The additional instruction name this command should respond to.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class CliCommandAliasAttribute(string name) : Attribute
{
    /// <summary>
    /// The additional instruction name this command should respond to.
    /// </summary>
    public string Name { get; } = name;
}
