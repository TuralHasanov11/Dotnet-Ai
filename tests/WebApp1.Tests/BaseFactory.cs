using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Keycloak;
using WebApp1.Tests;
using WebApp1.Tests.Identity;

[assembly: AssemblyFixture(typeof(BaseFactory))]

namespace WebApp1.Tests;

public class BaseFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string RealmName = "DotnetAi";

    public const string ClientId = "dotnet-ai-client-web-app-1";

    public const string ClientSecret = "your-client-secret-for-web-app-1";

    public const string RedirectUri = "http://localhost:5000/signin-oidc";

    public const string TokenUserName = "test-user";

    public const string TokenPassword = "password";

    private readonly KeycloakContainer _keycloakContainer = new KeycloakBuilder("quay.io/keycloak/keycloak:21.1")
        .WithRealm(Path.Combine(AppContext.BaseDirectory, "Keycloak", "DotnetAi-realm.json"))
        .WithUsername("admin")
        .WithPassword("admin")
        .Build();

    public BaseFactory()
    {
        UseKestrel(options => options.ListenLocalhost(5002));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:Authority"] = new Uri(new Uri(_keycloakContainer.GetBaseAddress()), $"realms/{RealmName}").ToString(),
                ["Keycloak:ClientId"] = ClientId,
                ["Keycloak:ClientSecret"] = ClientSecret,
                ["Keycloak:RedirectUri"] = RedirectUri,
                ["Keycloak:Username"] = TokenUserName,
                ["Keycloak:Password"] = TokenPassword,
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "TestScheme", options => { });
        });

        builder.UseEnvironment("Development");
    }

    public async ValueTask InitializeAsync()
    {
        try
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<BaseFactory>();

            logger.LogInformation("Starting Keycloak container...");
            await _keycloakContainer.StartAsync();
            logger.LogInformation("Keycloak started at {BaseAddress}", _keycloakContainer.GetBaseAddress());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize test containers", ex);
        }
    }

    public new async Task DisposeAsync()
    {
        try
        {
            if (_keycloakContainer is not null)
            {
                await _keycloakContainer.StopAsync();
                await _keycloakContainer.DisposeAsync();
            }

            await base.DisposeAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error during test container cleanup", ex);
        }
    }
}