using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkQuickStart.Middleware;

public static class GuardrailMiddleware
{
    public static async Task<AgentResponse> Handle(
        IEnumerable<ChatMessage> messages,
        AgentSession? session,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken)
    {
        // Pre-execution check: block requests containing sensitive words
        var lastMessage = messages.LastOrDefault()?.Text?.ToLowerInvariant() ?? "";
        string[] blockedWords = ["password", "secret", "credentials"];

        foreach (var word in blockedWords)
        {
            if (lastMessage.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[Guardrail] Blocked request containing '{word}'.");
                return new AgentResponse([new ChatMessage(ChatRole.Assistant,
                    $"Sorry, I cannot process requests containing '{word}'.")]);
            }
        }

        // Input passed validation — proceed with agent execution
        var response = await innerAgent.RunAsync(messages, session, options, cancellationToken);

        // Post-execution check: validate the output
        var responseText = response.Messages.LastOrDefault()?.Text ?? "";
        if (responseText.Length > 5000)
        {
            Console.WriteLine("[Guardrail] Response too long, truncating.");
            return new AgentResponse([new ChatMessage(ChatRole.Assistant,
                string.Concat(responseText.AsSpan(0, 5000), "... [truncated]"))]);
        }

        return response;
    }
}