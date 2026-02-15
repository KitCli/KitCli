# ADR 0001: Outcomes to Artefact Pipeline

## Status
Accepted

## Context
KitCli provides an extensible command-line interface framework where commands can communicate with each other through a pipeline mechanism. Commands execute and produce outcomes, which can be converted into artefacts that subsequent commands can use. This architecture enables building complex CLI workflows where commands can pass data and state to each other.

The framework needs a clear, documented pattern for:
1. Returning outcomes from command handlers
2. Creating `Outcome`s and mapping them to `Artefact`s with artefact factories (e.g., `ArtefactFactory<>`)
3. Using `Artefact`s in command factories (e.g., `CliCommandFactory<>`)

## Decision

### 1. Returning Outcomes from Commands

Commands return outcomes through their handlers implementing `ICliCommandHandler<TCommand>`. The handler's `Handle` method returns `Task<CliCommandOutcome[]>`, an array of outcomes.

**Outcome Types:**
- **Final Outcomes** (`OutcomeKind.Final`): End the workflow run
  - `FinalSayOutcome` - Final output message
  - `TableOutcome` - Display data in a table
  - `CliCommandNotFoundOutcome` - Command not found
  - `NothingOutcome` - No operation
- **Reusable Outcomes** (`OutcomeKind.Reusable`): Allow further operations
  - `SayOutcome` - Output a message (allows chaining)
  - `PageNumberOutcome` - Set page number
  - `PageSizeOutcome` - Set page size
  - `NextCliCommandOutcome` - Chain to next command
  - `ReactionOutcome` - Command reaction
- **Skippable Outcomes** (`OutcomeKind.Skippable`): No effect on workflow
  - `RanCliCommandOutcome` - Track command execution

**Example:**
```csharp
public class MyCommandHandler : CliCommandHandler<MyCommand>
{
    public override Task<Outcome[]> HandleCommand(MyCommand request, CancellationToken cancellationToken)
    {
        var result = PerformSomeWork();
        
        return FinishThisCommand()
            .ByResultingIn(new MyCustomOutcome(result))
            .EndAsync();
    }
}
```

**Fluent Outcome Building:**
The `CliCommandHandler<T>` base class provides a fluent API for building outcomes via `FinishThisCommand()`:
```csharp
protected static OutcomeList FinishThisCommand()  // Start building outcomes

// Fluent methods for adding outcomes:
.ByResultingIn(Outcome outcome)  // Add any custom outcome
.ByResultingIn(params Outcome[] outcomes)  // Add multiple outcomes
.BySaying(string message)  // Add reusable say outcome
.BySaying(params string[] messages)  // Add multiple say outcomes
.ByFinallySaying(string message)  // Add final say outcome (ends workflow)
.ByShowingTable(Table table)  // Add table outcome
.ByRememberingPageNumber(int pageNumber)  // Add page number
.ByRememberingPageSize(int pageSize)  // Add page size
.ByMovingToCommand(CliCommand nextCommand)  // Chain to next command
.ByReacting(CliCommandReaction reaction)  // Add reaction outcome
.ByFinallyDoingNothing()  // Add nothing outcome (ends workflow)
.ByNotFindingCommand()  // Add command not found outcome
.End()  // Complete and return Outcome[]
.EndAsync()  // Complete and return Task<Outcome[]>
```

Common usage pattern for simply outputting messages:
```csharp
return FinishThisCommand()
    .BySaying("Command executed successfully")
    .ByRememberingPageNumber(1)
    .ByRememberingPageSize(10)
    .EndAsync();
```

Handlers extend `CliCommandHandler<TCliCommand>` and override `HandleCommand()` to implement command logic.

### 2. Creating Custom Outcomes with Artefact Factories

To create custom reusable outcomes that can be converted to artefacts:

**Step 1: Create a Custom Outcome**
```csharp
public record MyCustomOutcome(int MyValue) : Outcome(OutcomeKind.Reusable);
```

**Step 2: Create a Corresponding Artefact**
```csharp
public record MyCustomArtefact(int MyValue) : AnonymousArtefact;
```

**Step 3: Create an Artefact Factory**

Extend `ArtefactFactory<TOutcome>` to simplify artefact creation:

```csharp
public class MyCustomArtefactFactory : ArtefactFactory<MyCustomOutcome>
{
    protected override AnonymousArtefact CreateArtefact(MyCustomOutcome outcome) 
        => new MyCustomArtefact(outcome.MyValue);
}
```

The base class automatically implements the `For()` method to check if an outcome matches the factory's type. The implementor only needs to focus on `CreateArtefact()` method.

**Step 4: Register the Factory**
```csharp
services.AddCommandArtefactFactory<MyCustomArtefactFactory>();
```

**Pipeline Flow:**
1. Command handler returns `MyCustomOutcome` (reusable)
2. `CliWorkflowCommandProvider.ConvertOutcomesToArtefacts()` processes outcomes
3. Factory's `For()` method identifies matching outcomes
4. Factory's `Create()` method converts outcome to artefact
5. Artefacts are passed to subsequent command factories

### 3. Using Artefacts in Commands

Command factories receive artefacts and can use them to determine if they can create a command and to configure the command.

**Step 1: Create a Command Factory**

Extend `CliCommandFactory<TCliCommand>` to access helper methods:

```csharp
public class MyCommandFactory : CliCommandFactory<MyCommand>
{
    public override bool CanCreateWhen()
    {
        // Use AnyArtefact() helper to check if required artefacts are present
        return AnyArtefact<int>("myValue");
    }

    public override CliCommand Create()
    {
        // Use GetRequiredArgument() to simplify argument extraction
        var value = GetRequiredArgument<int>("value");
        
        return new MyCommand(value.Value);
    }
}
```

**Step 2: Register Command Factories**

Command factories are registered automatically via reflection when using `AddCommandsFromAssembly()`:

```csharp
services.AddCommandsFromAssembly(Assembly.GetExecutingAssembly());
```

The framework automatically:
- Discovers all `CliCommandFactory<>` implementations
- Registers them with appropriate service keys based on command names
- Sets up both full and shorthand command name mappings

**Artefact Helper Methods:**
When extending `CliCommandFactory<T>`, you have access to:
```csharp
// Get instruction arguments
protected InstructionArgument<TArgumentType>? GetArgument<TArgumentType>(string? argumentName)
protected InstructionArgument<TArgumentType> GetRequiredArgument<TArgumentType>(string? argumentName)
protected IEnumerable<InstructionArgument<TArgumentType>> GetArguments<TArgumentType>()

// Get artefacts from previous commands (TArtefactType is the value type, e.g., int, string)
protected Artefact<TArtefactType>? GetArtefact<TArtefactType>(string? artefactName)
protected Artefact<TArtefactType> GetRequiredArtefact<TArtefactType>(string? artefactName)
protected IEnumerable<Artefact<TArtefactType>> GetArtefacts<TArtefactType>()

// Check if artefacts exist
protected bool AnyArtefact<TArtefactType>(string? artefactName = null)

// Check sub-command
protected bool SubCommandIs(string subCommandName)

// Artefacts and Instruction are available as properties (not nullable)
protected List<AnonymousArtefact> Artefacts
protected Instruction Instruction
```

The base factory class provides these helpers for common command creation patterns.

**Complete Example:**
```csharp
// First Command - Produces an outcome
public record SetValueCommand(int Value) : CliCommand;

// Handler - Returns reusable outcome using fluent API
public class SetValueCommandHandler : CliCommandHandler<SetValueCommand>
{
    public override Task<Outcome[]> HandleCommand(SetValueCommand command, CancellationToken cancellationToken)
    {
        return FinishThisCommand()
            .BySaying($"Set value to: {command.Value}")
            .ByResultingIn(new MyCustomOutcome(command.Value))  // Add custom reusable outcome
            .EndAsync();
    }
}

// Second Command - Consumes the artefact from first command
public record ProcessValueCommand(int InputValue) : CliCommand;

// Factory - Uses artefact from previous command
public class ProcessValueCommandFactory : CliCommandFactory<ProcessValueCommand>
{
    public override bool CanCreateWhen()
    {
        // Use helper method to check if artefact exists
        return AnyArtefact<int>("myValue");
    }

    public override CliCommand Create()
    {
        // Use GetRequiredArtefact() and GetRequiredArgument() to simplify extraction
        var artefact = GetRequiredArtefact<int>("myValue");
        var inputArg = GetRequiredArgument<int>("input");
        
        return new ProcessValueCommand(artefact.Value + inputArg.Value);
    }
}

// Handler for second command
public class ProcessValueCommandHandler : CliCommandHandler<ProcessValueCommand>
{
    public override Task<Outcome[]> HandleCommand(ProcessValueCommand command, CancellationToken cancellationToken)
    {
        return FinishThisCommand()
            .ByFinallySaying($"Processed value {command.InputValue} from previous command")
            .EndAsync();
    }
}
```

## Consequences

### Positive
- **Clear Separation of Concerns**: Outcomes represent command results, artefacts represent reusable state
- **Type Safety**: Strong typing throughout the pipeline ensures correctness
- **Extensibility**: New outcome/artefact types can be added without modifying core framework
- **Flexibility**: Commands can return multiple outcomes of different types
- **Composability**: Commands can be chained together through the artefact pipeline
- **Testability**: Each component (outcome, artefact, factory) can be tested independently

### Negative
- **Boilerplate**: Each custom outcome requires an artefact, factory, and registration
- **Learning Curve**: Developers must understand the distinction between outcomes and artefacts
- **Indirection**: The outcome-to-artefact conversion adds a layer of indirection

### Neutral
- **Workflow State Management**: The `CliWorkflowRunState` tracks all outcomes through `OutcomeCliWorkflowRunStateChange` objects, enabling workflow state inspection and debugging
- **Service Provider Integration**: All factories are resolved through dependency injection, enabling testing and extensibility
