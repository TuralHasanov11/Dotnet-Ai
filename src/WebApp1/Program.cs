using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection(KeycloakOptions.SectionName));
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
.AddCookie()
.AddOpenIdConnect(options =>
{
    var oidcConfig = builder.Configuration.GetSection("Keycloak");

    options.Authority = oidcConfig["Authority"];
    options.ClientId = oidcConfig["ClientId"];
    options.ClientSecret = oidcConfig["ClientSecret"];
    options.CallbackPath = new PathString("/signin-oidc");

    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.ResponseType = OpenIdConnectResponseType.Code;

    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;

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

builder.Services.AddOptions<OpenApiInfo>()
    .BindConfiguration("OpenApiInfo")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHealthChecks();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<InfoTransformer>();
    options.AddDocumentTransformer<WebSecuritySchemeTransformer>();
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

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapHealthChecks("/health").AllowAnonymous();

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