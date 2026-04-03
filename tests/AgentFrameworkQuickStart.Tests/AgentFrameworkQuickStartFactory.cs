using AgentFrameworkQuickStart.Agents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AgentFrameworkQuickStart.Tests;

public sealed class AgentFrameworkQuickStartFactory : WebApplicationFactory<AgentFrameworkQuickStart.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    public InMemoryKnowledgeBase KnowledgeBase => Services.GetRequiredService<IKnowledgeBase>() as InMemoryKnowledgeBase
        ?? throw new InvalidOperationException("Expected the in-memory knowledge base to be registered.");
}
