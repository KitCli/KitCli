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
public class MyCommandHandler : CliCommandHandler, ICliCommandHandler<MyCommand>
{
    public Task<CliCommandOutcome[]> Handle(MyCommand request, CancellationToken cancellationToken)
    {
        var outcomes = new CliCommandOutcome[]
        {
            new OutputCliCommandOutcome("Command executed successfully"),
            new PageNumberCliCommandOutcome(1),  // Reusable outcome
            new PageSizeCliCommandOutcome(10)     // Reusable outcome
        };
        
        return Task.FromResult(outcomes);
    }
}
```

**Helper Methods:**
The `CliCommandHandler` base class provides convenience methods for common outcome scenarios:
```csharp
protected static CliCommandOutcome[] OutcomeAs()  // Returns CliCommandNothingOutcome
protected static CliCommandOutcome[] OutcomeAs(string message)  // Returns OutputCliCommandOutcome
protected static CliCommandOutcome[] OutcomeAs(Table table)  // Returns TableCliCommandOutcome
protected static CliCommandOutcome[] OutcomeAs(CliListAggregatorFilter filter)  // Returns FilterCliCommandOutcome
```

Handlers can extend `CliCommandHandler` to access these helper methods.

### 2. Creating Custom Outcomes with Artefact Factories

To create custom reusable outcomes that can be converted to artefacts:

**Step 1: Create a Custom Outcome**
```csharp
public record MyCustomOutcome(int MyValue) : CliCommandOutcome(CliCommandOutcomeKind.Reusable);
```

**Step 2: Create a Corresponding Artefact**
```csharp
public class MyCustomArtefact(int myValue) : ValuedCliCommandArtefact<int>(nameof(MyValue), myValue)
{
    public int MyValue { get; } = myValue;
}
```

**Step 3: Create an Artefact Factory**

The framework automatically handles artefact creation through reflection for standard patterns. For custom cases, implement `ICliCommandArtefactFactory`:

```csharp
public class MyCustomArtefactFactory : ICliCommandArtefactFactory
{
    public bool For(CliCommandOutcome outcome) => outcome is MyCustomOutcome;

    public CliCommandArtefact Create(CliCommandOutcome outcome)
    {
        if (outcome is not MyCustomOutcome myOutcome)
            throw new InvalidOperationException($"Expected MyCustomOutcome, got {outcome.GetType().Name}");
            
        return new MyCustomArtefact(myOutcome.MyValue);
    }
}
```

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
```csharp
public class MyCommandFactory : ICliCommandFactory<MyCommand>
{
    public bool CanCreateWhen(CliInstruction instruction, List<CliCommandArtefact> artefacts)
    {
        // Check if required artefacts are present
        return artefacts.Any(x => x is MyCustomArtefact);
    }

    public CliCommand Create(CliInstruction instruction, List<CliCommandArtefact> artefacts)
    {
        // Extract artefact by value type
        var myArtefact = artefacts.OfType<int>();

        // Create command with artefact data
        return new MyCommand(myArtefact?.ArtefactValue ?? 0);
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

**Artefact Extension Methods:**
The framework provides helper methods for working with artefacts in command factories:
```csharp
// Get artefact by value type and name
ValuedCliCommandArtefact<int>? pageNumberArtefact = artefacts.OfType<int>("pageNumber");

// Get artefact by value type only
ValuedCliCommandArtefact<int>? intArtefact = artefacts.OfType<int>();

// Get required artefact (throws if not found)
ValuedCliCommandArtefact<int> requiredIntArtefact = artefacts.OfRequiredType<int>();

// Check for custom artefact class using LINQ
bool hasCustomArtefact = artefacts.Any(x => x is MyCustomArtefact);

// Check if last command ran was of specific type
bool wasLastCommand = artefacts.LastCommandRanWas<MyCommand>();

// Get list aggregator artefact
ListAggregatorCliCommandArtefact<TAggregate>? aggregator = artefacts.OfListAggregatorType<TAggregate>();
```

Command factories can extend base factory classes (like `ListCliCommandFactory`) to access additional helpers for common patterns.

**Complete Example:**
```csharp
// First Command - Produces an outcome
public record SetValueCommand(int Value) : CliCommand;

// Handler - Returns reusable outcome
public class SetValueCommandHandler : CliCommandHandler, ICliCommandHandler<SetValueCommand>
{
    public Task<CliCommandOutcome[]> Handle(SetValueCommand request, CancellationToken cancellationToken)
    {
        var outcomes = new CliCommandOutcome[]
        {
            new OutputCliCommandOutcome($"Set value to: {request.Value}"),
            new MyCustomOutcome(request.Value)  // Reusable outcome - will become artefact
        };
        return Task.FromResult(outcomes);
    }
}

// Second Command - Consumes the artefact from first command
public record ProcessValueCommand(int InputValue) : CliCommand;

// Factory - Uses artefact from previous command
public class ProcessValueCommandFactory : ICliCommandFactory<ProcessValueCommand>
{
    public bool CanCreateWhen(CliInstruction instruction, List<CliCommandArtefact> artefacts)
    {
        // Check if artefact from previous command exists
        return artefacts.Any(x => x is MyCustomArtefact);
    }

    public CliCommand Create(CliInstruction instruction, List<CliCommandArtefact> artefacts)
    {
        // Extract artefact by value type from previous command's outcome
        var artefact = artefacts.OfType<int>();
        return new ProcessValueCommand(artefact?.ArtefactValue ?? 0);
    }
}

// Handler for second command
public class ProcessValueCommandHandler : CliCommandHandler, ICliCommandHandler<ProcessValueCommand>
{
    public Task<CliCommandOutcome[]> Handle(ProcessValueCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new CliCommandOutcome[]
        {
            new OutputCliCommandOutcome($"Processed value {request.InputValue} from previous command")
        });
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
