using System.Diagnostics;

namespace Belsoft.Api.Middleware;

// Writes one log line per request. Placed first in the pipeline, so its timer covers everything after it.
public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger) // ①
{
    public async Task InvokeAsync(HttpContext context)                                // ②
    {
        long startedAt = Stopwatch.GetTimestamp();                                    // ③

        await next(context);                                                          // ④

        TimeSpan elapsed = Stopwatch.GetElapsedTime(startedAt);                       // ⑤

        // Path only, never the query string: queries can carry personal data or tokens.
        logger.LogInformation(                                                        // ⑥
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds:0.0} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsed.TotalMilliseconds);
    }
}