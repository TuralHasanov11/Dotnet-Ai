using System.Text.RegularExpressions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentFrameworkQuickStart.ContextProviders;

internal sealed class SimpleServiceMemoryProvider : MessageAIContextProvider
{
    private const string StateKey = nameof(SimpleServiceMemoryProvider);
    private static readonly ProviderSessionState<SimpleServiceMemoryState> SessionState =
        new(_ => new SimpleServiceMemoryState(), StateKey);

    protected override ValueTask<IEnumerable<ChatMessage>> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        if (context.Session is null)
        {
            return ValueTask.FromResult<IEnumerable<ChatMessage>>([]);
        }

        var state = SessionState.GetOrInitializeState(context.Session);
        if (state.Notes.Count == 0)
        {
            return ValueTask.FromResult<IEnumerable<ChatMessage>>([]);
        }

        var memoryText = string.Join(Environment.NewLine, state.Notes.Select(note => $"- {note}"));
        IEnumerable<ChatMessage> messages = [
            new ChatMessage(ChatRole.System, [
                new TextContent($"Service memory for this session:{Environment.NewLine}{memoryText}")
            ])
        ];

        return ValueTask.FromResult(messages);
    }

    protected override ValueTask InvokedCoreAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        if (context.Session is null)
        {
            return ValueTask.CompletedTask;
        }

        var userMessage = context.RequestMessages
            .Where(message => message.Role == ChatRole.User)
            .Select(message => message.Text)
            .LastOrDefault(text => !string.IsNullOrWhiteSpace(text));

        if (string.IsNullOrWhiteSpace(userMessage))
        {
            return ValueTask.CompletedTask;
        }

        if (!TryCreateMemoryNote(userMessage, out var note))
        {
            return ValueTask.CompletedTask;
        }

        var state = SessionState.GetOrInitializeState(context.Session);

        if (!state.Notes.Any(existing => string.Equals(existing, note, StringComparison.OrdinalIgnoreCase)))
        {
            state.Notes.Add(note);

            if (state.Notes.Count > 10)
            {
                state.Notes.RemoveRange(0, state.Notes.Count - 10);
            }

            SessionState.SaveState(context.Session, state);
        }

        return ValueTask.CompletedTask;
    }

    private static bool TryCreateMemoryNote(string userMessage, out string note)
    {
        var normalized = userMessage.Trim();

        var explicitRemember = Regex.Match(normalized, @"(?i)^(?:please\s+)?remember(?:\s+that)?\s+(?<value>.+)$");
        if (explicitRemember.Success)
        {
            note = $"User asked to remember: {explicitRemember.Groups["value"].Value.TrimEnd('.', '!', '?')}";
            return true;
        }

        var nameMatch = Regex.Match(normalized, @"(?i)^my name is\s+(?<value>.+)$");
        if (nameMatch.Success)
        {
            note = $"User's name is {nameMatch.Groups["value"].Value.TrimEnd('.', '!', '?')}";
            return true;
        }

        var preferenceMatch = Regex.Match(normalized, @"(?i)^(?:i prefer|my preference is)\s+(?<value>.+)$");
        if (preferenceMatch.Success)
        {
            note = $"User prefers {preferenceMatch.Groups["value"].Value.TrimEnd('.', '!', '?')}";
            return true;
        }

        note = string.Empty;
        return false;
    }

    private sealed class SimpleServiceMemoryState
    {
        public List<string> Notes { get; set; } = [];
    }
}