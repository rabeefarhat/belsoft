// Belsoft.Api entry point: register services, then build the HTTP request pipeline.
using Belsoft.Api.Configuration;
using Belsoft.Api.Data;
using Belsoft.Api.Middleware;
using Belsoft.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ────────────────────────────────────────
// Bind "Belsoft" to BelsoftOptions and refuse to start if it is invalid: a wrong setting fails at deploy, not at the first order.
builder.Services.AddOptions<BelsoftOptions>()                                          // ①
    .BindConfiguration(BelsoftOptions.SectionName)                                     // ②
    .ValidateDataAnnotations()                                                         // ③
    .ValidateOnStart();                                                                // ④

// Fail at startup with a clear message. The message names the setting, never its value.
string connectionString = builder.Configuration.GetConnectionString("Belsoft")         // ⑤
    ?? throw new InvalidOperationException(
        "Connection string 'Belsoft' is missing. Set it in user secrets (Development) or as the environment variable ConnectionStrings__Belsoft.");

// ── Services ─────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Unreadable JSON returns a generic message instead of the parser's, which names our internal types.
        options.AllowInputFormatterExceptionMessages = false);
builder.Services.AddOpenApi();

// One ProblemDetails format for every error the framework writes, including unhandled exceptions.
builder.Services.AddProblemDetails();

// Scoped by default: one DbContext per request.
builder.Services.AddDbContext<BelsoftDbContext>(options =>                             // ⑥
    options.UseSqlServer(connectionString));

// The clock is injected so logic never calls DateTimeOffset.UtcNow directly (and tests can control time later).
builder.Services.AddSingleton(TimeProvider.System);                                    // ⑦

// Scoped, because it depends on the scoped DbContext. Singleton here would be a captive dependency.
//builder.Services.AddScoped<IOrderService, InMemoryOrderService>();
builder.Services.AddScoped<IOrderService, EfOrderService>();                           // ⑧

var app = builder.Build();

// ── HTTP pipeline: top to bottom on the way in, bottom to top on the way out ──

// 1. Outermost, so its timer covers everything and it still sees the 500 the handler below writes.
app.UseMiddleware<RequestLoggingMiddleware>();

// 2. Safety net: any exception thrown further down becomes a 500 ProblemDetails, with no stack trace.
app.UseExceptionHandler();

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

// 6. The end of the line: route to a controller action. No Database.Migrate() here, on purpose. See L07 Part 1.  // ⑨
app.MapControllers();

app.Run();