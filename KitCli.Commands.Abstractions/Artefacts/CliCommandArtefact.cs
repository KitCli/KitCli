namespace KitCli.Commands.Abstractions.Artefacts;

/// <summary>
/// Defines a property that can be associated with a CLI command.
/// </summary>
public abstract class CliCommandArtefact(string artefactName)
{
    public string ArtefactName { get; set; } = artefactName;
}