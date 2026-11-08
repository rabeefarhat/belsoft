// Belsoft.Api entry point: register services, then build the HTTP request pipeline.
using Belsoft.Api.Configuration;
using Belsoft.Api.Data;
using Belsoft.Api.ErrorHandling;
using Belsoft.Api.Middleware;
using Belsoft.Api.Pricing;
using Belsoft.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ────────────────────────────────────────
// Bind "Belsoft" to BelsoftOptions and refuse to start if it is invalid: a wrong setting fails at deploy, not at the first order.
builder.Services.AddOptions<BelsoftOptions>()
    .BindConfiguration(BelsoftOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Fail at startup, not at the first request. The message names the setting, never its value.
string connectionString = builder.Configuration.GetConnectionString("Belsoft")
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

// Known conflicts (illegal transition, stale RowVersion) → 409 ProblemDetails. Used by UseExceptionHandler() below.
builder.Services.AddExceptionHandler<ConflictExceptionHandler>();

// Scoped by default: one DbContext per request.
builder.Services.AddDbContext<BelsoftDbContext>(options => options.UseSqlServer(connectionString));

// One shared clock for the app; logic asks it for the time instead of calling DateTimeOffset.UtcNow.
builder.Services.AddSingleton(TimeProvider.System);

// ── Pricing (L10) ────────────────────────────────────────
// Every rule, registered under its key. Singletons: they are stateless and pure.
// A NEW RULE = a new class + one line here. Checkout (EfOrderService) never changes.
builder.Services.AddKeyedSingleton<IPricingStrategy, StandardPricing>(StandardPricing.Key);              // ①
builder.Services.AddKeyedSingleton<IPricingStrategy, VolumeDiscountPricing>(VolumeDiscountPricing.Key);
builder.Services.AddKeyedSingleton<IPricingStrategy, LoyaltyDiscountPricing>(LoyaltyDiscountPricing.Key);
builder.Services.AddKeyedSingleton<IPricingStrategy, SpendThresholdPricing>(SpendThresholdPricing.Key);

// The ACTIVE rule, as a plain IPricingStrategy: consumers never see keys.
builder.Services.AddSingleton<IPricingStrategy>(services =>                                              // ②
{
    string rule = services.GetRequiredService<IOptions<BelsoftOptions>>().Value.PricingRule;

    // The rule name isn't secret, so naming it in the error is safe and saves a debugging session.
    return services.GetKeyedService<IPricingStrategy>(rule)
        ?? throw new InvalidOperationException(
            $"Pricing rule '{rule}' is not registered. Check Belsoft:PricingRule (keys are case-sensitive).");
});

// Scoped, because it depends on the scoped DbContext. The L04 interface makes this the only line that changes.
builder.Services.AddScoped<IOrderService, EfOrderService>();

var app = builder.Build();

// Resolve the active pricing rule once now, so a typo stops the app at startup, not at the first order.
_ = app.Services.GetRequiredService<IPricingStrategy>();                                                  // ③

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