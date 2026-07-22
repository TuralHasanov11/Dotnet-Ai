using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;

namespace AgentFrameworkQuickStart.Workflows;

internal sealed partial class HandleSpamExecutor : Executor<SpamDetectionResult>
{
    public HandleSpamExecutor() : base("HandleSpamExecutor") { }

    [MessageHandler]
    public override async ValueTask HandleAsync(SpamDetectionResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (message.IsSpam)
        {
            await context.YieldOutputAsync($"Email marked as spam: {message.Reason}", cancellationToken: cancellationToken);
        }
        else
        {
            throw new ArgumentException("This executor should only handle spam messages.");
        }
    }
}