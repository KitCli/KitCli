using KitCli.Commands.Abstractions.Artefacts;

namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>An artefact carrying whatever a test's outcome said.</summary>
public record TestArtefact(string Said) : Artefact<string>(nameof(Said), Said);
