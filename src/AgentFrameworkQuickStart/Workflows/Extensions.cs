using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;

namespace AgentFrameworkQuickStart.Workflows;

public static class Extensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddWorkflows()
        {
            builder.AddWorkflow("my-workflow", (sp, key) =>
            {
                var agent1 = sp.GetRequiredKeyedService<AIAgent>("agent-1");
                var agent2 = sp.GetRequiredKeyedService<AIAgent>("agent-2");
                return AgentWorkflowBuilder.BuildSequential(key, [agent1, agent2]);
            }).AddAsAIAgent(); // Now the workflow can be used as an agent

            builder.AddWorkflow("UppercaseTextExecutor", (sp, key) =>
            {
                var reverse = new ReverseTextExecutor();
                var uppercase = UppercaseTextExecutor.GetExecutor;
                WorkflowBuilder workflowBuilder = new(uppercase);
                workflowBuilder.AddEdge(uppercase, reverse).WithOutputFrom(reverse);
                var workflow = workflowBuilder.WithName(key).Build();

                return workflow;
            });

            builder.AddWorkflow("SpamDetectionExecutor", (sp, key) =>
            {
                var spamAgent = sp.GetRequiredKeyedService<AIAgent>("SpamDetectionAgent");
                var emailAssistantAgent = sp.GetRequiredKeyedService<AIAgent>("EmailAssistantAgent");

                var spamDetectionExecutor = new SpamDetectionExecutor(spamAgent);
                var handleSpamExecutor = new HandleSpamExecutor();
                var sendEmailExecutor = new SendEmailExecutor();
                var emailAssistantExecutor = new EmailAssistantExecutor(emailAssistantAgent);

                var workflow = new WorkflowBuilder(spamAgent)
                    .AddEdge(spamDetectionExecutor, emailAssistantExecutor, SpamEmailCondition.GetCondition(false))
                    .AddEdge(emailAssistantExecutor, sendEmailExecutor)
                    .AddEdge(spamDetectionExecutor, handleSpamExecutor, SpamEmailCondition.GetCondition(true))
                    .AddEdge(handleSpamExecutor, sendEmailExecutor)
                    .WithName(key)
                    .Build();    

                return workflow;
            });

            return builder;
        }
    }
}