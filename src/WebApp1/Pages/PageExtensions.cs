using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp1.Pages;

public static class PageExtensions
{
    private static readonly ConcurrentDictionary<string, string> _actionNameCache = new();

    extension(PageModel page)
    {
        public RedirectToPageResult RedirectToPage<TPage>(Expression<Action<TPage>> redirectExpression) where TPage : PageModel
        {
            if (redirectExpression.Body.NodeType != ExpressionType.Call)
            {
                throw new InvalidOperationException("Expression must be a method call.");
            }

            var methodCallExpression = (MethodCallExpression)redirectExpression.Body;

            var actionName = GetActionName(methodCallExpression);
            var pageName = typeof(TPage).Name.Replace("Model", string.Empty, StringComparison.OrdinalIgnoreCase);

            var routeValues = ExtractRouteValues(methodCallExpression);

            return page.RedirectToPage(pageName, actionName, routeValues);
        }
    }

    private static string GetActionName(MethodCallExpression methodCallExpression)
    {
        var cacheKey = $"{methodCallExpression.Method.Name}.{methodCallExpression.Object.Type.Name}";

        return _actionNameCache.GetOrAdd(cacheKey, _ =>
         {
             var methodName = methodCallExpression.Method.Name;
             var actionNameAttribute = methodCallExpression.Method.GetCustomAttributes(typeof(ActionNameAttribute), false)
                 .FirstOrDefault() as ActionNameAttribute;

             return actionNameAttribute?.Name ?? methodName;
         });
    }

    private static RouteValueDictionary ExtractRouteValues(MethodCallExpression methodCallExpression)
    {
        var parameters = methodCallExpression.Method.GetParameters().Select(p => p.Name);

        var values = methodCallExpression.Arguments.Select(arg =>
        {
            if (arg.NodeType == ExpressionType.Constant)
            {
                var constantExpression = (ConstantExpression)arg;
                return constantExpression.Value;
            }

            var convertedExpression = Expression.Convert(arg, typeof(object));
            var funcExpression = Expression.Lambda<Func<object>>(convertedExpression);

            return funcExpression.Compile()();
        });

        var routeValues = new RouteValueDictionary();
        foreach (var (parameter, value) in parameters.Zip(values, (p, v) => (p, v)))
        {
            routeValues.Add(parameter, value);
        }

        return routeValues;
    }
}