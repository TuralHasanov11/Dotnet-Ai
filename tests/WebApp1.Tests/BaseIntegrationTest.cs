using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using WebApp1.Tests.Identity;

namespace WebApp1.Tests;

[Trait("Category", "Integration")]
public class BaseIntegrationTest : IAsyncLifetime
{
    /// <summary>HTTP client configured for the test API</summary>
    protected HttpClient Client { get; }

    /// <summary>WebApplicationFactory for creating test clients and accessing DI container</summary>
    protected BaseFactory Factory { get; }

    public BaseIntegrationTest(BaseFactory factory)
    {
        Factory = factory;

        Client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "TestScheme";
                        options.DefaultChallengeScheme = "TestScheme";
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "TestScheme");
    }

    /// <summary>
    /// Initialize async resources (data seeding, etc).
    /// Override in derived classes to seed test-specific data.
    /// </summary>
    public virtual ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Clean up async resources.
    /// Override in derived classes to clean up test-specific data.
    /// </summary>
    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
