namespace KitCli.Commands.Abstractions.Artefacts;

public class ValuedCliCommandArtefact<TCommandPropertyValue> : CliCommandArtefact
{
    public TCommandPropertyValue ArtefactValue { get; set; }

    protected ValuedCliCommandArtefact(string artefactName, TCommandPropertyValue artefactArtefactValue) : base(artefactName)
    {
        ArtefactValue = artefactArtefactValue;
    }
}