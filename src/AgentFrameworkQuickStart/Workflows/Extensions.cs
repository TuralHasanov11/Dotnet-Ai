using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentFrameworkQuickStart.Adapters;
using AgentFrameworkQuickStart.ContextProviders;
using AgentFrameworkQuickStart.Middleware;
using AgentFrameworkQuickStart.Skills;
using AgentFrameworkQuickStart.Tools;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.A2A;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

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
                var workflow = new WorkflowBuilder(uppercase)
                    .AddEdge(uppercase, reverse)
                    .WithOutputFrom(reverse)
                    .WithName(key)
                    .Build();

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

        public WebApplicationBuilder AddAgents()
        {
            builder.AddAIAgent("WeatherAgent", (sp, _) =>
            {
                return sp.GetRequiredKeyedService<IChatClient>("chat-model-1")
                    .AsAIAgent(
                        name: "WeatherAgent",
                        instructions: "You are a helpful weather assistant that provides weather information.",
                        tools: [AIFunctionFactory.Create(WeatherTool.GetWeather, name: "get_weather")])
                    .AsBuilder()
                    .Use(
                        runFunc: MessageCounterAgentRunMiddleware.Handle,
                        runStreamingFunc: MessageCounterAgentRunMiddleware.HandleStreaming)
                    .Use(
                        runFunc: ExceptionHandlingMiddleware.Handle,
                        runStreamingFunc: null
                    )
                    .Build();
            }).WithInMemorySessionStore();

            // Register the A2A server for the "WeatherAgent" agent.
            builder.AddA2AServer("WeatherAgent", options =>
            {
#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                options.AgentRunMode = AgentRunMode.DisallowBackground;
#pragma warning restore MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            });


            builder.AddAIAgent(
                    name: "Person",
                    instructions: "You are a helpful assistant that provides information about people.",
                    chatClientServiceKey: "chat-model-1")
                .WithAITool(AIFunctionFactory.Create(PersonTool.GetPersonInfo));



            builder.AddAIAgent("RAG", (sp, _) =>
            {
                var simpleServiceMemory = sp.GetRequiredService<SimpleServiceMemoryProvider>();
                var chatHistoryProvider = sp.GetRequiredService<InMemoryChatHistoryProvider>();

                return sp.GetRequiredKeyedService<IChatClient>("chat-model-1")
                    .AsAIAgent(new ChatClientAgentOptions
                    {
                        Name = "RAG",
                        ChatOptions = new() { Instructions = "You are a helpful support specialist. Answer questions using the provided context and cite the source document when available." },
                        AIContextProviders = [
                            simpleServiceMemory,
                            new TextSearchProvider(SearchAdapter.Adapter, new TextSearchProviderOptions()
                            {
                                SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
                            })],
                        ChatHistoryProvider = chatHistoryProvider
                    });
            });


            // ### Agent Skills
            var unitConverterSkill = new UnitConverterSkill();
#pragma warning disable MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var skillsProvider = new AgentSkillsProviderBuilder()
                .UseSkill(unitConverterSkill)                                  // AgentClassSkill
                .Build();
#pragma warning restore MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            builder.AddAIAgent("UnitConverterAgent", (sp, _) =>
            {
                return sp.GetRequiredKeyedService<IChatClient>("chat-model-1")
                    .AsAIAgent(new ChatClientAgentOptions
                    {
                        Name = "UnitConverterAgent",
                        ChatOptions = new() { Instructions = "You are a helpful assistant that converts between common units using a multiplication factor. Use when asked to convert miles, kilometers, pounds, or kilograms." },
                        AIContextProviders = [skillsProvider],
                    });
            });

            return builder;
        }
    }
}