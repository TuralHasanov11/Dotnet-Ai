namespace SharedKernel.Cqrs;

public interface IMediator
{
    Task Send<TResponse>(IRequest<TResponse> request) where TResponse : class;
}