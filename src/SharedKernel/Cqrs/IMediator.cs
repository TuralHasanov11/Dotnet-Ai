using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedKernel.Cqrs;

public interface IMediator
{
    Task Send<TResponse>(IRequest<TResponse> request) where TResponse : class;
}