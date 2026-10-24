// Belsoft.Api entry point: register services, then build the HTTP request pipeline.
var builder = WebApplication.CreateBuilder(args);

// ── Services ─────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

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