using System.Net.Http.Headers;

namespace WebApp1.Tests.Identity;

public static class HttpClientExtensions
{
    public static HttpClient WithAuthentication(this HttpClient client, string jwt)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return client;
    }
}
