// Belsoft.Api entry point: register services, then build the HTTP request pipeline.
using Belsoft.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ─────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Unreadable JSON returns a generic message instead of the parser's, which names our internal types.
        options.AllowInputFormatterExceptionMessages = false);
builder.Services.AddOpenApi();

// Singleton: the in-memory service owns the data, so every request must share one instance (it is thread-safe).
builder.Services.AddSingleton<IOrderService, InMemoryOrderService>();

var app = builder.Build();

// ── HTTP pipeline (order matters: each step runs before the ones below it) ──
if (app.Environment.IsDevelopment())
{
    // API docs are a development tool: in production they would map our attack surface.
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "Belsoft API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization(); // No policies yet; L19 gives this something to enforce.

app.MapControllers();

app.Run();