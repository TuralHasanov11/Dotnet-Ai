using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using ServiceDefaults.Identity;

namespace ServiceDefaults.OpenApi;

public class ApiSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        if (authenticationSchemes.Any(authScheme => authScheme.Name == JwtBearerDefaults.AuthenticationScheme))
        {
            var keycloakOptions = context.ApplicationServices.GetRequiredService<IOptions<KeycloakOptions>>();
            var authority = new Uri(keycloakOptions.Value.Authority, UriKind.Absolute);
            var authorizationUrl = new Uri(authority, "protocol/openid-connect/auth");
            var tokenUrl = new Uri(authority, "protocol/openid-connect/token");

            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                ["Keycloak"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token",
                    Description = "JWT Authorization: Bearer {token}",
                    OpenIdConnectUrl = authorizationUrl,
                },
                ["KeycloakSelf"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = authorizationUrl,
                            TokenUrl = tokenUrl,
                            Scopes = keycloakOptions.Value.Scopes.Split(' ').ToDictionary(scope => scope, scope => scope)
                        }
                    },
                    Description = "OAuth2 Authorization Code Flow (Keycloak)"
                }
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;

            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Keycloak", Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
                });
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "KeycloakSelf", Type = ReferenceType.SecurityScheme } }] = Array.Empty<string>()
                });
            }
        }
    }
}