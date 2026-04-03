using AgentFrameworkQuickStart.Agents;

namespace AgentFrameworkQuickStart.Tests;

public class KnowledgeBaseTests
{
    [Fact]
    public void SearchRanksTheMostRelevantDocumentFirst()
    {
        var knowledgeBase = new InMemoryKnowledgeBase();

        knowledgeBase.Add(new KnowledgeDocumentDraft(
            "Release guide",
            "Use the release guide when you need deployment steps, validation, and rollback guidance.",
            null,
            ["release", "operations"]));

        knowledgeBase.Add(new KnowledgeDocumentDraft(
            "Agent onboarding",
            "Uploaded knowledge should explain the available agents, core tasks, and retrieval expectations.",
            null,
            ["agent", "knowledge"]));

        var matches = knowledgeBase.Search("How do I onboard an agent and use uploaded knowledge?", 2);

        Assert.Equal(2, matches.Count);
        Assert.Equal("Agent onboarding", matches[0].Document.Title);
    }

    [Fact]
    public void SearchReturnsNoMatchesWhenTheQueryIsEmpty()
    {
        var knowledgeBase = new InMemoryKnowledgeBase();

        knowledgeBase.Add(new KnowledgeDocumentDraft("Agent guide", "Content", null, []));

        var matches = knowledgeBase.Search(string.Empty, 3);

        Assert.Empty(matches);
    }
}
