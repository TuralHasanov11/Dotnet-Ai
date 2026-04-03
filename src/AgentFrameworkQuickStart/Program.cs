using System.Reflection;
using AgentFrameworkQuickStart.Agents;

namespace AgentFrameworkQuickStart;

#pragma warning disable CA1052
public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();

        builder.Services.Configure<AgentRuntimeOptions>(builder.Configuration.GetSection(AgentRuntimeOptions.SectionName));
        builder.Services.AddSingleton<IKnowledgeBase, InMemoryKnowledgeBase>();
        builder.Services.AddSingleton<IAgentOrchestrator, KnowledgeGroundedAgentOrchestrator>();
        builder.Services.AddHealthChecks();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        app.MapGet("/", () => Assembly.GetExecutingAssembly().GetName().Name);

        app.MapAgentEndpoints();

        app.MapHealthChecks("/health");

        await app.RunAsync();
    }
}
#pragma warning restore CA1052