using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection(KeycloakOptions.SectionName));

builder.Services.AddOpenApi();

builder.Services.AddHealthChecks();

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var oidcConfig = builder.Configuration.GetSection("Keycloak");

        options.MetadataAddress = oidcConfig["Authority"] + "/.well-known/openid-configuration";
        options.Authority = oidcConfig["Authority"];
        options.Audience = oidcConfig["Audience"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = oidcConfig["Audience"],
            ValidIssuer = oidcConfig["Authority"]
        };

        options.MapInboundClaims = false;
    });

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
            .AddPreferredSecuritySchemes("Keycloak", "KeycloakSelf")
            .AddHttpAuthentication("Keycloak", flow =>
            {
                flow.Description = "Keycloak Authentication";
                flow.Token = "";
            })
            .AddAuthorizationCodeFlow("KeycloakSelf", flow =>
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