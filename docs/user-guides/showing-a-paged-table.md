# Showing a paged table

**Show a long list one page at a time. The reader asks for the next page, and your second command rebuilds nothing.**

```
> /list-expenses                    > /next-page
+------------------------+          +------------------------+
| Category  | Total Cost |          | Category  | Total Cost |
|-----------+------------|          |-----------+------------|
| Groceries | 412.80     |          | Transport | 88.45      |
|-----------+------------|          |-----------+------------|
| Rent      | 1150.00    |          | Coffee    | 63.20      |
+------------------------+          +------------------------+
```

| Term | Meaning |
|---|---|
| Table builder | Joins aggregator, map and paging into a `Table`. See [Showing a table](showing-a-table.md). |
| Artefact | A remembered outcome a later factory looks up by type and name. |
| Factory | Builds the next command from the artefacts of the run so far. |

Your registry must call `AddArtefactFactoriesForAssembly` (see [Creating a registry](creating-a-registry.md)), or the second command finds nothing.

## How it looks

```mermaid
sequenceDiagram
    actor U as User
    participant A as Your app
    U->>A: /list-expenses
    Note over A: remembers builder,<br/>page size 20, page 1
    A-->>U: page 1
    U->>A: /next-page
    Note over A: factory reads builder and paging<br/>handler turns the page
    A-->>U: page 2
    Note over A: remembers page 2
```

## How to do it

**1. Remember the builder when you show the first page.**

```csharp
var tableBuilder = new ExpenseTableBuilder()
    .WithAggregator(aggregator)
    .WithMap<ExpenseTableMap>()
    .WithPageSize(20)
    .WithPageNumber(1);

return FinishThisCommand()
    .ByShowingTable(tableBuilder.Build())
    .ByRememberingPageSize(20)
    .ByRememberingPageNumber(1)
    .ByRememberingHowToBuildTable(tableBuilder)
    .EndAsync();
```

**2. Collect it back in the next command's factory.**

```csharp
public class NextExpensePageCliCommandFactory : PagedCliCommandFactory<NextExpensePageCliCommand>
{
    public override bool CanCreateWhen()
        => LastCommandIs<ListExpensesCliCommand>() || LastCommandIs<NextExpensePageCliCommand>();

    public override CliCommand Create()
    {
        var tableBuilder = GetRequiredArtefact<TableBuilder<Expense, ExpenseRow>>();
        var (pageSize, pageNumber) = GetPaging();

        return new NextExpensePageCliCommand(tableBuilder.Value, pageSize, pageNumber);
    }
}
```

**3. Turn the page in its handler, and remember where you got to.**

```csharp
command.TableBuilder
    .WithPageSize(command.PageSize)
    .WithPageNumber(command.PageNumber);

return FinishThisCommand()
    .ByShowingTable(command.TableBuilder.Build())
    .ByRememberingPageSize(command.PageSize)
    .ByRememberingPageNumber(command.PageNumber)
    .EndAsync();
```

### Where the page number comes from

`GetPaging()` takes what was typed, else the page the reader was on, else twenty rows and page one. Both of these work through the one factory:

```
/next-page --pageNumber 3
/next-page
```

```mermaid
flowchart TD
    A{"paging typed on the command?"} -->|yes| U["use what was typed"]
    A -->|no| B{"a page remembered from last time?"}
    B -->|yes| R["reuse it"]
    B -->|no| D["twenty rows, page one"]
```

## Common mistakes

- **Writing `--page-number`.** The names are `pageNumber` and `pageSize`, camelCase. Anything else is a different argument and is ignored.
- **Rebuilding the aggregator and map in the second command.** `ByRememberingHowToBuildTable` exists so you reconstruct none of it.
- **Forgetting to remember the new page.** The reader walks from page one every time.

## Learn more

- [Showing a table](showing-a-table.md): building the table in the first place.
- [Remembering state across separate asks](reusable-outcomes-and-the-workflow-run.md): remembering anything between commands.
- [Artefacts](../concepts/artefacts.md): how a remembered builder reaches the next factory.
