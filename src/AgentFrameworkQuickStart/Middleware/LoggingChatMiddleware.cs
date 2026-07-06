using System.Diagnostics;
using Microsoft.Extensions.AI;

namespace AgentFrameworkQuickStart.Middleware;

public static class LoggingChatMiddleware
{
    public static async Task<ChatResponse> Handle(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options,
        IChatClient innerChatClient,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"[ChatLog] Sending {messages.Count()} messages to model...");
        foreach (var msg in messages)
        {
            Console.WriteLine($"[ChatLog]   {msg.Role}: {msg.Text?[..Math.Min(msg.Text.Length, 80)]}");
        }

        var timestamp = Stopwatch.GetTimestamp();

        var response = await innerChatClient.GetResponseAsync(messages, options, cancellationToken);

        var elapsed = Stopwatch.GetElapsedTime(timestamp);
        Console.WriteLine($"Elapsed time: {elapsed} ticks");

        Console.WriteLine($"[ChatLog] Received {response.Messages.Count} response messages.");

        return response;
    }
}