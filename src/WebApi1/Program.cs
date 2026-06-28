using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using JasperFx.Resources;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using ServiceDefaults;
using ServiceDefaults.Identity;
using ServiceDefaults.OpenApi;
using WebApi1.Data;
using WebApi1.Features.Onboarding;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

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

// ### Saga
var dbConnectionString = builder.Configuration.GetConnectionString("Database");
builder.Host.UseWolverine(options =>
{
    // You'll need to independently tell Wolverine where and how to 
    // store messages as part of the transactional inbox/outbox
    options.PersistMessagesWithPostgresql(dbConnectionString!);

    options.Durability.EnableInboxPartitioning = true;
    
    // Adding EF Core transactional middleware, saga support,
    // and EF Core support for Wolverine storage operations
    options.UseEntityFrameworkCoreTransactions();

    options.Policies.DisableConventionalLocalRouting();
    options.Policies.AutoApplyTransactions();

    options.Policies.UseDurableOutboxOnAllSendingEndpoints();
    options.Policies.UseDurableInboxOnAllListeners();

    if(builder.Environment.IsDevelopment())
    {
        options.Durability.Mode = DurabilityMode.Solo;
    }
});

builder.Host.UseResourceSetupOnStartup();

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

// generate sending welcome email endpoint
app.MapPost("users/welcome-email", async (Guid code, IMessageBus bus) =>
{
    await bus.PublishAsync(new UserRegistered
    {
        UserId = code,
        Email = "test@test.com",
        FirstName = "John",
        LastName = "Doe"
    });

    return Results.Accepted();
});

app.MapPost("outbox-example", async (ApplicationDbContext dbContext, IDbContextOutbox outbox) =>
{
    outbox.Enroll(dbContext); 

    await outbox.PublishAsync(new UserRegistered
    {
        UserId = Guid.NewGuid(),
        Email = "test@test.com",
        FirstName = "John",
        LastName = "Doe"
    });

    await outbox.SaveChangesAndFlushMessagesAsync();

    return Results.Accepted();
});

await app.RunAsync();
