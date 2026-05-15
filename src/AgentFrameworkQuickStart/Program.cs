using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using ServiceDefaults;
using ServiceDefaults.Identity;
using ServiceDefaults.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new HeaderApiVersionReader("X-API-Version");
    options.ReportApiVersions = true;
});

var openApiInfoOptions = builder.Configuration.GetSection("OpenApiInfo").Get<Dictionary<string, OpenApiInfo>>();

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
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, "Keycloak", options =>
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

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();

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
                    });
            }
        })
        .AllowAnonymous();
}

app.MapGet("/", () => Assembly.GetExecutingAssembly().GetName().Name);


await app.RunAsync();