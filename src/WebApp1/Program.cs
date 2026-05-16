using System.IdentityModel.Tokens.Jwt;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using ServiceDefaults;
using ServiceDefaults.Identity;
using ServiceDefaults.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(config => config.ValidateOnBuild = true);
builder.WebHost.UseKestrel(options => options.AddServerHeader = false);

// Add service defaults & Aspire client integrations.
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
            options.AddDocumentTransformer<WebSecuritySchemeTransformer>();
        });
    }
}

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHttpClient("agent-framework-quick-start", (serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["AgentFrameworkQuickStart:BaseUrl"];

    if (!string.IsNullOrWhiteSpace(baseUrl) && Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
    {
        client.BaseAddress = uri;
    }
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, "Keycloak", options =>
    {
        var oidcConfig = builder.Configuration.GetSection("Keycloak");

        options.Authority = oidcConfig["Authority"];
        options.ClientId = oidcConfig["ClientId"];
        options.ClientSecret = oidcConfig["ClientSecret"];
        options.CallbackPath = new PathString("/signin-oidc");
        options.SignedOutCallbackPath = new PathString("/signout-oidc");

        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.ResponseType = OpenIdConnectResponseType.Code;

        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
        options.TokenValidationParameters.RoleClaimType = "roles";

        options.RequireHttpsMetadata = false;

        options.Scope.Add("access_as_user");

        options.Events = new OpenIdConnectEvents
        {
            // Add event handlers
            OnTicketReceived = async context => { },
            OnRedirectToIdentityProvider = async context => { },
            OnPushAuthorization = async context => { },
            OnMessageReceived = async context => { },
            OnAccessDenied = async context => { },
            OnAuthenticationFailed = async context => { },
            OnRemoteFailure = async context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("OnRemoteFailure from identity provider. Scheme: {Scheme: }", context.Scheme.Name);

                if (context.Failure != null)
                {
                    context.HandleResponse();
                    context.Response.Redirect($"/Error?remoteError={context.Failure.Message}");
                }

                await Task.CompletedTask;
            },
            // ...
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.FallbackPolicy = options.DefaultPolicy;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

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
                    .AddPreferredSecuritySchemes("Keycloak")
                    .AddAuthorizationCodeFlow("Keycloak", flow =>
                    {
                        flow.ClientId = keycloakOptions.ClientId;
                        flow.RedirectUri = keycloakOptions.RedirectUri;
                        flow.Pkce = Pkce.Sha256;
                    });
            }
        })
        .AllowAnonymous();
}

app.MapGet("/ping-agent", async (IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
{
    var client = httpClientFactory.CreateClient("agent-framework-quick-start");

    if (client.BaseAddress is null)
    {
        return Results.Problem(
            title: "Agent base URL is not configured",
            detail: "Set AgentFrameworkQuickStart__BaseUrl in configuration.",
            statusCode: StatusCodes.Status500InternalServerError);
    }

    try
    {
        using var response = await client.GetAsync("/", cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        return Results.Json(new
        {
            target = client.BaseAddress.ToString(),
            statusCode = (int)response.StatusCode,
            success = response.IsSuccessStatusCode,
            response = payload
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Failed to reach agent-framework-quick-start",
            detail: ex.Message,
            statusCode: StatusCodes.Status502BadGateway);
    }
});

await app.RunAsync();