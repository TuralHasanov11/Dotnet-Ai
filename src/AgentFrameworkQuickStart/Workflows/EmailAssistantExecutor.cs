using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace AgentFrameworkQuickStart.Workflows
{
    public class EmailAssistantExecutor : Executor<SpamDetectionResult, EmailResponse>
    {
        private readonly AIAgent _emailAssistantAgent;

        public EmailAssistantExecutor(AIAgent emailAssistantAgent) : base("EmailAssistantExecutor")
        {
            this._emailAssistantAgent = emailAssistantAgent;
        }

        [MessageHandler]
        public override async ValueTask<EmailResponse> HandleAsync(
            SpamDetectionResult message,
            IWorkflowContext context,
            CancellationToken cancellationToken = default)
        {
            if (message.IsSpam)
            {
                throw new InvalidOperationException("Cannot handle spam emails.");
            }

            var email = await context.ReadStateAsync<Email>(message.EmailId, scopeName: "EmailState", cancellationToken: cancellationToken);

            if (email == null)
            {
                throw new InvalidOperationException($"Email with ID {message.EmailId} not found.");
            }

            var response = await _emailAssistantAgent.RunAsync<EmailResponse>(email.Content, cancellationToken: cancellationToken);
            return response.Result;
        }
    }
}