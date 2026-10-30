// Belsoft.Api entry point: register services, then build the HTTP request pipeline.
using Belsoft.Api.Configuration;
using Belsoft.Api.Data;
using Belsoft.Api.Middleware;
using Belsoft.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ────────────────────────────────────────
// Bind "Belsoft" to BelsoftOptions and refuse to start if it is invalid: a wrong setting fails at deploy, not at the first order.
builder.Services.AddOptions<BelsoftOptions>()
    .BindConfiguration(BelsoftOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Fail at startup, not at the first request. The message names the setting, never its value.
string connectionString = builder.Configuration.GetConnectionString("Belsoft")                       // ①
    ?? throw new InvalidOperationException(
        "Connection string 'Belsoft' is missing. Set it in user secrets (Development) or the environment.");

// ── Services ─────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Unreadable JSON returns a generic message instead of the parser's, which names our internal types.
        options.AllowInputFormatterExceptionMessages = false);
builder.Services.AddOpenApi();

// One ProblemDetails format for every error the framework writes, including unhandled exceptions.
builder.Services.AddProblemDetails();

// Scoped by default: one DbContext per request.
builder.Services.AddDbContext<BelsoftDbContext>(options => options.UseSqlServer(connectionString)); // ②

// One shared clock for the app; logic asks it for the time instead of calling DateTimeOffset.UtcNow.
builder.Services.AddSingleton(TimeProvider.System);                                                  // ③

// Scoped, because it depends on the scoped DbContext. The L04 interface makes this the only line that changes.
builder.Services.AddScoped<IOrderService, EfOrderService>();                                         // ④

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

// 5. No policies yet; L20 adds UseAuthentication directly above this line.
app.UseAuthorization();

// 6. The end of the line: route to a controller action.
app.MapControllers();

app.Run();