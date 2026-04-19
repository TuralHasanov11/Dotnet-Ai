using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Cqrs;

namespace SharedKernel.Tests;

[Trait("Category", "Unit")]
public sealed class CqrsTests
{
    [Fact]
    public void AddMediator_RegistersMediatorService()
    {
        var services = new ServiceCollection();

        services.AddMediator(typeof(TestRequestHandler).Assembly);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var mediator = scope.ServiceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddMediator_RegistersHandlersFromProvidedAssemblies()
    {
        var services = new ServiceCollection();

        services.AddMediator(typeof(TestRequestHandler).Assembly);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetService<IRequestHandler<TestRequest, TestResponse>>();

        Assert.NotNull(handler);
    }

    [Fact]
    public void AddMediator_WithoutAssembly_UsesCallingAssembly()
    {
        var services = new ServiceCollection();

        services.AddMediator();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetService<IRequestHandler<TestRequest, TestResponse>>();

        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Send_WithoutHandler_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        services.AddScoped<IMediator, Mediator>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.Send(new NoHandlerRequest()));

        var requestTypeName = typeof(NoHandlerRequest).FullName ?? nameof(NoHandlerRequest);
        Assert.Contains(requestTypeName, exception.Message, StringComparison.Ordinal);
    }

    public sealed record TestResponse(string Value);

    public sealed record TestRequest : IRequest<TestResponse>;

    public sealed class TestRequestHandler : IRequestHandler<TestRequest, TestResponse>
    {
        public Task<TestResponse> Handle(TestRequest request)
        {
            return Task.FromResult(new TestResponse("ok"));
        }
    }

    public sealed record NoHandlerResponse(string Value);

    public sealed record NoHandlerRequest : IRequest<NoHandlerResponse>;
}