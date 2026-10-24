var builder = WebApplication.CreateBuilder(args);                    // ①

builder.Services.AddControllers();                                   // ②
builder.Services.AddOpenApi();                                       // ③

var app = builder.Build();                                           // ④

if (app.Environment.IsDevelopment())                                 // ⑤
{
    app.MapOpenApi();                                                // ⑥
    app.UseSwaggerUI(options =>                                      // ⑦
        options.SwaggerEndpoint("/openapi/v1.json", "Belsoft API v1"));
}

app.UseHttpsRedirection();                                           // ⑧
app.UseAuthorization();                                              // ⑨

app.MapControllers();                                                // ⑩

app.Run();                                                           // ⑪