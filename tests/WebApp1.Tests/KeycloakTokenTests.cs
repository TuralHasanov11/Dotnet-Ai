using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WebApp1.Tests;

public class KeycloakTokenTests(BaseFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CanRequestAccessTokenForSeededUser()
    {
        var configuration = Factory.Services.GetRequiredService<IConfiguration>();
        var httpClientFactory = Factory.Services.GetRequiredService<IHttpClientFactory>();
        var loggerFactory = Factory.Services.GetRequiredService<ILoggerFactory>();

        var tokenProvider = new WebAccessTokenProvider(configuration, httpClientFactory, loggerFactory);

        var token = await tokenProvider.GetApiToken(
            BaseFactory.ClientId,
            "openid profile email",
            BaseFactory.ClientSecret);

        Assert.False(string.IsNullOrWhiteSpace(token));
    }
}