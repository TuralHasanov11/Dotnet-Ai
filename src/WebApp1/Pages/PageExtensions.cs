using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp1.Pages;

public static class PageExtensions
{
    extension(PageModel page)
    {
        public RedirectToPageResult RedirectToPage<TPage>(Expression<Action<TPage>> redirectExpression) where TPage : PageModel
        {
            if (redirectExpression.Body.NodeType != ExpressionType.Call)
            {
                throw new InvalidOperationException("Expression must be a method call.");
            }

            var methodCallExpression = (MethodCallExpression)redirectExpression.Body;

            var actionName = methodCallExpression.Method.Name;
            var pageName = typeof(TPage).Name.Replace("Model", string.Empty, StringComparison.OrdinalIgnoreCase);

            return page.RedirectToPage(pageName, actionName);
        }
    }
}