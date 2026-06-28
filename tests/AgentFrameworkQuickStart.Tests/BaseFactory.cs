using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using AgentFrameworkQuickStart.Tests;

[assembly: AssemblyFixture(typeof(BaseFactory))]

namespace AgentFrameworkQuickStart.Tests;

public class BaseFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {

        });

        builder.UseEnvironment("Development");
    }

    public async ValueTask InitializeAsync()
    {
        try
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<BaseFactory>();
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
            await base.DisposeAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error during test container cleanup", ex);
        }
    }
}