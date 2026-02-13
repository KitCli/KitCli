namespace KitCli.Commands.Abstractions.Artefacts;

public class Artefact<TCommandPropertyValue> : AnonymousArtefact
{
    public TCommandPropertyValue Value { get; set; }

    protected Artefact(string name, TCommandPropertyValue value) : base(name)
    {
        Value = value;
    }
}