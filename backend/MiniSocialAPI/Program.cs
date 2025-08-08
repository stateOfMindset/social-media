var builder = WebApplication.CreateBuilder(args);

// Add CORS services and define a policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:8080")  // your frontend origin here
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add controllers etc.
builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Add routing middleware
app.UseRouting();

// Use CORS BEFORE authorization and endpoints
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

// Middleware to ensure UTF-8 charset on JSON responses
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.ContentType != null &&
        context.Response.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
    {
        if (!context.Response.ContentType.Contains("charset"))
        {
            context.Response.ContentType += "; charset=utf-8";
        }
    }
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
