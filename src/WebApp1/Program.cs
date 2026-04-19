var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("agent-framework-quick-start", (serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["AgentFrameworkQuickStart:BaseUrl"];

    if (!string.IsNullOrWhiteSpace(baseUrl) && Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
    {
        client.BaseAddress = uri;
    }
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapHealthChecks("/health");

app.MapGet("/ping-agent", async (IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
{
    var client = httpClientFactory.CreateClient("agent-framework-quick-start");

    if (client.BaseAddress is null)
    {
        return Results.Problem(
            title: "Agent base URL is not configured",
            detail: "Set AgentFrameworkQuickStart__BaseUrl in configuration.",
            statusCode: StatusCodes.Status500InternalServerError);
    }

    try
    {
        using var response = await client.GetAsync("/", cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        return Results.Json(new
        {
            target = client.BaseAddress.ToString(),
            statusCode = (int)response.StatusCode,
            success = response.IsSuccessStatusCode,
            response = payload
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Failed to reach agent-framework-quick-start",
            detail: ex.Message,
            statusCode: StatusCodes.Status502BadGateway);
    }
});

app.Run();