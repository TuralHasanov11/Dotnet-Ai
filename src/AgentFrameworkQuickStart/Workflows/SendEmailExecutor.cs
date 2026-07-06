using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.AI.Workflows;

namespace AgentFrameworkQuickStart.Workflows
{
    public class SendEmailExecutor : Executor<EmailResponse>
    {
        public SendEmailExecutor() : base("SendEmailExecutor")
        {
        }

        public override async ValueTask HandleAsync(EmailResponse message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.YieldOutputAsync($"Sending email with content: {message.Content}", cancellationToken: cancellationToken);
        }
    }
}