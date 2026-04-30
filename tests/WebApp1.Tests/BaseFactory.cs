using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.Keycloak;
using WebApp1.Tests;

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

        });

        builder.UseEnvironment("Development");
    }

    public async ValueTask InitializeAsync()
    {
        await _keycloakContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _keycloakContainer.StopAsync();
        await _keycloakContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}