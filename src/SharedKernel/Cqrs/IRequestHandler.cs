using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedKernel.Cqrs;

public interface IRequestHandler<TRequest, TResponse> where TRequest
    : IRequest<TResponse>
    where TResponse : class
{
    Task<TResponse> Handle(TRequest request);
}