using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgentFrameworkQuickStart.Workflows;


public sealed partial class SpamDetectionExecutor : Executor<ChatMessage, SpamDetectionResult>
{
    private readonly AIAgent _spamAgent;

    public SpamDetectionExecutor(AIAgent spamAgent) : base("SpamDetectionExecutor")
    {
        _spamAgent = spamAgent;
    }

    [MessageHandler]
    public override async ValueTask<SpamDetectionResult> HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var email = new Email { Content = message.Text ?? string.Empty, Id = Guid.NewGuid().ToString("N") };


        await context.QueueStateUpdateAsync(email.Id, email, scopeName: "EmailState", cancellationToken: cancellationToken);

        var response = await _spamAgent.RunAsync<SpamDetectionResult>(message, cancellationToken: cancellationToken);

        var result = response.Result;

        result.EmailId = email.Id;
        return result;
    }
}
