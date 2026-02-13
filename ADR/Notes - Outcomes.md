# High Level Notes
- Outcomes are the results of a command being run.
- They feed into what happens next, and will appear as artefacts in command factories.
- There are 3 types:
    - Reusable: Results of a command that can be reused to build other commands, and ultimately, serve a purpose in other operations.
    - Skippable: Typically results that only need to exist for a short time: to print a message to the terminal, to support a conditional, etc.
    - Final: Considered to be the final result of execution steps. For example, if multiple commands are used in tandem, this outcome would be the very last run and signal the end of command history. A new command after it would start a new command history.
- There are several that come built in.
- You can create custom outcomes by:
    - Creating a new class that inherits from `Outcome`.
    - Creating a new class that inherits from `Artefact`
    - Creating a `CliArtefactFactory<TArtefactType>` that demonstrates how to map it and registering it to DI.

# Future Opportunities
- Add a basic Artefact Factory for every artefact type to remove the boilerplate.
- Outcome Builder could have index optional overloads to set order of outcomes to run.