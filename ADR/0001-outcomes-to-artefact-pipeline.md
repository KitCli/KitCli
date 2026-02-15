# ADR 0001: Outcomes to Artefact Pipeline

## Status
Accepted

## Context
KitCli provides an extensible command-line interface framework where commands can communicate with each other through a pipeline mechanism. Commands execute and produce outcomes, which can be converted into artefacts that subsequent commands can use. This architecture enables building complex CLI workflows where commands can pass data and state to each other.

The framework needs a clear, documented pattern for:
1. Returning outcomes from command handlers
2. Creating `Outcome`s and mapping them to `Artefact`s with artefact factories (e.g., `ICliCommandArtefactFactory`)
3. Using `Artefact`s in command factories (e.g., `ICliCommandFactory<>`)

## Decision

### 1. Returning Outcomes from Commands

Commands return outcomes through their handlers implementing `ICliCommandHandler<TCommand>`. The handler's `Handle` method returns `Task<CliCommandOutcome[]>`, an array of outcomes.

**Outcome Types:**
- **Final Outcomes** (`CliCommandOutcomeKind.Final`): End the workflow run
  - `OutputCliCommandOutcome` - Output a message
  - `TableCliCommandOutcome` - Display data in a table
  - `CliCommandNotFoundOutcome` - Command not found
  - `ExceptionCliCommandOutcome` - Exception occurred
  - `CliCommandNothingOutcome` - No operation
- **Reusable Outcomes** (`CliCommandOutcomeKind.Reusable`): Allow further operations
  - `PageNumberCliCommandOutcome` - Set page number
  - `PageSizeCliCommandOutcome` - Set page size
  - `CliCommandMessageOutcome` - Pass a message
  - `CliCommandAggregatorOutcome` - Aggregator data
  - `ListAggregatorCliCommandOutcome` - List aggregator
  - `FilterCliCommandOutcome` - Metadata indicating a filter was applied to an aggregate
- **Skippable Outcomes** (`CliCommandOutcomeKind.Skippable`): No effect on workflow
  - `RanCliCommandOutcome` - Track command execution

**Example:**
```csharp
public class MyCommandHandler : CliCommandHandler<MyCommand>
{
    public override Task<Outcome[]> HandleCommand(MyCommand request, CancellationToken cancellationToken)
    {
        return FinishThisCommand()
            .BySaying("Command executed successfully")
            .WithPageNumber(1)  // Reusable outcome
            .WithPageSize(10)   // Reusable outcome
            .EndAsync();
    }
}
```

**Helper Methods:**
The `CliCommandHandler<T>` base class provides a fluent API for building outcomes via `FinishThisCommand()`:
```csharp
protected static OutcomeList FinishThisCommand()  // Start building outcomes

// Fluent methods for adding outcomes:
.BySaying(string message)  // Add output message
.ByFinallySaying(string message)  // Add final output message
.WithPageNumber(int pageNumber)  // Add page number
.WithPageSize(int pageSize)  // Add page size
.ByMovingToCommand(CliCommand nextCommand)  // Chain to next command
.EndAsync()  // Complete and return Task<Outcome[]>
```

Handlers extend `CliCommandHandler<TCliCommand>` and override `HandleCommand()` to implement command logic.

### 2. Creating Custom Outcomes with Artefact Factories

To create custom reusable outcomes that can be converted to artefacts:

**Step 1: Create a Custom Outcome**
```csharp
public record MyCustomOutcome(int MyValue) : CliCommandOutcome(CliCommandOutcomeKind.Reusable);
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
    {
        return new MyCustomArtefact(outcome.MyValue);
    }
}
```

The base class automatically handles type checking via the `For()` method.

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
        // Check if required artefacts are present
        return Artefacts!.Any(x => x is MyCustomArtefact);
    }

    public override CliCommand Create()
    {
        // Use helper methods to extract arguments
        var valueArg = GetArgument<int>("value");
        
        // Or get artefacts from previous commands
        var myArtefact = Artefacts!.OfType<MyCustomArtefact>().FirstOrDefault();
        
        return new MyCommand(myArtefact?.MyValue ?? valueArg?.Value ?? 0);
    }
}
```

**Step 2: Register Command Factories**

Command factories are registered automatically via reflection when using `AddCommandsFromAssembly()`:

```csharp
services.AddCommandsFromAssembly(Assembly.GetExecutingAssembly());
```

The framework automatically:
- Discovers all `ICliCommandFactory<>` implementations
- Registers them with appropriate service keys based on command names
- Sets up both full and shorthand command name mappings

**Artefact Helper Methods:**
When extending `CliCommandFactory<T>`, you have access to:
```csharp
// Get instruction arguments
protected InstructionArgument<TType>? GetArgument<TType>(string? argumentName)
protected InstructionArgument<TType> GetRequiredArgument<TType>(string? argumentName)

// Check sub-command
protected bool SubCommandIs(string subCommandName)

// Artefacts are available via the Artefacts property
protected List<AnonymousArtefact>? Artefacts

// Instruction is available via the Instruction property  
protected Instruction? Instruction
```

The base factory class provides these helpers for common command creation patterns.

**Complete Example:**
```csharp
// First Command - Produces an outcome
public record SetValueCommand(int Value) : CliCommand;

// Handler - Returns reusable outcome using fluent API
public class SetValueCommandHandler : CliCommandHandler<SetValueCommand>
{
    public override Task<Outcome[]> HandleCommand(SetValueCommand request, CancellationToken cancellationToken)
    {
        return FinishThisCommand()
            .BySaying($"Set value to: {request.Value}")
            .WithCustomOutcome(new MyCustomOutcome(request.Value))  // Reusable outcome
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
        // Check if artefact from previous command exists
        return Artefacts!.Any(x => x is MyCustomArtefact);
    }

    public override CliCommand Create()
    {
        // Extract artefact from previous command's outcome
        var artefact = Artefacts!.OfType<MyCustomArtefact>().FirstOrDefault();
        return new ProcessValueCommand(artefact?.MyValue ?? 0);
    }
}

// Handler for second command
public class ProcessValueCommandHandler : CliCommandHandler<ProcessValueCommand>
{
    public override Task<Outcome[]> HandleCommand(ProcessValueCommand request, CancellationToken cancellationToken)
    {
        return FinishThisCommand()
            .BySaying($"Processed value {request.InputValue} from previous command")
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
