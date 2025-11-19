using StackExchange.Redis;
using Uno.API.Services.Interfaces;
using Uno.API.Services.Implementations;
var builder = WebApplication.CreateBuilder(args);

// Load secrets configuration
builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis
var redisHost = builder.Configuration["Redis:Host"]
    ?? throw new InvalidOperationException("Redis:Host is not configured");
var redisPort = builder.Configuration.GetValue<int>("Redis:Port");
var redisUser = builder.Configuration["Redis:User"]
    ?? throw new InvalidOperationException("Redis:User is not configured");
var redisPassword = builder.Configuration["Redis:Password"]
    ?? throw new InvalidOperationException("Redis:Password is not configured");

var redisConfig = new ConfigurationOptions{
    EndPoints = { { redisHost, redisPort } },
    User = redisUser,
    Password = redisPassword
};
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConfig)
);
builder.Services.AddSingleton<IRedisService, RedisService>();

// Services
builder.Services.AddScoped<IGameService, GameService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
