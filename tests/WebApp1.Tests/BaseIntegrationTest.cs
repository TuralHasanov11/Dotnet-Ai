using Microsoft.AspNetCore.Mvc.Testing;

namespace WebApp1.Tests;

[Trait("Category", "Integration")]
public class BaseIntegrationTest : IAsyncLifetime
{
    protected HttpClient Client { get; }

    protected BaseFactory Factory { get; }

    public BaseIntegrationTest(BaseFactory factory)
    {
        Factory = factory;

        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    public virtual ValueTask InitializeAsync()
    {
        // Seed data here if needed

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }
}
