using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using WebApp1.Tests;

[assembly: AssemblyFixture(typeof(BaseFactory))]

namespace WebApp1.Tests;

public class BaseFactory : WebApplicationFactory<Program>
{
    public BaseFactory()
    {
        UseKestrel(options => options.ListenLocalhost(5002));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment variables or other configuration settings here
        Environment.SetEnvironmentVariable("SOME_KEY", "some values");

        builder.ConfigureServices(services =>
        {

        });

        builder.UseEnvironment("Development");
    }
}