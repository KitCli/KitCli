using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Io.Outcomes;

public class CliCommandOutcomeIo : CliIo, ICliCommandOutcomeIo
{
    public void Say(CliCommandOutcome[] outcomes)
    {
        foreach (var outcome in outcomes)
        {
            Say(outcome);
        }
    }
    
    public void Say(CliCommandOutcome outcome)
    {
        switch (outcome)
        {
            case CliCommandTableOutcome tableOutcome:
                Say(tableOutcome.Table.ToString());
                Say($"Results: {tableOutcome.Table.Rows.Count} rows");
                break;
            case CliCommandOutputOutcome outputOutcome:
                Say(outputOutcome.Output);
                break;
            case PageSizeCliCommandOutcome pageSizeOutcome:
                Say($"Page Size: {pageSizeOutcome.PageSize}");
                break;
            case PageNumberCliCommandOutcome pageNumberOutcome:
                Say($"Page Number: {pageNumberOutcome.PageNumber}");
                break;
            case CliCommandNotFoundOutcome:
                Say("Command not found.");
                break;
            case CliCommandExceptionOutcome exceptionOutcome:
                Say($"Exception occured: {exceptionOutcome.Exception.Message}");
                Say($"Exception occured: {exceptionOutcome.Exception.StackTrace}");
                while (exceptionOutcome.Exception.InnerException != null)
                    Say($"Exception occured: {exceptionOutcome.Exception.InnerException.StackTrace}");
                break;
        }
    }
}