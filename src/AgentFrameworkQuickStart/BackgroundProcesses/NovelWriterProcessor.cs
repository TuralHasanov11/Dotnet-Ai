using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.AI;

namespace AgentFrameworkQuickStart.BackgroundProcesses;

public class NovelWriterProcessor([FromKeyedServices("NovelAgent")] AIAgent agent, ILogger<NovelWriterProcessor> logger) : BackgroundService
{
    private readonly AgentRunOptions AgentRunOptions = new()
    {
        AllowBackgroundResponses = true
    };

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("NovelWriterProcessor is starting.");
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var session = await agent.CreateSessionAsync(stoppingToken);
        var responseStream = agent.RunStreamingAsync(
            "Write a very long novel about otters in space.",
            session, AgentRunOptions,
            cancellationToken: stoppingToken);

        await foreach (var update in responseStream)
        {
            logger.LogInformation("NovelWriterProcessor received update: {Update}", update.Text);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("NovelWriterProcessor is stopping.");
        await base.StopAsync(cancellationToken);
    }
}