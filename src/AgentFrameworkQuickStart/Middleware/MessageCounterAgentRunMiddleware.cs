using System.Runtime.CompilerServices;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkQuickStart.Middleware;

public static class MessageCounterAgentRunMiddleware
{
    public static async Task<AgentResponse> Handle(
        IEnumerable<ChatMessage> messages,
        AgentSession? session,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(messages.Count());
        var response = await innerAgent.RunAsync(messages, session, options, cancellationToken).ConfigureAwait(false);
        Console.WriteLine(response.Messages.Count);
        return response;
    }


    public static async IAsyncEnumerable<AgentResponseUpdate> HandleStreaming(
        IEnumerable<ChatMessage> messages,
        AgentSession? session,
        AgentRunOptions? options,
        AIAgent innerAgent,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Console.WriteLine(messages.Count());
        List<AgentResponseUpdate> updates = [];
        await foreach (var update in innerAgent.RunStreamingAsync(messages, session, options, cancellationToken))
        {
            updates.Add(update);
            yield return update;
        }

        Console.WriteLine(updates.ToAgentResponse().Messages.Count);
    }
}