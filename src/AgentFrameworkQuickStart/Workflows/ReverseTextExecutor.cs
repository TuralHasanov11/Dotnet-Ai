using Microsoft.Agents.AI.Workflows;

namespace AgentFrameworkQuickStart.Workflows;

public partial class ReverseTextExecutor() : Executor<string, string>("ReverseTextExecutor")
{
    [MessageHandler]
    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(string.Concat(message.Reverse()));
    }
}
