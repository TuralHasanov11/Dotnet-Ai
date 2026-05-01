using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ServiceDefaults;

public class ProblemExceptionHandler(
    ILogger<ProblemExceptionHandler> logger,
    IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    private readonly ILogger<ProblemExceptionHandler> _logger = logger;

    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {

        var problemDetails = new ProblemDetails
        {
            Status = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                NotImplementedException => StatusCodes.Status501NotImplemented,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            },
            Title = exception.GetType().Name,
            Detail = exception.Message,
            Type = exception.HelpLink,
        };

        _logger.LogProblemException(problemDetails.Detail, DateTime.UtcNow);

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails,
            });
    }
}

internal static partial class ProblemExceptionHandlerLogger
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Error Message: {Message}, Time of occurrence {Time}")]
    public static partial void LogProblemException(
        this ILogger<ProblemExceptionHandler> logger,
        string message, DateTime time);
}