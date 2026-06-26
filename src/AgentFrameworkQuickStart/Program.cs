using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using GitHub.Copilot;
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
using System.Text;

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


builder.Services.AddKeyedSingleton("chat-model-1", (_, _) => {
    var client = new OpenAIClient(builder.Configuration["OpenAIKey"]);
    var responsesClient = client.GetChatClient("gpt-4o-mini");
    return responsesClient;
});

builder.AddAIAgent("Hello", (sp, _) =>
{
    return sp.GetRequiredKeyedService<ChatClient>("chat-model-1")
        .AsAIAgent(
            name: "Hello",
            instructions: "You are a helpful assistant that greets people.");
});

builder.AddAIAgent("Weather", (sp, _) =>
{
    return sp.GetRequiredKeyedService<ChatClient>("chat-model-1")
        .AsAIAgent(
            name: "Weather",
            instructions: "You are a helpful weather assistant that provides weather information.",
            tools: [AIFunctionFactory.Create(WeatherTool.GetWeather)]);
});

builder.AddAIAgent(
    "Pirate",
    instructions: "You are a pirate. Speak like a pirate",
    description: "An agent that speaks like a pirate",
    chatClientServiceKey: "chat-model-2");

builder.Services.AddKeyedSingleton("TextWorkflow", (sp, _) => {
    var reverse = new TextWorkflow.ReverseTextExecutor();
    var uppercase = TextWorkflow.UppercaseTextExecutor;
    WorkflowBuilder workflowBuilder = new(uppercase);
    workflowBuilder.AddEdge(uppercase, reverse).WithOutputFrom(reverse);
    var workflow = workflowBuilder.Build();

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

app.MapGet("/weather", async ([FromKeyedServices("Weather")] AIAgent agent) =>
{
    AgentResponse response = await agent.RunAsync("What is the weather like in Hannover?");
    return Results.Ok(response);
}).AllowAnonymous();

app.MapGet("/text-workflow", async ([FromKeyedServices("TextWorkflow")] Workflow workflow) =>
{
    await using var run = await InProcessExecution.RunAsync(workflow, "Hello, World!");

    return Results.Ok(run.NewEvents.Where(e => e is ExecutorCompletedEvent).Select(e => ((ExecutorCompletedEvent)e).Data));
}).AllowAnonymous();

await app.RunAsync();