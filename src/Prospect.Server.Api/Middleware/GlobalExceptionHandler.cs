using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Prospect.Server.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhanded exception occurred");

        const int statusCode = StatusCodes.Status500InternalServerError;

        var problem = new ProblemDetails
        {
            Title = "Internal Server Error",
            Instance = httpContext.Request.Path,
            Status = statusCode,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1", // For HTTP 500
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
