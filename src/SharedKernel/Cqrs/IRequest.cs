using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedKernel.Cqrs;

public interface IRequest<TResponse> where TResponse : class
{
    
}