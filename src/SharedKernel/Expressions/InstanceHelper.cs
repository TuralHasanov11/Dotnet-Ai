using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace SharedKernel.Expressions;

public static class InstanceHelper
{
    public static object CreateInstance(Type type)
    {
        return ObjectFactoryCreator<TypeToIgnore, TypeToIgnore, TypeToIgnore>.CreateInstance(type, null, null, null);
    }

    public static object CreateInstance<TArg1>(Type type, TArg1 argument)
    {
        return ObjectFactoryCreator<TArg1, TypeToIgnore, TypeToIgnore>.CreateInstance(type, argument, null, null);
    }

    public static object CreateInstance<TArg1, TArg2>(Type type, TArg1 argument1, TArg2 argument2)
    {
        return ObjectFactoryCreator<TArg1, TArg2, TypeToIgnore>.CreateInstance(type, argument1, argument2, null);
    }

    public static object CreateInstance<TArg1, TArg2, TArg3>(Type type, TArg1 argument1, TArg2 argument2, TArg3 argument3)
    {
        return ObjectFactoryCreator<TArg1, TArg2, TArg3>.CreateInstance(type, argument1, argument2, argument3);
    }

    private sealed class TypeToIgnore
    {
    }

    private static class ObjectFactoryCreator<TArg1, Targ2, Targ3>
    {
        private static readonly ConcurrentDictionary<string, Func<TArg1, Targ2, Targ3, object>> _factoryCache = new();

        public static object CreateInstance(Type type, TArg1 argument1, Targ2 argument2, Targ3 argument3)
        {
            var cacheKey = $"{type.FullName}.{typeof(TArg1).FullName}.{typeof(Targ2).FullName}.{typeof(Targ3).FullName}";

            var factory = _factoryCache.GetOrAdd(cacheKey, _ =>
            {
                var argumentTypes = new[] { typeof(TArg1), typeof(Targ2), typeof(Targ3) };
                var constructorArgumentTypes = argumentTypes.Where(t => t != typeof(TypeToIgnore)).ToArray();

                var constructor = type.GetConstructor(constructorArgumentTypes);
                if (constructor == null)
                {
                    throw new InvalidOperationException($"No constructor found for type {type.FullName} with the specified argument types.");
                }

                var expressionParameters = argumentTypes.Select((t, i) => Expression.Parameter(t, $"param{i}")).ToArray();
                var expressionConstructorParameters = expressionParameters.Take(constructorArgumentTypes.Length).ToArray();

                var newExpression = Expression.New(constructor, expressionConstructorParameters);

                var lambda = Expression.Lambda<Func<TArg1, Targ2, Targ3, object>>(Expression.Convert(newExpression, typeof(object)), expressionParameters);

                return lambda.Compile();
            });

            return factory(argument1, argument2, argument3);
        }
    }
}