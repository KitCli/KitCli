# ADR 0001: Outcomes to Artefact Pipeline

## Status
Accepted

## Premise

KitCli is an extensible command-line interface framework designed to support complex CLI workflows where commands can communicate and pass state to each other. The framework allows commands to execute in sequence, with each command potentially producing outputs that influence subsequent commands in the workflow.

This architecture enables:
- **Command Composition**: Building complex CLI behaviors from simple, reusable command components
- **State Passing**: Transferring data between commands in a type-safe manner
- **Workflow Flexibility**: Creating dynamic command chains based on runtime conditions
- **Separation of Concerns**: Isolating command logic from state management

## Problem

Commands in a CLI workflow need to:
1. **Communicate Results**: Pass their execution results to subsequent commands
2. **Share State**: Make computed values or context available to other commands
3. **Chain Operations**: Enable one command's output to become another command's input
4. **Maintain Type Safety**: Ensure data passed between commands is correctly typed
5. **Support Multiple Outputs**: Allow a single command to produce various types of results

Without a structured approach, this leads to:
- **Tight Coupling**: Commands directly depending on other specific command types
- **State Management Chaos**: No clear pattern for storing and retrieving shared state
- **Type Ambiguity**: Unsafe casting and potential runtime errors
- **Limited Composability**: Difficulty in creating new command combinations

### Specific Challenges

- **How do commands express their results?** (What data structure represents command output?)
- **How is command-specific data made available to other commands?** (What's the state transfer mechanism?)
- **How do commands discover what state is available?** (What's the query interface?)
- **How is the transformation from results to reusable state handled?** (Who converts the data?)

## Solution

The framework implements an **Outcomes to Artefacts Pipeline** that separates command results (Outcomes) from reusable state (Artefacts):

```
Command → Outcomes → ArtefactFactory → Artefacts → Next Command Factory
```

### Architecture Overview

1. **Outcomes** (`Outcome`): Represent the immediate results of command execution
   - Three kinds: Final (end workflow), Reusable (continue workflow), Skippable (metadata only)
   - Returned by command handlers
   - Short-lived, exist only during outcome processing

2. **Artefacts** (`Artefact<T>`): Represent reusable state derived from outcomes
   - Type-safe wrappers around values
   - Passed to subsequent command factories
   - Persist through the workflow run

3. **Artefact Factories** (`ArtefactFactory<TOutcome>`): Convert outcomes to artefacts
   - One factory per outcome type
   - Automatic type matching via base class
   - Registered via dependency injection

### Solution Part 1: Returning Outcomes from Commands

Commands return outcomes through handlers that extend `CliCommandHandler<TCommand>` and override `HandleCommand()`.

#### Outcome Types

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

#### Fluent API for Building Outcomes

The `CliCommandHandler<T>` base class provides `FinishThisCommand()` which returns an `OutcomeList` supporting fluent outcome building:

```csharp
public class MyCommandHandler : CliCommandHandler<MyCommand>
{
    public override Task<Outcome[]> HandleCommand(MyCommand command, CancellationToken cancellationToken)
    {
        var result = PerformSomeWork();
        
        return FinishThisCommand()
            .ByResultingIn(new MyCustomOutcome(result))  // Add custom outcome
            .BySaying("Processing complete")              // Add message
            .EndAsync();                                  // Return Task<Outcome[]>
    }
}
```

**Available Fluent Methods:**
```csharp
.ByResultingIn(Outcome outcome)           // Add any custom outcome
.ByResultingIn(params Outcome[] outcomes) // Add multiple outcomes
.BySaying(string message)                 // Add reusable say outcome
.BySaying(params string[] messages)       // Add multiple say outcomes
.ByFinallySaying(string message)          // Add final say outcome (ends workflow)
.ByShowingTable(Table table)              // Add table outcome
.ByRememberingPageNumber(int pageNumber)  // Add page number
.ByRememberingPageSize(int pageSize)      // Add page size
.ByMovingToCommand(CliCommand nextCommand)// Chain to next command
.ByReacting(CliCommandReaction reaction)  // Add reaction outcome
.ByFinallyDoingNothing()                  // Add nothing outcome (ends workflow)
.ByNotFindingCommand()                    // Add command not found outcome
.End()                                    // Complete and return Outcome[]
.EndAsync()                               // Complete and return Task<Outcome[]>
```

**Common Pattern Example:**
```csharp
return FinishThisCommand()
    .BySaying("Command executed successfully")
    .ByRememberingPageNumber(1)
    .ByRememberingPageSize(10)
    .EndAsync();
```

### Solution Part 2: Creating Custom Outcomes with Artefact Factories

To enable custom state passing between commands, implement the outcome-factory-artefact pattern:

#### Step 1: Create a Custom Outcome

Define an outcome as a record extending `Outcome`:

```csharp
public record MyCustomOutcome(int MyValue) : Outcome(OutcomeKind.Reusable);
```

#### Step 2: Create a Corresponding Artefact

Define an artefact as a record extending `AnonymousArtefact`:

```csharp
public record MyCustomArtefact(int MyValue) : AnonymousArtefact;
```

#### Step 3: Create an Artefact Factory

Extend `ArtefactFactory<TOutcome>` to convert outcomes to artefacts:

```csharp
public class MyCustomArtefactFactory : ArtefactFactory<MyCustomOutcome>
{
    protected override AnonymousArtefact CreateArtefact(MyCustomOutcome outcome) 
        => new MyCustomArtefact(outcome.MyValue);
}
```

The base class automatically implements the `For()` method for type matching. Implementors only need to focus on the `CreateArtefact()` method.

#### Step 4: Register the Factory

```csharp
services.AddCommandArtefactFactory<MyCustomArtefactFactory>();
```

#### Pipeline Flow

1. Command handler returns `MyCustomOutcome` (reusable)
2. `CliWorkflowCommandProvider.ConvertOutcomesToArtefacts()` processes outcomes
3. Framework calls each factory's `For()` method to find matches
4. Matching factory's `CreateArtefact()` method converts outcome to artefact
5. Artefacts are passed to subsequent command factories

### Solution Part 3: Using Artefacts in Commands

Command factories extend `CliCommandFactory<TCliCommand>` to access artefacts and decide whether to create a command.

#### Creating a Command Factory

```csharp
public class MyCommandFactory : CliCommandFactory<MyCommand>
{
    public override bool CanCreateWhen()
    {
        // Check if required artefacts/arguments are present
        return AnyArtefact<int>("myValue");
    }

    public override CliCommand Create()
    {
        // Extract values and create command
        var value = GetRequiredArgument<int>("value");
        return new MyCommand(value.Value);
    }
}
```

#### Automatic Registration

Command factories are registered automatically via reflection:

```csharp
services.AddCommandsFromAssembly(Assembly.GetExecutingAssembly());
```

The framework automatically:
- Discovers all `CliCommandFactory<>` implementations
- Registers them with service keys based on command names
- Sets up both full and shorthand command name mappings

#### Helper Methods Reference

`CliCommandFactory<T>` provides helper methods for accessing arguments and artefacts:

```csharp
// Get instruction arguments
protected InstructionArgument<TArgumentType>? GetArgument<TArgumentType>(string? argumentName)
protected InstructionArgument<TArgumentType> GetRequiredArgument<TArgumentType>(string? argumentName)
protected IEnumerable<InstructionArgument<TArgumentType>> GetArguments<TArgumentType>()

// Get artefacts from previous commands
// Note: TArtefactType is the value type (e.g., int, string), not the artefact class
protected Artefact<TArtefactType>? GetArtefact<TArtefactType>(string? artefactName)
protected Artefact<TArtefactType> GetRequiredArtefact<TArtefactType>(string? artefactName)
protected IEnumerable<Artefact<TArtefactType>> GetArtefacts<TArtefactType>()

// Check if artefacts/arguments exist
protected bool AnyArtefact<TArtefactType>(string? artefactName = null)
protected bool AnyArgument<TArgumentType>(string? argumentName)

// Other helpers
protected bool SubCommandIs(string subCommandName)

// Properties (not nullable)
protected List<AnonymousArtefact> Artefacts
protected Instruction Instruction
```

**Important**: When using `GetArtefact<T>()`, the type parameter `T` is the **value type** (like `int`), not the artefact class type. The returned `Artefact<T>` has a `.Value` property to access the actual value.

### Complete Example: Command Pipeline

This example shows two commands where the first produces an outcome that becomes an artefact for the second:

```csharp
// ===== First Command: Produces an outcome =====
public record SetValueCommand(int Value) : CliCommand;

public class SetValueCommandHandler : CliCommandHandler<SetValueCommand>
{
    public override Task<Outcome[]> HandleCommand(SetValueCommand command, CancellationToken cancellationToken)
    {
        return FinishThisCommand()
            .BySaying($"Set value to: {command.Value}")
            .ByResultingIn(new MyCustomOutcome(command.Value))  // Reusable outcome
            .EndAsync();
    }
}

// ===== Outcome and Artefact Definitions =====
public record MyCustomOutcome(int MyValue) : Outcome(OutcomeKind.Reusable);
public record MyCustomArtefact(int MyValue) : AnonymousArtefact;

public class MyCustomArtefactFactory : ArtefactFactory<MyCustomOutcome>
{
    protected override AnonymousArtefact CreateArtefact(MyCustomOutcome outcome) 
        => new MyCustomArtefact(outcome.MyValue);
}

// ===== Second Command: Consumes the artefact =====
public record ProcessValueCommand(int InputValue) : CliCommand;

public class ProcessValueCommandFactory : CliCommandFactory<ProcessValueCommand>
{
    public override bool CanCreateWhen()
    {
        // Check if artefact from previous command exists
        return AnyArtefact<int>("myValue");
    }

    public override CliCommand Create()
    {
        // Use GetRequiredArtefact() and GetRequiredArgument() for clean code
        var artefact = GetRequiredArtefact<int>("myValue");
        var inputArg = GetRequiredArgument<int>("input");
        
        return new ProcessValueCommand(artefact.Value + inputArg.Value);
    }
}

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

## Constraints / Tradeoffs

### Design Constraints

1. **Outcome Immutability**: Outcomes cannot be modified after creation, ensuring predictable pipeline behavior
2. **Single Responsibility**: Each factory handles exactly one outcome type
3. **Type Safety**: Generics ensure compile-time type checking throughout the pipeline
4. **Workflow State**: `CliWorkflowRunState` tracks all outcomes via `OutcomeCliWorkflowRunStateChange` objects

### Tradeoffs

#### Chosen: Outcomes + Artefacts (Two-Stage Model)
**Benefits:**
- Clear separation between "what happened" (outcomes) and "what's available" (artefacts)
- Outcomes can be Final, Reusable, or Skippable - provides workflow control
- Multiple outcomes can map to a single artefact type
- Easy to add new outcome types without affecting artefacts

**Costs:**
- Requires factory boilerplate for custom outcomes
- Adds indirection layer between command execution and state access
- Developers must understand outcome kinds

**Alternative Rejected: Direct State Passing**
- Would couple commands directly to each other
- No clear workflow control mechanism
- Difficult to support multiple simultaneous state values

#### Chosen: Automatic Factory Registration via Reflection
**Benefits:**
- Reduces boilerplate in startup code
- Ensures consistent registration patterns
- Discovers all factories automatically

**Costs:**
- Slight startup performance cost
- Less explicit - factories registered "magically"
- Requires assembly scanning

**Alternative Rejected: Manual Registration**
- Would require explicit registration of every factory
- Easy to forget registrations
- More verbose startup code

#### Chosen: Generic Base Classes (`CliCommandHandler<T>`, `ArtefactFactory<T>`, `CliCommandFactory<T>`)
**Benefits:**
- Reduces boilerplate in implementations
- Provides type-safe helper methods
- Enforces consistent patterns

**Costs:**
- Learning curve for understanding generic base classes
- Less flexibility for unusual patterns

**Alternative Rejected: Interfaces Only**
- Would require reimplementing helper methods in each class
- More boilerplate code
- Inconsistent patterns across implementations

## Questions & Answers

### Q: Why separate Outcomes from Artefacts?

**A:** Outcomes represent the **immediate result** of a command (e.g., "I printed this message", "I computed this value"). Artefacts represent **reusable state** for subsequent commands (e.g., "here's a page size other commands can use"). 

This separation allows:
- Commands to produce multiple types of results (messages + data)
- Different outcome kinds (Final, Reusable, Skippable) for workflow control
- Transformation logic to live in factories rather than commands

### Q: When should I use Final vs Reusable vs Skippable outcomes?

**A:** 
- **Final**: Use when this is the last action in the workflow (e.g., `ByFinallySaying()`, `ByShowingTable()`). The workflow ends after a Final outcome.
- **Reusable**: Use when you want to pass data to subsequent commands (e.g., `PageNumberOutcome`, custom outcomes). The workflow continues.
- **Skippable**: Use for metadata that doesn't affect workflow flow (e.g., `RanCliCommandOutcome` for tracking).

### Q: How do I pass complex data between commands?

**A:**
1. Create a custom outcome record with your data
2. Create a corresponding artefact record
3. Create a factory that transforms outcome → artefact
4. Register the factory
5. Use `GetArtefact<T>()` in the next command's factory

Example: Passing a configuration object, a list of items, or computed results.

### Q: Why is the type parameter in `GetArtefact<T>()` the value type, not the artefact class?

**A:** The framework uses `Artefact<TValue>` as a generic wrapper. For example, `PageNumberArtefact` extends `Artefact<int>`. When retrieving it, you use `GetArtefact<int>("pageNumber")` which returns an `Artefact<int>?` with a `.Value` property to access the `int` value.

This design allows:
- Type-safe value access without casting
- Multiple artefacts with the same value type but different names
- Consistent query interface across all artefact types

### Q: Can a command produce multiple outcomes?

**A:** Yes! Use `.ByResultingIn()` multiple times or pass multiple outcomes:
```csharp
return FinishThisCommand()
    .ByResultingIn(new FirstOutcome(data1))
    .ByResultingIn(new SecondOutcome(data2))
    .BySaying("Both outcomes produced")
    .EndAsync();
```

Each reusable outcome will be converted to an artefact and made available to subsequent commands.

### Q: What happens if a factory isn't registered?

**A:** If an outcome has no matching factory, it's simply not converted to an artefact. This is by design - not all outcomes need to become artefacts (e.g., Final outcomes like `FinalSayOutcome` don't need artefacts since they end the workflow).

### Q: How do I debug the pipeline?

**A:**
- Check `CliWorkflowRunState` which tracks all outcomes
- Use logging in your `CreateArtefact()` methods
- Inspect the `Artefacts` property in your factory's `Create()` method
- Use `GetArtefacts<T>()` to see all available artefacts of a type

### Q: Can I have multiple factories for the same outcome type?

**A:** No. Each outcome type should have exactly one factory. If you need different transformations, create different outcome types.

### Q: Should every outcome have a factory?

**A:** No. Only **Reusable** outcomes that need to provide data to subsequent commands require factories. Final and Skippable outcomes typically don't need factories because:
- Final outcomes end the workflow (no subsequent commands)
- Skippable outcomes are metadata only

### Q: How do I test commands that depend on artefacts?

**A:**
1. **Test the factory**: Verify `CanCreateWhen()` returns true/false correctly
2. **Test with mock artefacts**: Create test artefacts directly and pass them to the factory
3. **Test the handler**: Test `HandleCommand()` in isolation
4. **Integration test**: Run commands in sequence to verify the full pipeline

Example:
```csharp
var factory = new MyCommandFactory();
factory.Attach(instruction, artefacts);  // Inject test data
Assert.True(factory.CanCreateWhen());
var command = factory.Create();
// Assert on command properties
```
