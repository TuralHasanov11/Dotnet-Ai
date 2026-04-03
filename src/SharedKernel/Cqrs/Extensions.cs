using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SharedKernel.Cqrs;

public static class Extensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<IMediator, Mediator>();

        var genericRequestHandlerType = typeof(IRequestHandler<,>);
        var requestHandlerTypes = assemblies.SelectMany(a => a.DefinedTypes)
            .Where(c => !c.IsAbstract && !c.IsInterface)
            .SelectMany(t => t.GetInterfaces().Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericRequestHandlerType)
            .Select(i => new
            {
                Interface = i,
                Implementation = t
            }));

        foreach (var handler in requestHandlerTypes)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }
        
        return services;
    }

    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        return AddMediator(services, Assembly.GetCallingAssembly());
    }
}