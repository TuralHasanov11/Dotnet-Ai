namespace AgentFrameworkQuickStart.Agents;

public sealed class AgentRuntimeOptions
{
    public const string SectionName = "AgentRuntime";

    public string Name { get; init; } = "Knowledge Assistant";

    public string ProviderMode { get; init; } = "provider-agnostic scaffold";

    public int DefaultRetrievalLimit { get; init; } = 3;
}

public sealed record KnowledgeDocumentDraft(
    string Title,
    string Content,
    string? Source,
    IReadOnlyList<string>? Tags);

public sealed record KnowledgeDocument(
    Guid Id,
    string Title,
    string Content,
    string? Source,
    IReadOnlyList<string> Tags,
    DateTimeOffset UploadedAt);

public sealed record KnowledgeMatch(
    KnowledgeDocument Document,
    int Score,
    string Excerpt);

public sealed record AgentQuestionRequest(
    string Question,
    int? MaxResults);

public sealed record AgentAnswerResponse(
    string AgentName,
    string ProviderMode,
    string Question,
    string Answer,
    int RetrievedCount,
    IReadOnlyList<KnowledgeMatch> Sources);

public sealed record AgentCapabilitiesResponse(
    string AgentName,
    string ProviderMode,
    int DefaultRetrievalLimit,
    IReadOnlyList<string> SupportedOperations);
