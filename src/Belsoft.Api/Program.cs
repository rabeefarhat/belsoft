// Belsoft.Api entry point: register services, then build the HTTP request pipeline.
using Belsoft.Api.Middleware;
using Belsoft.Api.Services;
using Belsoft.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ────────────────────────────────────────
// Bind "Belsoft" to BelsoftOptions and refuse to start if it is invalid: a wrong setting fails at deploy, not at the first order.
builder.Services.AddOptions<BelsoftOptions>()                                          // ①
    .BindConfiguration(BelsoftOptions.SectionName)                                     // ②
    .ValidateDataAnnotations()                                                         // ③
    .ValidateOnStart();                                                                // ④

// ── Services ─────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Unreadable JSON returns a generic message instead of the parser's, which names our internal types.
        options.AllowInputFormatterExceptionMessages = false);
builder.Services.AddOpenApi();

// One ProblemDetails format for every error the framework writes, including unhandled exceptions.
builder.Services.AddProblemDetails();                                                  // ①

// Singleton: the in-memory service owns the data, so every request must share one instance (it is thread-safe).
builder.Services.AddSingleton<IOrderService, InMemoryOrderService>();

var app = builder.Build();

// ── HTTP pipeline: top to bottom on the way in, bottom to top on the way out ──

// 1. Outermost, so its timer covers everything and it still sees the 500 the handler below writes.
app.UseMiddleware<RequestLoggingMiddleware>();                                         // ②

// 2. Safety net: any exception thrown further down becomes a 500 ProblemDetails, with no stack trace.
app.UseExceptionHandler();                                                             // ③

// 3. API docs are a development tool: in production they would map our attack surface.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "Belsoft API v1"));
}

// 4. Plain-HTTP requests are redirected before they reach any endpoint.
app.UseHttpsRedirection();

// 5. No policies yet; L19 adds UseAuthentication directly above this line.
app.UseAuthorization();

// 6. The end of the line: route to a controller action.
app.MapControllers();

app.Run();