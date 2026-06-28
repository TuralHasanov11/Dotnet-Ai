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

builder.Services.AddKeyedSingleton("chat-model-1", (_, _) =>
{
    var client = new OpenAIClient(builder.Configuration["OpenAIKey"]);
    var responsesClient = client.GetChatClient("gpt-4o-mini")
        .AsIChatClient()
        .AsBuilder()
        .UseOpenTelemetry(sourceName: SourceName, configure: (cfg) => cfg.EnableSensitiveData = false)    // Enable OpenTelemetry instrumentation with sensitive data
        // .Use(DurationChatClientMiddleware)
        .Build();
    return responsesClient;
});

// ### Agents
builder.AddAIAgent("Hello", (sp, _) =>
{
    return sp.GetRequiredKeyedService<IChatClient>("chat-model-1")
        .AsAIAgent(
            name: "Hello",
            instructions: "You are a helpful assistant that greets people.");
});

builder.AddAIAgent("WeatherAgent", (sp, _) =>
{
    return sp.GetRequiredKeyedService<IChatClient>("chat-model-1")
        .AsAIAgent(
            name: "WeatherAgent",
            instructions: "You are a helpful weather assistant that provides weather information.",
            tools: [AIFunctionFactory.Create(WeatherTool.GetWeather, name: "get_weather")]);
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
    return sp.GetRequiredKeyedService<IChatClient>("chat-model-1")
        .AsAIAgent(new ChatClientAgentOptions
        {
            Name = "RAG",
            ChatOptions = new() { Instructions = "You are a helpful support specialist. Answer questions using the provided context and cite the source document when available." },
            AIContextProviders = [
                new TextSearchProvider(SearchAdapter.Adapter, new TextSearchProviderOptions()
                {
                    SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
                })],
            // Since we are using ChatCompletion which stores chat history locally, we can also add a message filter
            // that removes messages produced by the TextSearchProvider before they are added to the chat history, so that
            // we don't bloat chat history with all the search result messages.
            // By default the chat history provider will store all messages, except for those that came from chat history in the first place.
            // We also want to maintain that exclusion here.
            ChatHistoryProvider = new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions
            {
                StorageInputRequestMessageFilter = messages => messages.Where(m => m.GetAgentRequestMessageSourceType() != AgentRequestMessageSourceType.AIContextProvider && m.GetAgentRequestMessageSourceType() != AgentRequestMessageSourceType.ChatHistory)
            })
        });
});

// ### Workflows
builder.AddWorkflow("my-workflow", (sp, key) =>
{
    var agent1 = sp.GetRequiredKeyedService<AIAgent>("agent-1");
    var agent2 = sp.GetRequiredKeyedService<AIAgent>("agent-2");
    return AgentWorkflowBuilder.BuildSequential(key, [agent1, agent2]);
}).AddAsAIAgent(); // Now the workflow can be used as an agent

builder.AddWorkflow("TextWorkflow", (sp, key) =>
{
    var reverse = new TextWorkflow.ReverseTextExecutor();
    var uppercase = TextWorkflow.UppercaseTextExecutor;
    WorkflowBuilder workflowBuilder = new(uppercase);
    workflowBuilder.AddEdge(uppercase, reverse).WithOutputFrom(reverse);
    var workflow = workflowBuilder.WithName(key).Build();

    return workflow;
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

app.MapGet("/hello", async ([FromKeyedServices("Hello")] AIAgent agent) =>
{
    AgentResponse response = await agent.RunAsync("Hi there!");
    return Results.Ok(response);
}).AllowAnonymous();

app.MapGet("/pirate", async ([FromKeyedServices("Pirate")] AIAgent agent) =>
{
    return Results.Ok(await agent.RunAsync("Ahoy there! How are you doing?"));
}).AllowAnonymous();

app.MapGet("/weather", async ([FromKeyedServices("WeatherAgent")] AIAgent agent) =>
{
    Microsoft.Extensions.AI.ChatMessage message = new(ChatRole.User, [
        new TextContent("What is the weather like in Hannover?")
    ]);

    var response = await agent.RunAsync(message);
    return Results.Ok(response.Text);
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

// generate an endpoint for rag agent
app.MapGet("/rag", async ([FromKeyedServices("RAG")] AIAgent agent) =>
{
    Microsoft.Extensions.AI.ChatMessage message = new(ChatRole.User, [
        new TextContent("What is the return policy for Contoso Outdoors?")
    ]);

    var response = await agent.RunAsync(message);
    return Results.Ok(response.Text);
}).AllowAnonymous();


app.MapGet("/text-workflow", async ([FromKeyedServices("TextWorkflow")] Workflow workflow) =>
{
    await using var run = await InProcessExecution.RunAsync(workflow, "Hello, World!");

    return Results.Ok(run.NewEvents.Where(e => e is ExecutorCompletedEvent).Select(e => ((ExecutorCompletedEvent)e).Data));
}).AllowAnonymous();

await app.RunAsync();