using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using ApiTest.ApiService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Configure DbContext with Azure Default Credentials
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=your-database;";

builder.Services.AddDbContext<AppDbContext>(x =>
{
    x.UseSqlServer("Server=tcp:sql-dev-testing.database.windows.net;Database=hradb-dev;Authentication=Active Directory Default");
});

var app = builder.Build();

app.UseDeveloperExceptionPage();

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

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

app.MapGet("/test-db", async (AppDbContext dbContext) =>
{
    try
    {
        // Test if we can connect to the database
        var canConnect = await dbContext.Database.CanConnectAsync();

        if (canConnect)
        {
            // Try a simple query to test the connection
            var testResult = await dbContext.TestEntities.CountAsync();
            return Results.Ok(new
            {
                success = true,
                message = "Database connection successful!",
                entityCount = testResult,
                timestamp = DateTime.UtcNow
            });
        }
        else
        {
            return Results.Ok(new
            {
                success = false,
                message = "Cannot connect to database"
            });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: $"Database connection failed: {ex.Message}",
                              statusCode: 500);
    }
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
