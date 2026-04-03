using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedKernel.Cqrs;

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public Task Send<TResponse>(IRequest<TResponse> request) where TResponse : class
    {
        var requestHandlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        return serviceProvider.GetService(requestHandlerType) is not IRequestHandler<IRequest<TResponse>, TResponse> handler
            ? throw new InvalidOperationException($"No handler found for request of type {request.GetType()}")
            : (Task)handler.Handle(request);
    }
}