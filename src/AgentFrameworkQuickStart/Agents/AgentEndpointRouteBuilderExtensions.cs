using Microsoft.Extensions.Options;

namespace AgentFrameworkQuickStart.Agents;

public static class AgentEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapAgentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/agents")
            .WithTags("AI Agents");

        group.MapGet("/capabilities", (IAgentOrchestrator orchestrator) =>
        {
            return Results.Ok(orchestrator.GetCapabilities());
        });

        group.MapGet("/knowledge/documents", (IKnowledgeBase knowledgeBase) =>
        {
            return Results.Ok(knowledgeBase.GetAll());
        });

        group.MapGet("/knowledge/documents/{id:guid}", (Guid id, IKnowledgeBase knowledgeBase) =>
        {
            var document = knowledgeBase.GetById(id);

            return document is null ? Results.NotFound() : Results.Ok(document);
        });

        group.MapPost("/knowledge/documents", (KnowledgeDocumentDraft request, IKnowledgeBase knowledgeBase) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.Title)] = ["A title is required."],
                    [nameof(request.Content)] = ["Content is required."]
                });
            }

            var document = knowledgeBase.Add(request);

            return Results.Ok(document);
        });

        group.MapPost("/answer", (AgentQuestionRequest request, IAgentOrchestrator orchestrator) =>
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.Question)] = ["A question is required."]
                });
            }

            var response = orchestrator.Answer(request);

            return Results.Ok(response);
        });

        group.MapGet("/runtime", (IOptions<AgentRuntimeOptions> options) =>
        {
            var runtime = options.Value;

            return Results.Ok(new
            {
                runtime.Name,
                runtime.ProviderMode,
                runtime.DefaultRetrievalLimit
            });
        });

        return endpoints;
    }
}
