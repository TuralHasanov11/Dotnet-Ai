using System.Net.Http.Json;
using AgentFrameworkQuickStart.Agents;

namespace AgentFrameworkQuickStart.Tests;

public class AgentEndpointTests : IClassFixture<AgentFrameworkQuickStartFactory>
{
    private readonly AgentFrameworkQuickStartFactory _factory;

    public AgentEndpointTests(AgentFrameworkQuickStartFactory factory)
    {
        _factory = factory;
        _factory.KnowledgeBase.Clear();
    }

    [Fact]
    public async Task CanUploadKnowledgeAndRetrieveAGroundedAnswer()
    {
        var client = _factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/agents/knowledge/documents", new KnowledgeDocumentDraft(
            "Agent onboarding",
            "Uploaded knowledge should include clear product policies, definitions, and operating guidance.",
            "internal-docs",
            ["onboarding", "policy"]), TestContext.Current.CancellationToken);

        createResponse.EnsureSuccessStatusCode();

        var answerResponse = await client.PostAsJsonAsync("/api/agents/answer", new AgentQuestionRequest(
            "What should uploaded knowledge include?",
            3), TestContext.Current.CancellationToken);

        answerResponse.EnsureSuccessStatusCode();

        var answer = await answerResponse.Content.ReadFromJsonAsync<AgentAnswerResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(answer);
        Assert.Equal("Knowledge Assistant", answer!.AgentName);
        Assert.Equal("provider-agnostic scaffold", answer.ProviderMode);
        Assert.Equal(1, answer.RetrievedCount);
        Assert.Contains("Uploaded knowledge should include clear product policies", answer.Answer, StringComparison.Ordinal);
        Assert.Single(answer.Sources);
        Assert.Equal("Agent onboarding", answer.Sources[0].Document.Title);
    }

    [Fact]
    public async Task CanReadAgentCapabilities()
    {
        var client = _factory.CreateClient();

        var capabilities = await client.GetFromJsonAsync<AgentCapabilitiesResponse>("/api/agents/capabilities", TestContext.Current.CancellationToken);

        Assert.NotNull(capabilities);
        Assert.Equal("Knowledge Assistant", capabilities!.AgentName);
        Assert.Contains("Upload knowledge documents", capabilities.SupportedOperations);
    }
}
