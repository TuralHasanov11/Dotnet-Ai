using Microsoft.Extensions.Options;

namespace AgentFrameworkQuickStart.Agents;

public interface IAgentOrchestrator
{
    AgentCapabilitiesResponse GetCapabilities();

    AgentAnswerResponse Answer(AgentQuestionRequest request);
}

public sealed class KnowledgeGroundedAgentOrchestrator : IAgentOrchestrator
{
    private readonly IKnowledgeBase _knowledgeBase;
    private readonly AgentRuntimeOptions _options;

    public KnowledgeGroundedAgentOrchestrator(
        IKnowledgeBase knowledgeBase,
        IOptions<AgentRuntimeOptions> options)
    {
        _knowledgeBase = knowledgeBase;
        _options = options.Value;
    }

    public AgentCapabilitiesResponse GetCapabilities()
    {
        return new AgentCapabilitiesResponse(
            _options.Name,
            _options.ProviderMode,
            _options.DefaultRetrievalLimit,
            [
                "Upload knowledge documents",
                "Retrieve grounded answers",
                "Inspect sources",
                "List stored knowledge"
            ]);
    }

    public AgentAnswerResponse Answer(AgentQuestionRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Question);

        var limit = request.MaxResults.GetValueOrDefault(_options.DefaultRetrievalLimit);
        var matches = _knowledgeBase.Search(request.Question.Trim(), limit);

        if (matches.Count == 0)
        {
            return new AgentAnswerResponse(
                _options.Name,
                _options.ProviderMode,
                request.Question.Trim(),
                "I could not find any matching documents in the uploaded knowledge base yet. Upload a document that covers the topic or ask a more specific question.",
                0,
                []);
        }

        var answer = BuildAnswer(request.Question.Trim(), matches);

        return new AgentAnswerResponse(
            _options.Name,
            _options.ProviderMode,
            request.Question.Trim(),
            answer,
            matches.Count,
            matches);
    }

    private static string BuildAnswer(string question, IReadOnlyList<KnowledgeMatch> matches)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine($"I found {matches.Count} relevant document(s) for: {question}");
        builder.AppendLine();
        builder.AppendLine("Grounded summary:");

        foreach (var match in matches)
        {
            builder.AppendLine($"- {match.Document.Title}: {match.Excerpt}");
        }

        builder.AppendLine();
        builder.Append("This scaffold is ready for a model-backed response layer when you select a provider.");

        return builder.ToString();
    }
}
