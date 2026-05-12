namespace SharedKernel.Cqrs;


public interface IRequest;

public interface IRequest<TResponse> where TResponse : class;