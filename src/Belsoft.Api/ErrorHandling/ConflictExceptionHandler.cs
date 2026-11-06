using Belsoft.Api.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Belsoft.Api.ErrorHandling;

// Expected clashes with the current state of an order become 409 ProblemDetails.
// Anything this handler doesn't recognise falls through to the generic 500 (L05).
public sealed class ConflictExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ConflictExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails? problem = exception switch                                         // ①
        {
            DomainException domain => new ProblemDetails
            {
                Title = "The order cannot make that change.",
                Detail = domain.Message,                                                  // ②
            },
            DbUpdateConcurrencyException => new ProblemDetails
            {
                Title = "The order was changed by someone else.",
                Detail = "Reload the order and send its current ETag in If-Match.",       // ③
            },
            _ => null,
        };

        if (problem is null)
        {
            return false;                                                                  // ④
        }

        problem.Status = StatusCodes.Status409Conflict;
        problem.Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10";

        logger.LogWarning(                                                                 // ⑤
            "{Method} {Path} refused with 409: {Reason}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            problem.Title);

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext        // ⑥
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
        });
    }
}