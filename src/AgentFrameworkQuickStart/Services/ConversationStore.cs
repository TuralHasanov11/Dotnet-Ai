using System.Collections.Concurrent;
using Microsoft.Agents.AI;

namespace AgentFrameworkQuickStart.Services;

internal sealed class ConversationStore
{
    public const string DefaultSessionId = "default";

    private readonly ConcurrentDictionary<string, Task<AgentSession>> _sessions = new(StringComparer.OrdinalIgnoreCase);

    public static string NormalizeSessionId(string? sessionId)
        => string.IsNullOrWhiteSpace(sessionId) ? DefaultSessionId : sessionId.Trim();

    public async Task<AgentSession> GetOrCreateSessionAsync(string? sessionId, Func<Task<AgentSession>> createSession)
    {
        ArgumentNullException.ThrowIfNull(createSession);

        var key = NormalizeSessionId(sessionId);

        if (_sessions.TryGetValue(key, out var existingSession))
        {
            return await existingSession;
        }

        var createdSession = createSession();
        var storedSession = _sessions.GetOrAdd(key, createdSession);

        return await storedSession;
    }
}

internal sealed record RagHistoryMessage(string Role, string? Text, string? AuthorName);