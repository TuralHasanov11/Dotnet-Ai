using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkQuickStart.Middleware;

public static class ExceptionHandlingMiddleware
{
    public static async Task<AgentResponse> Handle(
        IEnumerable<ChatMessage> messages,
        AgentSession? session,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine("[ExceptionHandler] Executing agent run...");
            return await innerAgent.RunAsync(messages, session, options, cancellationToken);
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"[ExceptionHandler] Caught timeout: {ex.Message}");
            return new AgentResponse([new ChatMessage(ChatRole.Assistant,
                "Sorry, the request timed out. Please try again later.")]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExceptionHandler] Caught error: {ex.Message}");
            return new AgentResponse([new ChatMessage(ChatRole.Assistant,
                "An error occurred while processing your request.")]);
        }
    }
}