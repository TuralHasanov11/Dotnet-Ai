using System.Net;

namespace WebApp1.Tests;

public class PrivacyPageTests(BaseFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task PageLoadsSuccessfully()
    {
        var response = await Client.GetAsync("/privacy", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("<h1>Privacy Policy</h1>", content, StringComparison.OrdinalIgnoreCase);
    }
}