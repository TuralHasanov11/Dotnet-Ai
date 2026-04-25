using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using SharedKernel.Identity;
using SharedKernel.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddHealthChecks();

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddOptions<OpenApiInfo>()
    .BindConfiguration("OpenApiInfo")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHealthChecks();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<InfoTransformer>();
    options.AddDocumentTransformer<ApiSecuritySchemeTransformer>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();

    var keycloakOptions = app.Services.GetRequiredService<IOptions<KeycloakOptions>>().Value;

    app.MapScalarApiReference(
        options => options
            .AddPreferredSecuritySchemes("Keycloak")
            .AddAuthorizationCodeFlow("Keycloak", flow =>
            {
                flow.ClientId = keycloakOptions.ClientId;
                flow.ClientSecret = keycloakOptions.ClientSecret;
                flow.RedirectUri = keycloakOptions.RedirectUri;
                flow.Pkce = Pkce.Sha256;
            }))
        .AllowAnonymous();
}

app.UseHttpsRedirection();

app.MapGet("/", () => Assembly.GetExecutingAssembly().GetName().Name);

app.MapHealthChecks("/health");

await app.RunAsync();