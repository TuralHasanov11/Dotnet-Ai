using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenAI;
using Scalar.AspNetCore;
using ServiceDefaults;
using ServiceDefaults.Identity;
using ServiceDefaults.OpenApi;
using OpenAI.Chat;
using AgentFrameworkQuickStart.Tools;
using AgentFrameworkQuickStart.Workflows;
using Microsoft.Agents.AI.Workflows;
using SharedKernel.Identity;
using AgentFrameworkQuickStart.Adapters;
using AgentFrameworkQuickStart.Skills;
using AgentFrameworkQuickStart.ContextProviders;
using AgentFrameworkQuickStart.Services;
using Microsoft.AspNetCore.Mvc;
using AgentFrameworkQuickStart.Middleware;

var builder = WebApplication.CreateBuilder(args);
const string KeycloakSecurityScheme = "Keycloak";

builder.AddServiceDefaults();

var openApiInfoOptions = builder.Configuration.GetSection("OpenApiInfo").Get<Dictionary<string, OpenApiInfo>>() ?? [];

foreach (var version in openApiInfoOptions.Keys)
{
    var versionedOpenApiInfo = builder.Configuration.GetSection($"OpenApiInfo:{version}").Get<OpenApiInfo>();

    if (versionedOpenApiInfo is not null)
    {
        builder.Services.AddOpenApi(version, options =>
        {
            options.AddApiVersionTransformer(versionedOpenApiInfo);
            options.AddDocumentTransformer<ApiSecuritySchemeTransformer>();
        });
    }
}

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, KeycloakSecurityScheme, options =>
    {
        var oidcConfig = builder.Configuration.GetSection("Keycloak");

        options.MetadataAddress = oidcConfig["Authority"] + "/.well-known/openid-configuration";
        options.Authority = oidcConfig["Authority"];
        options.Audience = oidcConfig["Audience"];

        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
        options.TokenValidationParameters.RoleClaimType = "roles";

        options.RequireHttpsMetadata = false;
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddHealthChecks();

// builder.Services.AddKeyedSingleton<IChatClient>("chat-model", (_, _) => new OllamaChatClient(
//     new Uri("http://host.docker.internal:11434"),
//     modelId: "qwen3.5:0.8b"));

// ### AI Clients

var SourceName = Assembly.GetExecutingAssembly().GetName().Name;
var ragChatHistoryProvider = new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions
{
    StorageInputRequestMessageFilter = messages => messages.Where(m => m.GetAgentRequestMessageSourceType() != AgentRequestMessageSourceType.AIContextProvider && m.GetAgentRequestMessageSourceType() != AgentRequestMessageSourceType.ChatHistory)
});

builder.Services.AddSingleton(ragChatHistoryProvider);
builder.Services.AddSingleton<SimpleServiceMemoryProvider>();
builder.Services.AddSingleton<ConversationStore>();

builder.Services.AddKeyedSingleton("chat-model-1", (_, _) =>
{
    var client = new OpenAIClient(builder.Configuration["OpenAIKey"]);
    var responsesClient = client.GetChatClient("gpt-4o-mini")
        .AsIChatClient()
        .AsBuilder()
        .UseOpenTelemetry(sourceName: SourceName, configure: (cfg) => cfg.EnableSensitiveData = false)    // Enable OpenTelemetry instrumentation with sensitive data
        .Use(getResponseFunc: LoggingChatMiddleware.Handle, getStreamingResponseFunc: null)
        .Build();
    return responsesClient;
});

// ### Agents
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


builder.AddAIAgent("Person", (sp, _) =>
{
    return sp.GetRequiredKeyedService<IChatClient>("chat-model-1")
        .AsAIAgent(
            name: "Person",
            instructions: "You are a helpful assistant that provides information about people.",
            tools: [AIFunctionFactory.Create(PersonTool.GetPersonInfo)]);
});

builder.AddAIAgent(
    "Pirate",
    instructions: "You are a pirate. Speak like a pirate",
    description: "An agent that speaks like a pirate",
    chatClientServiceKey: "chat-model-2");

builder.AddAIAgent("agent-1", instructions: "you are agent 1!");
builder.AddAIAgent("agent-2", instructions: "you are agent 2!");

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

// ### Workflows
builder.AddWorkflows();

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

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi()
        // .WithDocumentPerVersion() // Preview
        .AllowAnonymous();

    var keycloakOptions = app.Services.GetRequiredService<IOptions<KeycloakOptions>>().Value;

    app.MapScalarApiReference(
        options =>
        {
            var descriptions = app.DescribeApiVersions();

            for (var i = 0; i < descriptions.Count; i++)
            {
                var description = descriptions[i];
                var isDefault = i == descriptions.Count - 1;

                // isDefault is used to mark the default API version in Scalar.
                // This decides which version is selected by default when users visit the Scalar UI.
                options.AddDocument(description.GroupName, description.GroupName, isDefault: isDefault)
                    .AddPreferredSecuritySchemes(KeycloakSecurityScheme, "KeycloakSelf")
                    .AddHttpAuthentication(KeycloakSecurityScheme, flow =>
                    {
                        flow.Description = "Keycloak Authentication";
                        flow.Token = "";
                    })
                    .AddAuthorizationCodeFlow("KeycloakSelf", flow =>
                    {
                        flow.ClientId = keycloakOptions.ClientId;
                        flow.RedirectUri = keycloakOptions.RedirectUri;
                        flow.Pkce = Pkce.Sha256;
                    });
            }
        })
        .AllowAnonymous();
}

app.MapGet("/", () => Assembly.GetExecutingAssembly().GetName().Name)
    .AllowAnonymous()
    .ExcludeFromDescription();

app.MapGet("/pirate", async ([FromKeyedServices("Pirate")] AIAgent agent) =>
{
    return Results.Ok(await agent.RunAsync("Ahoy there! How are you doing?"));
}).AllowAnonymous();

app.MapGet("/weather", async (
    [FromKeyedServices("WeatherAgent")] AIAgent agent,
    ConversationStore conversationStore,
    [FromQuery] string? sessionId,
    [FromQuery] string? message) =>
{
    var session = await conversationStore.GetOrCreateSessionAsync(sessionId, async () => await agent.CreateSessionAsync());
    var userMessageText = string.IsNullOrWhiteSpace(message)
        ? "What is the weather like in Hannover?"
        : message.Trim();

    Microsoft.Extensions.AI.ChatMessage userMessage = new(ChatRole.User, [
        new TextContent(userMessageText)
    ]);

    var response = await agent.RunAsync(userMessage, session);
    return Results.Ok(new
    {
        sessionId = ConversationStore.NormalizeSessionId(sessionId),
        response.Text
    });
}).AllowAnonymous();

app.MapGet("/person", async ([FromKeyedServices("Person")] AIAgent agent) =>
{
    Microsoft.Extensions.AI.ChatMessage message = new(ChatRole.User, [
        new TextContent("Please provide information about John Smith, who is a 35-year-old software engineer.")
    ]);

    AgentRunOptions runOptions = new()
    {
        ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema<UserInfo>()
    };

    var response = await agent.RunAsync<UserInfo>(message, options: runOptions);
    return Results.Ok(response.Result);
})
    .AllowAnonymous()
    .Produces<UserInfo>();

app.MapGet("/rag", async (
    [FromKeyedServices("RAG")] AIAgent agent,
    ConversationStore conversationStore,
    [FromQuery] string? sessionId,
    [FromQuery] string? message) =>
{
    var session = await conversationStore.GetOrCreateSessionAsync(sessionId, async () => await agent.CreateSessionAsync());
    var userMessageText = string.IsNullOrWhiteSpace(message)
        ? "What is the return policy for Contoso Outdoors?"
        : message.Trim();

    Microsoft.Extensions.AI.ChatMessage userMessage = new(ChatRole.User, [
        new TextContent(userMessageText)
    ]);

    var response = await agent.RunAsync(userMessage, session);
    return Results.Ok(new
    {
        sessionId = ConversationStore.NormalizeSessionId(sessionId),
        response.Text
    });
}).AllowAnonymous();

app.MapGet("/rag/history", async (
    ConversationStore conversationStore,
    InMemoryChatHistoryProvider chatHistoryProvider,
    [FromKeyedServices("RAG")] AIAgent agent,
    [FromQuery] string? sessionId) =>
{
    var session = await conversationStore.GetOrCreateSessionAsync(sessionId, async () => await agent.CreateSessionAsync());
    var history = chatHistoryProvider.GetMessages(session)
        .Select(message => new RagHistoryMessage(
            message.Role.ToString(),
            string.IsNullOrWhiteSpace(message.Text) ? null : message.Text,
            message.AuthorName));

    return Results.Ok(new
    {
        sessionId = ConversationStore.NormalizeSessionId(sessionId),
        messages = history
    });
}).AllowAnonymous();

app.MapGet("/text-workflow", async ([FromKeyedServices("UppercaseTextExecutor")] Workflow workflow) =>
{
    await using var run = await InProcessExecution.RunAsync(workflow, "Hello, World!");

    return Results.Ok(run.NewEvents.Where(e => e is ExecutorCompletedEvent).Select(e => ((ExecutorCompletedEvent)e).Data));
}).AllowAnonymous();

app.MapPost("/unit-converter", async ([FromKeyedServices("UnitConverterAgent")] AIAgent agent, [FromQuery] double value, [FromQuery] string fromUnit, [FromQuery] string toUnit) =>
{
    Microsoft.Extensions.AI.ChatMessage message = new(ChatRole.User, [
        new TextContent($"Please convert {value} {fromUnit} to {toUnit}.")
    ]);

    var response = await agent.RunAsync(message);
    return Results.Ok(response.Text);
}).AllowAnonymous();

app.MapPost("/spam-workflow", async ([FromKeyedServices("SpamDetectionExecutor")] Workflow workflow) => {
    const string emailContent = "Congratulations! You've won $1,000,000! Click here to claim your prize now!";
    StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, emailContent));

    await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

    await foreach (var evt in run.WatchStreamAsync().ConfigureAwait(false))
    {
        if (evt is WorkflowOutputEvent outputEvent)
        {
            Console.WriteLine($"{outputEvent}");
        }

        if (evt is ExecutorCompletedEvent executorComplete)
        {
            Console.WriteLine($"{executorComplete.ExecutorId}: {executorComplete.Data}");
        }

        if (evt is SuperStepCompletedEvent superStepCompletedEvt)
        {
            // Access the checkpoint
            CheckpointInfo? checkpoint = superStepCompletedEvt.CompletionInfo?.Checkpoint;
            Console.WriteLine($"Checkpoint: {checkpoint?.CheckpointId}");
        }
    }

    var checkpoints = run.Checkpoints;
    Console.WriteLine($"Checkpoints: {checkpoints.Count}");

    return Results.Ok();
});

await app.RunAsync();