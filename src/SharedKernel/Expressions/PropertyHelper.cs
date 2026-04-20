using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace SharedKernel.Expressions;

public class PropertyHelper
{
    private static readonly Type TypeOfObject = typeof(object);
    private static readonly ConcurrentDictionary<Type, PropertyHelper[]> _propertiesCache = new();

    public string Name { get; set; }

    // obj => obj.Property
    public Func<object, object> Getter { get; set; }

    public static PropertyHelper[] GetProperties(Type type)
    {
        return _propertiesCache.GetOrAdd(type, _ =>
         [.. type.GetProperties()
            .Select(property =>
            {
                var parameter = Expression.Parameter(TypeOfObject, "obj");

                var parameterConvert = Expression.Convert(parameter, type);

                var body = Expression.MakeMemberAccess(parameterConvert, property);

                var bodyConvert = Expression.Convert(body, TypeOfObject);

                var lambda = Expression.Lambda<Func<object, object>>(bodyConvert, parameter);

                var propertyGetterFunc = lambda.Compile();

                return new PropertyHelper
                    {
                        Name = property.Name,
                        Getter = propertyGetterFunc
                    };
            })]
        );
    }
}