using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Workflow.Abstractions;

namespace KitCli.Workflow.Extensions;

/// <summary>
/// Helpers for reading outcomes out of a workflow's runs.
/// </summary>
public static class CliWorkflowRunExtensions
{
    extension(IEnumerable<ICliWorkflowRun> runs)
    {
        /// <summary>
        /// The outcomes kept for the whole session. Only the last run is read: its history starts with the
        /// session outcomes it was handed, followed by its own. Empty before the first run.
        /// </summary>
        /// <returns>The last run's outcomes whose lifetime is <see cref="OutcomeLifetime.Session"/>.</returns>
        public Outcome[] AllSessionOutcomes()
            => runs
                .TakeLast(1)
                .SelectMany(lastRun => lastRun.State.AllOutcomeStateChanges())
                .SelectMany(change => change.Outcomes)
                .Where(outcome => outcome.Lifetime == OutcomeLifetime.Session)
                .ToArray();
    }
}
