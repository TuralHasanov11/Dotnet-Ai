namespace SharedKernel.Cqrs;

public interface IRequestHandler<TRequest> where TRequest
    : IRequest
{
    Task Handle(TRequest request);
}

public interface IRequestHandler<TRequest, TResponse> where TRequest
    : IRequest<TResponse>
    where TResponse : class
{
    Task<TResponse> Handle(TRequest request);
}